using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WampFramework.API
{
    /// <summary>
    /// all wamp events need to use this delegate type, otherwise this event will not sopport the wamp subscribe
    /// there is an example here:
    ///   define an event   +-------------------------------------------------------------------------------+
    ///                     |   [WampEventAttribute(true, typeof(int), "val1", , typeof(float), "val2")]    |
    ///                     |   public event WampEvent TestEvent = null;                                    |
    ///                     +-------------------------------------------------------------------------------+
    ///   invoke this event +-------------------------------------------------------------------------------+
    ///                     |   int v1 = 0; float v2 = 1;                                                   |
    ///                     |   if (ValuedChanged != null)                                                  |
    ///                     |   {                                                                           |
    ///                     |       object[] ret = { v1, v2 };                                              |
    ///                     |       TestEvent("TestEvent", ret);                                            |
    ///                     |   }                                                                           |
    ///                     +-------------------------------------------------------------------------------+
    /// </summary>
    /// <param name="sender">event name</param>
    /// <param name="agrs">the arguments need to be tranfered out in this event</param>
    public delegate void WampEvent(string sender, object[] agrs);

    /// <summary>
    /// all json type need inhereted from this interface
    /// </summary>
    public interface IWampJson
    {
        /// <summary>
        /// get json from this class
        /// </summary>
        /// <returns>string content</returns>
        string ToJson();
        /// <summary>
        /// contrcut this class from json data
        /// </summary>
        /// <returns>success or not</returns>
        bool Construct(string json);
    }
}
