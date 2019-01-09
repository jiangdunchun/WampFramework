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
        internal bool Export { get { return _export; } }

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
        private bool _threadSecurity;
        /// <summary>
        /// export or not
        /// </summary>
        internal bool Export { get { return _export; } }
        /// <summary>
        /// thread security or not
        /// </summary>
        internal bool ThreadSecurity { get { return _threadSecurity; } }

        public WampMethodAttribute(bool export, bool threadScurity = false)
        {
            _export = export;
            _threadSecurity = threadScurity;
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
        internal bool Export { get { return _export; } }
        /// <summary>
        /// all arguments need to be add in this list, in order to export to api file
        /// </summary>
        internal List<WampArgument> Args { get { return _args; } }

        public WampEventAttribute(bool export, Type a1Type, string a1Name)
        {
            _export = export;

            _args.Add(new WampArgument() { Type = a1Type, Name = a1Name});
        }
        public WampEventAttribute(bool export, Type a1Type, string a1Name, Type a2Type, string a2Name)
        {
            _export = export;

            _args.Add(new WampArgument() { Type = a1Type, Name = a1Name });
            _args.Add(new WampArgument() { Type = a2Type, Name = a2Name });
        }
        public WampEventAttribute(bool export, Type a1Type, string a1Name, Type a2Type, string a2Name, Type a3Type, string a3Name)
        {
            _export = export;

            _args.Add(new WampArgument() { Type = a1Type, Name = a1Name });
            _args.Add(new WampArgument() { Type = a2Type, Name = a2Name });
            _args.Add(new WampArgument() { Type = a3Type, Name = a3Name });
        }
        public WampEventAttribute(bool export, Type a1Type, string a1Name, Type a2Type, string a2Name, Type a3Type, string a3Name, Type a4Type, string a4Name)
        {
            _export = export;

            _args.Add(new WampArgument() { Type = a1Type, Name = a1Name });
            _args.Add(new WampArgument() { Type = a2Type, Name = a2Name });
            _args.Add(new WampArgument() { Type = a3Type, Name = a3Name });
            _args.Add(new WampArgument() { Type = a4Type, Name = a4Name });
        }
    }

    internal struct WampArgument
    {
        public string Name;
        public Type Type;
    }
}
