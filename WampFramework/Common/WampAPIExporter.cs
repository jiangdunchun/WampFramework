using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WampFramework.Common
{
    struct WampArgumentAPI
    {
        public string Name;
        public string Type;
        public string Description;
    }

    struct WampMethodAPI
    {
        public string Name;
        public string ReturnType;
        public List<WampArgumentAPI> Args;
        public string Description;

        public WampMethodAPI(string name, Type returnType, string description = "")
        {
            Name = name;
            ReturnType = returnType.Name;
            Args = new List<WampArgumentAPI>();
            Description = description;
        }
        public void AddArgument(string name, Type type, string description = "")
        {
            WampArgumentAPI arg = new WampArgumentAPI()
            {
                Name = name,
                Type = type.Name,
                Description = description
            };
            Args.Add(arg);
        }
    }

    struct WampEventAPI
    {
        public string Name;
        public List<WampArgumentAPI> Args;
        public string Description;

        public WampEventAPI(string name, string description = "")
        {
            Name = name;
            Args = new List<WampArgumentAPI>();
            Description = description;
        }
        public void AddArgument(string name, Type type, string description = "")
        {
            WampArgumentAPI arg = new WampArgumentAPI()
            {
                Name = name,
                Type = type.Name,
                Description = description
            };
            Args.Add(arg);
        }
    }

    struct WampClassAPI
    {
        public string Name;
        public List<WampMethodAPI> Methods;
        public List<WampEventAPI> Events;
        public string Description;

        public WampClassAPI(string name, string description = "")
        {
            Name = name;
            Methods = new List<WampMethodAPI>();
            Events = new List<WampEventAPI>();
            Description = description;
        }
        public void AddEvents(List<WampEventAPI> es)
        {
            Events.AddRange(es);
        }
        public void AddMethods(List<WampMethodAPI> ms)
        {
            Methods.AddRange(ms);
        }
    }

    class WampAPIWriter
    {
        const string ClassNote = "/// WAMP module {0}, support by VRMaker(www.vrmaker.com.cn)";
        const string MethodNote = "// description: {0}\n// callback: {1}\n// arguments: {2}";
        const string EventNote = "// description: {0}\n// callback arguments: {1}";
        const string ClassBody = "var {0} = {\n{1}};\n\nexport default {0};";
        const string MethodBody = "{0} : function ({1}callback){\n{2}\n},";
        const string EventBody = "{0} : function (callback){\n{1}\n},";

        private string _path;
        private Dictionary<string, WampClassAPI> _cAPIs = new Dictionary<string, WampClassAPI>();

        private string _getClassNote(WampClassAPI cAPI)
        {
            string ret_str = ClassNote.Replace("{0}", cAPI.Name);
            return ret_str;
        }
        private string _getMethodNote(WampMethodAPI mAPI)
        {
            string ret_str = MethodNote.Replace("{0}", mAPI.Description);

            ret_str = ret_str.Replace("{1}", mAPI.ReturnType);

            string args_str = "";
            foreach(WampArgumentAPI a_api in mAPI.Args)
            {
                args_str += string.Format("{0}:{1} ", a_api.Name, a_api.Type);
            }
            ret_str = ret_str.Replace("{2}", args_str);

            return ret_str;
        }
        private string _getEventNote(WampEventAPI eAPI)
        {
            string ret_str = EventNote.Replace("{0}", eAPI.Description);

            string args_str = "";
            foreach (WampArgumentAPI a_api in eAPI.Args)
            {
                args_str += string.Format("{0}:{1} ", a_api.Name, a_api.Type);
            }
            ret_str = ret_str.Replace("{1}", args_str);

            return ret_str;
        }
        private string _getClassBody(WampClassAPI cAPI)
        {
            string req_str = "const _socket = require('./WampConfig.js');\nconst WampCommon = require('./WampCommon.js')(_socket); ";
            string ret_str = ClassBody.Replace("{0}", cAPI.Name);

            string met_str = "";
            foreach(WampMethodAPI m_api in cAPI.Methods)
            {
                met_str += _getMethodBody(m_api, cAPI.Name) + "\n";
            }
            string eve_str = "";
            foreach (WampEventAPI e_api in cAPI.Events)
            {
                eve_str += _getEventBody(e_api, cAPI.Name) + "\n";
            }

            ret_str = ret_str.Replace("{1}", met_str + eve_str);

            string note = _getClassNote(cAPI);

            return note + "\n\n" + req_str + "\n\n" + ret_str;
        }
        private string _getMethodBody(WampMethodAPI mAPI, string cName)
        {
            string ret_str = MethodBody.Replace("{0}", mAPI.Name);

            string args_str = "";
            foreach (WampArgumentAPI a_api in mAPI.Args)
            {
                args_str += string.Format("{0},", a_api.Name);
            }
            ret_str = ret_str.Replace("{1}", args_str);

            string met_body = "";
            if (!string.IsNullOrEmpty(args_str))
            {
                met_body += string.Format("var args = new Array({0});", args_str.Substring(0, args_str.Length - 1)); 
            }
            else
            {
                met_body += "var args = new Array();";
            }
            met_body += string.Format("\nWampCommon.WampCall('{0}','{1}',args,callback);", cName, mAPI.Name);

            ret_str = ret_str.Replace("{2}", met_body);

            string note = _getMethodNote(mAPI);
            return note + "\n" + ret_str;
        }
        private string _getEventBody(WampEventAPI eAPI, string cName)
        {
            string ret_str = EventBody.Replace("{0}", eAPI.Name);

            // @ to do, replace {1}, event body 
            string sube_body = string.Format("WampCommon.WampSubscribe('{0}','{1}',callback);", cName, eAPI.Name);
            string unsube_body = string.Format("WampCommon.WampUnsubscribe('{0}','{1}',callback);", cName, eAPI.Name);
            string add_str = ret_str.Replace("{1}", sube_body);
            string del_str = ret_str.Replace("{1}", unsube_body);

            string note = _getEventNote(eAPI);

            return note + "\n" + "Add" + add_str + "\n" + note + "\n" + "Delete" + del_str;
        }
        private void _writeClass(string className)
        {
            string cla_path = string.Format("{0}/Wamp{1}.js", _path, className);

            WampClassAPI c_api = _cAPIs[className];

            string cal_body = _getClassBody(c_api);

            StreamWriter sw = new StreamWriter(cla_path, false, System.Text.Encoding.Default);
            sw.WriteLine(cal_body);
            sw.Close();
        }

        internal WampAPIWriter(string path)
        {
            _path = path;
        }

        internal void Add(WampClassAPI cAPI)
        {
            if (!_cAPIs.ContainsKey(cAPI.Name))
            {
                _cAPIs.Add(cAPI.Name, cAPI);
            }
            else
            {
                _cAPIs[cAPI.Name].AddEvents(cAPI.Events);
                _cAPIs[cAPI.Name].AddMethods(cAPI.Methods);
            }
        }
        internal void Add(List<WampClassAPI> cAPIs)
        {
            foreach (WampClassAPI cAPI in cAPIs)
            {
                Add(cAPI);
            }
        }
        internal void Write()
        {
            foreach(string c_name in _cAPIs.Keys)
            {
                _writeClass(c_name);
            }
        }
    }
}
