using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using ServiceStack;

namespace MongoDBScriptExample
{
    public static class MongoScriptUtilities
    {
        private static Assembly _assembly;
        private static ConcurrentDictionary<string, object> _cache = new ConcurrentDictionary<string, object>();
        private static ConcurrentDictionary<int, string> _preparedCache = new ConcurrentDictionary<int, string>();

        /*
         * ...the mode where the dot also matches newlines is called "single-line mode". This is a bit unfortunate, because it is 
         * easy to mix up this term with "multi-line mode". Multi-line mode only affects anchors, and single-line mode only 
         * affects the dot ... When using the regex classes of the .NET framework, you activate this mode by specifying 
         * RegexOptions.Singleline, such as in Regex.Match("string", "regex", RegexOptions.Singleline).
         */
        private static Regex _regexTests = new Regex(@"[\r\n]*//\s*\<test\>.*?\</test>.*?(?=[\r\n]*)", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        static MongoScriptUtilities()
        {
            _assembly = Assembly.GetAssembly(typeof(MongoScriptUtilities));
        }

        public static string GetString<T>(string s)
        {
            return GetString(typeof(T), s);
        }

        public static string GetString(Type t, string s)
        {
            var key = t.Namespace + "." + s;
            object value = null;

            if(!_cache.TryGetValue(key, out value))
            {
                using(var stream = _assembly.GetManifestResourceStream(key))
                {
                    if(stream == null) return null;

                    using(var reader = new StreamReader(stream))
                    {
                        value = reader.ReadToEnd();

                        _cache[key] = value;
                    }
                }
            }

            return (string)value;
        }

        public static string PrepareScript(string script, params object[] doItParams)
        {
            string preparedScript = null;

            if(!_preparedCache.TryGetValue(script.GetHashCode(), out preparedScript))
            {
                preparedScript = _regexTests.Replace(script, string.Empty);

                _preparedCache[script.GetHashCode()] = preparedScript;
            }

            preparedScript = "function() {\r\n" + preparedScript;

            preparedScript += "\r\nreturn doIt(";
            
            var paramList = new List<string>();

            if(doItParams != null && doItParams.Length > 0)
            {
                foreach(var p in doItParams)
                {
                    if(p != null)
                        paramList.Add(p.ToJson());
                    else
                        paramList.Add("null");
                }
            }

            preparedScript += string.Join(", ", paramList);

            preparedScript += ");";

            preparedScript += "\r\n}";

            return preparedScript;
        }

        //public static CommandDocument PrepareEvalCommand(string script)
        //{
        //    var cmd = new CommandDocument();
        //    cmd["eval"] = script;
        //    cmd["lock"] = false;

        //    return cmd;
        //}

        public static void ExecuteScript(MongoDatabase db, string script)
        {
            var results = db.Eval(new EvalArgs() { Code = script, Lock = false });
        }

        public static List<T> GetResultsAs<T>(MongoDatabase db, string script)
        {
            var ret = new List<T>();

            //var cmd = PrepareEvalCommand(script);

            var results = db.Eval(new EvalArgs() { Code = script, Lock = false });

            var value = results;

            if(value.IsBsonNull)
            {
                return ret;
            }
            else if(value.IsBsonArray)
            {
                // multiple

                var arr = value.AsBsonArray;

                foreach(var item in arr)
                {
                    var val = BsonSerializer.Deserialize<T>(item.AsBsonDocument);
                    ret.Add(val);
                }
            }
            else
            {
                // single
                var val = BsonSerializer.Deserialize<T>(value.AsBsonDocument);
                ret.Add(val);
            }

            return ret;
        }
    }
}
