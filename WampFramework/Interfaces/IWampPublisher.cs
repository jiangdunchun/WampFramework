using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WampFramework.Common;

namespace WampFramework.Interfaces
{
    interface IWampPublisher
    {
        /// <summary>
        /// subscribe an event in simultaneous mode
        /// </summary>
        /// <param name="eventName">event name</param>
        /// <returns>whether this subscribe is success or not</returns>
        bool Subscribe(string eventName);
        /// <summary>
        /// cancle a subscription in simultaneous mode
        /// </summary>
        /// <param name="eventName">event name</param>
        /// <returns>whether this unsubscribe is success or not</returns>
        bool Unsubscribe(string eventName);
        /// <summary>
        /// subscribe an event in asynchronous mode
        /// </summary>
        /// <param name="eventName">event name</param>
        /// <returns>whether this subscribe is success or not</returns>
        Task<bool> SubscribeAsync(string eventName);
        /// <summary>
        /// cancle a subscription in asynchronous mode
        /// </summary>
        /// <param name="eventName">event name</param>
        /// <returns>whether this unsubscribe is success or not</returns>
        Task<bool> UnsubscribeAsync(string eventName);
        /// <summary>
        /// export all events' api
        /// </summary>
        /// <returns>all WampEventAPI in a list</returns>
        List<WampEventAPI> ExportEvents();
    }
}
