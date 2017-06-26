using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using Newtonsoft.Json;
using SocketServer;

namespace TestClient
{
    class Program
    {
        private static Timer timer;
        static void Main(string[] args)
        {
            Thread.Sleep(2000);

            now = DateTime.Now;

            timer = new Timer((e) =>
            {
                var diff = (DateTime.Now - now).TotalSeconds;

                if (diff > 10)
                {
                    now = DateTime.Now;
                }

                Console.WriteLine($"Total Games: {totalGames} Games: {games} Answers: {totalAnswers} APGPS:{totalAnswers / (double)totalGames / diff}");
            }, null, 0, 500);

            List<Task> tasks = new List<Task>();
            while (true)
            {
                for (int i = tasks.Count; i < 300; i++)
                {
                    tasks.Add(Task.Run(startRunner));
                }
                Task.WaitAny(tasks.ToArray());
                tasks.RemoveAll(a => a.IsCompleted);
            }
        }


        static int totalGames = 0;
        static int games = 0;
        static int totalAnswers = 0;
        private static DateTime now;

        private static async Task startRunner()
        {
            var socketUrl = await getSocketUrl();
            ClientWebSocket ws = new ClientWebSocket();
            await ws.ConnectAsync(new Uri($"ws://{socketUrl}"), CancellationToken.None);
            var buffer = new byte[1024 * 10];
            await send(ws, new CreateNewGameRequestSocketMessage() { GameType = "sevens" });

            while (true)
            {
                var segment = new ArraySegment<byte>(buffer);

                var result = await ws.ReceiveAsync(segment, CancellationToken.None);

                switch (result.MessageType)
                {
                    case WebSocketMessageType.Binary:

                        byte[] bytes = new byte[result.Count];
                        Array.ConstrainedCopy(segment.Array, 0, bytes, 0, result.Count);
                        var obj = Serializer.Deserialize(bytes);
                        if (obj is GameStartedSocketMessage)
                        {
                            totalGames++;
                            games++;
                        }
                        else if (obj is AskQuestionSocketMessage)
                        {
                            var answerQ = (AskQuestionSocketMessage)obj;
                            await send(ws, new AnswerQuestionSocketMessage()
                            {
                                AnswerIndex = (short)(answerQ.Answers.Length > 1 ? 1 : 0)
                            });
                            totalAnswers++;
                        }
                        else if (obj is GameOverSocketMessage)
                        {
                            await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
                            games--;
                            return;
                        }

                        break;
                    case WebSocketMessageType.Close:
                        await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Done", CancellationToken.None);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }


            }

        }

        private static async Task send(ClientWebSocket ws, object message)
        {
            await ws.SendAsync(new ArraySegment<byte>(Serializer.Serialize(message)), WebSocketMessageType.Binary, true, CancellationToken.None);
        }

        private static async Task<string> getSocketUrl()
        {

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri("http://127.0.0.1:3579");

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = await client.GetAsync("/api/gateway");
            var result = await response.Content.ReadAsStringAsync();
            var resp = JsonConvert.DeserializeObject<GatewayResponse>(result);
            return resp.Data.GatewayUrl;
        }
    }

    public class GatewayResponse
    {
        public GatewayDataResponse Data { get; set; }
    }

    public class GatewayDataResponse
    {
        public string GatewayUrl { get; set; }
    }
}
