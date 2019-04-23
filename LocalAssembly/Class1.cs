using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using WampFramework.API;

namespace LocalAssemble
{
    [WampClass(true)]
    public class ClassTest
    {
        int i = 0;
        private System.Timers.Timer _monitorTimer = null;

        private void _sendWSTimerElapsed(object source, ElapsedEventArgs e)
        {
            I++;

            if (ValuedChanged != null)
            {
                object[] ret = { I };
                ValuedChanged("ValuedChanged", ret);
            }
        }

        public ClassTest()
        {
            _monitorTimer = new System.Timers.Timer(10 * 1000);
            _monitorTimer.Elapsed += _sendWSTimerElapsed;
            _monitorTimer.AutoReset = true;
            _monitorTimer.Start();
        }
        public int I
        {
            get
            {
                return i;
            }
            set
            {
                i = value;
            }
        }

        [WampEventAttribute(true, typeof(int), "value")]
        public event WampEvent ValuedChanged = null;

        [WampMethodAttribute(true)]
        public void AddOne()
        {
            I++;
        }
        [WampMethodAttribute(true)]
        public void Add(int num)
        {
            I = I + num;
        }
        [WampMethodAttribute(true)]
        public int Get()
        {
            Thread.Sleep(20 * 1000);
            return I;
        }
        [WampMethodAttribute(true)]
        public void SetArray(int[] array)
        {
            //Thread.Sleep(20 * 1000);
            //return I;
        }


        //[WampMethod(true)]
        //public string GetStringE()
        //{
        //    return "english";
        //}

        //[WampMethod(true)]
        //public string GetStringC()
        //{
        //    return "中文";
        //}

        //[WampMethod(true)]
        //public string[] GetStringCE()
        //{
        //    List<string> ss = new List<string>();
        //    ss.Add("中文english");
        //    ss.Add("english中文");
        //    ss.Add("english");
        //    ss.Add("中文");
        //    return ss.ToArray();
        //}
    }

    public class WampExpoterTest : IWampLocalExporter
    {
    }
}
