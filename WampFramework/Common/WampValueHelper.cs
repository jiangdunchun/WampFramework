using Fleck;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WampFramework.Common
{
    static class WampValueHelper
    {
        static internal bool ParseString(byte[] value, out string ret)
        {
            ret = BitConverter.ToString(value, 0);

            return true;
        }
        static internal bool ParseByte(string value, out byte ret)
        {
            if (value == "NaN")
            {
                ret = 0;
                return true;
            }

            return byte.TryParse(value, out ret);
        }
        static internal bool ParseUInt16(string value, out UInt16 ret)
        {
            if (value == "NaN")
            {
                ret = 0;
                return true;
            }

            return UInt16.TryParse(value, out ret);
        }
        static internal bool ParseUInt16(byte[] value, out UInt16 ret)
        {
            ret = BitConverter.ToUInt16(value, 0);

            return true;
        }
        static internal bool ParseInt(string value, out int ret)
        {
            if (value == "NaN")
            {
                ret = 0;
                return true;
            }

            return int.TryParse(value, out ret);
        }
        static internal bool ParseInt(byte[] value, out int ret)
        {
            ret = BitConverter.ToInt32(value, 0);

            return true;
        }
        static internal bool ParseDouble(string value, out double ret)
        {
            if (value == "NaN")
            {
                ret = double.NaN;
                return true;
            }

            return double.TryParse(value, out ret);
        }
        static internal bool ParseDouble(byte[] value, out double ret)
        {
            ret = BitConverter.ToDouble(value, 0);

            return true;
        }
        static internal bool ParseBytes(string value, out byte[] ret, Encoding encoding = null)
        {
            if (encoding == null)
            {
                ret = Encoding.Default.GetBytes(value);
            }
            else
            {
                ret = encoding.GetBytes(value);
            }
            
            return true;
        }
        static internal bool ParseBytes(short value, out byte[] ret)
        {
            ret =  BitConverter.GetBytes(value);
            return true;
        }
        static internal bool ParseBytes(int value, out byte[] ret)
        {
            ret = BitConverter.GetBytes(value);
            return true;
        }
        static internal bool ParseBytes(float value, out byte[] ret)
        {
            ret = BitConverter.GetBytes(value);
            return true;
        }
        static internal bool ParseBytes(double value, out byte[] ret)
        {
            ret = BitConverter.GetBytes(value);
            return true;
        }
        static internal bool ParseBytes(UInt16 value, out byte[] ret)
        {
            ret = BitConverter.GetBytes(value);
            return true;
        }

        static internal object ParseArgument(string str, ParameterInfo info)
        {
            if (info.ParameterType == typeof(int))
            {
                int ret;
                if (ParseInt(str, out ret))
                    return ret;
            }
            else if (info.ParameterType == typeof(uint))
            {
                uint ret;
                if (uint.TryParse(str, out ret))
                    return ret;
            }
            else if (info.ParameterType == typeof(double))
            {
                double ret;
                if (ParseDouble(str, out ret))
                    return ret;
            }
            return null;
        }
    }
}
