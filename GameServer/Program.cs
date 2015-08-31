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
            BackgroundWorker thread = null;
            AnswerManager answerResponse = new AnswerManager();
            thread = new BackgroundWorker();
            thread.WorkerReportsProgress = true;
            thread.ProgressChanged += (a,b) =>
            {
                Thread.Sleep(1);
                var questionc = (Tuple<Thread,CardGameQuestion>) b.UserState;
                var question = questionc.Item2;

                Console.WriteLine(question.User.UserName + ": " + question.Question);
                foreach (var answer in question.Answers)
                {
                    Console.WriteLine(answer);
                }
                answerResponse.Answer = 1;//int.Parse(Console.ReadKey().KeyChar.ToString());
                if ((questionc.Item1.ThreadState & ThreadState.Suspended) == ThreadState.Suspended)
                {
                    questionc.Item1.Resume();
                    return;
                }
                Console.WriteLine(questionc.Item1.ThreadState);
                Console.WriteLine(questionc.Item1.ThreadState);
                if ((questionc.Item1.ThreadState & ThreadState.Background) == ThreadState.Background)
                {
                    questionc.Item1.Resume();
                }
            };


            thread.DoWork += (t, cc) =>
            { 

                Jint.Engine engine = new Jint.Engine();
                engine.SetValue("log", new Action<object>(a => { Console.WriteLine(a); }));


                engine.SetValue("CardGame", TypeReference.CreateTypeReference(engine, typeof (CardGameLibrary.GameCardGame)));
                engine.SetValue("Pile", TypeReference.CreateTypeReference(engine, typeof (CardGameLibrary.CardGamePile)));

                engine.SetValue("TableTextArea", TypeReference.CreateTypeReference(engine, typeof (CardGameLibrary.GameCardGameTextArea)));
                engine.SetValue("TableSpace", TypeReference.CreateTypeReference(engine, typeof (CardGameLibrary.CardGameTableSpace)));
                engine.SetValue("Pile", TypeReference.CreateTypeReference(engine, typeof (CardGameLibrary.CardGamePile)));
                engine.SetValue("GameUtils", TypeReference.CreateTypeReference(engine, typeof (CardGameLibrary.GameUtils)));
                engine.SetValue("Shuff", TypeReference.CreateTypeReference(engine, typeof (CardGameLibrary.Shuff)));

                try
                {

                    var sevens = File.ReadAllText("js/sevens.js");
                    var c = executeES6(engine, sevens);
                    c = c.Execute("var cg=new CardGame()");

                    c.GetValue("cg").AsObject().Get("__init__").Invoke(c.GetValue("cg"), new JsValue[] {12});

                    c = c.Execute("var _=new GameUtils()");
                    c = c.Execute("var shuff=new Shuff()");
                    c.GetValue("shuff").AsObject().Get("SetDelegates").Invoke(c.GetValue("shuff"), new JsValue[]
                    {
                        JsValue.FromObject(engine, new CardGameDelegates()
                        {
                            AskQuestionCallback = new AskQuestionDelegate(question => askQuestion(thread, answerResponse, question))
                        })
                    });
                    c = c.Execute("var sevens=new Sevens()");

                    c = c.Execute("sevens.constructor(cg);");

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

            Console.WriteLine("Press any [Enter] to close the host.");
            Console.ReadLine();
        }

        private static CardGameAnswer askQuestion(BackgroundWorker thread, AnswerManager answerResponse, CardGameQuestion question)
        {
            thread.ReportProgress(0, Tuple.Create(Thread.CurrentThread, question));
            Thread.CurrentThread.Suspend();
            return new CardGameAnswer() {Value = answerResponse.Answer };
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
