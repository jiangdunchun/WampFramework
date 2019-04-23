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
            WampRouter.Instance.SetLogger(new ConsoleLogger());

            WampExpoterTest test = new WampExpoterTest();

            WampRouter.Instance.Regist(test);
            WampRouter.Instance.ExportTo("C:/Users/JDC/Desktop");
            WampRouter.Instance.Context.IsByteMode = false;

            WampHost host = new WampHost(2223, "wamp");
            host.Start();
            WampRouter.Instance.Host = host;

            Console.ReadLine();
        }
    }
}
