using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WampFramework.Common;
using WampFramework.API;
using Fleck;
using WampFramework.Interfaces;

namespace WampFramework.Router
{
    class WampDealer
    {
        private Dictionary<UInt16, IWebSocketConnection> _methods = new Dictionary<UInt16, IWebSocketConnection>();

        internal Dictionary<string, IWampCallee> CalleeDic = new Dictionary<string, IWampCallee>();
        internal static readonly WampDealer Instance = new WampDealer();

        internal void Call(IWebSocketConnection socket, WampMessage data)
        {
            WampMessage ret_msg = new WampMessage();

            List<string> args = new List<string>();
            foreach (object arg in data.Args)
            {
                args.Add(arg.ToString());
            }

            //if the id is not existing
            if (!_methods.ContainsKey(data.ID))
            {
                // if entity is existing
                if (CalleeDic.ContainsKey(data.Entity))
                {
                    // call the method, and if proccess was success
                    Tuple<bool, object> call_back = CalleeDic[data.Entity].Call(data.Name, args.ToArray());

                    if (call_back.Item1)
                    {
                        // add the id and socket in method pool
                        _methods.Add(data.ID, socket);

                        ret_msg.Construct(WampProtocolHead.CAL_SUC, data.ID, data.Entity, data.Name, new object[] { call_back.Item2 });
                        ret_msg.Send(socket);

                        // remove the id and socket from method pool
                        _methods.Remove(data.ID);

                        return;
                    }
                }
            }

            ret_msg.Construct(WampProtocolHead.CAL_FAL, data.ID, data.Entity, data.Name);
            ret_msg.Send(socket);
        }
        internal async Task CallAsync(IWebSocketConnection socket, WampMessage data)
        {
            WampMessage ret_msg = new WampMessage();

            List<string> args = new List<string>();
            foreach (object arg in data.Args)
            {
                args.Add(arg.ToString());
            }

            //if the id is not existing
            if (!_methods.ContainsKey(data.ID))
            {
                // if entity is existing
                if (CalleeDic.ContainsKey(data.Entity))
                {
                    // call the method, and if proccess was success
                    Task <Tuple< bool, object>> call_task = CalleeDic[data.Entity].CallAsync(data.Name, args.ToArray());
                    call_task.Start();
                    Tuple<bool, object> call_ret = await call_task;

                    if (call_ret.Item1)
                    {
                        // add the id and socket in method pool
                        _methods.Add(data.ID, socket);

                        ret_msg.Construct(WampProtocolHead.CAL_SUC, data.ID, data.Entity, data.Name, new object[] { call_ret.Item2 });
                        ret_msg.Send(socket);

                        // remove the id and socket from method pool
                        _methods.Remove(data.ID);

                        return;
                    }
                }
            }

            ret_msg.Construct(WampProtocolHead.CAL_FAL, data.ID, data.Entity, data.Name);
            ret_msg.Send(socket);
        }
        internal void RemoveSocket(IWebSocketConnection socket)
        {
            foreach (UInt16 id in _methods.Keys)
            {
                if (_methods[id] == socket)
                {
                    _methods.Remove(id);
                }
            }
        }
        internal List<WampClassAPI> Export()
        {
            List<WampClassAPI> c_apis = new List<WampClassAPI>();

            foreach(string name in CalleeDic.Keys)
            {
                WampClassAPI c_api = new WampClassAPI(name);
                c_api.AddMethods(CalleeDic[name].ExportMethods());
                c_apis.Add(c_api);
            }

            return c_apis;
        }
    }
}
