using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using WampFramework.API;

namespace LocalAssemble
{
    [WampClassAttribute(true)]
    public class ClassTest
    {
        int i = 0;
        private System.Timers.Timer _monitorTimer = null;

        private void sendWSTimerElapsed(object source, ElapsedEventArgs e)
        {
            I++;

            if (ValuedChanged != null)
            {
                EventArgs es = new EventArgs();

                //ValuedChanged(this, I);
                object[] ret = { I };
                ValuedChanged("ValuedChanged", ret);
            }
        }

        public ClassTest()
        {
            _monitorTimer = new System.Timers.Timer(10 * 1000);
            _monitorTimer.Elapsed += sendWSTimerElapsed;
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
    }

    public class WampExpoterTest : IWampLocalExporter
    {
    }
}
