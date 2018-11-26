using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WampFramework.Common;

namespace WampFramework.Interfaces
{
    interface IWampCallee
    {
        /// <summary>
        /// call a method in simultaneous mode
        /// </summary>
        /// <param name="methodName">method name</param>
        /// <param name="parameters">all parameters in a array</param>
        /// <returns>Item1 represents the whether this call is success or not,Item2 represents the return object</returns>
        Tuple<bool, object> Call(string methodName, string[] parameters);
        /// <summary>
        /// call a method in asynchronous mode
        /// </summary>
        /// <param name="methodName">method name</param>
        /// <param name="parameters">all parameters in a array</param>
        /// <returns>Item1 represents the whether this call is success or not,Item2 represents the return object</returns>
        Task<Tuple<bool, object>> CallAsync(string methodName, string[] parameters);
        /// <summary>
        /// export all methods' api
        /// </summary>
        /// <returns>all WampMethodAPI in a list</returns>
        List<WampMethodAPI> ExportMethods();
    }
}
