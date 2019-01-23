using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WampFramework.API;
using WampFramework.Common;
using WampFramework.Interfaces;
using WampFramework.Local;

namespace WampFramework.Router
{
    public class WampRouterContext
    {
        internal WampRouterContext() { }

        /// <summary>
        /// whether the message is in Byte[] Mode or string Mode
        /// </summary>
        public bool IsByteMode
        {
            set
            {
                WampProperties.IsByteMode = value;
            }
            get
            {
                return WampProperties.IsByteMode;
            }
        }
        /// <summary>
        /// whether the message is little endian or big endian in Byte[] Mode
        /// </summary>
        public bool IsLittleEndian
        {
            set
            {
                WampProperties.IsLittleEndian = value;
            }
            get
            {
                return WampProperties.IsLittleEndian;
            }
        }
        /// <summary>
        /// whether APIs could be invoked in a different thread
        /// </summary>
        public bool IsThreadSecurity
        {
            set
            {
                WampProperties.IsThreadSecurity = value;
            }
            get
            {
                return WampProperties.IsThreadSecurity;
            }
        }
        /// <summary>
        /// whether the RPC command is invoked asynchronously
        /// </summary>
        public bool IsAsyncMode
        {
            set
            {
                WampProperties.IsAsyncMode = value;
            }
            get
            {
                return WampProperties.IsAsyncMode;
            }
        }
        /// <summary>
        /// the decoding and encoding mode
        /// </summary>
        public Encoding StrEncoding
        {
            get
            {
                return WampProperties.StrEncoding;
            }
            set
            {
                WampProperties.StrEncoding = value;
            }
        }
    }

    public class WampRouter
    {
        struct MessageItem
        {
            public WampClient Socket;
            public WampMessage Message;
        }

        public static readonly WampRouter Instance = new WampRouter();

        private WampHost _host = null;
        private Dictionary<string, WampClassAPI> _apiPool = new Dictionary<string, WampClassAPI>();
        private List<MessageItem> _messageQueue = new List<MessageItem>();

        private async void _resolveMessage(WampClient socket, object message)
        {
            WampMessage rec_obj = new WampMessage();

            if (rec_obj.Construct(message))
            {
                // log, when a right message received
                if (WampProperties.Logger != null && WampProperties.LogReceived)
                {
                    string log_str = string.Format("Receive a message from {0}, is {1}", 
                        socket.ToString(), rec_obj.ToString());
                    WampProperties.Logger.Log(log_str);
                }

                switch (rec_obj.Protocol)
                {
                    case WampProtocolHead.CAL:
                        // if not thread security, the message would be push to a queue, 
                        // and excuted when the MessageHandler() was invoked
                        if (!WampProperties.IsThreadSecurity)
                        {
                            lock (_messageQueue)
                            {
                                _messageQueue.Add(new MessageItem() { Socket = socket, Message = rec_obj });
                            }
                        }
                        else
                        {
                            if (!WampProperties.IsAsyncMode)
                            {
                                WampDealer.Instance.Call(socket, rec_obj);
                            }
                            else
                            {
                                await WampDealer.Instance.CallAsync(socket, rec_obj);
                            }
                        }
                        break;
                    case WampProtocolHead.SBS:
                        if (!WampProperties.IsAsyncMode)
                        {
                            WampBroker.Instance.Subscribe(socket, rec_obj);
                        }
                        else
                        {
                            await WampBroker.Instance.SubscribeAsync(socket, rec_obj);
                        }
                        break;
                    case WampProtocolHead.UNSBS:
                        if (!WampProperties.IsAsyncMode)
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
                if (WampProperties.Logger != null)
                {
                    string log_str = string.Format("Receive a wrong message from {0}, is {1}", 
                        socket.ToString(), rec_obj.ToString());
                    WampProperties.Logger.Log(log_str);
                }

                rec_obj.SendErro(socket, message);
            }
        }
        private void _socketBroken(WampClient socket)
        {
            WampBroker.Instance.RemoveSocket(socket);
            WampDealer.Instance.RemoveSocket(socket);

            // log, when a sockect broken
            if (WampProperties.Logger != null)
            {
                string log_str = string.Format("Break up a socket from {0}", socket.ToString());
                WampProperties.Logger.Log(log_str);
            }
        }
        private void _socketConnected(WampClient socket)
        {
            // log, when a sockect connected
            if (WampProperties.Logger != null)
            {
                string log_str = string.Format("Connect in a new socket from {0}", socket.ToString());
                WampProperties.Logger.Log(log_str);
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

        /// <summary>
        /// websocket host
        /// </summary>
        public WampHost Host
        {
            set
            {
                if (_host != null)
                {
                    _host.MessageReceived -= _resolveMessage;
                    _host.ClientBroken -= _socketBroken;
                    _host.ClientConnected -= _socketConnected;
                }

                _host = value;
                _host.MessageReceived += _resolveMessage;
                _host.ClientBroken += _socketBroken;
                _host.ClientConnected += _socketConnected;
            }
            get
            {
                return _host;
            }
        }
        /// <summary>
        /// context of this router
        /// </summary>
        public WampRouterContext Context = new WampRouterContext();

        /// <summary>
        /// register all APIs in the assembly of this class
        /// </summary>
        /// <param name="exporter">interface of exporter</param>
        /// <returns>success or not</returns>
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
            if (WampProperties.Logger != null)
            {
                string log_str = string.Format("Registered a local assemble {0}", type.Name);
                WampProperties.Logger.Log(log_str);
            }

            return true;
        }
        /// <summary>
        /// export all APIs to js files
        /// </summary>
        /// <param name="path">js files' path</param>
        /// <returns>success or not</returns>
        public bool Export(string path)
        {
            WampAPIWriter writer = new WampAPIWriter(path);

            writer.Add(WampDealer.Instance.Export());
            writer.Add(WampBroker.Instance.Export());

            writer.Write();

            return true;
        }
        /// <summary>
        /// set logger and log standard
        /// </summary>
        /// <param name="logger">Logger of wamp framework</param>
        /// <param name="standard">log standard</param>
        public void SetLogger(IWampLogger logger, LogStandard standard = LogStandard.ALL)
        {
            WampProperties.Logger = logger;
            switch (standard)
            {
                case LogStandard.RECEIVED_ONLY:
                    WampProperties.LogReceived = true;
                    WampProperties.LogSend = false;
                    break;
                case LogStandard.SEND_ONLY:
                    WampProperties.LogReceived = false;
                    WampProperties.LogSend = true;
                    break;
                case LogStandard.ALL:
                    WampProperties.LogReceived = true;
                    WampProperties.LogSend = true;
                    break;
            }
        }
        /// <summary>
        /// if not in thread security mode, all RPC commands will be handled when this method was invoked
        /// </summary>
        public void MessageHandler()
        {
            List<MessageItem> _messageBuffer = new List<MessageItem>();
            lock (_messageQueue)
            {
                _messageBuffer.AddRange(_messageQueue);
                _messageQueue.Clear();
            }

            foreach (MessageItem message in _messageBuffer)
            {
                WampDealer.Instance.Call(message.Socket, message.Message);
            }
        }
    }
}
