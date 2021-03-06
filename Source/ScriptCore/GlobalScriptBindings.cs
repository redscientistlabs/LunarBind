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
            }
        }

        /// <summary>
        /// Register classes as user data (classes tagged with <see cref="MoonSharpUserDataAttribute"/> in an assembly
        /// </summary>
        /// <param name="assemblies"></param>
        public static void RegisterAssemblyUserData(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                UserData.RegisterAssembly(assembly);
            }
        }

        /// <summary>
        /// Register all the callback functions for specific classes only.
        /// <param name="assemblies"></param>
        public static void HookClasses(params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterTypeFuncs(type);
            }
        }

        /// <summary>
        /// Manually register a type to use in Lua. Only a static form of registration is available.
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


        public static void HookActionProps<T0>(Type type)
        {
            PropertyInfo[] props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var prop in props)
            {
                var attr = (LuaFunctionAttribute)Attribute.GetCustomAttribute(prop, typeof(LuaFunctionAttribute));
                if (attr != null)
                {
                    var val = prop.GetValue(null);
                    if (val.GetType().IsAssignableFrom(typeof(Action<T0>)))
                    {
                        var action = ((Action<T0>)val);
                        var del = Delegate.CreateDelegate(typeof(Action<T0>), action, "Invoke");
                        string name = attr.Name ?? prop.Name;
                        callbackFunctions[name] = new CallbackFunc(name, del, "", "");
                    }
                }
            }
        }

        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void AddAction(string name, Action action, string documentation = "", string example = "")
        {
            callbackFunctions[name] = new CallbackFunc(name, action, documentation, example);
        }
        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings, using the method's Name as the name
        /// </summary>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void AddAction(Action action, string documentation = "", string example = "")
        {
            callbackFunctions[action.Method.Name] = new CallbackFunc(action.Method.Name, action, documentation, example);
        }

        /// <summary>
        /// Add specific <see cref="Action"/>s to the bindings, using the method's Name as the name for each
        /// </summary>
        /// <param name="actions"></param>
        public static void AddActions(params Action[] actions)
        {
            foreach (var action in actions)
            {
                callbackFunctions[action.Method.Name] = new CallbackFunc(action.Method.Name, action);
            }
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/> to the bindings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void AddDelegate(string name, Delegate del, string documentation = "", string example = "")
        {
            callbackFunctions[name] = new CallbackFunc(name, del, documentation, example);
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/> to the bindings using its Name as the name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void AddDelegate(Delegate del, string documentation = "", string example = "")
        {
            callbackFunctions[del.Method.Name] = new CallbackFunc(del.Method.Name, del, documentation, example);
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/> to the bindings using its Name as the name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void AddDelegates(params Delegate[] dels)
        {
            foreach (var del in dels)
            {
                callbackFunctions[del.Method.Name] = new CallbackFunc(del.Method.Name, del);
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
            //lua.DoString(defaultCode, null, "Lua Helper Code");
        }
    }

}
