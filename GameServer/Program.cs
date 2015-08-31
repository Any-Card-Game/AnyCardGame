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

            //            var thread=new Thread(() =>
            //            {
            Jint.Engine engine = new Jint.Engine();
            engine.SetValue("log", new Action<object>(a =>
            {
                 Console.WriteLine(a);
            }));


            engine.SetValue("CardGame", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.GameCardGame)));
            engine.SetValue("Pile", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.CardGamePile)));

            engine.SetValue("TableTextArea", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.GameCardGameTextArea)));
            engine.SetValue("TableSpace", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.CardGameTableSpace)));
            engine.SetValue("Pile", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.CardGamePile)));
            engine.SetValue("GameUtils", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.GameUtils)));
            engine.SetValue("Shuff", TypeReference.CreateTypeReference(engine, typeof(CardGameLibrary.Shuff)));


            var sevens = File.ReadAllText("js/sevens.js");
            var c = executeES6(engine, sevens);
            c = c.Execute("var cg=new CardGame()");

            c.GetValue("cg").AsObject().Get("__init__").Invoke(c.GetValue("cg"), new JsValue[] { 12 });

            c = c.Execute("var _=new GameUtils()");
            c = c.Execute("var shuff=new Shuff()");
            c = c.Execute("var sevens=new Sevens()");

            c = c.Execute("sevens.constructor(cg);");

            c = c.Execute("sevens.runGame(cg);");

            //            });
            //            thread.Start();


            /*mcc = mcc.Execute("var f=new foo()");
            var fva = mcc.GetValue("f");
            var cd = fva.AsObject().Get("bar").Invoke(200);
            var nn = cd.AsNumber();*/


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
