using LocalAssemble;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WampFramework.API;
using WampFramework.Router;

namespace WampConsoleTest
{
    class ConsoleLogger:IWampLogger
    {
        public void Log(string message)
        {
            Console.WriteLine(string.Format("{0} {1}", DateTime.Now.ToString("hh:mm:ss"), message));
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            WampExpoterTest test = new WampExpoterTest();

            WampRouter.Instance.Register(test);

            //WampRouter.Instance.Context.IsLittleEndian = true;
            WampRouter.Instance.Export("C:/Users/JDC/Desktop");
            WampRouter.Instance.SetLogger(new ConsoleLogger());

            //WampRouter.Instance.ClientConnected = (msg) => { Console.WriteLine("********{0}", msg); };
            //WampRouter.Instance.ClientBroken = (msg) => { Console.WriteLine("********{0}", msg); };

            WampHost host = new WampHost(2223, "wamp");
            host.Start();
            WampRouter.Instance.Host = host;

            Console.ReadLine();
        }
    }
}
