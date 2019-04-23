using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using WampFramework.API;
using WampFramework.Common;
using WampFramework.Local;

namespace WampFramework.Router
{
    /// <summary>
    /// wamp router context
    /// </summary>
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

    /// <summary>
    /// wamp router
    /// </summary>
    public class WampRouter
    {
        struct Command
        {
            public WampClient Socket;
            public WampMessage Message;
        }

        /// <summary>
        /// readonly instance of this class
        /// </summary>
        public static readonly WampRouter Instance = new WampRouter();

        private WampRouter() { }

        private WampHost _host = null;
        private Dictionary<string, WampClassAPI> _apiPool = new Dictionary<string, WampClassAPI>();
        private List<Command> _commandQueue = new List<Command>();

        private async void _resolveMessage(WampClient socket, object message)
        {
            WampMessage rec_obj = new WampMessage();

            if (rec_obj.Construct(message))
            {
                // log, when a right message received
                if (WampProperties.Logger != null && (WampProperties.LoggerOptions & LogOption.RECEIVED) > 0)
                {
                    string log = string.Format("Receive a message from {0}, is {1}", 
                        socket.ToString(), rec_obj.ToString());
                    WampProperties.Logger.Log(log);
                }

                switch (rec_obj.Protocol)
                {
                    case WampProtocolHead.CAL:
                        // if not thread security, the message would be push to a queue, 
                        // and excuted when the MessageHandler() was invoked
                        if (!WampProperties.IsThreadSecurity)
                        {
                            lock (_commandQueue)
                            {
                                _commandQueue.Add(new Command() { Socket = socket, Message = rec_obj });
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
                        WampBroker.Instance.Subscribe(socket, rec_obj);
                        break;
                    case WampProtocolHead.UNSBS:
                        WampBroker.Instance.Unsubscribe(socket, rec_obj);
                        break;
                    //@TODO: registe remote publisher and callee
                    //case WampProtocolHead.REG:
                    //    break;
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
                    string log = string.Format("Receive a wrong message from {0}, is {1}", 
                        socket.ToString(), rec_obj.ToString());
                    WampProperties.Logger.Log(log);
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
                string log = string.Format("Break up a socket from {0}", socket.ToString());
                WampProperties.Logger.Log(log);
            }
        }
        private void _socketConnected(WampClient socket)
        {
            // log, when a sockect connected
            if (WampProperties.Logger != null)
            {
                string log = string.Format("Connect in a socket from {0}", socket.ToString());
                WampProperties.Logger.Log(log);
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
        /// regist all APIs in the assembly of this class
        /// </summary>
        /// <param name="exporter">interface of exporter</param>
        /// <returns>success or not</returns>
        public bool Regist(IWampLocalExporter exporter)
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
                string log = string.Format("Registered a local assemble {0}", type.Assembly.FullName);
                WampProperties.Logger.Log(log);
            }

            return true;
        }
        /// <summary>
        /// export all APIs to js files
        /// </summary>
        /// <param name="path">js files' path</param>
        /// <returns>success or not</returns>
        public bool ExportTo(string path)
        {
            WampAPIExporter writer = new WampAPIExporter(path);

            writer.Add(WampDealer.Instance.Export());
            writer.Add(WampBroker.Instance.Export());

            writer.Export();

            return true;
        }
        /// <summary>
        /// set logger and log options
        /// </summary>
        /// <param name="logger">Logger of wamp framework</param>
        /// <param name="options">log options</param>
        public void SetLogger(IWampLogger logger, LogOption options = LogOption.NONE)
        {
            WampProperties.Logger = logger;
            WampProperties.LoggerOptions = options;
        }
        /// <summary>
        /// if not in thread security mode, all RPC commands will be resolved when this method was invoked
        /// </summary>
        public void ResolveCommand()
        {
            List<Command> _commandBuffer = new List<Command>();
            lock (_commandQueue)
            {
                _commandBuffer.AddRange(_commandQueue);
                _commandQueue.Clear();
            }

            foreach (Command command in _commandBuffer)
            {
                WampDealer.Instance.Call(command.Socket, command.Message);
            }
        }
    }
}
