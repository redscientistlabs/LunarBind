namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    internal struct LuaFunc
    {
        public string path;
        public object target;
        public MethodBase function;

        public LuaFunc(string path, object target, MethodBase function)
        {
            this.path = path;
            this.target = target;
            this.function = function;
        }
    }
}
