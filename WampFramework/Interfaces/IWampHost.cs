using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WampFramework.Common;
using WampFramework.Router;

namespace WampFramework.Interfaces
{
    public abstract class WampHost
    {
        protected string _location
        {
            set
            {
                WampProperties.Location = value;
            }
            get
            {
                return WampProperties.Location;
            }
        }
        protected bool _isOpened
        {
            set
            {
                WampProperties.IsOpened = value;
            }
            get
            {
                return WampProperties.IsOpened;
            }
        }

        internal Action<IWampClient> ClientConnected;
        internal Action<IWampClient> ClientBroken;
        internal Action<IWampClient, object> MessageReceived;

        internal string Location { get { return _location; } }
        internal bool IsOpen { get { return _isOpened; } }

        internal abstract void Open();
        internal abstract void Stop();
        internal abstract void SendString(IWampClient client, string message);
        internal abstract void SendBytes(IWampClient client, byte[] message);
    }

    interface IWampClient
    {
        void Send(string message);
        void Send(byte[] message);
    }
}
