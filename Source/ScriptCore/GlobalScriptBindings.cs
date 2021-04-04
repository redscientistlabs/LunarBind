namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using MoonSharp.Interpreter.Interop;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Yielding;

    //Todo: Rename to something more appropriate, since this handles more than just bindings
    //Todo: make a sort of shared interface or make this class have an instance of ScriptBindings
    public static class GlobalScriptBindings
    {
        //Todo: decide on standards for added syntax
        public const string TYPE_PREFIX = "_";

        private static Dictionary<string, CallbackFunc> callbackFunctions = new Dictionary<string,CallbackFunc>();
        private static Dictionary<string, Type> yieldableTypes = new Dictionary<string, Type>();
        private static Dictionary<string, Type> newableTypes = new Dictionary<string, Type>();
        private static Dictionary<string, Type> staticTypes = new Dictionary<string, Type>();

        private static string bakedTypeString = null;
        private static string bakedYieldableTypeString = null;

        //This must happen before any script is initialized
        static GlobalScriptBindings()
        {
            RegisterAssemblyFuncs(typeof(GlobalScriptBindings).Assembly);
            UserData.RegisterAssembly(typeof(GlobalScriptBindings).Assembly);
            Yielders.Initialize();
        }

        public static void RegisterYieldableType(string name, Type t)
        {
            RegisterUserDataType(t);
            yieldableTypes[TYPE_PREFIX + name] = t;
            BakeYieldables();
        }

        /// <summary>
        /// Also allows you to access static functions on the type by using _TypeName
        /// </summary>
        /// <param name="name"></param>
        /// <param name="t"></param>
        public static void RegisterNewableType(string name, Type t)
        {
            RegisterUserDataType(t);
            newableTypes[TYPE_PREFIX + name] = t;
            BakeNewables();
        }

        public static void RegisterNewableType(Type t)
        {
            RegisterUserDataType(t);
            newableTypes[TYPE_PREFIX + t.Name] = t;
            BakeNewables();
        }

        public static void RemoveNewableType(string name, Type t)
        {
            newableTypes.Remove(TYPE_PREFIX + name);
            BakeNewables();
        }


        /// <summary>
        /// Allows you to access static functions on the type by using _TypeName
        /// </summary>
        /// <param name="t"></param>
        public static void RegisterStaticType(Type t)
        {
            RegisterUserDataType(t);
            staticTypes[TYPE_PREFIX + t.Name] = t;
        }

        /// <summary>
        /// Call this to register all the callback functions in all assemblies in the current app domain. Not recommended.
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
        /// Register all the callback functions for specific assemblies only.
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
        /// Automatically register all the static functions with the <see cref="LuaFunctionAttribute"/> for specific types
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
        /// Automatically register classes as user data (classes tagged with <see cref="MoonSharpUserDataAttribute"/>) in an assembly
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

        private static void RegisterTypeFuncs(Type type)
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
                    string name = attr.Name ?? mi.Name;
                    callbackFunctions[name] = new CallbackFunc(name, del, documentation?.Data ?? "", example?.Data ?? "");
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
        /// Add a specific <see cref="Delegate"/> to the bindings using the method Name as the name
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
        /// Add a specific <see cref="Delegate"/>s to the bindings using the method Names as the name
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

        /// <summary>
        /// Initializes a script with C# callback functions, static types, and yieldables
        /// </summary>
        /// <param name="lua"></param>
        public static void Initialize(Script lua)
        {
            foreach (var func in callbackFunctions)
            {
                lua.Globals[func.Value.Path] = func.Value.Callback;
                if (func.Value.IsYieldable)
                {
                    lua.DoString(func.Value.YieldableString);
                }
            }
            foreach (var type in staticTypes)
            {
                lua.Globals[type.Key] = type.Value;
            }

            InitializeNewables(lua);
            InitializeYieldables(lua);
        }

        private static void BakeNewables()
        {
            bakedTypeString = Bake(newableTypes);
        }

        private static void BakeYieldables()
        {
            bakedYieldableTypeString = Bake(yieldableTypes);
        }

        private static string Bake(Dictionary<string,Type> source)
        {
            StringBuilder s = new StringBuilder();

            foreach (var type in source)
            {
                string typeName = type.Key;
                string newFuncName = type.Key.Remove(0, TYPE_PREFIX.Length);
                HashSet<int> paramCounts = new HashSet<int>();
                var ctors = type.Value.GetConstructors();
                foreach (var ctor in ctors)
                {
                    var pars = ctor.GetParameters();
                    foreach (var item in pars)
                    {
                        if (!item.ParameterType.IsPrimitive && item.ParameterType != typeof(string) && !UserData.IsTypeRegistered(item.ParameterType))
                        {
                            throw new Exception("CLR type constructor parameters must be added to UserData or be a primitive type or string");
                        }
                    }

                    if (!paramCounts.Contains(pars.Length))
                    {
                        string parString = "";
                        paramCounts.Add(pars.Length);
                        for (int j = 0; j < pars.Length; j++)
                        {
                            if (j == 0) { parString += $"t{j}"; }
                            else { parString += $",t{j}"; }
                        }
                        s.AppendLine($"function {newFuncName}({parString}) return {typeName}.__new({parString}) end");
                    }
                }
            }
            return s.ToString();
        }

        /// <summary>
        /// Initializes a script with the newable types
        /// </summary>
        /// <param name="lua"></param>
        private static void InitializeNewables(Script lua)
        {
            foreach (var type in newableTypes)
            {
                lua.Globals[type.Key] = type.Value;
            }

            if (bakedTypeString != null)
            {
                lua.DoString(bakedTypeString);
            }
        }
        
        /// <summary>
        /// Initializes a script with the yielder types
        /// </summary>
        /// <param name="lua"></param>
        internal static void InitializeYieldables(Script lua)
        {
            foreach (var type in yieldableTypes)
            {
                lua.Globals[type.Key] = type.Value;
            }

            if (bakedYieldableTypeString != null)
            {
                lua.DoString(bakedYieldableTypeString);
            }
        }

        //static StringBuilder paramsSB = new StringBuilder();
        //private static string ConstructFunction(CallbackFunc func)
        //{
        //    paramsSB.Clear();
        //    int numPars = func.NumParams;
        //    for (int j = 0; j < numPars; j++)
        //    {
        //        if (j == 0) { paramsSB.Append("n0"); }
        //        else
        //        {
        //            paramsSB.Append(",n");
        //            paramsSB.Append(j);
        //        }
        //    }
        //    string pars = numPars > 0 ? paramsSB.ToString() : "";
        //    if (func.IsYieldable)
        //    {
        //        return $"function {func.Path}({pars}) coroutine.yield() end";
        //    }
        //    else
        //    {
        //        return $"function {func.Path}({pars})  end";
        //    }
        //}

    }

}
