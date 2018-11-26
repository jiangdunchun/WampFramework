using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WampFramework.API;
using WampFramework.Common;
using WampFramework.Local;

namespace WampFramework.Router
{
    public class WampRouter
    {
        private WampHost _host = null;
        private Dictionary<string, WampClassAPI> _apiPool = new Dictionary<string, WampClassAPI>();

        private async void _resolveMessage(IWebSocketConnection socket, object message)
        {
            WampMessage rec_obj = new WampMessage();

            if (rec_obj.Construct(message))
            {
                // log, when a right message received
                if (WampSetting.Logger != null)
                {
                    string log_str = string.Format("Receive a message from {0}:{1}, is {2}", 
                        socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort, rec_obj.ToString());
                    WampSetting.Logger.Log(log_str);
                }

                switch (rec_obj.Protocol)
                {
                    case WampProtocolHead.CAL:
                        if (!WampSetting.IsAsyncMode)
                        {
                            WampDealer.Instance.Call(socket, rec_obj);
                        }
                        else
                        {
                            await WampDealer.Instance.CallAsync(socket, rec_obj);
                        }
                        break;
                    case WampProtocolHead.SBS:
                        if (!WampSetting.IsAsyncMode)
                        {
                            WampBroker.Instance.Subscribe(socket, rec_obj);
                        }
                        else
                        {
                            await WampBroker.Instance.SubscribeAsync(socket, rec_obj);
                        }
                        break;
                    case WampProtocolHead.UNSBS:
                        if (!WampSetting.IsAsyncMode)
                        {
                            WampBroker.Instance.Unsubscribe(socket, rec_obj);
                        }
                        else
                        {
                            await WampBroker.Instance.UnsubscribeAsync(socket, rec_obj);
                        }
                        break;
                    default:
                        rec_obj.SendErro(socket, message);
                        break;
                }
            }
            else
            {
                // log, when a wrong message received
                if (WampSetting.Logger != null)
                {
                    string log_str = string.Format("Receive a wrong message from {0}:{1}, is {2}", 
                        socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort, rec_obj.ToString());
                    WampSetting.Logger.Log(log_str);
                }

                rec_obj.SendErro(socket, message);
            }
        }
        private void _socketBroken(IWebSocketConnection socket)
        {
            WampBroker.Instance.RemoveSocket(socket);
            WampDealer.Instance.RemoveSocket(socket);

            // log, when a sockect broken
            if (WampSetting.Logger != null)
            {
                string log_str = string.Format("Break up a socket from {0}:{1}", socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort);
                WampSetting.Logger.Log(log_str);
            }
        }
        private void _socketConnected(IWebSocketConnection socket)
        {
            // log, when a sockect connected
            if (WampSetting.Logger != null)
            {
                string log_str = string.Format("Connect in a new socket from {0}:{1}", socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort);
                WampSetting.Logger.Log(log_str);
            }
        }
        private void _registerLocalType(Type type)
        {
            object[] objs = type.GetCustomAttributes(typeof(WampClassAttribute), true);

            foreach (object obj in objs)
            {
                WampClassAttribute attr = obj as WampClassAttribute;
                if (attr != null && attr.Export)
                {
                    WampLocalCalleePublisher localAssembly = new WampLocalCalleePublisher(type, type.Name); ;

                    WampDealer.Instance.CalleeDic.Add(type.Name, localAssembly);
                    WampBroker.Instance.PublisherDic.Add(type.Name, localAssembly);

                    break;
                }
            }
        }

        internal WampHost Host
        {
            set
            {
                if (_host != null)
                {
                    _host.MessageReceived -= _resolveMessage;
                    _host.SocketBroken -= _socketBroken;
                    _host.SocketConnected -= _socketConnected;
                    _host.Dispose();
                }

                _host = value;
                _host.MessageReceived += _resolveMessage;
                _host.SocketBroken += _socketBroken;
                _host.SocketConnected += _socketConnected;
            }
        }
        public bool IsByteMode
        {
            set
            {
                WampSetting.IsByteMode = value;
            }
            get
            {
                return WampSetting.IsByteMode;
            }
        }
        public bool IsAsyncMode
        {
            set
            {
                WampSetting.IsAsyncMode = value;
            }
            get
            {
                return WampSetting.IsAsyncMode;
            }
        }
        public IWampLogger Logger
        {
            set
            {
                WampSetting.Logger = value;
            }
        }
        public static readonly WampRouter Instance = new WampRouter();

        public bool Register(IWampLocalExporter exporter)
        {
            Type type = exporter.GetType();
            if (type.Assembly != Assembly.GetExecutingAssembly())
            {
                Type[] types = type.Assembly.GetTypes();
                foreach (Type t in types)
                {
                    _registerLocalType(t);
                }
            }

            // log, when a local assemble was registered
            if (WampSetting.Logger != null)
            {
                string log_str = string.Format("Registered a local assemble {0}", type.Name);
                WampSetting.Logger.Log(log_str);
            }

            return true;
        }
        public bool Export(string path)
        {
            WampAPIWriter writer = new WampAPIWriter(path);

            writer.Add(WampDealer.Instance.Export());
            writer.Add(WampBroker.Instance.Export());

            writer.Write();

            return true;
        }
    }
}
