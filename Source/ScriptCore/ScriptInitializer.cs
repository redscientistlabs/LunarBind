namespace ScriptCore
{
    using NLua;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public static class ScriptInitializer
    {
        static Dictionary<string, List<string>> links = new Dictionary<string, List<string>>();
        static List<LuaFunc> luaFunctions = new List<LuaFunc>();

        const string libCode = @"
            function waitFrames(frames)
                LUA_YIELD = frames
                coroutine.yield()
            end
            function waitFrame()
                LUA_YIELD = 0
                coroutine.yield()
            end
        ";

        public static void RegisterFunc(string path, object target, MethodBase function)
        {
            luaFunctions.Add(new LuaFunc(path, target, function));
        }

        public static void AddAssemblyAndNamespaces(string assembly, params string[] namespaces)
        {
            links[assembly] = new List<string>();
            links[assembly].AddRange(namespaces);
        }
        public static void AddNamespaces(string assembly, params string[] namespaces)
        {
            links[assembly].AddRange(namespaces);
        }
        public static void AddAssembly(string assembly)
        {
            links[assembly] = new List<string>();
        }
        public static void AddNamespace(string assembly, string namespce)
        {
            links[assembly].Add(namespce);
        }

        internal static void Initialize(Lua lua)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var assemb in links)
            {
                foreach (var namespc in assemb.Value)
                {
                    sb.AppendLine($@"import ('{assemb.Key}', '{namespc}')");
                }
            }
            lua.DoString(sb.ToString(), "imports");
            lua.DoString(libCode, "Additional Code");

            foreach (var func in luaFunctions)
            {
                lua.RegisterFunction(func.path, func.target, func.function);
            }
        }
    }

}
