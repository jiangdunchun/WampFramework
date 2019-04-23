using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WampFramework.API
{
    /// <summary>
    /// exceptions happened when wamp message is constructed or sent
    /// </summary>
    [Serializable]
    public class WampMessageException : ApplicationException
    {
        internal WampMessageException(string message)
            : base(message) { }
    }

    /// <summary>
    /// exceptions happened when wamp host is started
    /// </summary>
    [Serializable]
    public class WampHostException : ApplicationException
    {
        internal WampHostException(string message)
            : base(message) { }
    }
}
