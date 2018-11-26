using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WampFramework.API
{
    /// <summary>
    /// realizing this interface, and setting it to "WampRouter.Instance.Logger", then could get the log of wamp
    /// </summary>
    public interface IWampLogger
    {
        void Log(string log);
    }
}
