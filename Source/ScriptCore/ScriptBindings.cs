namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using MoonSharp.Interpreter.Interop;
    using ScriptCore.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    public class ScriptBindings
    {
        private Dictionary<string, CallbackFunc> callbackFunctions = new Dictionary<string, CallbackFunc>();

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

        public ScriptBindings(object obj)
        {
            RegisterObjectFuncs(obj);
        }

        public ScriptBindings(params Action[] actions)
        {
            AddActions(actions);
        }

        public ScriptBindings(params Delegate[] dels)
        {
            AddDelegates(dels);
        }

        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void AddAction(string name, Action action, string documentation = "", string example = "")
        {
            callbackFunctions[name] = new CallbackFunc(name, action, documentation, example);
        }
        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings, using the method's Name as the name
        /// </summary>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void AddAction(Action action, string documentation = "", string example = "")
        {
            callbackFunctions[action.Method.Name] = new CallbackFunc(action.Method.Name, action, documentation, example);
        }

        /// <summary>
        /// Add specific <see cref="Action"/>s to the bindings, using the method's Name as the name for each
        /// </summary>
        /// <param name="actions"></param>
        public void AddActions(params Action[] actions)
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
        public void AddDelegate(string name, Delegate del, string documentation = "", string example = "")
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
        public void AddDelegate(Delegate del, string documentation = "", string example = "")
        {
            callbackFunctions[del.Method.Name] = new CallbackFunc(del.Method.Name, del, documentation, example);
        }

        /// <summary>
        /// Add specific <see cref="Delegate"/>s to the bindings using the method Name as the name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void AddDelegates(params Delegate[] dels)
        {
            foreach (var del in dels)
            {
                callbackFunctions[del.Method.Name] = new CallbackFunc(del.Method.Name, del);
            }
        }


        public void HookActionProps<T0>(object target)
        {
            Type type = target.GetType();
            PropertyInfo[] props = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var prop in props)
            {
                var attr = (LuaFunctionAttribute)Attribute.GetCustomAttribute(prop, typeof(LuaFunctionAttribute));
                if (attr != null)
                {
                    var val = prop.GetValue(target);
                    if (val.GetType().IsAssignableFrom(typeof(Action<T0>)))
                    {
                        var action = ((Action<T0>)val);
                        string name = attr.Name ?? prop.Name;
                        callbackFunctions[name] = new CallbackFunc(name, action, "", "");
                    }
                }
            }
        }

        public void HookActionProps<T0>(Type type)
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
                        string name = attr.Name ?? prop.Name;
                        callbackFunctions[name] = new CallbackFunc(name, action, "", "");
                    }
                }
            }
        }

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

        /// <summary>
        /// Automatically register all the instance methods with <see cref="Attributes.LuaFunctionAttribute"/> on an instance of an object.
        /// </summary>
        /// <param name="assemblies"></param>
        public void HookObject(object obj)
        {        
            RegisterObjectFuncs(obj);
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
                    var del = HelperFuncs.CreateDelegate(mi, target);
                    callbackFunctions[attr.Name] = new CallbackFunc(attr.Name, del, documentation?.Data ?? "", example?.Data ?? "");
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
                    var del = HelperFuncs.CreateDelegate(mi);
                    string name = attr.Name ?? mi.Name;
                    callbackFunctions[name] = new CallbackFunc(name, del, documentation?.Data ?? "", example?.Data ?? "");
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

        internal void Initialize(Script lua)
        {
            foreach (var func in callbackFunctions)
            {
                lua.Globals[func.Value.Path] = func.Value.Callback;
            }
        }

        internal void Clean(Script lua)
        {
            foreach (var func in callbackFunctions)
            {
                lua.Globals.Remove(func.Value.Path);
            }
        }

    }
}
