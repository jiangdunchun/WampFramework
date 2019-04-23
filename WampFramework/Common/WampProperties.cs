using System;
using System.Collections.Generic;
using System.Text;
using WampFramework.API;

namespace WampFramework.Common
{
    static class WampProperties
    {
        static private bool _needReversed = !BitConverter.IsLittleEndian;
        // True in defualt
        static private bool _isLittleEndian = true;
        // UTF8 in default
        static private Encoding _strEncoding = Encoding.UTF8;
        // string in default
        static private bool _isByteMode = false;
        // security in default
        static private bool _isThreadSecurity = true;
        // async in default
        static private bool _isAsyncMode = true;
        static private IWampLogger _logger = null;
        static private LogOption _loggerOptions = LogOption.NONE;
        static private List<Type> _supportedTypes = new List<Type>() {
            typeof(void),
            typeof(string),
            typeof(byte),
            typeof(bool),
            typeof(ushort),
            typeof(short),
            typeof(int),
            typeof(float),
            typeof(double),
            typeof(string[]),
            typeof(byte[]),
            typeof(bool[]),
            typeof(ushort[]),
            typeof(short[]),
            typeof(int[]),
            typeof(float[]),
            typeof(double[]),
        };
        static private List<Type> _supportedInterfaces = new List<Type>() {
            typeof(IWampJson),
            typeof(IWampJson[]),
        };

        static internal bool NeedReversed
        {
            get
            {
                return _needReversed;
            }
        }
        static internal bool IsLittleEndian
        {
            set
            {
                _isLittleEndian = value;

                _needReversed = BitConverter.IsLittleEndian != _isLittleEndian;
            }
            get
            {
                return _isLittleEndian;
            }
        }
        static internal Encoding StrEncoding
        {
            get
            {
                return _strEncoding;
            }
            set
            {
                _strEncoding = value;
            }
        }
        static internal bool IsByteMode
        {
            set
            {
                _isByteMode = value;
            }
            get
            {
                return _isByteMode;
            }
        }
        static internal bool IsThreadSecurity
        {
            set
            {
                _isThreadSecurity = value;
                if (!value)
                {
                    _isAsyncMode = false;
                }
            }
            get
            {
                return _isThreadSecurity;
            }
        }
        static internal bool IsAsyncMode
        {
            set
            {
                _isAsyncMode = value;
            }
            get
            {
                return _isAsyncMode;
            }
        }
        static internal IWampLogger Logger
        {
            set
            {
                _logger = value;
            }
            get
            {
                return _logger;
            }
        }
        static internal LogOption LoggerOptions
        {
            set
            {
                _loggerOptions = value;
            }
            get
            {
                return _loggerOptions;
            }
        }

        static internal bool IsSupportedType(List<Type> types)
        {
            foreach (Type type in types)
            {
                if (_supportedTypes.Contains(type))
                {
                    return true;
                }
                foreach (Type itfc in _supportedInterfaces)
                {
                    if (itfc.IsAssignableFrom(type))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        static internal string GetArgType(object obj)
        {
            if (obj != null)
            {
                if (_supportedTypes.Contains(obj.GetType())) return obj.GetType().Name;
                foreach (Type itfc in _supportedInterfaces)
                {
                    if (itfc.IsAssignableFrom(obj.GetType())) return itfc.Name;
                }
            }
            return "null";
        }
    }
}
