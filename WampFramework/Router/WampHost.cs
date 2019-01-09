using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using WampFramework.Common;
using WampFramework.API;
using WampFramework.Interfaces;

namespace WampFramework.Router
{
    public class WampFleckHost : IDisposable
    {
        public WampFleckHost(string port)
        {
            WampProperties.Location = string.Format("ws://0.0.0.0:{0}", port);

            WampRouter.Instance.Host = this;
        }
        ~WampFleckHost()
        {
            Dispose(false);
        }

        private WebSocketServer _server = null;

        internal delegate void SocketEvent(IWebSocketConnection socket);
        internal delegate void MessageEvent(IWebSocketConnection socket, object message);
        internal event SocketEvent SocketConnected = null;
        internal event SocketEvent SocketBroken = null;
        internal event MessageEvent MessageReceived = null;

        /// <summary>
        /// the server's location
        /// </summary>
        public string Location { get { return WampProperties.Location; } }
        /// <summary>
        /// whether the server is openning
        /// </summary>
        public bool IsOpen { get { return WampProperties.IsOpened; } }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_server != null)
                {
                    _server.Dispose();
                    _server = null;
                }
            }
        }

        /// <summary>
        /// Open a server host
        /// </summary>
        public void Open()
        {
            Close();

            string log_str = string.Empty;

            _server = new WebSocketServer(WampProperties.Location);
            try
            {
                _server.Start(socket =>
                {
                    socket.OnOpen = () =>
                    {
                        if (SocketConnected != null) SocketConnected(socket);
                    };
                    socket.OnClose = () =>
                    {
                        if (SocketBroken != null) SocketBroken(socket);
                    };
                    socket.OnMessage = (message) =>
                    {
                        if (MessageReceived != null) MessageReceived(socket, message);
                    };
                    socket.OnBinary = (message) =>
                    {
                        if (MessageReceived != null) MessageReceived(socket, message);
                    };
                });

                WampProperties.IsOpened = true;

                log_str = string.Format("Start wamp server successfully in {0}", Location);
            }
            catch (Exception)
            {
                string e_str = string.Format("Start wamp server unsuccessfully in {0}, check the location", Location);

                //log_str = e_str;

                WampHostException wh_e = new WampHostException(e_str);
                throw wh_e;
            }

            // log, when the server started
            if (WampProperties.Logger != null)
            {
                WampProperties.Logger.Log(log_str);
            }
        }
        /// <summary>
        /// close a server host
        /// </summary>
        public void Close()
        {
            if (_server != null)
            {
                _server.Dispose();

                // log, when the server closed
                if (WampProperties.Logger != null)
                {
                    string log_str = string.Format("Close wamp server in {0}", Location);
                    WampProperties.Logger.Log(log_str);
                }
            }
            WampProperties.IsOpened = false;
        }
        /// <summary>
        /// dipose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
