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
        /// <summary>
        /// export or not
        /// </summary>
        internal readonly bool Export;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="export">export or not</param>
        public WampClassAttribute(bool export)
        {
            Export = export;
        }
    }

    /// <summary>
    /// the attribute of wamp method, not support the out or ref attributes type now
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, Inherited = true)]
    public class WampMethodAttribute : Attribute
    {
        /// <summary>
        /// export or not
        /// </summary>
        internal readonly bool Export;
        /// <summary>
        /// if this method could be invoked in any thread or not
        /// </summary>
        internal readonly bool ThreadSecurity;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="export">export or not</param>
        /// <param name="threadScurity">if this method could be invoked in any thread or not</param>
        public WampMethodAttribute(bool export, bool threadScurity = false)
        {
            Export = export;
            ThreadSecurity = threadScurity;
        }
    }

    /// <summary>
    /// the attribute of wamp event, only support WampEvent now
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Event, Inherited = true)]
    public class WampEventAttribute : Attribute
    {
        /// <summary>
        /// export or not
        /// </summary>
        internal readonly bool Export;
        /// <summary>
        /// all arguments need to be add in this list, in order to export to api file
        /// </summary>
        internal readonly List<WampArgument> Args = new List<WampArgument>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="export">export or not</param>
        /// <param name="a1Type">the type of arg_1</param>
        /// <param name="a1Name">the name of arg_1</param>
        public WampEventAttribute(bool export, Type a1Type, string a1Name)
        {
            Export = export;

            Args.Add(new WampArgument() { Type = a1Type, Name = a1Name});
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="export">export or not</param>
        /// <param name="a1Type">the type of arg_1</param>
        /// <param name="a1Name">the name of arg_1</param>
        /// <param name="a2Type">the type of arg_2</param>
        /// <param name="a2Name">the name of arg_2</param>
        public WampEventAttribute(bool export, Type a1Type, string a1Name, Type a2Type, string a2Name)
        {
            Export = export;

            Args.Add(new WampArgument() { Type = a1Type, Name = a1Name });
            Args.Add(new WampArgument() { Type = a2Type, Name = a2Name });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="export">export or not</param>
        /// <param name="a1Type">the type of arg_1</param>
        /// <param name="a1Name">the name of arg_1</param>
        /// <param name="a2Type">the type of arg_2</param>
        /// <param name="a2Name">the name of arg_2</param>
        /// <param name="a3Type">the type of arg_3</param>
        /// <param name="a3Name">the name of arg_3</param>
        public WampEventAttribute(bool export, Type a1Type, string a1Name, Type a2Type, string a2Name, Type a3Type, string a3Name)
        {
            Export = export;

            Args.Add(new WampArgument() { Type = a1Type, Name = a1Name });
            Args.Add(new WampArgument() { Type = a2Type, Name = a2Name });
            Args.Add(new WampArgument() { Type = a3Type, Name = a3Name });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="export">export or not</param>
        /// <param name="a1Type">the type of arg_1</param>
        /// <param name="a1Name">the name of arg_1</param>
        /// <param name="a2Type">the type of arg_2</param>
        /// <param name="a2Name">the name of arg_2</param>
        /// <param name="a3Type">the type of arg_3</param>
        /// <param name="a3Name">the name of arg_3</param>
        /// <param name="a4Type">the type of arg_4</param>
        /// <param name="a4Name">the name of arg_4</param>
        public WampEventAttribute(bool export, Type a1Type, string a1Name, Type a2Type, string a2Name, Type a3Type, string a3Name, Type a4Type, string a4Name)
        {
            Export = export;

            Args.Add(new WampArgument() { Type = a1Type, Name = a1Name });
            Args.Add(new WampArgument() { Type = a2Type, Name = a2Name });
            Args.Add(new WampArgument() { Type = a3Type, Name = a3Name });
            Args.Add(new WampArgument() { Type = a4Type, Name = a4Name });
        }
    }

    internal struct WampArgument
    {
        public string Name;
        public Type Type;
    }
}
