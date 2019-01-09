using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WampFramework.API
{
    /// <summary>
    /// all wamp events need to use this delegate type, otherwise this event will not sopport the wamp subscribe
    /// </summary>
    /// <param name="sender">event name</param>
    /// <param name="agrs">the arguments need to tranfer by this event</param>
    public delegate void WampEvent(string sender, object[] agrs);

    /// <summary>
    /// all json type need inhereted from this interface
    /// </summary>
    public interface IWampJsonData
    {
        /// <summary>
        /// get json from this class
        /// </summary>
        /// <returns>string content</returns>
        string ToJson();
        /// <summary>
        /// contrcut this class from json data
        /// </summary>
        /// <returns></returns>
        bool Construct(string json);
    }
}
