using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WampFramework.API;

namespace WampFramework.Common
{
    static class WampValueHelper
    {
        static private Byte[] _reverse(Byte[] data)
        {
            Array.Reverse(data);
            return data;
        }

        // tranfer different type to byte[]
        static internal bool ParseBytes(short value, out byte[] ret)
        {
            ret = BitConverter.GetBytes(value);

            if (WampProperties.NeedReversed)
            {
                ret = _reverse(ret);
            }

            return true;
        }
        static internal bool ParseBytes(ushort value, out byte[] ret)
        {
            ret = BitConverter.GetBytes(value);

            if (WampProperties.NeedReversed)
            {
                ret = _reverse(ret);
            }

            return true;
        }
        static internal bool ParseBytes(int value, out byte[] ret)
        {
            ret = BitConverter.GetBytes(value);

            if (WampProperties.NeedReversed)
            {
                ret = _reverse(ret);
            }

            return true;
        }
        static internal bool ParseBytes(float value, out byte[] ret)
        {
            ret = BitConverter.GetBytes(value);

            if (WampProperties.NeedReversed)
            {
                ret = _reverse(ret);
            }

            return true;
        }
        static internal bool ParseBytes(double value, out byte[] ret)
        {
            ret = BitConverter.GetBytes(value);

            if (WampProperties.NeedReversed)
            {
                ret = _reverse(ret);
            }

            return true;
        }
        static internal bool ParseBytes(string value, out byte[] ret, Encoding encoding)
        {
            ret = encoding.GetBytes(value);

            if (WampProperties.NeedReversed)
            {
                ret = _reverse(ret);
            }

            return true;
        }
        static internal bool ParseBytes(IWampJsonData value, out byte[] ret, Encoding encoding)
        {
            string str_value = value.ToJson();

            ret = encoding.GetBytes(str_value);

            if (WampProperties.NeedReversed)
            {
                ret = _reverse(ret);
            }

            return true;
        }

        // tranfer byte[] to different type
        static internal bool ParseShort(byte[] data, out short ret)
        {
            if (WampProperties.NeedReversed)
            {
                data = _reverse(data);
            }

            ret = BitConverter.ToInt16(data, 0);

            return true;
        }
        static internal bool ParseUshort(byte[] data, out ushort ret)
        {
            if (WampProperties.NeedReversed)
            {
                data = _reverse(data);
            }

            ret = BitConverter.ToUInt16(data, 0);

            return true;
        }
        static internal bool ParseInt(byte[] data, out int ret)
        {
            if (WampProperties.NeedReversed)
            {
                data = _reverse(data);
            }

            ret = BitConverter.ToInt32(data, 0);

            return true;
        }
        static internal bool ParseFloat(byte[] data, out float ret)
        {
            if (WampProperties.NeedReversed)
            {
                data = _reverse(data);
            }

            ret = BitConverter.ToSingle(data, 0);

            return true;
        }
        static internal bool ParseDouble(byte[] data, out double ret)
        {
            if (WampProperties.NeedReversed)
            {
                data = _reverse(data);
            }

            ret = BitConverter.ToDouble(data, 0);

            return true;
        }
        static internal bool ParseString(byte[] data, out string ret)
        {
            if (WampProperties.NeedReversed)
            {
                data = _reverse(data);
            }

            ret = BitConverter.ToString(data, 0);

            return true;
        }
        static internal bool ParseJson(byte[] data, Type jsonType, out IWampJsonData ret)
        {
            if (WampProperties.NeedReversed)
            {
                data = _reverse(data);
            }

            string str_ret = BitConverter.ToString(data, 0);

            if (ParseJson(str_ret, jsonType, out IWampJsonData json_ret))
            {
                ret = json_ret;
                return true;
            }
            else
            {
                ret = null;
                return false;
            }
        }

        // tranfer string to different type
        static internal bool ParseByte(string value, out byte ret)
        {
            if (byte.TryParse(value, out ret))
            {
                return true;
            }

            return false;
        }
        static internal bool ParseBool(string value, out bool ret)
        {
            ret = false;
            if (value == "True" || value == "true" || value == "1")
            {
                ret = true;
                return true;
            }
            else if (value == "False" || value == "false" || value == "0")
            {
                return true;
            }

            return false;
        }
        static internal bool ParseShort(string data, out short ret)
        {
            if (short.TryParse(data, out ret))
            {
                return true;
            }

            return false;
        }
        static internal bool ParseUshort(string data, out ushort ret)
        {
            if (UInt16.TryParse(data, out ret))
            {
                return true;
            }

            return false;
        }
        static internal bool ParseInt(string data, out int ret)
        {
            if (int.TryParse(data, out ret))
            {
                return true;
            }

            return false;
        }
        static internal bool ParseFloat(string data, out float ret)
        {
            if (float.TryParse(data, out ret))
            {
                return true;
            }

            return false;
        }
        static internal bool ParseDouble(string data, out double ret)
        {
            if (double.TryParse(data, out ret))
            {
                return true;
            }

            return false;
        }
        static internal bool ParseJson(string data, Type jsonType, out IWampJsonData ret)
        {
            ConstructorInfo cst_inf = jsonType.GetConstructor(System.Type.EmptyTypes);
            IWampJsonData json_instance = (IWampJsonData)(cst_inf.Invoke(null));
            if (json_instance != null)
            {
                if (json_instance.Construct(data))
                {
                    ret = json_instance;
                    return true;
                }
            }

            ret = null;
            return false;
        }

        static internal object GetArgument(object obj, ParameterInfo info)
        {
            if (info.ParameterType == obj.GetType())
            {
                return obj;
            }
            else if (obj.GetType() == typeof(string))
            {
                string obj_str = (string)obj;

                if (info.ParameterType == typeof(byte))
                {
                    if (ParseByte(obj_str, out byte ret))
                    {
                        return ret;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (info.ParameterType == typeof(bool))
                {
                    if (ParseBool(obj_str, out bool ret))
                    {
                        return ret;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (info.ParameterType == typeof(short))
                {
                    if (ParseShort(obj_str, out short ret))
                    {
                        return ret;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (info.ParameterType == typeof(ushort))
                {
                    if (ParseUshort(obj_str, out ushort ret))
                    {
                        return ret;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (info.ParameterType == typeof(int))
                {
                    if (ParseInt(obj_str, out int ret))
                    {
                        return ret;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (info.ParameterType == typeof(float))
                {
                    if (ParseFloat(obj_str, out float ret))
                    {
                        return ret;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (info.ParameterType == typeof(double))
                {
                    if (ParseDouble(obj_str, out double ret))
                    {
                        return ret;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (typeof(IWampJsonData).IsAssignableFrom(info.ParameterType))
                {
                    if (ParseJson(obj_str, info.ParameterType, out IWampJsonData ret))
                    {
                        return ret;
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            else if (obj.GetType() == typeof(string[]))
            {
                string[] obj_strs = (string[])obj;

                if (info.ParameterType == typeof(byte[]))
                {
                    List<byte> byte_ret = new List<byte>();
                    foreach (string obj_str in obj_strs)
                    {
                        if (ParseByte(obj_str, out byte ret))
                        {
                            byte_ret.Add(ret);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    return byte_ret.ToArray();
                }
                else if (info.ParameterType == typeof(bool[]))
                {
                    List<bool> bool_ret = new List<bool>();
                    foreach (string obj_str in obj_strs)
                    {
                        if (ParseBool(obj_str, out bool ret))
                        {
                            bool_ret.Add(ret);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    return bool_ret.ToArray();
                }
                else if (info.ParameterType == typeof(short[]))
                {
                    List<short> short_ret = new List<short>();
                    foreach (string obj_str in obj_strs)
                    {
                        if (ParseShort(obj_str, out short ret))
                        {
                            short_ret.Add(ret);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    return short_ret.ToArray();
                }
                else if (info.ParameterType == typeof(ushort[]))
                {
                    List<ushort> ushort_ret = new List<ushort>();
                    foreach (string obj_str in obj_strs)
                    {
                        if (ParseUshort(obj_str, out ushort ret))
                        {
                            ushort_ret.Add(ret);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    return ushort_ret.ToArray();
                }
                else if (info.ParameterType == typeof(int[]))
                {
                    List<int> int_ret = new List<int>();
                    foreach (string obj_str in obj_strs)
                    {
                        if (ParseInt(obj_str, out int ret))
                        {
                            int_ret.Add(ret);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    return int_ret.ToArray();
                }
                else if (info.ParameterType == typeof(float[]))
                {
                    List<float> float_ret = new List<float>();
                    foreach (string obj_str in obj_strs)
                    {
                        if (ParseFloat(obj_str, out float ret))
                        {
                            float_ret.Add(ret);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    return float_ret.ToArray();
                }
                else if (info.ParameterType == typeof(double[]))
                {
                    List<double> double_ret = new List<double>();
                    foreach (string obj_str in obj_strs)
                    {
                        if (ParseDouble(obj_str, out double ret))
                        {
                            double_ret.Add(ret);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    return double_ret.ToArray();
                }
                else if (typeof(IWampJsonData[]).IsAssignableFrom(info.ParameterType))
                {
                    List<IWampJsonData> json_ret = new List<IWampJsonData>();
                    foreach (string obj_str in obj_strs)
                    {
                        if (ParseJson(obj_str, info.ParameterType, out IWampJsonData ret))
                        {
                            json_ret.Add(ret);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    return json_ret.ToArray();
                }
            }

            return null;
        }
    }
}
