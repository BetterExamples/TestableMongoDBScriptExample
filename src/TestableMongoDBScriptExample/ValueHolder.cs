using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDBScriptExample
{
    public class ValueHolder<T>
    {
        public T Value { get; set; }
    }

    public class ValueHolder<T, U>
    {
        public T Value1 { get; set; }
        public U Value2 { get; set; }
    }

    public class ValueHolder<T, U, V>
    {
        public T Value1 { get; set; }
        public U Value2 { get; set; }
        public V Value3 { get; set; }
    }
}
