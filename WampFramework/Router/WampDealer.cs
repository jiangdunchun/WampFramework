using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WampFramework.Common;
using WampFramework.API;
using WampFramework.Interfaces;

namespace WampFramework.Router
{
    // resolve RPC(Remote Procedure Call) message
    class WampDealer
    {
        internal static readonly WampDealer Instance = new WampDealer();
        private WampDealer() { }

        private Dictionary<ushort, WampClient> _methods = new Dictionary<ushort, WampClient>();

        internal Dictionary<string, IWampCallee> CalleeDic = new Dictionary<string, IWampCallee>();

        internal void Call(WampClient socket, WampMessage data)
        {
            WampMessage ret_msg = new WampMessage();

            List<object> args = data.Args.ToList();

            //if the id is not existing
            if (!_methods.ContainsKey(data.ID))
            {
                // if entity is existing
                if (CalleeDic.ContainsKey(data.Entity))
                {
                    // add the id and socket in method pool
                    _methods.Add(data.ID, socket);

                    // call the method, and if proccess was success
                    Tuple<bool, object> call_back = CalleeDic[data.Entity].Call(data.Name, args.ToArray());

                    // remove the id and socket from method pool
                    _methods.Remove(data.ID);

                    if (call_back.Item1)
                    {
                        ret_msg.Construct(WampProtocolHead.CAL_SUC, data.ID, data.Entity, data.Name, new object[] { call_back.Item2 });
                        ret_msg.Send(socket);

                        return;
                    }
                }
            }

            ret_msg.Construct(WampProtocolHead.CAL_FAL, data.ID, data.Entity, data.Name);
            ret_msg.Send(socket);
        }
        internal async Task CallAsync(WampClient socket, WampMessage data)
        {
            WampMessage ret_msg = new WampMessage();

            List<object> args = data.Args.ToList();

            //if the id is not existing
            if (!_methods.ContainsKey(data.ID))
            {
                // if entity is existing
                if (CalleeDic.ContainsKey(data.Entity))
                {
                    // add the id and socket in method pool
                    _methods.Add(data.ID, socket);

                    // call the method, and if proccess was success
                    Task<Tuple<bool, object>> call_task = CalleeDic[data.Entity].CallAsync(data.Name, args.ToArray());
                    call_task.Start();
                    Tuple<bool, object> call_ret = await call_task;

                    // remove the id and socket from method pool
                    _methods.Remove(data.ID);

                    if (call_ret.Item1)
                    {
                        ret_msg.Construct(WampProtocolHead.CAL_SUC, data.ID, data.Entity, data.Name, new object[] { call_ret.Item2 });
                        ret_msg.Send(socket);

                        return;
                    }
                }
            }

            ret_msg.Construct(WampProtocolHead.CAL_FAL, data.ID, data.Entity, data.Name);
            ret_msg.Send(socket);
        }
        internal void RemoveSocket(WampClient socket)
        {
            for (int i = _methods.Keys.Count; i > 0; i--)
            {
                ushort id = _methods.Keys.ElementAt(i-1);

                if (_methods[id] == socket)
                {
                    _methods.Remove(id);
                }
            }
        }
        internal List<WampClassAPI> Export()
        {
            List<WampClassAPI> c_apis = new List<WampClassAPI>();

            foreach (string name in CalleeDic.Keys)
            {
                WampClassAPI c_api = new WampClassAPI(name);
                c_api.AddMethods(CalleeDic[name].ExportMethods());
                c_apis.Add(c_api);
            }

            return c_apis;
        }
    }
}
