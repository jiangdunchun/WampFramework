using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WampFramework.Common;
using WampFramework.Interfaces;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WampFramework.Router
{
    public class WampSharpHost : WampHost
    {
        public WampSharpHost(string port)
        {
            WampSharpClient.ClientConnected = (socket) =>
            {
                if (ClientConnected != null)
                {
                    ClientConnected(socket);
                }
            };
            WampSharpClient.ClientBroken = (socket) =>
            {
                if (ClientBroken != null)
                {
                    ClientBroken(socket);
                }
            };
            WampSharpClient.MessageReceived = (socket, msg) =>
            {
                if (MessageReceived != null)
                {
                    MessageReceived(socket, msg);
                }
            };

            _server = new WebSocketServer(port);

            _location = string.Format("ws://0.0.0.0:{0}", port);
        }

        private WebSocketServer _server = null;

        internal override void Open()
        {
            if (_server == null)
            {
                _server = new WebSocketServer(_location);
                _server.Start();
                if (_server.IsListening)
                {
                    // is listening
                }
            }
        }
        internal override void Stop()
        {
            if (_server != null)
            {
                _server.Stop();
                _server = null;
            }
        }
        internal override void SendString(IWampClient client, string message)
        {
            
        }
        internal override void SendBytes(IWampClient client, byte[] message)
        {
            
        }
    }

    class WampSharpClient : WebSocketBehavior, IWampClient
    {
        protected override void OnOpen()
        {
            if (ClientConnected != null)
            {
                ClientConnected(this);
            }
        }
        protected override void OnClose(CloseEventArgs e)
        {
            if (ClientBroken != null)
            {
                ClientBroken(this);
            }
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            if (MessageReceived != null)
            {
                object ret = null;
                if (e.IsText)
                {
                    ret = e.Data;
                }
                else if (e.IsBinary)
                {
                    ret = e.RawData;
                }

                MessageReceived(this, ret);
            }
        }

        static public Action<WampSharpClient> ClientConnected;
        static public Action<WampSharpClient> ClientBroken;
        static public Action<WampSharpClient, object> MessageReceived;

        void IWampClient.Send(string message)
        {
            base.Send(message);
        }
        void IWampClient.Send(byte[] message)
        {
            base.Send(message);
        }
    }
}
