using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using ScriptCore.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCore
{
    public class ScriptBindings
    {

        Dictionary<string, CallbackFunc> callbackFunctions = new Dictionary<string, CallbackFunc>();

        public string InitCode { get; set; } = null;

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

        public void AddDelegate(string funcIdentifier, Delegate del)
        {
            callbackFunctions[funcIdentifier] = new CallbackFunc(funcIdentifier, del, "", "");
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
                        var del = Delegate.CreateDelegate(typeof(Action<T0>), action, "Invoke");
                        string name = attr.Name ?? prop.Name;
                        callbackFunctions[name] = new CallbackFunc(name, del, "", "");
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
                        var del = Delegate.CreateDelegate(typeof(Action<T0>), action, "Invoke");
                        string name = attr.Name ?? prop.Name;
                        callbackFunctions[name] = new CallbackFunc(name, del, "", "");
                    }
                }
            }
        }

        /// <summary>
        /// Register all the callback functions for specific assemblies only. This is the preferred method
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
        /// Register all the callback functions for specific assemblies only. This is the preferred method
        /// </summary>
        /// <param name="assemblies"></param>
        public void HookObject(object obj)
        {        
            RegisterObjectFuncs(obj);
        }


        /// <summary>
        /// Manually register a type to use in Lua
        /// </summary>
        /// <param name="t"></param>
        public void RegisterUserDataType(Type t)
        {
            UserData.RegisterType(t);
        }

        public void RegisterUserDataType(Type t, InteropAccessMode mode)
        {
            UserData.RegisterType(t, mode);
        }

        public void RegisterUserDataType(Type t, IUserDataDescriptor descriptor)
        {
            UserData.RegisterType(t, descriptor);
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
                    callbackFunctions[attr.Name] = new CallbackFunc(attr.Name, del, documentation?.Data ?? "", example?.Data ?? "");
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
            if (InitCode != null)
            {
                lua.DoString(InitCode, null, "Lua Binding Helper Code");
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
