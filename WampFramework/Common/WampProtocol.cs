using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WampFramework.API;

namespace WampFramework.Common
{
    enum WampProtocolHead:byte
    {
        CAL = 0,
        CAL_SUC = 8,
        CAL_FAL = 9,

        SBS = 10,
        SBS_BCK = 11,
        SBS_SUC = 18,
        SUB_FAL = 19,

        UNSBS = 20,
        UNSBS_SUC = 28,
        UNSBS_FAL = 29,

        REG = 30,
        REG_SUC = 38,
        REG_FAL = 39,

        ERROR = 101
    }

    enum WampArgType : byte
    {
        NULL = 0,
        STRING = 1,
        BYTES = 2,
        INT = 4,
        DOUBLE = 8
    }

    /* WampProtocol: byte
     * ID: UInt16
     * string message formate
     * received message: WampProtocol,ID     ,ClassName,MethodorEventName(,arg1           ,arg2           ,arg3                     )
     * 
     * send message:     WampProtocol,ID     ,ClassName,MethodorEventName(,arg1           ,arg2           ,arg3                     )
     * 
     * byte message foramte
     * received message: WampProtocol|ID     |ClassName,MethodorEventName(|arg1           |arg2           |arg3                     )
     * data type:        byte        |UInt16 |byte+string                (|byte+int       |byte+double    |byte+int+string         )
     * data description: WampProtocol|ID     |namelength+namebyte        (|argtype+argbyte|argtype+argbyte|argtype+arglength+argbyte)
     * byte length:      1           |2      |1+n                        (|1+4            |1+8            |1+4+n                    )
     * 
     * send message:     WampProtocol|ID     |ClassName,MethodorEventName(|arg1           |arg2           |arg3                     )
     * data type:        byte        |UInt16 |byte+string                (|byte+int       |byte+double    |byte+int+string         )
     * data description: WampProtocol|ID     |namelength+namebyte        (|argtype+argbyte|argtype+argbyte|argtype+arglength+argbyte)
     * byte length:      1           |2      |1+n                        (|1+4            |1+8            |1+4+n                    )
    */
    class WampMessage
    {
        private readonly byte MAX_NAME_LENGTH = 255;
        private readonly int MAX_ARG_LENGTH = 2147483647;
        private readonly char[] SPLIT_CHARS = { ',', '|' };

        private WampProtocolHead _protocol;
        private UInt16 _id;
        private string _entity;
        private string _name;
        private object[] _args;

        private void _sendString(IWebSocketConnection socket)
        {
            string ret_str = string.Empty;

            if (_args == null || _args.Length == 0)
            {
                ret_str = string.Format("{0},{1},{2},{3}", (byte)_protocol, _id, _entity, _name);
            }
            else
            {
                string args_str = string.Empty;
                foreach (object arg in _args)
                {
                    if (arg == null)
                    {
                        args_str += ",";
                    }
                    else
                    {
                        args_str += arg.ToString() + ",";
                    }
                }

                if (!string.IsNullOrEmpty(args_str))
                {
                    args_str = args_str.Substring(0, args_str.Length - 1);
                }

                ret_str = string.Format("{0},{1},{2},{3},{4}", (byte)_protocol, _id, _entity, _name, args_str);
            }

            socket.Send(ret_str);
        }
        private void _sendByte(IWebSocketConnection socket)
        {
            List<byte> ret_byte = new List<byte>();

            // protocol, length is 1
            ret_byte.Add((byte)_protocol);

            // id, length is 2
            byte[] id_bs;
            if (WampValueHelper.ParseBytes(_id, out id_bs))
            {
                ret_byte.AddRange(id_bs);
            }

            // entity and name, "_entity,_name", length is 1+n, 1 is n's length(less than 255)
            byte[] name_bs;
            if (WampValueHelper.ParseBytes(string.Format("{0},{1}", _entity, _name), out name_bs))
            {
                if (name_bs.Length >= MAX_NAME_LENGTH)
                {
                    string e_str = string.Format("the length of the name(type is string) is more than {0}", MAX_NAME_LENGTH);

                    //if (WampAttributes.Logger != null)
                    //{
                    //    WampAttributes.Logger.Log(e_str);
                    //}

                    WampMessageException wm_e = new WampMessageException(e_str);
                    throw wm_e;
                }

                ret_byte.Add((byte)(name_bs.Length));
                ret_byte.AddRange(name_bs);
            }

            if (_args != null)
            {
                foreach (object arg in _args)
                {
                    if (arg == null)
                    {
                        ret_byte.Add((byte)(WampArgType.NULL));
                    }
                    else
                    {
                        byte[] arg_bs;
                        Type arg_type = arg.GetType();

                        // string or char type, length is 1+4+n, 1 is arg type, 4 is arg length, n is string's length(less than 2147483647)
                        if (arg_type == typeof(string) || arg_type == typeof(char))
                        {
                            if (WampValueHelper.ParseBytes((string)arg, out arg_bs))
                            {
                                if (arg_bs.Length >= MAX_ARG_LENGTH)
                                {
                                    string e_str = string.Format("the length of an arg(type is string) is more than {0}", MAX_ARG_LENGTH);

                                    //if (WampAttributes.Logger != null)
                                    //{
                                    //    WampAttributes.Logger.Log(e_str);
                                    //}

                                    WampMessageException wm_e = new WampMessageException(e_str);
                                    throw wm_e;
                                }

                                ret_byte.Add((byte)(WampArgType.STRING));
                                byte[] arg_length;
                                WampValueHelper.ParseBytes(arg_bs.Length, out arg_length);
                                ret_byte.AddRange(arg_length);
                                ret_byte.AddRange(arg_bs);
                            }
                        }
                        // bytes type, length is 1+4+n, 1 is arg type, 4 is arg length, n is byte[]'s length(less than 2147483647)
                        else if (arg_type == typeof(byte[]))
                        {
                            arg_bs = (byte[])arg;

                            if (arg_bs.Length >= MAX_ARG_LENGTH)
                            {
                                string e_str = string.Format("the length of an arg(type is byte[]) is more than {0}", MAX_ARG_LENGTH);

                                //if (WampAttributes.Logger != null)
                                //{
                                //    WampAttributes.Logger.Log(e_str);
                                //}

                                WampMessageException wm_e = new WampMessageException(e_str);
                                throw wm_e;
                            }

                            ret_byte.Add((byte)(WampArgType.BYTES));
                            byte[] arg_length;
                            WampValueHelper.ParseBytes(arg_bs.Length, out arg_length);
                            ret_byte.AddRange(arg_length);
                            ret_byte.AddRange(arg_bs);
                        }
                        // float and double type, length is 1+8, 1 is arg type, 8 is double's length
                        else if (arg_type == typeof(float) || arg_type == typeof(double))
                        {
                            if (WampValueHelper.ParseBytes((double)arg, out arg_bs))
                            {
                                ret_byte.Add((byte)(WampArgType.DOUBLE));
                                ret_byte.AddRange(arg_bs);
                            }
                        }
                        // int type, length is 1+4, 1 is arg type, 4 is int's length
                        else if (arg_type == typeof(int))
                        {
                            if (WampValueHelper.ParseBytes((int)arg, out arg_bs))
                            {
                                ret_byte.Add((byte)(WampArgType.INT));
                                ret_byte.AddRange(arg_bs);
                            }
                        }
                    }
                }
            }

            socket.Send(ret_byte.ToArray());
        }
        private bool _getArgs(List<byte> msg_bs, ref List<object> args)
        {
            if (msg_bs == null || msg_bs.Count == 0) return true;

            switch ((WampArgType)msg_bs[0])
            {
                case WampArgType.STRING:
                    string s = string.Empty;
                    byte[] s_len_bs = new byte[4];
                    msg_bs.CopyTo(1, s_len_bs, 0, 4);
                    int args_len;
                    if (WampValueHelper.ParseInt(s_len_bs, out args_len))
                    {
                        byte[] s_bs = new byte[args_len];
                        msg_bs.CopyTo(5, s_bs, 0, args_len);
                        msg_bs.RemoveRange(0, args_len + 5);
                        WampValueHelper.ParseString(s_bs, out s);
                        args.Add(s);
                    }
                    break;
                case WampArgType.BYTES:
                    // @ to do, might have some trouble in the byte[] type arg
                    byte[] b_len_bs = new byte[4];
                    msg_bs.CopyTo(1, b_len_bs, 0, 4);
                    int argb_len;
                    if (WampValueHelper.ParseInt(b_len_bs, out argb_len))
                    {
                        byte[] bs_bs = new byte[argb_len];
                        msg_bs.CopyTo(5, bs_bs, 0, argb_len);
                        msg_bs.RemoveRange(0, argb_len + 5);
                        args.Add(bs_bs.ToString());
                    }
                    break;
                case WampArgType.INT:
                    int i = 0;
                    byte[] i_bs = new byte[4];
                    msg_bs.CopyTo(1, i_bs, 0, 4);
                    msg_bs.RemoveRange(0, 5);
                    WampValueHelper.ParseInt(i_bs, out i);
                    args.Add(i.ToString());
                    break;
                case WampArgType.DOUBLE:
                    double d = 0.0;
                    byte[] d_bs = new byte[8];
                    msg_bs.CopyTo(1, d_bs, 0, 8);
                    msg_bs.RemoveRange(0, 9);
                    WampValueHelper.ParseDouble(d_bs, out d);
                    args.Add(d.ToString());
                    break;
                default:
                    return false;
            }
            return _getArgs(msg_bs, ref args);
        }

        internal WampProtocolHead Protocol { get { return _protocol; } }
        internal UInt16 ID { get { return _id; } }
        internal string Entity { get { return _entity; } }
        internal string Name { get { return _name; } }
        internal object[] Args { get { return _args; } }

        internal bool Construct(object message)
        {
            if (message.GetType() == typeof(string))
            {
                string mes_str = (string)message;

                try
                {
                    string[] tokens = mes_str.Split(SPLIT_CHARS, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length == 0) return false;

                    byte protocol;
                    if (!WampValueHelper.ParseByte(tokens[0], out protocol)) return false;
                    UInt16 id;
                    if (!WampValueHelper.ParseUInt16(tokens[1], out id)) return false;

                    List<string> paras = new List<string>(tokens);
                    paras.RemoveRange(0, 2);
                    List<string> args = new List<string>(paras);
                    args.RemoveRange(0, 2);

                    _protocol = (WampProtocolHead)protocol;
                    _id = id;
                    _entity = paras[0];
                    _name = paras[1];
                    _args = args.ToArray();
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }
            else if (message.GetType() == typeof(byte[]))
            {
                List<byte> msg_bs = ((byte[])message).ToList();

                try
                {
                    _protocol = (WampProtocolHead)msg_bs[0];
                    msg_bs.RemoveAt(0);

                    byte[] id_bs = new byte[2];
                    msg_bs.CopyTo(0, id_bs, 0, 2);
                    msg_bs.RemoveRange(0, 2);
                    if (!WampValueHelper.ParseUInt16(id_bs, out _id)) return false;

                    string e = string.Empty;
                    byte[] e_bs = new byte[msg_bs[0]];
                    msg_bs.CopyTo(1, e_bs, 0, msg_bs[0]);
                    msg_bs.RemoveRange(0, 1 + msg_bs[0]);
                    if (!WampValueHelper.ParseString(e_bs, out e)) return false;
                    _entity = e.Split(SPLIT_CHARS)[0];
                    _name = e.Split(SPLIT_CHARS)[1];

                    List<object> args = new List<object>();
                    if (!_getArgs(msg_bs, ref args)) return false;
                    _args = args.ToArray();
                }
                catch (Exception)
                {
                    return false;
                }

                return true;
            }

            return false;
        }
        internal bool Construct(WampProtocolHead protocol, UInt16 id, string entity, string name, object[] args = null)
        {
            _protocol = protocol;
            _id = id;
            _entity = entity;
            _name = name;
            _args = args;

            return true;
        }
        internal void Send(IWebSocketConnection socket)
        {
            if (WampSetting.IsByteMode)
            {
                _sendByte(socket);
            }
            else
            {
                _sendString(socket);
            }

            // log, when send a message back
            if (WampSetting.Logger != null)
            {
                string log_str = string.Format("Send a message from {0}:{1}, is {2}",
                    socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort, this.ToString());
                WampSetting.Logger.Log(log_str);
            }
        }
        internal void SendErro(IWebSocketConnection socket, object message)
        {
            if (message.GetType() == typeof(byte[]))
            {
                List<byte> ret_bs = new List<byte>();
                ret_bs.Add((byte)WampProtocolHead.ERROR);
                ret_bs.AddRange((byte[])message);
                socket.Send(ret_bs.ToArray());
            }
            else
            {
                string ret_str = string.Format("{0},{1}", (byte)WampProtocolHead.ERROR, message);
                socket.Send(ret_str);
            }

            // log, when send an erro message back
            if (WampSetting.Logger != null)
            {
                string log_str = string.Format("Send an erro message from {0}:{1}, is {2},{3}",
                    socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort, (byte)WampProtocolHead.ERROR, message);
                WampSetting.Logger.Log(log_str);
            }
        }
        public override string ToString()
        {
            string args_str = string.Empty;
            if (_args != null && _args.Length != 0)
            {
                foreach (object arg in _args)
                {
                    if (arg == null)
                    {
                        args_str += ",";
                    }
                    else
                    {
                        args_str += arg.ToString() + ",";
                    }
                }

                if (!string.IsNullOrEmpty(args_str))
                {
                    args_str = args_str.Substring(0, args_str.Length - 1);
                }
            }

            return string.Format("Protocol:{0};ID:{1};Entity:{2};Name:{3};Args:{4}", Protocol, ID, Entity, Name, args_str);
        }
    }
}
