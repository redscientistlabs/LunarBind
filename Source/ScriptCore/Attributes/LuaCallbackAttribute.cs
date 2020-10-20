using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCore.Attributes
{

    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LuaCallbackAttribute : Attribute
    {
        readonly string name;

        public string Name
        {
            get { return name; }
        }
        public LuaCallbackAttribute(string name)
        {
            this.name = name;
        }
    }
}
