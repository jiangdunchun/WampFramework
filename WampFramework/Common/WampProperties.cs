using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WampFramework.API;

namespace WampFramework.Common
{
    static class WampProperties
    {
        static private bool _needReversed = false;
        static private bool _isLittleEndian = BitConverter.IsLittleEndian;
        static private Encoding _strEncoding = Encoding.Default;
        static private bool _isByteMode = false;
        static private bool _isThreadSecurity = true;
        static private bool _isAsyncMode = true;
        static private IWampLogger _logger = null;
        static private bool _logReceived = false;
        static private bool _logSend = false;
        static private List<Type> _supportTypes = new List<Type>() {
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
        static private List<Type> _supportInterface = new List<Type>() {
            typeof(IWampJsonData),
            typeof(IWampJsonData[]),
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
        static internal bool IsBigEndian
        {
            set
            {
                _isLittleEndian = !value;

                _needReversed = BitConverter.IsLittleEndian != _isLittleEndian;
            }
            get
            {
                return !_isLittleEndian;
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
        static internal bool IsStringMode
        {
            set
            {
                _isByteMode = !value;
            }
            get
            {
                return !_isByteMode;
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
        static internal bool IsSimulMode
        {
            set
            {
                _isAsyncMode = !value;
            }
            get
            {
                return !_isAsyncMode;
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
        static internal bool LogReceived
        {
            set
            {
                _logReceived = value;
            }
            get
            {
                return _logReceived;
            }
        }
        static internal bool LogSend
        {
            set
            {
                _logSend = value;
            }
            get
            {
                return _logSend;
            }
        }

        static internal bool IsSupportType(List<Type> types)
        {
            foreach (Type type in types)
            {
                if (_supportTypes.Contains(type))
                {
                    return true;
                }
                foreach (Type itfc in _supportInterface)
                {
                    if (itfc.IsAssignableFrom(type))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
