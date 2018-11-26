using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WampFramework.API;

namespace WampFramework.Common
{
    static class WampSetting
    {
        static private bool _isByteMode = false;
        static private bool _isAsyncMode = true;
        static private bool _isServerOpen = false;
        static private string _serverAddress = "ws://0.0.0.0:9527";
        static private IWampLogger _logger = null;

        static public bool IsByteMode
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
        static public bool IsStringMode
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
        static public bool IsAsyncMode
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
        static public bool IsSimulMode
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
        static public bool IsOpened
        {
            set
            {
                _isServerOpen = value;
            }
            get
            {
                return _isServerOpen;
            }
        }
        static public string Location
        {
            set
            {
                _serverAddress = value;
            }
            get
            {
                return _serverAddress;
            }
        }
        static public IWampLogger Logger
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
    }
}
