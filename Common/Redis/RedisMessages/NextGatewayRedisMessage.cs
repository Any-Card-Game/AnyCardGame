namespace Common.Redis.RedisMessages
{
    public abstract class RedisMessage
    {
        protected RedisMessage()
        {
            Guid = System.Guid.NewGuid().ToString("N");
        }

        public string Guid { get; set; }
    }
    public class DefaultRedisMessage : RedisMessage
    {
    }
    public enum RedisChannels
    {
        GetNextGatewayRequest,
        GetNextGatewayResponse,
        CreateNewGameRequest,
    }

    public class NextGatewayResponseRedisMessage : RedisMessage
    {
        public string GatewayUrl { get; set; }
    }
    public class CreateNewGameRequest : RedisMessage
    {
        public string GatewayKey { get; set; }
        public string UserKey { get; set; }
        public string GameName { get; set; }
    }
    public class GameUpdateRedisMessage : RedisMessage
    {
        public string GameId { get; set; }
        public GameStatus GameStatus { get; set; }
        public CardGameQuestionTransport Question { get; set; }
        public string GameServer { get; set; }
        public string UserKey { get; set; }
    }
    public class GameServerRedisMessage : RedisMessage
    {
        public string GameId { get; set; }
        public int AnswerIndex { get; set; }
    }

    public class CardGameQuestionTransport
    {

        public string User { get; set; }
        public string Question { get; set; }
        public string[] Answers { get; set; }
    }
    public enum GameStatus
    {
        Started,
        AskQuestion,
        GameOver
    }


}