﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WampFramework.API;
using WampFramework.Common;
using WampFramework.Interfaces;

namespace WampFramework.Router
{
    // resolve Pub&Sub(Publish and Subscribe) message
    class WampBroker
    {
        struct SubeventInfo
        {
            public string Entity;
            public string Event;
        }

        internal static readonly WampBroker Instance = new WampBroker();
        private WampBroker() { }

        private Dictionary<SubeventInfo, Dictionary<ushort, WampClient>> _events = new Dictionary<SubeventInfo, Dictionary<UInt16, WampClient>>();

        internal Dictionary<string, IWampPublisher> PublisherDic = new Dictionary<string, IWampPublisher>();

        internal void EventInvoked(string pubName, string eventName, object[] args)
        {
            SubeventInfo e_inf = new SubeventInfo()
            {
                Entity = pubName,
                Event = eventName
            };

            WampMessage ret_msg = new WampMessage();

            if (!_events.ContainsKey(e_inf)) return;

            foreach (ushort id in _events[e_inf].Keys)
            {
                ret_msg.Construct(WampProtocolHead.SBS_BCK, id, e_inf.Entity, e_inf.Event, args);
                ret_msg.Send(_events[e_inf][id]);
            }
        }
        internal void Subscribe(WampClient socket, WampMessage data)
        {
            SubeventInfo e_inf = new SubeventInfo()
            {
                Entity = data.Entity,
                Event = data.Name
            };

            WampMessage ret_msg = new WampMessage();

            // if the id is not existing
            if (!_events.ContainsKey(e_inf) || (_events.ContainsKey(e_inf) && !_events[e_inf].ContainsKey(data.ID)))
            {
                // if entity is existing
                if (PublisherDic.ContainsKey(data.Entity))
                {
                    // if this is the first subscribe of this event
                    if (!_events.ContainsKey(e_inf))
                    {
                        // add a delegate, and if adding proccess was success
                        if (PublisherDic[data.Entity].Subscribe(data.Name))
                        {
                            // add new event type in event pool
                            _events.Add(e_inf, new Dictionary<ushort, WampClient>());
                        }
                        // if adding proccess was failed 
                        else
                        {
                            ret_msg.Construct(WampProtocolHead.SUB_FAL, data.ID, data.Entity, data.Name);
                            ret_msg.Send(socket);
                            return;
                        }
                    }

                    // add this subscribe in event pool
                    _events[e_inf].Add(data.ID, socket);

                    ret_msg.Construct(WampProtocolHead.SBS_SUC, data.ID, data.Entity, data.Name);
                    ret_msg.Send(socket);
                    return;
                }
            }

            ret_msg.Construct(WampProtocolHead.SUB_FAL, data.ID, data.Entity, data.Name);
            ret_msg.Send(socket);
        }
        internal void Unsubscribe(WampClient socket, WampMessage data)
        {
            SubeventInfo e_inf = new SubeventInfo()
            {
                Entity = data.Entity,
                Event = data.Name
            };

            WampMessage ret_msg = new WampMessage();

            // if this event type exist in event pool
            if (_events.ContainsKey(e_inf))
            {
                // if this subscribe id exist in event pool
                if (_events[e_inf].ContainsKey(data.ID))
                {
                    // if this subscribe socket exist in event pool
                    if (socket == _events[e_inf][data.ID])
                    {
                        // remove this subscribe from event pool
                        _events[e_inf].Remove(data.ID);

                        // if this event type has no subscribe
                        if (_events[e_inf].Count == 0)
                        {
                            // remove the delegate, and if removement proccess was success
                            if (PublisherDic[data.Entity].Unsubscribe(data.Name))
                            {
                                _events.Remove(e_inf);
                            }
                        }

                        ret_msg.Construct(WampProtocolHead.UNSBS_SUC, data.ID, data.Entity, data.Name);
                        ret_msg.Send(socket);

                        return;
                    }
                }
            }

            ret_msg.Construct(WampProtocolHead.UNSBS_FAL, data.ID, data.Entity, data.Name);
            ret_msg.Send(socket);
        }
        internal void RemoveSocket(WampClient socket)
        {
            for (int i = _events.Keys.Count; i > 0; i--)
            {
                SubeventInfo e_inf = _events.Keys.ElementAt(i - 1);

                for (int j = _events[e_inf].Keys.Count; j > 0; j--)
                {
                    ushort id = _events[e_inf].Keys.ElementAt(j - 1);

                    if (_events[e_inf][id] == socket)
                    {
                        _events[e_inf].Remove(id);
                    }
                }
            }

            for (int i = _events.Keys.Count; i > 0; i--)
            {
                SubeventInfo e_inf = _events.Keys.ElementAt(i - 1);

                if (_events[e_inf].Count == 0)
                {
                    // remove the delegate, and if removement proccess was success
                    if (PublisherDic[e_inf.Entity].Unsubscribe(e_inf.Event))
                    {
                        _events.Remove(e_inf);
                    }
                }
            }
        }
        internal List<WampClassAPI> Export()
        {
            List<WampClassAPI> c_apis = new List<WampClassAPI>();

            foreach (string name in PublisherDic.Keys)
            {
                WampClassAPI c_api = new WampClassAPI(name);
                c_api.AddEvents(PublisherDic[name].ExportEvents());
                c_apis.Add(c_api);
            }

            return c_apis;
        }
    }
}
