using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jint;
using Jint.Native;
using Jint.Native.Function;
using Jint.Native.Object;
using Jint.Native.String;
using Jint.Runtime;
using Jint.Runtime.Descriptors;
using Jint.Runtime.Interop;

namespace GameServer
{
    public class Pile
    {
        public string Name { get; set; }
        public Pile(string name)
        {
            Name = name;
        }
    }


    public sealed class SalConstructor : FunctionInstance, IConstructor
    {
        private readonly Engine _engine;
        public SalPrototype PrototypeObject { get; private set; }

        private SalConstructor(Engine engine) : base(engine, null, null, false)
        {
            _engine = engine;
        }

        public static SalConstructor CreateObjectConstructor(Engine engine)
        {
            SalConstructor objectConstructor = new SalConstructor(engine);
            objectConstructor.Extensible = true;
            objectConstructor.PrototypeObject = SalPrototype.CreatePrototypeObject(engine, objectConstructor);
            objectConstructor.FastAddProperty("length", (JsValue)1.0, false, false, false);
            objectConstructor.FastAddProperty("prototype", (JsValue)((ObjectInstance)objectConstructor.PrototypeObject), false, false, false);
            return objectConstructor;
        }

        public void Configure()
        {
            Prototype = Engine.Function.PrototypeObject;

            Prototype.FastAddProperty("shoes", new ClrFunctionInstance(Engine, GetPrototypeOf, 1), true, false, true);
        }

        public JsValue GetPrototypeOf(JsValue thisObject, JsValue[] arguments)
        {
            throw new NotImplementedException();
        }

        public override JsValue Call(JsValue thisObject, JsValue[] arguments)
        {
            throw new NotImplementedException();
        }

        public ObjectInstance Construct(JsValue[] arguments)
        {
            var name = arguments[0].AsString();
            return (ObjectInstance) this.PrototypeObject;

        }
    }
    public sealed class SalPrototype : ObjectInstance
    {
        private SalPrototype(Engine engine)
          : base(engine)
        {
        }

        public static SalPrototype CreatePrototypeObject(Engine engine, SalConstructor objectConstructor)
        {
            SalPrototype objectPrototype1 = new SalPrototype(engine);
            objectPrototype1.Extensible = true;
            SalPrototype objectPrototype2 = objectPrototype1;
            objectPrototype2.FastAddProperty("constructor", (JsValue)((ObjectInstance)objectConstructor), true, false, true);
            objectPrototype2.FastAddProperty("shoes", (JsValue)((ObjectInstance)new ClrFunctionInstance(engine, new Func<JsValue, JsValue[], JsValue>(getThing))), true, false, true);
            return objectPrototype2;
        }

        private static JsValue getThing(JsValue arg1, JsValue[] arg2)
        {
            return arg2[0].AsNumber()*12;
        }

        public void Configure()
        {
        }

    }




    class Program
    {
        static Jint.Engine engine = new Jint.Engine();
        static void Main(string[] args)
        {
            engine.SetValue("log", new Action<object>(a =>
            {
                
            })); 
            //todo make a generic way to add new classes and shit
            engine.SetValue("Pile", SalConstructor.CreateObjectConstructor(engine));
//            engine = engine.Execute("var j=new Pile('spades');log(j.doThing(15));");
//            var cdc = engine.GetValue("j");



            var sevens = File.ReadAllText("js/sevens.js");
            var c = executeES6(sevens);



            var mcc = c.Execute("var sevens=new Sevens()");
            //do stuff to it
            mcc = mcc.Execute("sevens.constructor();");


            /*mcc = mcc.Execute("var f=new foo()");
            var fva = mcc.GetValue("f");
            var cd = fva.AsObject().Get("bar").Invoke(200);
            var nn = cd.AsNumber();*/


            Console.WriteLine("Press any [Enter] to close the host.");
            Console.ReadLine();
        }

        private static Engine executeES6(string text)
        {
            var typescript = File.ReadAllText("js/typescript/typescript.js");
            var mc = engine.Execute(typescript);
            var tsc = mc.GetValue("ts");
            var cc = tsc.AsObject().Get("transpile");

            var c = cc.Invoke(text).AsString();

            var mcc = engine.Execute(c);
            return mcc;
        }
    }
}
