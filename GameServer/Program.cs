using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
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

    class Program
    {
        static void Main(string[] args)
        {
            Thread thread=null;
            thread = new Thread(() =>
             {
                 Jint.Engine engine = new Jint.Engine();
                 engine.SetValue("log", new Action<object>(a =>{Console.WriteLine(a);}));


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
                     c.GetValue("shuff").AsObject().Get("SetThread").Invoke(c.GetValue("shuff"), new JsValue[] { JsValue.FromObject(engine, thread) });
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

             });
            thread.Start();

            Console.WriteLine("Press any [Enter] to close the host.");
            Console.ReadLine();
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
