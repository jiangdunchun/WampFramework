using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WampFramework.API
{
    /// <summary>
    /// log options of wamp message
    /// </summary>
    public enum LogOption
    {
        /// <summary>
        /// default mode, only log some essential message
        /// </summary>
        NONE = 0,
        /// <summary>
        /// log received message 
        /// </summary>
        RECEIVED = 1 << 0,
        /// <summary>
        /// log sent message
        /// </summary>
        SENT = 1 << 1,
        /// <summary>
        /// log time diagnose
        /// </summary>
        TIME_DIAG = 1 << 2
    }

    /// <summary>
    /// realizing this interface, and setting it to "WampRouter.Instance.Logger", then could get the log of wamp
    /// </summary>
    public interface IWampLogger
    {
        /// <summary>
        /// Log message in wamp
        /// </summary>
        /// <param name="log">log content</param>
        void Log(string log);
    }
}
