using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using WampFramework.API;
using WampFramework.Common;
using WampFramework.Interfaces;
using WampFramework.Router;

namespace WampFramework.Local
{
    class WampLocalCalleePublisher:IWampCallee,IWampPublisher
    {
        private object _instance;
        private Type _type;
        private string _name;
        private Dictionary<string, EventInfo> _events = new Dictionary<string, EventInfo>();
        private Dictionary<string, MethodInfo> _methods = new Dictionary<string, MethodInfo>();

        public string Name { get { return _name; } }

        public WampLocalCalleePublisher(Type type, string name)
        {
            _type = type;
            _name = name;

            EventInfo[] e_infs = type.GetEvents(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);

            foreach (EventInfo e_inf in e_infs)
            {
                object[] objs = e_inf.GetCustomAttributes(typeof(WampEventAttribute), true);
                foreach (object obj in objs)
                {
                    WampEventAttribute attr = obj as WampEventAttribute;
                    if (attr != null && attr.Export && e_inf.EventHandlerType == typeof(WampEvent))
                    {
                        List<Type> arg_types = new List<Type>();
                        foreach (WampArgument arg in attr.Args)
                        {
                            arg_types.Add(arg.Type);
                        }

                        if (WampProperties.IsSupportType(arg_types))
                        {
                            _events.Add(e_inf.Name, e_inf);

                            break;
                        }
                    }
                }
            }

            MethodInfo[] m_infs = type.GetMethods(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);

            foreach (MethodInfo m_inf in m_infs)
            {
                object[] objs = m_inf.GetCustomAttributes(typeof(WampMethodAttribute), true);
                foreach (object obj in objs)
                {
                    WampMethodAttribute attr = obj as WampMethodAttribute;
                    if (attr != null && attr.Export)
                    {
                        List<Type> arg_types = new List<Type>
                        {
                            m_inf.ReturnType
                        };
                        arg_types.AddRange(m_inf.GetGenericArguments());

                        if (WampProperties.IsSupportType(arg_types))
                        {
                            _methods.Add(m_inf.Name, m_inf);

                            break;
                        }
                    }
                }
            }
        }

        public void EventInvoked(string eventName, object[] args)
        {
            WampBroker.Instance.EventInvoked(Name, eventName, args);
        }
        public Tuple<bool, object> Call(string methodName, object[] parameters)
        {
            object ret = null;

            if (_methods.ContainsKey(methodName))
            {
                try
                {
                    MethodInfo m_inf = _methods[methodName];
                    if (_instance == null)
                    {
                        ConstructorInfo cst_inf = _type.GetConstructor(System.Type.EmptyTypes);
                        _instance = cst_inf.Invoke(null);
                    }

                    ParameterInfo[] p_infs = m_inf.GetParameters();
                    List<object> args = new List<object>();

                    for (int i = 0; i < p_infs.Length; i++)
                    {
                        object arg = WampValueHelper.GetArgument(parameters[i], p_infs[i]);
                        if (arg == null)
                        {
                            ret = null;
                            return new Tuple<bool, object>(false, ret);
                        }
                        args.Add(arg);
                    }

                    ret = m_inf.Invoke(_instance, args.ToArray());

                    return new Tuple<bool, object>(true, ret);
                }
                catch(Exception)
                {
                    ret = null;
                    return new Tuple<bool, object>(false, ret);
                }
            }
            else
            {
                ret = null;
                return new Tuple<bool, object>(false, ret);
            }
        }
        public bool Subscribe(string eventName)
        {
            if (_events.ContainsKey(eventName))
            {
                try
                {
                    EventInfo e_inf = _events[eventName];

                    if (_instance == null)
                    {
                        ConstructorInfo cst_inf = _type.GetConstructor(System.Type.EmptyTypes);
                        _instance = cst_inf.Invoke(null);
                    }

                    MethodInfo m_inf = (this.GetType()).GetMethod(nameof(EventInvoked));
                    // @to do, throw exception when the type is not EventHandler
                    Delegate dlg = Delegate.CreateDelegate(e_inf.EventHandlerType, this, m_inf);
                    e_inf.AddEventHandler(_instance, dlg);
                }
                catch(Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public bool Unsubscribe(string eventName)
        {
            if (_events.ContainsKey(eventName))
            {
                try
                {
                    EventInfo e_inf = _events[eventName];

                    if (_instance == null) return false;

                    MethodInfo m_inf = (this.GetType()).GetMethod(nameof(EventInvoked));
                    // throw exception when the type is not EventHandler
                    Delegate dlg = Delegate.CreateDelegate(e_inf.EventHandlerType, this, m_inf);
                    e_inf.RemoveEventHandler(_instance, dlg);
                }
                catch(Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
            return true;
        }
        public Task<Tuple<bool, object>> CallAsync(string methodName, object[] parameters)
        {
            return new Task<Tuple<bool, object>>(() => Call(methodName, parameters));
        }
        public Task<bool> SubscribeAsync(string eventName)
        {
            return new Task<bool>(() => Subscribe(eventName));
        }
        public Task<bool> UnsubscribeAsync(string eventName)
        {
            return new Task<bool>(() => Unsubscribe(eventName));
        }
        public List<WampMethodAPI> ExportMethods()
        {
            List<WampMethodAPI> m_apis = new List<WampMethodAPI>();

            foreach(string m_name in _methods.Keys)
            {
                Type ret_type = _methods[m_name].ReturnType;
                WampMethodAPI m_api = new WampMethodAPI(m_name, ret_type);
                ParameterInfo[] p_infs = _methods[m_name].GetParameters();
                foreach(ParameterInfo p_inf in p_infs)
                {
                    m_api.AddArgument(p_inf.Name, p_inf.ParameterType);
                }

                m_apis.Add(m_api);
            }

            return m_apis;
        }
        public List<WampEventAPI> ExportEvents()
        {
            List<WampEventAPI> e_apis = new List<WampEventAPI>();

            foreach(string e_name in _events.Keys)
            {
                WampEventAPI e_api = new WampEventAPI(e_name);
                // handle the event api
                object[] objs = _events[e_name].GetCustomAttributes(typeof(WampEventAttribute), true);
                foreach (object obj in objs)
                {
                    WampEventAttribute attr = obj as WampEventAttribute;
                    if (attr != null && attr.Export)
                    {
                        foreach (WampArgument arg in attr.Args)
                        {
                            e_api.AddArgument(arg.Name, arg.Type);
                        }
                        break;
                    }
                }
                e_apis.Add(e_api);
            }

            return e_apis;
        }
    }
}
