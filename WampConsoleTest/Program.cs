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

            //WampRouter.Instance.IsAsyncMode = false;
            //WampRouter.Instance.IsByteMode = true;
            //WampRouter.Instance.Export("C:/Users/JDC/Desktop");
            //WampRouter.Instance.Logger = new ConsoleLogger();

            WampHost host = new WampHost("2222");
            host.Open();

            Console.ReadLine();
        }
    }
}
