namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using MoonSharp.Interpreter.Interop;
    using ScriptCore.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class GlobalScriptBindings
    {
        static Dictionary<string,CallbackFunc> callbackFunctions = new Dictionary<string,CallbackFunc>();

        const string defaultCode = @"
            function waitFrames(frames)
                LUA_YIELD = WaitForFrames(frames)
                coroutine.yield()
            end
            function waitFrame()
                LUA_YIELD = WaitForFrames(0)
                coroutine.yield()
            end

            function RegisterCoroutine(co, name)
                local cor = coroutine.create(co)

                local cfunc = function()
                    if coroutine.status(cor) ~= 'dead' then coroutine.resume(cor) end
                end

                RegisterHook(cfunc,name)
            end
        ";


        static GlobalScriptBindings()
        {
            RegisterAssemblyFuncs(typeof(GlobalScriptBindings).Assembly);
            UserData.RegisterAssembly(typeof(GlobalScriptBindings).Assembly);
        }

        /// <summary>
        /// Call this to register all the callback functions
        /// </summary>
        public static void HookAllAssemblies()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyFuncs(assembly);
                UserData.RegisterAssembly(assembly);
            }
        }

        /// <summary>
        /// Register all the callback functions for specific assemblies only. This is the preferred method
        /// </summary>
        /// <param name="assemblies"></param>
        public static void HookAssemblies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyFuncs(assembly);
                UserData.RegisterAssembly(assembly);
            }
        }

        /// <summary>
        /// Register all the callback functions for specific assemblies only. This is the preferred method
        /// </summary>
        /// <param name="assemblies"></param>
        public static void HookClasses(params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterTypeFuncs(type);
            }
        }

        /// <summary>
        /// Manually register a type to use in Lua
        /// </summary>
        /// <param name="t"></param>
        public static void RegisterUserDataType(Type t)
        {
            UserData.RegisterType(t);
        }

        public static void RegisterUserDataType(Type t, InteropAccessMode mode)
        {
            UserData.RegisterType(t, mode);
        }

        public static void RegisterUserDataType(Type t, IUserDataDescriptor descriptor)
        {
            UserData.RegisterType(t, descriptor);
        }


        static void RegisterTypeFuncs(Type type)
        {
            MethodInfo[] mis = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var mi in mis)
            {
                var attr = (LuaFunctionAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaFunctionAttribute));
                if (attr != null)
                {
                    var documentation = (LuaDocumentationAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaDocumentationAttribute));
                    var example = (LuaExampleAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaExampleAttribute));
                    var del = HelperFuncs.CreateDelegate(mi);
                    callbackFunctions[attr.Name] = new CallbackFunc(attr.Name, del, documentation?.Data ?? "", example?.Data ?? "");
                }
            }           
        }

        static void RegisterAssemblyFuncs(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (var type in types)
            {
                RegisterTypeFuncs(type);
            }
        }

        internal static void Initialize(Script lua)
        {
            foreach (var func in callbackFunctions)
            {
                lua.Globals[func.Value.Path] = func.Value.Callback;
            }
            lua.DoString(defaultCode, null, "Lua Helper Code");
        }
    }

}
