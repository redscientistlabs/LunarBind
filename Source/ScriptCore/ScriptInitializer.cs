namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using ScriptCore.Attributes;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public static class ScriptInitializer
    {
        //static List<CallbackFunc> callbackFunctions = new List<CallbackFunc>();
        static Dictionary<string,CallbackFunc> callbackFunctions = new Dictionary<string,CallbackFunc>();

        const string libCode = @"
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


        /// <summary>
        /// Call this to register all the callback functions
        /// </summary>
        public static void Start()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyFuncs(assembly);
            }
        }

        static void RegisterAssemblyFuncs(Assembly assembly)
        {
            Type[] types = assembly.GetTypes();
            foreach (var type in types)
            {
                MethodInfo[] mis = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                foreach (var mi in mis)
                {
                    var attr = (LuaCallbackAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaCallbackAttribute));
                    if (attr != null)
                    {
                        var documentation = (LuaDocumentationAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaDocumentationAttribute));
                        var example = (LuaExampleAttribute)Attribute.GetCustomAttribute(mi, typeof(LuaExampleAttribute));
                        var del = CreateDelegate(mi);
                        callbackFunctions[attr.Name] = new CallbackFunc(attr.Name, del, documentation?.Data ?? "", example?.Data ?? "");
                    }
                }
            }
        }

        //Taken from https://stackoverflow.com/a/40579063
        private static Delegate CreateDelegate(MethodInfo methodInfo, object target = null)
        {
            Func<Type[], Type> getType;
            var isAction = methodInfo.ReturnType.Equals((typeof(void)));
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            if (isAction)
            {
                getType = System.Linq.Expressions.Expression.GetActionType;
            }
            else
            {
                getType = System.Linq.Expressions.Expression.GetFuncType;
                types = types.Concat(new[] { methodInfo.ReturnType });
            }

            if (methodInfo.IsStatic)
            {
                return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
            }

            return Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
        }

        internal static void Initialize(Script lua)
        {
            foreach (var func in callbackFunctions)
            {
                lua.Globals[func.Value.Path] = func.Value.Callback;
            }
            lua.DoString(libCode, null, "Lua Helper Code");
        }
    }

}
