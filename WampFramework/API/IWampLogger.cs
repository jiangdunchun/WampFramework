using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WampFramework.API
{
    /// <summary>
    /// log standard of wamp message
    /// </summary>
    public enum LogStandard
    {
        RECEIVED_ONLY,
        SEND_ONLY,
        ALL
    }

    /// <summary>
    /// realizing this interface, and setting it to "WampRouter.Instance.Logger", then could get the log of wamp
    /// </summary>
    public interface IWampLogger
    {
        void Log(string log);
    }
}
