using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace MongoDBScriptExample
{
    public static class MongoScripts
    {
        public static readonly string TestScript = MongoScriptUtilities.GetString(typeof(MongoScriptUtilities), "Test.js");

        public static string Test(this MongoDatabase db, string text)
        {
            var script = MongoScriptUtilities.PrepareScript(TestScript, new { name = text });

            var ret = MongoScriptUtilities.GetResultsAs<ValueHolder<string>>(db, script).FirstOrDefault();

            return ret.Value;
        }
    }
}
