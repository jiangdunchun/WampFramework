using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fleck;
using WampFramework.Common;
using WampFramework.API;

namespace WampFramework.Router
{
    public class WampHost : IDisposable
    {
        private WebSocketServer _server = null;

        internal delegate void SocketEvent(IWebSocketConnection socket);
        internal delegate void MessageEvent(IWebSocketConnection socket, object message);
        internal event SocketEvent SocketConnected = null;
        internal event SocketEvent SocketBroken = null;
        internal event MessageEvent MessageReceived = null;

        public string Location { get { return WampSetting.Location; } }
        public bool IsOpen { get { return WampSetting.IsOpened; } }

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

        public WampHost(string port)
        {
            WampSetting.Location = string.Format("ws://0.0.0.0:{0}", port);

            WampRouter.Instance.Host = this;
        }
        ~WampHost()
        {
            Dispose(false);
        }

        public void Open()
        {
            Close();

            string log_str = string.Empty;

            _server = new WebSocketServer(WampSetting.Location);
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

                WampSetting.IsOpened = true;

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
            if (WampSetting.Logger != null)
            {
                WampSetting.Logger.Log(log_str);
            }
        }
        public void Close()
        {
            if (_server != null)
            {
                _server.Dispose();

                // log, when the server closed
                if (WampSetting.Logger != null)
                {
                    string log_str = string.Format("Close wamp server in {0}", Location);
                    WampSetting.Logger.Log(log_str);
                }
            }
            WampSetting.IsOpened = false;
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
