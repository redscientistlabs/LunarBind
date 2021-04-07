namespace ScriptCore
{
    using MoonSharp.Interpreter;
    using ScriptCore.Yielding;
    using System;
    using System.Collections;

    /// <summary>
    /// A class implementing advanced coroutine behaviors
    /// </summary>
    public class ScriptHook
    {
        public DynValue LuaFunc { get; private set; }
        public DynValue Coroutine { get; private set; }
        public bool IsCoroutine { get; private set; }

        internal Yielder CurYielder { get; set; } = null;

        public bool AutoResetCoroutine { get; set; }
        
        public event Action Done;

        public Script scriptRef { get; private set; }

        /// <summary>
        /// Creates a script hook from a reference script and a string containing a function
        /// </summary>
        /// <param name="scriptRef"></param>
        /// <param name="luaCodeContainingFunction"></param>
        /// <param name="coroutine"></param>
        /// <param name="autoreset"></param>
        public ScriptHook(Script scriptRef, string luaCodeContainingFunction, bool coroutine = false, bool autoreset = true)
        {
            this.scriptRef = scriptRef;
            LuaFunc = scriptRef.LoadFunction(luaCodeContainingFunction);
            IsCoroutine = coroutine;
            AutoResetCoroutine = autoreset;
            Coroutine = coroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
        }

        //internal ScriptHook(Script scriptRef, DynValue function, bool coroutine = false, bool autoreset = true)
        //{
        //    this.scriptRef = scriptRef;
        //    LuaFunc = function;
        //    IsCoroutine = coroutine;
        //    AutoResetCoroutine = autoreset;
        //    Coroutine = coroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
        //}

        /// <summary>
        /// Clone the other scripthook. Also creates a new coroutine
        /// </summary>
        /// <param name="other"></param>
        public ScriptHook(ScriptHook other)
        {
            this.scriptRef = other.scriptRef;
            LuaFunc = other.LuaFunc;
            IsCoroutine = other.IsCoroutine;
            AutoResetCoroutine = other.AutoResetCoroutine;
            Coroutine = IsCoroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
        }

        /// <summary>
        /// Creates a script hook from a function from the script 
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="scriptRef"></param>
        /// <param name="coroutine"></param>
        /// <param name="autoreset"></param>
        public ScriptHook(string funcName, Script scriptRef, bool coroutine = false, bool autoreset = true)
        {
            this.scriptRef = scriptRef;
            LuaFunc = scriptRef.Globals.Get(funcName);
            IsCoroutine = coroutine;
            AutoResetCoroutine = autoreset;
            Coroutine = coroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
        }

        public ScriptHook(Script scriptRef, DynValue del, DynValue coroutine = null, bool autoResetCoroutine = false)
        {
            this.scriptRef = scriptRef;
            IsCoroutine = coroutine != null;
            LuaFunc = del;
            Coroutine = coroutine;
            AutoResetCoroutine = autoResetCoroutine;
        }

        ~ScriptHook()
        {
            CurYielder = null;
            LuaFunc = null;
            Coroutine = null;
            scriptRef = null;
        }

        public bool CheckYieldStatus()
        {
            if(CurYielder != null)
            {
                if (CurYielder.CheckStatus())
                {
                    CurYielder = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        public void SetYielder(Yielder yielder)
        {
            CurYielder = yielder;
        }

        public void OnDone()
        {
            Done?.Invoke();
        }

        public void ResetCoroutine()
        {
            Coroutine.Assign(scriptRef.CreateCoroutine(LuaFunc));
        }

        public DynValue ExecuteWithCallback(Action callback, params object[] args)
        {
            if (IsCoroutine)
            {
                if (Coroutine.Coroutine.State == CoroutineState.Dead || !CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
                {
                    return null;
                }
                DynValue ret = Coroutine.Coroutine.Resume(args);
                switch (Coroutine.Coroutine.State)
                {
                    case CoroutineState.Suspended:

                        if (ret.IsNotNil())
                        {
                            CurYielder = ret.ToObject<Yielder>();
                        }
                        else
                        {
                            CurYielder = null;
                        }
                        break;
                    case CoroutineState.Dead:
                        CurYielder = null;
                        callback?.Invoke();
                        OnDone();
                        if (AutoResetCoroutine)
                        {
                            Coroutine.Assign(scriptRef.CreateCoroutine(LuaFunc));
                        }
                        break;
                    default:
                        break;
                }
                return ret;
            }
            else
            {
                var ret = scriptRef.Call(LuaFunc, args);
                callback?.Invoke();
                OnDone();
                return ret;
            }
        }

        public DynValue Execute(params object[] args)
        {
            if (IsCoroutine)
            {
                if (Coroutine.Coroutine.State == CoroutineState.Dead || !CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
                {
                    return DynValue.Nil;
                }
                DynValue ret = Coroutine.Coroutine.Resume(args);
                switch (Coroutine.Coroutine.State)
                {
                    case CoroutineState.Suspended:

                        if (ret.IsNotNil())
                        {
                            CurYielder = ret.ToObject<Yielder>();
                        }
                        else
                        {
                            CurYielder = null;
                        }
                        break;
                    case CoroutineState.Dead:
                        CurYielder = null;
                        OnDone();
                        if (AutoResetCoroutine)
                        {
                            Coroutine.Assign(scriptRef.CreateCoroutine(LuaFunc));
                        }
                        break;
                    default:
                        break;
                }
                return ret;
            }
            else
            {
                var ret = scriptRef.Call(LuaFunc, args);
                OnDone();
                return ret;
            }
        }

        public IEnumerator AsUnityCoroutine(params object[] args)
        {
            CoroutineState state = CoroutineState.NotStarted;
            var hook = new ScriptHook(this) { AutoResetCoroutine = false };
            while (state != CoroutineState.Dead)
            {
                state = RunAsCoroutine(hook, null, args);
                yield return null;
            }
        }

        /// <summary>
        /// Clones this script hook. The script hook run is not the one 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public IEnumerator AsUnityCoroutine(Action callback, params object[] args)
        {
            CoroutineState state = CoroutineState.NotStarted;
            var hook = new ScriptHook(this) { AutoResetCoroutine = false };
            while (state != CoroutineState.Dead)
            {
                state = RunAsCoroutine(hook, callback, args);
                yield return null;
            }
        }

        private static CoroutineState RunAsCoroutine(ScriptHook hook, Action callback, object[] args)
        {
            if (hook.IsCoroutine)
            {
                if (hook.Coroutine.Coroutine.State == CoroutineState.Dead)
                {
                    return CoroutineState.Dead;
                }

                if (!hook.CheckYieldStatus())
                {
                    return CoroutineState.Suspended;
                }
                DynValue ret = hook.Coroutine.Coroutine.Resume(args);
                switch (hook.Coroutine.Coroutine.State)
                {
                    case CoroutineState.Suspended:

                        if (ret.IsNotNil())
                        {
                            Yielder yielder = ret.ToObject<Yielder>();
                            hook.CurYielder = yielder;
                        }
                        else
                        {
                            hook.CurYielder = null;
                        }
                        return CoroutineState.Suspended;
                    case CoroutineState.Dead:
                        hook.CurYielder = null;
                        callback?.Invoke();
                        hook.OnDone();
                        if (hook.AutoResetCoroutine)
                        {
                            hook.ResetCoroutine();
                        }
                        return CoroutineState.Dead;
                    default:
                        break;
                }
                return hook.Coroutine.Coroutine.State;
            }
            else
            {
                var ret = hook.scriptRef.Call(hook.LuaFunc, args);
                callback?.Invoke();
                hook.OnDone();
                return CoroutineState.Dead;
            }
        }


    }

}
