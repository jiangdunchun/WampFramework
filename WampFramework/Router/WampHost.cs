using System;
using System.IO;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace WampFramework.Router
{
    /// <summary>
    /// websocket host based on WebSocketSharp
    /// </summary>
    public class WampHost
    {
        /// <summary>
        /// constructor of WampHost
        /// </summary>
        /// <param name="port">port of this host</param>
        /// <param name="router">router of this host</param>
        public WampHost(int port, string router)
        {
            WampClient.ClientConnected = (socket) =>
            {
                ClientConnected?.Invoke(socket);
                UserConnected?.Invoke(new WampUser(socket));
            };
            WampClient.ClientBroken = (socket) =>
            {
                ClientBroken?.Invoke(socket);
                UserBroken?.Invoke(new WampUser(socket));
            };
            WampClient.MessageReceived = (socket, msg) =>
            {
                MessageReceived?.Invoke(socket, msg);
            };

            _server = new WebSocketServer(port);
            _server.AddWebSocketService<WampClient> (string.Format("/{0}", router));

            Port = port;
            Router = router;
            IsOpen = false;
        }

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
        public int Port { protected set; get; }
        /// <summary>
        /// router of this wamp host, no "/"
        /// </summary>
        public string Router { protected set; get; }
        /// <summary>
        /// is this wamp host opening
        /// </summary>
        public bool IsOpen { protected set; get; }

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
                    IsOpen = true;
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
                IsOpen = false;
                _server = null;
            }
        }
    }

    /// <summary>
    /// websocket client based on WebSocketSharp
    /// </summary>
    public class WampUser
    {
        internal WampUser(WampClient client)
        {
            _client = client;
        }

        private WampClient _client;

        /// <summary>
        /// IP of the Wamp User
        /// </summary>
        public string IP { get { return _client.IP; } }
        /// <summary>
        /// Port of the Wamp User
        /// </summary>
        public int Port { get { return _client.Port; } }
    }

    class WampClient : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            IP = base.Context.UserEndPoint.Address.ToString();
            Port = base.Context.UserEndPoint.Port;

            ClientConnected?.Invoke(this);
        }
        protected override void OnClose(CloseEventArgs e)
        {
            ClientBroken?.Invoke(this);
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

        public string IP { protected set; get; }
        public int Port { protected set; get; }

        public new void Send(string message)
        {
            base.Send(message);
        }
        public new void Send(byte[] message)
        {
            base.Send(message);
        }
        public new void Send(FileInfo file)
        {
            base.Send(file);
        }
        public new void SendAsync(string message, Action<bool> complete)
        {
            base.SendAsync(message, complete);
        }
        public new void SendAsync(byte[] message, Action<bool> complete)
        {
            base.SendAsync(message, complete);
        }
        public new void SendAsync(FileInfo file, Action<bool> complete)
        {
            base.SendAsync(file, complete);
        }
        public override string ToString()
        {
            return string.Format("{0}:{1}", IP, Port);
        }
    }
}
