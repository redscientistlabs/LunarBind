namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using MoonSharp.Interpreter.Interop;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    public class ScriptBindings
    {
        private Dictionary<string, CallbackItem> callbackItems = new Dictionary<string, CallbackItem>();
        private Dictionary<string, Type> yieldableTypes = new Dictionary<string, Type>();
        private Dictionary<string, Type> newableTypes = new Dictionary<string, Type>();
        private Dictionary<string, Type> staticTypes = new Dictionary<string, Type>();
        private string bakedTypeString = null;
        private string bakedYieldableTypeString = null;
        public ScriptBindings()
        {

        }

        public ScriptBindings(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyFuncs(assembly);
                UserData.RegisterAssembly(assembly);
            }
        }

        public ScriptBindings(params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterTypeFuncs(type);
            }
        }

        public ScriptBindings(params object[] objs)
        {
            foreach (var obj in objs)
            {
                RegisterObjectFuncs(obj);
            }
        }

        public ScriptBindings(params Action[] actions)
        {
            HookActions(actions);
        }

        public ScriptBindings(params Delegate[] dels)
        {
            HookDelegates(dels);
        }

        public void HookTypes(params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterTypeFuncs(type);
            }
        }
        public void HookObjects(params object[] objs)
        {
            foreach (var obj in objs)
            {
                RegisterObjectFuncs(obj);
            }
        }
        public void HookObjects<T>(params T[] objs)
        {
            foreach (var obj in objs)
            {
                RegisterObjectFuncs(obj);
            }
        }

        public void HookType(Type type)
        {
            RegisterTypeFuncs(type);
        }
        public void HookObject(object obj)
        {
            RegisterObjectFuncs(obj);
        }


        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void HookAction(string name, Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateCallbackItem(callbackItems, name, action, documentation ?? "", example ?? "");
            //callbackItems[name] = new CallbackFunc(name, action, documentation, example);
        }
        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings, using the method's Name as the name
        /// </summary>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void HookAction(Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateCallbackItem(callbackItems, action.Method.Name, action, documentation ?? "", example ?? "");
            //callbackItems[action.Method.Name] = new CallbackFunc(action.Method.Name, action, documentation, example);
        }

        /// <summary>
        /// Add specific <see cref="Action"/>s to the bindings, using the method's Name as the name for each
        /// </summary>
        /// <param name="actions"></param>
        public void HookActions(params Action[] actions)
        {
            foreach (var action in actions)
            {
                BindingHelpers.CreateCallbackItem(callbackItems, action.Method.Name, action, "", "");
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
        public void HookDelegate(string name, Delegate del, string documentation = "", string example = "")
        {
            BindingHelpers.CreateCallbackItem(callbackItems, name, del, documentation ?? "", example ?? "");
            //callbackItems[name] = new CallbackFunc(name, del, documentation, example);
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/> to the bindings using its Name as the name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void HookDelegate(Delegate del, string documentation = "", string example = "")
        {
            BindingHelpers.CreateCallbackItem(callbackItems, del.Method.Name, del, documentation ?? "", example ?? "");
            //callbackItems[del.Method.Name] = new CallbackFunc(del.Method.Name, del, documentation, example);
        }

        /// <summary>
        /// Add specific <see cref="Delegate"/>s to the bindings using the method Name as the name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void HookDelegates(params Delegate[] dels)
        {
            foreach (var del in dels)
            {
                BindingHelpers.CreateCallbackItem(callbackItems, del.Method.Name, del, "", "");
                //callbackItems[del.Method.Name] = new CallbackFunc(del.Method.Name, del);
            }
        }


        ///// <summary>
        ///// Unstable, untested :)
        ///// </summary>
        ///// <typeparam name="T0"></typeparam>
        ///// <param name="target"></param>
        //public void HookActionProps<T0>(object target)
        //{
        //    Type type = target.GetType();
        //    PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        //    foreach (var prop in props)
        //    {
        //        var attr = (LuaFunctionAttribute)Attribute.GetCustomAttribute(prop, typeof(LuaFunctionAttribute));
        //        if (attr != null)
        //        {
        //            var val = prop.GetValue(target);
        //            if (val.GetType().IsAssignableFrom(typeof(Action<T0>)))
        //            {
        //                var action = ((Action<T0>)val);
        //                string name = attr.Name ?? prop.Name;
        //                BindingHelpers.CreateCallbackItem(callbackItems, name, action, "", "");
        //                //callbackItems[name] = new CallbackFunc(name, action, "", "");
        //            }
        //        }
        //    }
        //}

        ///// <summary>
        ///// Unstable, untested :)
        ///// </summary>
        ///// <typeparam name="T0"></typeparam>
        ///// <param name="type"></param>
        //public void HookActionProps<T0>(Type type)
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
        //                string name = attr.Name ?? prop.Name;
        //                callbackItems[name] = new CallbackFunc(name, action, "", "");
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Automatically register all the static functions with <see cref="Attributes.LuaFunctionAttribute"/> for specific assemblies
        /// </summary>
        /// <param name="assemblies"></param>
        public void HookAssemblies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyFuncs(assembly);
                UserData.RegisterAssembly(assembly);
            }
        }

        private void RegisterObjectFuncs(object target)
        {
            if (target == null) { throw new ArgumentNullException(nameof(target)); }
            Type type = target.GetType();
            MethodInfo[] mis = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var mi in mis)
            {
                var attr = (LuaFunctionAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaFunctionAttribute));
                if (attr != null)
                {
                    var documentation = (LuaDocumentationAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaDocumentationAttribute));
                    var example = (LuaExampleAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaExampleAttribute));
                    var del = BindingHelpers.CreateDelegate(mi, target);
                    string name = attr.Name ?? mi.Name;
                    BindingHelpers.CreateCallbackItem(callbackItems, name, del, documentation?.Data ?? "", example?.Data ?? "");
                    //callbackItems[name] = new CallbackFunc(name, del, documentation?.Data ?? "", example?.Data ?? "");
                }
            }
        }

        private void RegisterTypeFuncs(Type type)
        {
            MethodInfo[] mis = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var mi in mis)
            {
                var attr = (LuaFunctionAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaFunctionAttribute));
                if (attr != null)
                {
                    var documentation = (LuaDocumentationAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaDocumentationAttribute));
                    var example = (LuaExampleAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaExampleAttribute));
                    var del = BindingHelpers.CreateDelegate(mi);
                    string name = attr.Name ?? mi.Name;
                    BindingHelpers.CreateCallbackItem(callbackItems, name, del, documentation?.Data ?? "", example?.Data ?? "");
                    //callbackItems[name] = new CallbackFunc(name, del, documentation?.Data ?? "", example?.Data ?? "");
                }
            }
        }

        private void RegisterAssemblyFuncs(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (var type in types)
            {
                RegisterTypeFuncs(type);
            }
        }
        public static void RegisterUserDataType(Type t)
        {
            UserData.RegisterType(t);
        }

        public void RegisterYieldableType(string name, Type t)
        {
            RegisterUserDataType(t);
            yieldableTypes[GlobalScriptBindings.TYPE_PREFIX + name] = t;
            BakeYieldables();
        }

        /// <summary>
        /// Also allows you to access static functions on the type by using _TypeName
        /// </summary>
        /// <param name="name"></param>
        /// <param name="t"></param>
        public void RegisterNewableType(string name, Type t)
        {
            RegisterUserDataType(t);
            newableTypes[GlobalScriptBindings.TYPE_PREFIX + name] = t;
            BakeNewables();
        }

        public void RegisterNewableType(Type t)
        {
            RegisterUserDataType(t);
            newableTypes[GlobalScriptBindings.TYPE_PREFIX + t.Name] = t;
            BakeNewables();
        }

        public void RemoveNewableType(string name, Type t)
        {
            newableTypes.Remove(GlobalScriptBindings.TYPE_PREFIX + name);
            BakeNewables();
        }

        /// <summary>
        /// Allows you to access static functions on the type by using _TypeName
        /// </summary>
        /// <param name="t"></param>
        public void RegisterStaticType(Type t)
        {
            RegisterUserDataType(t);
            staticTypes[GlobalScriptBindings.TYPE_PREFIX + t.Name] = t;
        }


        /// <summary>
        /// Exposed initialize function to initialize non-scriptcore moonsharp scripts
        /// </summary>
        /// <param name="lua"></param>
        public void Initialize(Script lua)
        {
            foreach (var item in callbackItems.Values)
            {
                item.AddToScript(lua);
                //lua.Globals[func.Value.Name] = func.Value.Callback;
                //if (func.Value.IsYieldable)
                //{
                //    lua.DoString(func.Value.YieldableString);
                //}
            }
            foreach (var type in staticTypes)
            {
                lua.Globals[type.Key] = type.Value;
            }

            InitializeNewables(lua);
            InitializeYieldables(lua);
        }

        private static string Bake(Dictionary<string, Type> source)
        {
            StringBuilder s = new StringBuilder();

            foreach (var type in source)
            {
                string typeName = type.Key;
                string newFuncName = type.Key.Remove(0, GlobalScriptBindings.TYPE_PREFIX.Length);
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

        private void BakeNewables()
        {
            bakedTypeString = Bake(newableTypes);
        }

        private void BakeYieldables()
        {
            bakedYieldableTypeString = Bake(yieldableTypes);
        }

        /// <summary>
        /// Initializes a script with the newable types
        /// </summary>
        /// <param name="lua"></param>
        private void InitializeNewables(Script lua)
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

        private void InitializeYieldables(Script lua)
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


        /// <summary>
        /// Removes all callback functions from a script
        /// </summary>
        /// <param name="lua"></param>
        internal void Clean(Script lua)
        {
            foreach (var func in callbackItems)
            {
                lua.Globals.Remove(func.Value.Name);
            }
        }

    }
}
