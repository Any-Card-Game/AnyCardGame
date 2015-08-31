using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GameServer.CardGameLibrary;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.String;
using Jint.Runtime;
using Jint.Runtime.Debugger;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace GameServer
{
    public class AnswerManager
    {
        public int Answer { get; set; }
    }
    class Program
    {

        static void Main(string[] args)
        {
            int totalGames = 500;
            ThreadPool.SetMinThreads(500, 500);
            for (int i = 0; i < totalGames; i++)
            {
                runThing(i);
            }
            DateTime now=DateTime.Now;
            DateTime startNow=DateTime.Now;
            bool first = false;
            while (true)
            {
                if (answers == 0)
                {
                    continue;
                }
                else
                {
                    if (!first)
                    {
                        Console.WriteLine("First");
                        first = true;
                        startNow = DateTime.Now;
                        now = DateTime.Now;
                    }
                    if (now.AddSeconds(1) <= DateTime.Now)
                    {
                        if (GamesDone == totalGames)
                        {
                            break;
                        }
                        now = DateTime.Now;
                        Console.WriteLine(answers/ (DateTime.Now - startNow).TotalSeconds +" Answers per second");
                    }
                }

            }

            Console.WriteLine("Press any [Enter] to close the host.");
            Console.ReadLine();
        }

        private static int answers = 0;
        private static int GamesDone = 0;
        private static void runThing(int gameId)
        {
            BackgroundWorker thread = null;
            AnswerManager answerResponse = new AnswerManager();
            thread = new BackgroundWorker();
            thread.WorkerReportsProgress = true;
            int questionIndex = 0;
            thread.ProgressChanged += (a, b) =>
            {
                if (b.ProgressPercentage == 0)
                {
                    answers++;

//                                    Console.WriteLine("GAME INDEX::::" + gameId+"::::::"+ questionIndex++);
                    var questionc = (Tuple<Thread, CardGameQuestion>)b.UserState;
                    var question = questionc.Item2;
                    //                    Console.WriteLine(question.User.UserName + ": " + question.Question);
                    foreach (var answer in question.Answers)
                    {
                        //                        Console.WriteLine(answer);
                    }
                    answerResponse.Answer = 1; //int.Parse(Console.ReadKey().KeyChar.ToString());
                    while ((questionc.Item1.ThreadState & ThreadState.Suspended) != ThreadState.Suspended)
                    {
                        Thread.Sleep(10);
                    }

                    if ((questionc.Item1.ThreadState & ThreadState.Suspended) == ThreadState.Suspended)
                    {
                        questionc.Item1.Resume();
                    }
                }
                else if (b.ProgressPercentage == 1)
                {
                    GamesDone++;
                    Console.WriteLine("GAME INDEX::::" + gameId + "::::::");
                    var userc = (Tuple<Thread, CardGameUser>)b.UserState;
                    var user = userc.Item2;

                    Console.WriteLine(user.UserName + " Has won!");
                }
            };


            thread.DoWork += (t, cc) =>
            {
                Console.WriteLine("Started "+gameId);
                Jint.Engine engine = new Jint.Engine();
                engine.SetValue("log", new Action<object>(a => { Console.WriteLine(a); }));


                engine.SetValue("CardGame", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.GameCardGame)));
                engine.SetValue("Pile", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.CardGamePile)));

                engine.SetValue("TableTextArea", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.GameCardGameTextArea)));
                engine.SetValue("TableSpace", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.CardGameTableSpace)));
                engine.SetValue("Pile", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.CardGamePile)));
                engine.SetValue("GameUtils", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.GameUtils)));
                engine.SetValue("Shuff", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.Shuff)));

                try
                {
                    var sevens = File.ReadAllText("js/sevens.js");
                    var c = executeES6(engine, sevens);
                    c = c.Execute("var cg=new CardGame()");

                    c.GetValue("cg").AsObject().Get("__init__").Invoke(c.GetValue("cg"), new JsValue[] { 12 });

                    c = c.Execute("var _=new GameUtils()");
                    c = c.Execute("var shuff=new Shuff()");
                    c.GetValue("shuff").AsObject().Get("SetDelegates").Invoke(c.GetValue("shuff"), new JsValue[]
                    {
                        JsValue.FromObject(engine, new CardGameDelegates()
                        {
                            AskQuestionCallback = question => askQuestion(thread, answerResponse, question),
                            DeclareWinnerCallback = user => declareWinner(thread, user)
                        })
                    });
                    c = c.Execute("var sevens=new Sevens()");

                    c = c.Execute("sevens.constructor(cg);");
                    Console.WriteLine("Running game "+gameId);

                    c = c.Execute("sevens.runGame(cg);");
                }
                catch (Exception exc)
                {
                    if (exc.InnerException is JavaScriptException)
                    {
                        var location = engine.GetLastSyntaxNode().Location;
                        var javaScriptException = (exc.InnerException as JavaScriptException);
                        throw new ApplicationException($"{javaScriptException.Error} " +
                                                       $"({location.Source}: Line {location.Start.Line}, Column {location.Start.Column} to Line {location.End.Line}, Column {location.End.Column})", exc);
                    }
                }
            };

            thread.RunWorkerAsync();
        }

        private static void declareWinner(BackgroundWorker thread, CardGameUser user)
        {
            thread.ReportProgress(1, Tuple.Create(Thread.CurrentThread, user));
            Console.WriteLine("Game over");
            Thread.CurrentThread.Abort();
            Console.WriteLine("Should never get here");
        }

        private static CardGameAnswer askQuestion(BackgroundWorker thread, AnswerManager answerResponse, CardGameQuestion question)
        {
            thread.ReportProgress(0, Tuple.Create(Thread.CurrentThread, question));
            Thread.CurrentThread.Suspend();
            return new CardGameAnswer() { Value = answerResponse.Answer };
        }

        private static Engine executeES6(Engine engine, string text)
        {
            /*
                        var typescript = File.ReadAllText("js/typescript/typescript.js");
                        var mc = engine.Execute(typescript);
                        var tsc = mc.GetValue("ts");
                        var cc = tsc.AsObject().Get("transpile");

                        var c = cc.Invoke(text).AsString();

                        var mcc = engine.Execute(c);
            */
            var mcc = engine.Execute(text);
            return mcc;
        }
    }

}
