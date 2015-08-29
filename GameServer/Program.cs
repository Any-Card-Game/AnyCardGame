using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint.Native;

namespace GameServer
{
    class Program
    {
        static void Main(string[] args)
        {

            Jint.Engine engine = new Jint.Engine();
            var typescript = File.ReadAllText("js/typescript/typescript.js");
            Console.WriteLine("Read.");
            var mc = engine.Execute(typescript);
            Console.WriteLine("Parse.");
            var tsc = mc.GetValue("ts");
            var cc = tsc.AsObject().Get("transpile");
            var j = @"class foo{bar(c){let j=15+c;return j+2;}}";
            var c = cc.Invoke(j).AsString();


            var mcc = engine.Execute(c);
            mcc=mcc.Execute("var f=new foo()");
            var fva = mcc.GetValue("f");
            var cd = fva.AsObject().Get("bar").Invoke(200);
            var nn = cd.AsNumber();

            Console.WriteLine("Trans.");
            Console.WriteLine(nn);


            Console.WriteLine("Press any [Enter] to close the host.");
            Console.ReadLine();
        }
    }
}
