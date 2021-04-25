namespace LunarBind
{
    using MoonSharp.Interpreter;
    using MoonSharp.Interpreter.Interop;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Yielding;

    //TODO: Make GlobalScriptBindings contain and reference a static ScriptBindings to reduce code duplication and improve maintainability
    //TODO: Generate documentation
    public static class GlobalScriptBindings
    {
        private static readonly Dictionary<string, BindItem> bindItems = new Dictionary<string, BindItem>();
        private static readonly Dictionary<string, Type> yieldableTypes = new Dictionary<string, Type>();
        private static readonly Dictionary<string, Type> newableTypes = new Dictionary<string, Type>();
        private static readonly Dictionary<string, Type> staticTypes = new Dictionary<string, Type>();

        private static string bakedNewableTypeString = null;
        private static string bakedYieldableNewableTypeString = null;

        //Todo: decide if all newable base types should be put into a table or left in global space
        /// <summary>
        /// The type prefix for all bindings to use for newable types
        /// </summary>
        public static string TypePrefix { get; set; } = "_";

        /// <summary>
        /// The table newable constructors are stored under
        /// </summary>
        public static string NewableTable { get; set; } = "new";


        /// <summary>
        /// Should functions that return Yield be automatically wrapped in coroutine.yield()?
        /// </summary>
        public static bool AutoYield { get; set; } = true;
        /// <summary>
        /// Use this to initialize all scripts with custom Lua code. This runs after all global bindings have been set up
        /// </summary>
        public static string CustomInitializerString { get; set; } = null;

        //This must happen before any script is initialized, static constructor assures that this will happen
        static GlobalScriptBindings()
        {
            Script.WarmUp();
            RegisterAssemblyFuncs(typeof(GlobalScriptBindings).Assembly);
            UserData.RegisterAssembly(typeof(GlobalScriptBindings).Assembly);
            InitializeYielders();
        }

        private static void InitializeYielders()
        {
            GlobalScriptBindings.AddYieldableType<WaitFrames>();
            //Other internal yielder types are not meant to be created in Lua
        }


        /// <summary>
        /// Initializes a script with C# callback functions, types, newable types, enums, and yieldables
        /// </summary>
        /// <param name="lua"></param>
        public static void Initialize(Script lua)
        {
            foreach (var item in bindItems.Values)
            {
                item.AddToScript(lua);
            }

            foreach (var type in staticTypes)
            {
                lua.Globals[type.Key] = type.Value;
            }

            InitializeNewables(lua);
            InitializeYieldables(lua);

            if (CustomInitializerString != null)
            {
                lua.DoString(CustomInitializerString);
            }
        }

        public static void AddYieldableType<T>(string name = null) where T : Yielder
        {
            if (name == null) { name = typeof(T).Name; }
            RegisterUserDataType(typeof(T));
            yieldableTypes[name] = typeof(T);
            BakeYieldableNewables();
        }

        /// <summary>
        /// Allows you to use the type's name as the constructor, under the table <see cref="GlobalScriptBindings.NewableTable"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="t"></param>
        public static void AddNewableType(string name, Type t)
        {
            RegisterUserDataType(t);
            newableTypes[name] = t;
            BakeNewables();
        }

        public static void AddNewableType(Type t)
        {
            RegisterUserDataType(t);
            newableTypes[t.Name] = t;
            BakeNewables();
        }

        //Todo: remove prefix thing
        public static void RemoveNewableType(string name)
        {
            newableTypes.Remove(name);
            BakeNewables();
        }

        public static void AddEnum<T>() where T : Enum
        {
            BindingHelpers.CreateBindEnum(bindItems, typeof(T).Name, typeof(T));
        }

        public static void AddEnum<T>(string path)
        {
            BindingHelpers.CreateBindEnum(bindItems, path, typeof(T));
        }

        /// <summary>
        /// Allows you to access static functions and members on the type by using the Lua global with the name<para/>
        /// Equivalent to script.Globals[t.Name] = t
        /// </summary>
        /// <param name="t"></param>
        public static void AddGlobalType(Type t)
        {
            RegisterUserDataType(t);
            staticTypes[t.Name] = t;
        }

        /// <summary>
        /// Allows you to access static functions and members on the type by using the Lua global with the name<para/>
        /// <para/>
        /// Equivalent to script.Globals[name] = t
        /// </summary>
        /// <param name="name"></param>
        /// <param name="t"></param>
        public static void AddGlobalType(string name, Type t)
        {
            RegisterUserDataType(t);
            staticTypes[name] = t;
        }

        public static void AddGlobalObject(string path, object o)
        {
            RegisterUserDataType(o.GetType());
            BindingHelpers.CreateBindUserObject(bindItems, path, o);
        }

        /// <summary>
        ///  Bind all static functions with the [<see cref="LunarBindFunctionAttribute"/>] attribute on each type in each assembly
        /// </summary>
        /// <param name="assemblies"></param>
        public static void BindAssemblyFuncs(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                
            }
        }


        public static void BindAssemblies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyFuncs(assembly);
                //RegisterAssemblyInstantiables(assembly);
            }
            //.GetConstructor(Type.EmptyTypes)
        }

        //private static void RegisterAssemblyInstantiables(Assembly assembly)
        //{
        //    Type[] types = assembly.GetTypes();
        //    foreach (var type in types)
        //    {
        //        var attr = (LunarBindInstanceAttribute)type.GetCustomAttribute(typeof(LunarBindInstanceAttribute));
        //        if (attr != null)
        //        {
        //            AddInstantiableType(attr.Name, type);
        //        }
        //    }
        //}

        private static void AddInstantiableType(string name, Type type)
        {
            throw new NotImplementedException();
        }

        //TODO: rename to differentiate from the AddGlobalType, etc
        /// <summary>
        /// Use <see cref="BindTypeFuncs(Type[])"/> instead
        /// </summary>
        /// <param name="assemblies"></param>
        [Obsolete]
        public static void AddTypes(params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterTypeFuncs(type);
            }
        }

        /// <summary>
        /// Bind all static functions with the [<see cref="LunarBindFunctionAttribute"/>] attribute on each type
        /// </summary>
        /// <param name="types"></param>
        public static void BindTypeFuncs(params Type[] types)
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
        /// Manually register a type to use in Lua. Calls MoonSharp's <see cref="UserData.RegisterType(Type, InteropAccessMode)"/> Only a static form of registration is available.
        /// </summary>
        /// <param name="t"></param>
        public static void RegisterUserDataType(Type t)
        {
            UserData.RegisterType(t);
        }
        /// <summary>
        /// Manually register a type to use in Lua. Calls MoonSharp's <see cref="UserData.RegisterType(Type, InteropAccessMode)"/> Only a static form of registration is available.
        /// </summary>
        /// <param name="t"></param>
        public static void RegisterUserDataType(Type t, InteropAccessMode mode)
        {
            UserData.RegisterType(t, mode);
        }
        /// <summary>
        /// Manually register a type to use in Lua. Calls MoonSharp's <see cref="UserData.RegisterType(Type, IUserDataDescriptor)"/> Only a static form of registration is available.
        /// </summary>
        /// <param name="t"></param>
        public static void RegisterUserDataType(Type t, IUserDataDescriptor descriptor)
        {
            UserData.RegisterType(t, descriptor);
        }

        /// <summary>
        /// Internal method for registering functions on a type
        /// </summary>
        /// <param name="type"></param>
        private static void RegisterTypeFuncs(Type type)
        {
            MethodInfo[] mis = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var mi in mis)
            {
                var attr = (LunarBindFunctionAttribute)Attribute.GetCustomAttribute(mi, typeof(LunarBindFunctionAttribute));
                if (attr != null)
                {
                    var documentation = (LunarBindDocumentationAttribute)Attribute.GetCustomAttribute(mi, typeof(LunarBindDocumentationAttribute));
                    var example = (LunarBindExampleAttribute)Attribute.GetCustomAttribute(mi, typeof(LunarBindExampleAttribute));
                    var del = BindingHelpers.CreateDelegate(mi);
                    string name = attr.Name ?? mi.Name;

                    BindingHelpers.CreateBindFunction(bindItems, name, del, attr.AutoYield, documentation?.Data ?? "", example?.Data ?? "");
                    //callbackItems[name] = new CallbackFunc(name, del, documentation?.Data ?? "", example?.Data ?? "");
                }
            }
        }

        //public static void HookActionProps<T0>(Type type)
        //{
        //    PropertyInfo[] props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        //    foreach (var prop in props)
        //    {
        //        var attr = (LuaFunctionAttribute)Attribute.GetCustomAttribute(prop, typeof(LuaFunctionAttribute));
        //        if (attr != null)
        //        {
        //            var val = prop.GetValue(null);
        //            if (val.GetType().IsAssignableFrom(typeof(Action<T0>)))
        //            {
        //                var action = ((Action<T0>)val);
        //                var del = Delegate.CreateDelegate(typeof(Action<T0>), action, "Invoke");
        //                string name = attr.Name ?? prop.Name;
        //                BindingHelpers.CreateCallbackItem(callbackItems, name, del, documentation?.Data ?? "", example?.Data ?? "");
        //                //callbackItems[name] = new CallbackFunc(name, del, "", "");
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void AddAction(string name, Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, name, action, false, documentation ?? "", example ?? "");
        }
        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings, using the method's Name as the name
        /// </summary>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void AddAction(Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, action.Method.Name, action, false, documentation ?? "", example ?? "");
            //callbackItems[action.Method.Name] = new CallbackFunc(action.Method.Name, action, documentation, example);
        }

        /// <summary>
        /// Add specific <see cref="Action"/>s to the bindings, using the method's Name as the name for each
        /// </summary>
        /// <param name="actions"></param>
        public static void AddActions(params Action[] actions)
        {
            foreach (var action in actions)
            {
                BindingHelpers.CreateBindFunction(bindItems, action.Method.Name, action, false, "", "");
                //callbackItems[action.Method.Name] = new CallbackFunc(action.Method.Name, action);
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
            BindingHelpers.CreateBindFunction(bindItems, name, del, false, documentation ?? "", example ?? "");
            //callbackItems[name] = new CallbackFunc(name, del, documentation, example);
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
            BindingHelpers.CreateBindFunction(bindItems, del.Method.Name, del, false, documentation ?? "", example ?? "");
            //callbackItems[del.Method.Name] = new CallbackFunc(del.Method.Name, del, documentation, example);
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
                BindingHelpers.CreateBindFunction(bindItems, del.Method.Name, del, false, "", "");
                //callbackItems[del.Method.Name] = new CallbackFunc(del.Method.Name, del);
            }
        }

        private static void RegisterAssemblyFuncs(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (var type in types)
            {
                RegisterTypeFuncs(type);
            }
        }



        private static void BakeNewables()
        {
            bakedNewableTypeString = BakeNewableTypeString(newableTypes);
        }

        private static void BakeYieldableNewables()
        {
            bakedYieldableNewableTypeString = BakeNewableTypeString(yieldableTypes);
        }

        private static string BakeNewableTypeString(Dictionary<string, Type> source)
        {
            StringBuilder s = new StringBuilder();

            foreach (var type in source)
            {
                string typeName = type.Key;
                string newFuncName = type.Key;//.Remove(0, TypePrefix.Length);
                HashSet<int> paramCounts = new HashSet<int>();
                var ctors = type.Value.GetConstructors().Where(x => x.GetCustomAttribute(typeof(MoonSharpHiddenAttribute)) == null);// x.CustomAttributes.Any(y => y.AttributeType == typeof(MoonSharp.Interpreter.MoonSharpHiddenAttribute)));
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

                        //REVERT
                        //s.AppendLine($"function {newFuncName}({parString}) return {typeName}.__new({parString}) end");
                        s.AppendLine($"{NewableTable}.{newFuncName} = function({parString}) return {typeName}.__new({parString}) end");
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
            if (lua.Globals.Get(NewableTable).Type != DataType.Table)
            {
                lua.Globals[NewableTable] = new Table(lua);
            }

            foreach (var type in newableTypes)
            {
                lua.Globals[type.Key] = type.Value;
            }

            if (bakedNewableTypeString != null)
            {
                lua.DoString(bakedNewableTypeString);
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

            if (bakedYieldableNewableTypeString != null)
            {
                lua.DoString(bakedYieldableNewableTypeString);
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