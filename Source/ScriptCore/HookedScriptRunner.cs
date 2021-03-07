using MoonSharp.Interpreter;
using ScriptCore.Yielding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCore
{
    public class HookedScriptRunner
    {
        private Script lua;

        private HookedScriptContainer scriptContainer = null;

        public HookedScriptRunner()
        {
            lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);

            //scriptrunner local methods, will have to hardcode documentation for this
            lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            lua.Globals["RegisterCoroutine"] = (Action<DynValue, string>)RegisterCoroutine;
            lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;

            //Global init
            GlobalScriptBindings.Initialize(lua);
        }

        public HookedScriptRunner(ScriptBindings bindings) : this()
        {
            bindings.Initialize(lua);
        }

        public void LoadScript(string scriptString)
        {
            scriptContainer?.ResetHooks();
            scriptContainer = new HookedScriptContainer(scriptString);
            //Initialize the hookable script
            lua.DoString(scriptContainer.ScriptString);
        }


        #region callbacks
        void RegisterCoroutine(DynValue del, string name)
        {
            var coroutine = lua.CreateCoroutine(del);
            scriptContainer.Hooks[name] = new ScriptHook(coroutine);
        }

        void RegisterHook(DynValue del, string name)
        {
            scriptContainer.Hooks[name] = new ScriptHook(del);
        }

        void RemoveHook(string name)
        {
            scriptContainer.Hooks.Remove(name);
        }
        #endregion

        public void Execute(string hookName, params object[] args)
        {
            try
            {
                RunLua(scriptContainer, hookName, args);
            }
            catch (ScriptRuntimeException ex)
            {
                //Todo: error handling
                throw ex;
            }
        }

        public T Query<T>(string hookName, params object[] args)
        {
            try
            {
                var ret = RunLua(scriptContainer, hookName, args);
                if (ret != null)
                {
                    return ret.ToObject<T>();
                }
                else
                {
                    return default;
                }
            }
            catch (ScriptRuntimeException ex)
            {
                //Todo: error handling
                throw ex;
            }
        }

        private DynValue RunLua(HookedScriptContainer script, string hookName, params object[] args)
        {
            var hook = script.GetHook(hookName);
            if (hook != null)
            {
                if (hook.IsCoroutine)
                {
                    if (hook.IsCoroutineDead || !hook.CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
                    {
                        return null;
                    }
                    DynValue ret = hook.LuaFunc.Coroutine.Resume(args);

                    switch (hook.LuaFunc.Coroutine.State)
                    {
                        case CoroutineState.Suspended:
                            Yielder yielder = ret.ToObject<Yielder>();
                            hook.CurYielder = yielder;
                            break;
                        case CoroutineState.Dead:
                            hook.CurYielder = null;
                            hook.IsCoroutineDead = true;
                            break;
                        default:
                            break;
                    }
                    return null;
                }
                else
                {
                   return lua.Call(hook.LuaFunc, args);
                }
            }
            else
            {
                return null;
            }
        }
    }
}
