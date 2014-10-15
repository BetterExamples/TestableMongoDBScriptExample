using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using System.Configuration;

namespace MongoDBScriptExample
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new MongoClient(ConfigurationManager.AppSettings["Mongo:Server"]);
            var db = client.GetServer().GetDatabase(ConfigurationManager.AppSettings["Mongo:DB"]);

            var output1 = db.Test("World");
            var output2 = MongoScripts.Test(db, "Mundo");

            Console.WriteLine(output1);
            Console.WriteLine(output2);
        }
    }
}
