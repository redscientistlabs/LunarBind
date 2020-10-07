using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCore
{
    public static class Scripts
    {
        static Dictionary<string, List<string>> links = new Dictionary<string, List<string>>();
        static List<LuaFunc> luaFunctions = new List<LuaFunc>();

        //TODO: initialize/add preset functions better, add sandboxing?
        const string libCode = @"
            function waitFrames(frames)
                yieldTimer = frames
                coroutine.yield()
            end
            function waitFrame()
                yieldTimer = 0
                coroutine.yield()
            end
        ";

        public static Script Create(string script)
        {
            Script scr = new Script(script);
            Initialize(scr);
            scr.Initialize();
            return scr;
        }

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

        public static void Initialize(Script script)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var assemb in links)
            {
                foreach (var namespc in assemb.Value)
                {
                    sb.AppendLine($@"import ('{assemb.Key}', '{namespc}')");
                }
            }
            script.lua.DoString(sb.ToString(), "imports");
            script.lua.DoString(libCode, "yieldCode");

            foreach (var func in luaFunctions)
            {
                script.lua.RegisterFunction(func.path, func.target, func.function);
            }

        }



    }


}
