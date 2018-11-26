using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WampFramework.API
{
    /// <summary>
    /// the attribute of wamp class
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = true)]
    public class WampClassAttribute : Attribute
    {
        private bool _export;
        /// <summary>
        /// export or not
        /// </summary>
        public bool Export { get { return _export; } }

        public WampClassAttribute(bool export)
        {
            _export = export;
        }
    }

    /// <summary>
    /// the attribute of wamp method, not support the out or ref attributes type now
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = true)]
    public class WampMethodAttribute : Attribute
    {
        private bool _export;
        /// <summary>
        /// export or not
        /// </summary>
        public bool Export { get { return _export; } }

        public WampMethodAttribute(bool export)
        {
            _export = export;
        }
    }

    /// <summary>
    /// the attribute of wamp event
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Event, Inherited = true)]
    public class WampEventAttribute : Attribute
    {
        private bool _export;
        private List<WampArgument> _args = new List<WampArgument>();
        /// <summary>
        /// export or not
        /// </summary>
        public bool Export { get { return _export; } }
        /// <summary>
        /// all arguments need to be add in this list, in order to export to api file
        /// </summary>
        public List<WampArgument> Args { get { return _args; } }

        public WampEventAttribute(bool export, Type a1Type, string a1Name)
        {
            _export = export;

            _args.Add(new WampArgument(a1Type, a1Name));
        }
        public WampEventAttribute(bool export, Type a1Type, string a1Name, Type a2Type, string a2Name)
        {
            _export = export;

            _args.Add(new WampArgument(a1Type, a1Name));
            _args.Add(new WampArgument(a2Type, a2Name));
        }
        public WampEventAttribute(bool export, Type a1Type, string a1Name, Type a2Type, string a2Name, Type a3Type, string a3Name)
        {
            _export = export;

            _args.Add(new WampArgument(a1Type, a1Name));
            _args.Add(new WampArgument(a2Type, a2Name));
            _args.Add(new WampArgument(a3Type, a3Name));
        }
        public WampEventAttribute(bool export, Type a1Type, string a1Name, Type a2Type, string a2Name, Type a3Type, string a3Name, Type a4Type, string a4Name)
        {
            _export = export;

            _args.Add(new WampArgument(a1Type, a1Name));
            _args.Add(new WampArgument(a2Type, a2Name));
            _args.Add(new WampArgument(a3Type, a3Name));
            _args.Add(new WampArgument(a4Type, a4Name));
        }
    }

    /// <summary>
    /// event argument description
    /// </summary>
    public class WampArgument
    {
        public string Name;
        public Type Type;
        public WampArgument(Type type, string name)
        {
            Type = type;
            Name = name;
        }
    }

    /// <summary>
    /// all wamp events need to use this delegate type, otherwise this event will not sopport the wamp subscribe
    /// </summary>
    /// <param name="sender">event name</param>
    /// <param name="agrs">the arguments need to tranfer by this event</param>
    public delegate void WampEvent(string sender, object[] agrs);
}
