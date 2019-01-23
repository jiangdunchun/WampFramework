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
    public class WampHost
    {
        public WampHost(int port, string router)
        {
            WampClient.ClientConnected = (socket) =>
            {
                if (ClientConnected != null)
                {
                    ClientConnected(socket);
                }
                if (UserConnected != null)
                {
                    UserConnected(new WampUser(socket));
                }
            };
            WampClient.ClientBroken = (socket) =>
            {
                if (ClientBroken != null)
                {
                    ClientBroken(socket);
                }
                if (UserBroken != null)
                {
                    UserBroken(new WampUser(socket));
                }
            };
            WampClient.MessageReceived = (socket, msg) =>
            {
                if (MessageReceived != null)
                {
                    MessageReceived(socket, msg);
                }
            };

            _server = new WebSocketServer(port);
            _server.AddWebSocketService<WampClient> (string.Format("/{0}", router));

            _port = port;
            _router = router;
        }

        private int _port = -1;
        private string _router = string.Empty;
        private bool _isOpened = false;
        private WebSocketServer _server = null;

        internal Action<WampClient> ClientConnected;
        internal Action<WampClient> ClientBroken;
        internal Action<WampClient, object> MessageReceived;

        /// <summary>
        /// invoked when a client connected
        /// </summary>
        public Action<WampUser> UserConnected;
        /// <summary>
        /// invoked when a client broken
        /// </summary>
        public Action<WampUser> UserBroken;

        /// <summary>
        /// Port of this wamp host
        /// </summary>
        public int Port { get { return _port; } }
        /// <summary>
        /// router of this wamp host, no "/"
        /// </summary>
        public string Router { get { return _router; } }
        /// <summary>
        /// is this wamp host opening
        /// </summary>
        public bool IsOpen { get { return _isOpened; } }

        /// <summary>
        /// start this wamp host, and listen to the client
        /// </summary>
        public void Start()
        {
            if (_server != null)
            {
                _server.Start();
                if (_server.IsListening)
                {
                    // is listening
                    _isOpened = true;
                }
            }
        }
        /// <summary>
        /// close this wamp host
        /// </summary>
        public void Close()
        {
            if (_server != null)
            {
                _server.Stop();
                _isOpened = false;
                _server = null;
            }
        }
    }

    public class WampUser
    {
        internal WampUser(WampClient client)
        {
            _client = client;
        }

        private WampClient _client;

        public string IP { get { return _client.IP; } }
        public int Port { get { return _client.Port; } }
    }

    class WampClient : WebSocketBehavior
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

        static public Action<WampClient> ClientConnected;
        static public Action<WampClient> ClientBroken;
        static public Action<WampClient, object> MessageReceived;

        public string IP { get { return base.Context.UserEndPoint.Address.ToString(); } }
        public int Port { get { return base.Context.UserEndPoint.Port; } }

        public new void Send(string message)
        {
            base.Send(message);
        }
        public new void Send(byte[] message)
        {
            base.Send(message);
        }
        public override string ToString()
        {
            return string.Format("{0}:{1}", IP, Port);
        }
    }
}
