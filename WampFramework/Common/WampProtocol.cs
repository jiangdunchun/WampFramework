using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WampFramework.API;

namespace WampFramework.Common
{
    enum WampProtocolHead : byte
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
        BYTE = 2,
        BOOL = 3,
        USHORT = 4,
        SHORT = 5,
        INT = 6,
        FLOAT = 7,
        DOUBLE = 8,
        JSON = 9,

        STRINGS = 11,
        BYTES = 12,
        BOOLS = 13,
        USHORTS = 14,
        SHORTS = 15,
        INTS = 16,
        FLOATS = 17,
        DOUBLES = 18,
        JSONS = 19
    }

    static class WampByteModeHelper
    {
        static internal bool ParseStringArray(List<byte> value, ref List<string> ret)
        {
            if (value.Count != 0)
            {
                try
                {
                    byte[] str_length_ba = new byte[4];
                    value.CopyTo(0, str_length_ba, 0, 4);
                    if (WampValueHelper.ParseInt(str_length_ba, out int str_lenth))
                    {
                        byte[] str_entity_ba = new byte[str_lenth];
                        value.CopyTo(5, str_entity_ba, 0, str_lenth);
                        if (WampValueHelper.ParseString(str_entity_ba, out string str_entity))
                        {
                            ret.Add(str_entity);
                            value.RemoveRange(0, 4 + str_lenth);
                            return ParseStringArray(value, ref ret);
                        }
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        static internal bool ParseByteArray(List<byte> value, ref List<byte> ret)
        {
            if (value.Count != 0)
            {
                try
                {
                    ret.Add(value[0]);
                    value.RemoveAt(0);
                    return ParseByteArray(value, ref ret);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        static internal bool ParseBoolArray(List<byte> value, ref List<bool> ret)
        {
            if (value.Count != 0)
            {
                try
                {
                    ret.Add(value[0] != 0);
                    value.RemoveAt(0);
                    return ParseBoolArray(value, ref ret);
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        static internal bool ParseUshortArray(List<byte> value, ref List<ushort> ret)
        {
            if (value.Count != 0)
            {
                try
                {
                    byte[] us_entity_ba = new byte[2];
                    value.CopyTo(0, us_entity_ba, 0, 2);
                    if (WampValueHelper.ParseUshort(us_entity_ba, out ushort us_entity))
                    {
                        ret.Add(us_entity);
                        value.RemoveRange(0, 2);
                        return ParseUshortArray(value, ref ret);
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        static internal bool ParseShortArray(List<byte> value, ref List<short> ret)
        {
            if (value.Count != 0)
            {
                try
                {
                    byte[] sh_entity_ba = new byte[2];
                    value.CopyTo(0, sh_entity_ba, 0, 2);
                    if (WampValueHelper.ParseShort(sh_entity_ba, out short sh_entity))
                    {
                        ret.Add(sh_entity);
                        value.RemoveRange(0, 2);
                        return ParseShortArray(value, ref ret);
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        static internal bool ParseIntArray(List<byte> value, ref List<int> ret)
        {
            if (value.Count != 0)
            {
                try
                {
                    byte[] int_entity_ba = new byte[4];
                    value.CopyTo(0, int_entity_ba, 0, 4);
                    if (WampValueHelper.ParseInt(int_entity_ba, out int int_entity))
                    {
                        ret.Add(int_entity);
                        value.RemoveRange(0, 4);
                        return ParseIntArray(value, ref ret);
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        static internal bool ParseFloatArray(List<byte> value, ref List<float> ret)
        {
            if (value.Count != 0)
            {
                try
                {
                    byte[] float_entity_ba = new byte[4];
                    value.CopyTo(0, float_entity_ba, 0, 4);
                    if (WampValueHelper.ParseFloat(float_entity_ba, out float float_entity))
                    {
                        ret.Add(float_entity);
                        value.RemoveRange(0, 4);
                        return ParseFloatArray(value, ref ret);
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }
        static internal bool ParseDoubleArray(List<byte> value, ref List<double> ret)
        {
            if (value.Count != 0)
            {
                try
                {
                    byte[] double_entity_ba = new byte[8];
                    value.CopyTo(0, double_entity_ba, 0, 4);
                    if (WampValueHelper.ParseDouble(double_entity_ba, out double double_entity))
                    {
                        ret.Add(double_entity);
                        value.RemoveRange(0, 4);
                        return ParseDoubleArray(value, ref ret);
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        static internal List<byte> GetBytes(List<string> value)
        {
            List<byte> ret = new List<byte>();

            List<byte> str_entity = new List<byte>();
            foreach (string arg_str in value)
            {
                if (WampValueHelper.ParseBytes(arg_str, out byte[] arg_bs))
                {
                    if (WampValueHelper.ParseBytes(arg_bs.Length, out byte[] arg_length))
                    {
                        str_entity.AddRange(arg_length);
                        str_entity.AddRange(arg_bs);
                    }
                }
            }

            if (WampValueHelper.ParseBytes(str_entity.Count, out byte[] args_length))
            {
                ret.AddRange(args_length);
                ret.AddRange(str_entity);
            }

            return ret;
        }
        static internal List<byte> GetBytes(List<byte> value)
        {
            List<byte> ret = new List<byte>();

            if (WampValueHelper.ParseBytes(value.Count, out byte[] args_length))
            {
                ret.AddRange(args_length);
                ret.AddRange(value);
            }

            return ret;
        }
        static internal List<byte> GetBytes(List<bool> value)
        {
            List<byte> ret = new List<byte>();

            List<byte> bool_entity = new List<byte>();
            foreach (bool arg_bool in value)
            {
                bool_entity.Add((byte)(arg_bool ? 0 : 1));
            }

            if (WampValueHelper.ParseBytes(bool_entity.Count, out byte[] args_length))
            {
                ret.AddRange(args_length);
                ret.AddRange(bool_entity);
            }

            return ret;
        }
        static internal List<byte> GetBytes(List<ushort> value)
        {
            List<byte> ret = new List<byte>();

            List<byte> ushort_entity = new List<byte>();
            foreach (ushort arg_ushort in value)
            {
                if (WampValueHelper.ParseBytes(arg_ushort, out byte[] arg_bs))
                {
                    ushort_entity.AddRange(arg_bs);
                }
            }

            if (WampValueHelper.ParseBytes(ushort_entity.Count, out byte[] args_length))
            {
                ret.AddRange(args_length);
                ret.AddRange(ushort_entity);
            }

            return ret;
        }
        static internal List<byte> GetBytes(List<short> value)
        {
            List<byte> ret = new List<byte>();

            List<byte> short_entity = new List<byte>();
            foreach (short arg_short in value)
            {
                if (WampValueHelper.ParseBytes(arg_short, out byte[] arg_bs))
                {
                    short_entity.AddRange(arg_bs);
                }
            }

            if (WampValueHelper.ParseBytes(short_entity.Count, out byte[] args_length))
            {
                ret.AddRange(args_length);
                ret.AddRange(short_entity);
            }

            return ret;
        }
        static internal List<byte> GetBytes(List<int> value)
        {
            List<byte> ret = new List<byte>();

            List<byte> int_entity = new List<byte>();
            foreach (int arg_int in value)
            {
                if (WampValueHelper.ParseBytes(arg_int, out byte[] arg_bs))
                {
                    int_entity.AddRange(arg_bs);
                }
            }

            if (WampValueHelper.ParseBytes(int_entity.Count, out byte[] args_length))
            {
                ret.AddRange(args_length);
                ret.AddRange(int_entity);
            }

            return ret;
        }
        static internal List<byte> GetBytes(List<float> value)
        {
            List<byte> ret = new List<byte>();

            List<byte> float_entity = new List<byte>();
            foreach (float arg_float in value)
            {
                if (WampValueHelper.ParseBytes(arg_float, out byte[] arg_bs))
                {
                    float_entity.AddRange(arg_bs);
                }
            }

            if (WampValueHelper.ParseBytes(float_entity.Count, out byte[] args_length))
            {
                ret.AddRange(args_length);
                ret.AddRange(float_entity);
            }

            return ret;
        }
        static internal List<byte> GetBytes(List<double> value)
        {
            List<byte> ret = new List<byte>();

            List<byte> double_entity = new List<byte>();
            foreach (double arg_double in value)
            {
                if (WampValueHelper.ParseBytes(arg_double, out byte[] arg_bs))
                {
                    double_entity.AddRange(arg_bs);
                }
            }

            if (WampValueHelper.ParseBytes(double_entity.Count, out byte[] args_length))
            {
                ret.AddRange(args_length);
                ret.AddRange(double_entity);
            }

            return ret;
        }
        static internal List<byte> GetBytes(List<IWampJsonData> value)
        {
            List<byte> ret = new List<byte>();

            List<byte> json_entity = new List<byte>();
            foreach (IWampJsonData arg_json in value)
            {
                if (WampValueHelper.ParseBytes(arg_json, out byte[] arg_bs))
                {
                    if (WampValueHelper.ParseBytes(arg_bs.Length, out byte[] arg_length))
                    {
                        json_entity.AddRange(arg_length);
                        json_entity.AddRange(arg_bs);
                    }
                }
            }

            if (WampValueHelper.ParseBytes(json_entity.Count, out byte[] args_length))
            {
                ret.AddRange(args_length);
                ret.AddRange(json_entity);
            }

            return ret;
        }
    }

    /* WampProtocol: byte
     * ID: ushort
     * string message formate
     * received message: WampProtocol|ID     |ClassName|MethodorEventName(|arg1           |arg2           |arg3                     )
     * 
     * send message:     WampProtocol|ID     |ClassName|MethodorEventName(|arg1           |arg2           |arg3                     )
     * 
     * byte message foramte
     * received message: WampProtocol|ID     |ClassName|MethodorEventName(|arg1           |arg2           |arg3                     )
     * data type:        byte        |ushort |byte+string                (|byte+int       |byte+double    |byte+int+string         )
     * data description: WampProtocol|ID     |namelength+namebyte        (|argtype+argbyte|argtype+argbyte|argtype+arglength+argbyte)
     * byte length:      1           |2      |1+n                        (|1+4            |1+8            |1+4+n                    )
     * 
     * send message:     WampProtocol|ID     |ClassName,MethodorEventName(|arg1           |arg2           |arg3                     )
     * data type:        byte        |ushort |byte+string                (|byte+int       |byte+double    |byte+int+string         )
     * data description: WampProtocol|ID     |namelength+namebyte        (|argtype+argbyte|argtype+argbyte|argtype+arglength+argbyte)
     * byte length:      1           |2      |1+n                        (|1+4            |1+8            |1+4+n                    )
    */
    class WampMessage
    {
        private readonly byte MAX_NAME_LENGTH = byte.MaxValue; // the byte[] length is no more than 255 in Byte Mode  
        private readonly char[] ITEM_SPLIT_CHAR = { '|' }; // split each items in String Mode, or split class name and method name in Byte Mode 
        private readonly string ITEM_SPLIT_STR = "|"; // split each items in String Mode, or split class name and method name in Byte Mode
        private readonly char[] ARRAY_SPLIT_CHAR = { ';' }; // split array items in String Mode
        private readonly string ARRAY_SPLIT_STR = ";"; // split array items in String Mode
        private readonly string ARRAY_START_STR = "["; // occupy the first positon in array items in String Mode
        private readonly string ARRAY_END_STR = "]"; // occupy the last positon in array items in String Mode

        private WampProtocolHead _protocol;
        private ushort _id;
        private string _entity;
        private string _name;
        private object[] _args;

        private void _sendString(IWebSocketConnection socket)
        {
            string send_str = (byte)_protocol + ITEM_SPLIT_STR + _id + ITEM_SPLIT_STR + _entity + ITEM_SPLIT_STR + _name;

            if (_args != null && _args.Length != 0)
            {
                string args_str = string.Empty;

                List<string> arg_strs = new List<string>();
                _getStringsbyArgs(_args.ToList(), ref arg_strs);
                foreach (string arg_str in arg_strs)
                {
                    args_str += arg_str + ITEM_SPLIT_STR;
                }

                if (!string.IsNullOrEmpty(args_str))
                {
                    args_str = args_str.Substring(0, args_str.Length - 1);
                }

                send_str = send_str + ITEM_SPLIT_STR + args_str;
            }

            socket.Send(send_str);
        }
        private void _sendByte(IWebSocketConnection socket)
        {
            List<byte> ret_byte = new List<byte>
            {
                // protocol, length is 1
                (byte)_protocol
            };

            // id, length is 2
            if (WampValueHelper.ParseBytes(_id, out byte[] id_bs))
            {
                ret_byte.AddRange(id_bs);
            }

            // entity and name, "_entity,_name", length is 1+n, 1 is n's length(less than 255)
            if (WampValueHelper.ParseBytes(string.Format("{0},{1}", _entity, _name), out byte[] name_bs))
            {
                if (name_bs.Length >= MAX_NAME_LENGTH)
                {
                    string e_str = string.Format("the length of the name(type is string) is more than {0}", MAX_NAME_LENGTH);

                    WampMessageException wm_e = new WampMessageException(e_str);
                    throw wm_e;
                }

                ret_byte.Add((byte)(name_bs.Length));
                ret_byte.AddRange(name_bs);
            }

            if (_args != null)
            {
                _getBytesbyArgs(_args.ToList(), ref ret_byte);
            }

            socket.Send(ret_byte.ToArray());
        }
        private bool _getArgsbyBytes(List<byte> msg_bs, ref List<object> args)
        {
            if (msg_bs == null || msg_bs.Count == 0) return true;

            object arg_obj = null;
            WampArgType arg_Type = WampArgType.NULL;
            msg_bs.RemoveAt(0);
            // null 
            if (arg_Type == WampArgType.NULL)
            {
                arg_obj = null;
            }
            // string 4+n
            else if (arg_Type == WampArgType.STRING)
            {
                // string length
                byte[] str_len_bs = new byte[4];
                msg_bs.CopyTo(0, str_len_bs, 0, 4);
                if (WampValueHelper.ParseInt(str_len_bs, out int str_len))
                {
                    // string entity
                    byte[] str_bs = new byte[str_len];
                    msg_bs.CopyTo(4, str_bs, 0, str_len);
                    if (WampValueHelper.ParseString(str_bs, out string arg_str))
                    {
                        arg_obj = arg_str;
                    }
                }

                msg_bs.RemoveRange(0, str_len + 4);
            }
            // byte 1
            else if (arg_Type == WampArgType.BYTE)
            {
                byte arg_byte = msg_bs[0];
                arg_obj = arg_byte;

                msg_bs.RemoveAt(0);

            }
            // bool 1
            else if (arg_Type == WampArgType.BOOL)
            {
                bool arg_bool = msg_bs[0] != 0;
                arg_obj = arg_bool;

                msg_bs.RemoveAt(0);
            }
            // ushort 2
            else if (arg_Type == WampArgType.USHORT)
            {
                byte[] ushort_bs = new byte[2];
                msg_bs.CopyTo(0, ushort_bs, 0, 2);
                if (WampValueHelper.ParseUshort(ushort_bs, out ushort arg_ushort))
                {
                    arg_obj = arg_ushort;
                }

                msg_bs.RemoveRange(0, 2);
            }
            // short 2
            else if (arg_Type == WampArgType.SHORT)
            {
                byte[] short_bs = new byte[2];
                msg_bs.CopyTo(0, short_bs, 0, 2);
                if (WampValueHelper.ParseShort(short_bs, out short arg_short))
                {
                    arg_obj = arg_short;
                }

                msg_bs.RemoveRange(0, 2);
            }
            // int 4
            else if (arg_Type == WampArgType.INT)
            {
                byte[] int_bs = new byte[4];
                msg_bs.CopyTo(0, int_bs, 0, 4);
                if (WampValueHelper.ParseInt(int_bs, out int arg_int))
                {
                    arg_obj = arg_int;
                }

                msg_bs.RemoveRange(0, 4);
            }
            // float 4
            else if (arg_Type == WampArgType.FLOAT)
            {
                byte[] float_bs = new byte[4];
                msg_bs.CopyTo(0, float_bs, 0, 4);
                if (WampValueHelper.ParseFloat(float_bs, out float arg_float))
                {
                    arg_obj = arg_float;
                }

                msg_bs.RemoveRange(0, 4);
            }
            // double 8
            else if (arg_Type == WampArgType.DOUBLE)
            {
                byte[] double_bs = new byte[8];
                msg_bs.CopyTo(0, double_bs, 0, 8);
                if (WampValueHelper.ParseDouble(double_bs, out double arg_double))
                {
                    arg_obj = arg_double;
                }

                msg_bs.RemoveRange(0, 8);
            }
            // json 4+n, return as string, need to transfer in other place
            else if (arg_Type == WampArgType.JSON)
            {
                byte[] json_len_bs = new byte[4];
                msg_bs.CopyTo(0, json_len_bs, 0, 4);
                if (WampValueHelper.ParseInt(json_len_bs, out int json_len))
                {
                    byte[] json_bs = new byte[json_len];
                    msg_bs.CopyTo(4, json_bs, 0, json_len);
                    if (WampValueHelper.ParseString(json_bs, out string arg_json))
                    {
                        arg_obj = arg_json;
                    }
                }

                msg_bs.RemoveRange(0, json_len + 4);
            }
            // string array 4+4+n1+4+n2+...
            else if (arg_Type == WampArgType.STRINGS)
            {
                byte[] strs_len_bs = new byte[4];
                msg_bs.CopyTo(0, strs_len_bs, 0, 4);
                if (WampValueHelper.ParseInt(strs_len_bs, out int strs_len))
                {
                    byte[] strs_bs = new byte[strs_len];
                    msg_bs.CopyTo(4, strs_bs, 0, strs_len);
                    List<string> arg_strs = new List<string>();
                    if (WampByteModeHelper.ParseStringArray(strs_bs.ToList(), ref arg_strs))
                    {
                        arg_obj = arg_strs.ToArray();
                    }
                }

                msg_bs.RemoveRange(0, 4 + strs_len);
            }
            // byte array 4+1+1+...
            else if (arg_Type == WampArgType.BYTES)
            {
                byte[] bytes_len_bs = new byte[4];
                msg_bs.CopyTo(0, bytes_len_bs, 0, 4);
                if (WampValueHelper.ParseInt(bytes_len_bs, out int bytes_len))
                {
                    byte[] bs_bs = new byte[bytes_len];
                    msg_bs.CopyTo(4, bs_bs, 0, bytes_len);
                    List<byte> arg_bytes = new List<byte>();
                    if (WampByteModeHelper.ParseByteArray(bs_bs.ToList(), ref arg_bytes))
                    {
                        arg_obj = arg_bytes.ToArray();
                    }
                }

                msg_bs.RemoveRange(0, 4 + bytes_len);
            }
            // bool array 4+1+1+...
            else if (arg_Type == WampArgType.BOOLS)
            {
                byte[] bools_len_bs = new byte[4];
                msg_bs.CopyTo(0, bools_len_bs, 0, 4);
                if (WampValueHelper.ParseInt(bools_len_bs, out int bools_len))
                {
                    byte[] bools_bs = new byte[bools_len];
                    msg_bs.CopyTo(4, bools_bs, 0, bools_len);
                    List<bool> args_bools = new List<bool>();
                    if (WampByteModeHelper.ParseBoolArray(bools_bs.ToList(), ref args_bools))
                    {
                        arg_obj = args_bools.ToArray();
                    }
                }

                msg_bs.RemoveRange(0, 4 + bools_len);
            }
            // ushort array 4+2+2+...
            else if (arg_Type == WampArgType.USHORTS)
            {
                byte[] ushorts_len_bs = new byte[4];
                msg_bs.CopyTo(0, ushorts_len_bs, 0, 4);
                if (WampValueHelper.ParseInt(ushorts_len_bs, out int ushorts_len))
                {
                    byte[] ushorts_bs = new byte[ushorts_len];
                    msg_bs.CopyTo(4, ushorts_bs, 0, ushorts_len);
                    List<ushort> args_ushorts = new List<ushort>();
                    if (WampByteModeHelper.ParseUshortArray(ushorts_bs.ToList(), ref args_ushorts))
                    {
                        arg_obj = args_ushorts.ToArray();
                    }
                }

                msg_bs.RemoveRange(0, 4 + ushorts_len);
            }
            // short array 4+2+2+...
            else if (arg_Type == WampArgType.SHORTS)
            {
                byte[] shorts_len_bs = new byte[4];
                msg_bs.CopyTo(0, shorts_len_bs, 0, 4);
                if (WampValueHelper.ParseInt(shorts_len_bs, out int shorts_len))
                {
                    byte[] shorts_bs = new byte[shorts_len];
                    msg_bs.CopyTo(4, shorts_bs, 0, shorts_len);
                    List<short> args_shorts = new List<short>();
                    if (WampByteModeHelper.ParseShortArray(shorts_bs.ToList(), ref args_shorts))
                    {
                        arg_obj = args_shorts.ToArray();
                    }
                }

                msg_bs.RemoveRange(0, 4 + shorts_len);
            }
            // int array 4+4+4+...
            else if (arg_Type == WampArgType.INTS)
            {
                byte[] ints_len_bs = new byte[4];
                msg_bs.CopyTo(0, ints_len_bs, 0, 4);
                if (WampValueHelper.ParseInt(ints_len_bs, out int ints_len))
                {
                    byte[] ints_bs = new byte[ints_len];
                    msg_bs.CopyTo(4, ints_bs, 0, ints_len);
                    List<int> args_ints = new List<int>();
                    if (WampByteModeHelper.ParseIntArray(ints_bs.ToList(), ref args_ints))
                    {
                        arg_obj = args_ints.ToArray();
                    }
                }

                msg_bs.RemoveRange(0, 4 + ints_len);
            }
            // float array 4+4+4+...
            else if (arg_Type == WampArgType.FLOATS)
            {
                byte[] floats_len_bs = new byte[4];
                msg_bs.CopyTo(0, floats_len_bs, 0, 4);
                if (WampValueHelper.ParseInt(floats_len_bs, out int floats_len))
                {
                    byte[] floats_bs = new byte[floats_len];
                    msg_bs.CopyTo(4, floats_bs, 0, floats_len);
                    List<float> args_floats = new List<float>();
                    if (WampByteModeHelper.ParseFloatArray(floats_bs.ToList(), ref args_floats))
                    {
                        arg_obj = args_floats.ToArray();
                    }
                }

                msg_bs.RemoveRange(0, 4 + floats_len);
            }
            // double array 4+8+8+...
            else if (arg_Type == WampArgType.DOUBLES)
            {
                byte[] doubles_len_bs = new byte[4];
                msg_bs.CopyTo(0, doubles_len_bs, 0, 4);
                if (WampValueHelper.ParseInt(doubles_len_bs, out int doubles_len))
                {
                    byte[] doubles_bs = new byte[doubles_len];
                    msg_bs.CopyTo(4, doubles_bs, 0, doubles_len);
                    List<double> args_doubles = new List<double>();
                    if (WampByteModeHelper.ParseDoubleArray(doubles_bs.ToList(), ref args_doubles))
                    {
                        arg_obj = args_doubles.ToArray();
                    }
                }

                msg_bs.RemoveRange(0, 4 + doubles_len);
            }
            // string array 4+4+n1+4+n2+..., return as string[], need to transfer in other place
            else if (arg_Type == WampArgType.JSONS)
            {
                byte[] jsons_len_bs = new byte[4];
                msg_bs.CopyTo(0, jsons_len_bs, 0, 4);
                if (WampValueHelper.ParseInt(jsons_len_bs, out int jsons_len))
                {
                    byte[] jsons_bs = new byte[jsons_len];
                    msg_bs.CopyTo(4, jsons_bs, 0, jsons_len);
                    List<string> arg_jsons = new List<string>();
                    if (WampByteModeHelper.ParseStringArray(jsons_bs.ToList(), ref arg_jsons))
                    {
                        arg_obj = arg_jsons.ToArray();
                    }
                }

                msg_bs.RemoveRange(0, 4 + jsons_len);
            }

            args.Add(arg_obj);

            return _getArgsbyBytes(msg_bs, ref args);
        }
        private bool _getBytesbyArgs(List<object> args, ref List<byte> msg_bs)
        {
            foreach (object arg in args)
            {
                if (arg == null)
                {
                    msg_bs.Add((byte)WampArgType.NULL);
                }
                else if (arg.GetType() == typeof(string))
                {
                    if (WampValueHelper.ParseBytes((string)arg, out byte[] arg_bs))
                    {
                        msg_bs.Add((byte)(WampArgType.STRING));
                        if (WampValueHelper.ParseBytes(arg_bs.Length, out byte[] arg_length))
                        {
                            msg_bs.AddRange(arg_length);
                            msg_bs.AddRange(arg_bs);
                        }
                    }
                }
                else if (arg.GetType() == typeof(byte))
                {
                    msg_bs.Add((byte)(WampArgType.BYTE));
                    msg_bs.Add((byte)arg);
                }
                else if (arg.GetType() == typeof(bool))
                {
                    msg_bs.Add((byte)(WampArgType.BOOL));
                    msg_bs.Add((byte)((bool)arg ? 1 : 0));
                }
                else if (arg.GetType() == typeof(ushort))
                {
                    if (WampValueHelper.ParseBytes((ushort)arg, out byte[] arg_bs))
                    {
                        msg_bs.Add((byte)(WampArgType.USHORT));
                        msg_bs.AddRange(arg_bs);
                    }
                }
                else if (arg.GetType() == typeof(short))
                {
                    if (WampValueHelper.ParseBytes((short)arg, out byte[] arg_bs))
                    {
                        msg_bs.Add((byte)(WampArgType.SHORT));
                        msg_bs.AddRange(arg_bs);
                    }
                }
                else if (arg.GetType() == typeof(int))
                {
                    if (WampValueHelper.ParseBytes((int)arg, out byte[] arg_bs))
                    {
                        msg_bs.Add((byte)(WampArgType.INT));
                        msg_bs.AddRange(arg_bs);
                    }
                }
                else if (arg.GetType() == typeof(float))
                {
                    if (WampValueHelper.ParseBytes((float)arg, out byte[] arg_bs))
                    {
                        msg_bs.Add((byte)(WampArgType.FLOAT));
                        msg_bs.AddRange(arg_bs);
                    }
                }
                else if (arg.GetType() == typeof(double))
                {
                    if (WampValueHelper.ParseBytes((double)arg, out byte[] arg_bs))
                    {
                        msg_bs.Add((byte)(WampArgType.DOUBLE));
                        msg_bs.AddRange(arg_bs);
                    }
                }
                else if (typeof(IWampJsonData).IsAssignableFrom(arg.GetType()))
                {
                    if (WampValueHelper.ParseBytes((IWampJsonData)arg, out byte[] arg_bs))
                    {
                        msg_bs.Add((byte)(WampArgType.JSON));
                        if (WampValueHelper.ParseBytes(arg_bs.Length, out byte[] arg_length))
                        {
                            msg_bs.AddRange(arg_length);
                            msg_bs.AddRange(arg_bs);
                        }
                    }
                }
                else if (arg.GetType() == typeof(string[]))
                {
                    msg_bs.Add((byte)(WampArgType.STRINGS));
                    msg_bs.AddRange(WampByteModeHelper.GetBytes(((string[])arg).ToList()));
                }
                else if (arg.GetType() == typeof(byte[]))
                {
                    msg_bs.Add((byte)(WampArgType.BYTES));
                    msg_bs.AddRange(WampByteModeHelper.GetBytes(((byte[])arg).ToList()));
                }
                else if (arg.GetType() == typeof(bool[]))
                {
                    msg_bs.Add((byte)(WampArgType.BOOLS));
                    msg_bs.AddRange(WampByteModeHelper.GetBytes(((bool[])arg).ToList()));
                }
                else if (arg.GetType() == typeof(ushort[]))
                {
                    msg_bs.Add((byte)(WampArgType.USHORTS));
                    msg_bs.AddRange(WampByteModeHelper.GetBytes(((ushort[])arg).ToList()));
                }
                else if (arg.GetType() == typeof(short))
                {
                    msg_bs.Add((byte)(WampArgType.SHORTS));
                    msg_bs.AddRange(WampByteModeHelper.GetBytes(((short[])arg).ToList()));
                }
                else if (arg.GetType() == typeof(int[]))
                {
                    msg_bs.Add((byte)(WampArgType.INTS));
                    msg_bs.AddRange(WampByteModeHelper.GetBytes(((int[])arg).ToList()));
                }
                else if (arg.GetType() == typeof(float[]))
                {
                    msg_bs.Add((byte)(WampArgType.FLOATS));
                    msg_bs.AddRange(WampByteModeHelper.GetBytes(((float[])arg).ToList()));
                }
                else if (arg.GetType() == typeof(double[]))
                {
                    msg_bs.Add((byte)(WampArgType.DOUBLES));
                    msg_bs.AddRange(WampByteModeHelper.GetBytes(((double[])arg).ToList()));
                }
                else if (typeof(IWampJsonData[]).IsAssignableFrom(arg.GetType()))
                {
                    msg_bs.Add((byte)(WampArgType.JSONS));
                    msg_bs.AddRange(WampByteModeHelper.GetBytes(((IWampJsonData[])arg).ToList()));
                }
            }
            return true;
        }
        private bool _getArgsbyStrings(List<string> msg_strs, ref List<object> args)
        {
            foreach (string msg in msg_strs)
            {
                if (msg.StartsWith(ARRAY_START_STR) && msg.EndsWith(ARRAY_END_STR))
                {
                    string mag_str = msg.Remove(0, 1);
                    mag_str = mag_str.Remove(mag_str.Length - 1, 1);
                    string[] arg = mag_str.Split(ARRAY_SPLIT_CHAR);
                    args.Add(arg);
                }
                else
                {
                    args.Add(msg);
                }
            }
            return true;
        }
        private bool _getStringsbyArgs(List<object> args, ref List<string> msg_strs)
        {
            foreach (object arg in _args)
            {
                if (arg == null)
                {
                    msg_strs.Add(string.Empty);
                }
                else if (arg.GetType().IsArray)
                {
                    string array_str = ARRAY_START_STR;

                    Array obj_array = (Array)arg;
                    if (typeof(IWampJsonData[]).IsAssignableFrom(arg.GetType()))
                    {
                        foreach (object obj in obj_array)
                        {
                            array_str += ((IWampJsonData)obj).ToJson() + ARRAY_SPLIT_STR;
                        }
                    }
                    else
                    {
                        foreach (object obj in obj_array)
                        {
                            array_str += obj.ToString() + ARRAY_SPLIT_STR;
                        }
                    }

                    if (array_str.Length > 1)
                    {
                        array_str = array_str.Remove(array_str.Length - 1, 1);
                    }

                    array_str += ARRAY_END_STR;

                    msg_strs.Add(array_str);
                }
                else
                {
                    if (typeof(IWampJsonData).IsAssignableFrom(arg.GetType()))
                    {
                        msg_strs.Add(((IWampJsonData)arg).ToJson());
                    }
                    else
                    {
                        msg_strs.Add(arg.ToString());
                    }
                }
            }

            return true;
        }

        internal WampProtocolHead Protocol { get { return _protocol; } }
        internal ushort ID { get { return _id; } }
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
                    string[] tokens = mes_str.Split(ITEM_SPLIT_CHAR, StringSplitOptions.RemoveEmptyEntries);

                    if (tokens.Length == 0) return false;

                    if (!WampValueHelper.ParseByte(tokens[0], out byte protocol)) return false;
                    if (!WampValueHelper.ParseUshort(tokens[1], out ushort id)) return false;

                    List<string> paras = new List<string>(tokens);
                    paras.RemoveRange(0, 2);
                    List<string> args_objs = new List<string>(paras);
                    args_objs.RemoveRange(0, 2);

                    _protocol = (WampProtocolHead)protocol;
                    _id = id;
                    _entity = paras[0];
                    _name = paras[1];

                    List<object> args = new List<object>();
                    if (!_getArgsbyStrings(args_objs, ref args)) return false;
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
                    // protocol byte 1
                    _protocol = (WampProtocolHead)msg_bs[0];
                    msg_bs.RemoveAt(0);

                    // id ushort 2
                    byte[] id_bs = new byte[2];
                    msg_bs.CopyTo(0, id_bs, 0, 2);
                    msg_bs.RemoveRange(0, 2);
                    if (!WampValueHelper.ParseUshort(id_bs, out _id)) return false;

                    // class and method or event string 1+n
                    byte[] e_bs = new byte[msg_bs[0]];
                    msg_bs.CopyTo(1, e_bs, 0, msg_bs[0]);
                    msg_bs.RemoveRange(0, 1 + msg_bs[0]);
                    if (!WampValueHelper.ParseString(e_bs, out string e)) return false;
                    _entity = e.Split(ITEM_SPLIT_CHAR)[0];
                    _name = e.Split(ITEM_SPLIT_CHAR)[1];

                    // args 4+n
                    List<object> args = new List<object>();
                    if (!_getArgsbyBytes(msg_bs, ref args)) return false;
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
        internal bool Construct(WampProtocolHead protocol, ushort id, string entity, string name, object[] args = null)
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
            if (WampProperties.IsByteMode)
            {
                _sendByte(socket);
            }
            else
            {
                _sendString(socket);
            }

            // log, when send a message back
            if (WampProperties.Logger != null && WampProperties.LogSend)
            {
                string log_str = string.Format("Send a message from {0}:{1}, is {2}",
                    socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort, this.ToString());
                WampProperties.Logger.Log(log_str);
            }
        }
        internal void SendErro(IWebSocketConnection socket, object message)
        {
            if (message.GetType() == typeof(byte[]))
            {
                List<byte> ret_bs = new List<byte>
                {
                    (byte)WampProtocolHead.ERROR
                };
                ret_bs.AddRange((byte[])message);
                socket.Send(ret_bs.ToArray());
            }
            else
            {
                string ret_str = string.Format("{0}|{1}", (byte)WampProtocolHead.ERROR, message);
                socket.Send(ret_str);
            }

            // log, when send an erro message back
            if (WampProperties.Logger != null)
            {
                string log_str = string.Format("Send an erro message from {0}:{1}, is {2}|{3}",
                    socket.ConnectionInfo.ClientIpAddress, socket.ConnectionInfo.ClientPort, (byte)WampProtocolHead.ERROR, message);
                WampProperties.Logger.Log(log_str);
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
                        args_str += ITEM_SPLIT_STR;
                    }
                    else if (typeof(IWampJsonData).IsAssignableFrom(arg.GetType()))
                    {
                        args_str += ((IWampJsonData)arg).ToJson() + ITEM_SPLIT_STR;
                    }
                    else
                    {
                        args_str += arg.ToString() + ITEM_SPLIT_STR;
                    }
                }

                if (!string.IsNullOrEmpty(args_str))
                {
                    args_str = args_str.Substring(0, args_str.Length - 1);
                }
            }

            return string.Format("Protocol:{0}|ID:{1}|Entity:{2}|Name:{3}|Args:{4}", Protocol, ID, Entity, Name, args_str);
        }
    }
}
