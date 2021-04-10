namespace LunarBind
{
    using MoonSharp.Interpreter;
    using LunarBind.Yielding;
    using System;
    using System.Collections;
    using LunarBind.Standards;

    /// <summary>
    /// A class implementing advanced function call and coroutine behaviors
    /// </summary>
    public class ScriptFunction
    {
        /// <summary>
        /// A reference to the script, is used to create coroutines and execute functions
        /// </summary>
        public Script ScriptRef { get; private set; }
        /// <summary>
        /// A reference to the original MoonSharp function
        /// </summary>
        public DynValue LuaFunc { get; private set; }

        //internal Table TableAssignedTo { get; private set; }

        /// <summary>
        /// A reference the internally created Coroutine dynval
        /// </summary>
        public DynValue Coroutine { get; private set; }
        /// <summary>
        /// Is it a coroutine?
        /// </summary>
        public bool IsCoroutine { get; private set; }

        public CoroutineState CoroutineState 
        { 
            get
            {
                if (!IsCoroutine)
                {
                    return CoroutineState.Dead;
                }
                else
                {
                    return Coroutine.Coroutine.State;
                }
            } 
        }


        private bool _autoResetCoroutine;
        /// <summary>
        /// Automatically create a new coroutine when the current one is dead?
        /// </summary>
        public bool AutoResetCoroutine {
            get { return _autoResetCoroutine; }
            set {
                _autoResetCoroutine = value;
                FuncType = StandardHelpers.GetLuaFuncType(IsCoroutine, value);
            }
        }


        /// <summary>
        /// The current coroutine yielder. When running as a unity coroutine 
        /// </summary>
        public Yielder CurYielder { get; set; } = null;

        public LuaFuncType FuncType { get; private set; }

        /// <summary>
        /// Creates a script hook from a reference script and a string containing a function
        /// </summary>
        /// <param name="scriptRef">A MoonSharp script to associate this <see cref="ScriptFunction"/> with</param>
        /// <param name="singleFunctionString">an unnamed function string<para/>Example: "function() print('test') end"</param>
        /// <param name="coroutine">is it a coroutine?</param>
        /// <param name="autoreset">should the coroutine reset automatically?</param>
        public ScriptFunction(Script scriptRef, string singleFunctionString, bool coroutine = false, bool autoreset = true)
        {
            this.ScriptRef = scriptRef;
            LuaFunc = scriptRef.LoadFunction(singleFunctionString);
            IsCoroutine = coroutine;
            AutoResetCoroutine = autoreset;
            Coroutine = coroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
            FuncType = StandardHelpers.GetLuaFuncType(coroutine, autoreset);
        }

        public ScriptFunction(Script scriptRef, DynValue function, bool coroutine = false, bool autoreset = true)
        {
            this.ScriptRef = scriptRef;
            LuaFunc = function;
            IsCoroutine = coroutine;
            AutoResetCoroutine = autoreset;
            Coroutine = coroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
            FuncType = StandardHelpers.GetLuaFuncType(coroutine, autoreset);
        }

        public ScriptFunction(Script scriptRef, DynValue function, LuaFuncType type)
        {
            this.ScriptRef = scriptRef;
            LuaFunc = function;
            IsCoroutine = !type.HasFlag(LuaFuncType.Function);
            AutoResetCoroutine = type.HasFlag(LuaFuncType.AutoCoroutine);
            Coroutine = IsCoroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
            FuncType = type;
        }


        /// <summary>
        /// Clone the other scripthook. Also creates a new coroutine with the same settings as the original
        /// </summary>
        /// <param name="other"></param>
        public ScriptFunction(ScriptFunction other)
        {
            this.ScriptRef = other.ScriptRef;
            LuaFunc = other.LuaFunc;
            IsCoroutine = other.IsCoroutine;
            AutoResetCoroutine = other.AutoResetCoroutine;
            Coroutine = IsCoroutine ? ScriptRef.CreateCoroutine(LuaFunc) : null;
            if (IsCoroutine) { Coroutine.Coroutine.AutoYieldCounter = other.Coroutine.Coroutine.AutoYieldCounter; }
            FuncType = other.FuncType;
        }

        /// <summary>
        /// Creates a script hook from a function from the script's global table
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="scriptRef"></param>
        /// <param name="coroutine"></param>
        /// <param name="autoreset"></param>
        public ScriptFunction(string funcName, Script scriptRef, bool coroutine = false, bool autoreset = true)
        {
            this.ScriptRef = scriptRef;
            LuaFunc = scriptRef.Globals.Get(funcName);
            if(LuaFunc.Type != DataType.Function)
            {
                throw new Exception($"Global key [{funcName}] was not a lua function");
            }
            IsCoroutine = coroutine;
            AutoResetCoroutine = autoreset;
            Coroutine = coroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
        }

        //public ScriptFunction(Script scriptRef, DynValue del, DynValue coroutine, bool autoResetCoroutine = false)
        //{
        //    this.ScriptRef = scriptRef;
        //    IsCoroutine = coroutine != null;
        //    LuaFunc = del;
        //    Coroutine = coroutine;
        //    AutoResetCoroutine = autoResetCoroutine;
        //}

        ~ScriptFunction()
        {
            CurYielder = null;
            LuaFunc = null;
            Coroutine = null;
            ScriptRef = null;
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
            else
            {
                return true;
            }
        }

        public void SetYielder(Yielder yielder)
        {
            CurYielder = yielder;
        }

        public void ResetCoroutine()
        {
            Coroutine.Assign(ScriptRef.CreateCoroutine(LuaFunc));
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
                        if (AutoResetCoroutine)
                        {
                            Coroutine.Assign(ScriptRef.CreateCoroutine(LuaFunc));
                        }
                        break;
                    default:
                        break;
                }
                return ret;
            }
            else
            {
                var ret = ScriptRef.Call(LuaFunc, args);
                callback?.Invoke();
                return ret;
            }
        }

        public DynValue Execute(params object[] args)
        {
            if (IsCoroutine)
            {
                var co = Coroutine.Coroutine;
                if (co.State == CoroutineState.Dead || !CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
                {
                    return DynValue.Nil;
                }
                DynValue ret;// = co.Resume(args);

                if (co.State == CoroutineState.NotStarted)
                {
                    ret = co.Resume(args);
                }
                else
                {
                    ret = co.Resume();
                }

                switch (co.State)
                {
                    case CoroutineState.Suspended:
                        if (ret.IsNotNil())
                        {
                            try
                            {
                                CurYielder = ret.ToObject<Yielder>();
                            }
                            catch
                            {
                                //TODO: throw error?
                            }
                        }
                        break;
                    case CoroutineState.Dead:
                        CurYielder = null;
                        if (AutoResetCoroutine)
                        {
                            //Create new coroutine, assign it to our dynvalue
                            Coroutine.Assign(ScriptRef.CreateCoroutine(LuaFunc));
                        }
                        break;
                    default:
                        break;
                }
                return ret;
            }
            else
            {
                //Not coroutine, just call the function
                var ret = ScriptRef.Call(LuaFunc, args);
                return ret;
            }
        }

        /// <summary>
        /// Returns an IEnumerator with a clone of this ScriptFunction that yield returns null every frame. If it is a normal function it will call once and yield break.<para/>
        /// The <see cref="Yielder"/> attached to the ScriptFunction is checked every iteration
        /// </summary>
        /// <param name="args">The arguments to be passed into the first call of the coroutine</param>
        /// <returns></returns>
        public IEnumerator AsUnityCoroutine(params object[] args)
        {
            if (!IsCoroutine)
            {
                Execute(args);
                yield break;
            }
            else
            {
                CoroutineState state = CoroutineState.NotStarted;
                var func = new ScriptFunction(this);
                func._autoResetCoroutine = false;
                func.FuncType = LuaFuncType.SingleUseCoroutine;
                Coroutine co = func.Coroutine.Coroutine;
                while (state != CoroutineState.Dead)
                {
                    state = RunAsUnityCoroutine(co, null, args);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Returns an IEnumerator with a clone of this ScriptFunction that yield returns null every frame. 
        /// If it is a normal function it will call once and yield break.<para/>
        /// The <see cref="Yielder"/> attached to the ScriptFunction is checked every iteration
        /// </summary>
        /// <param name="callback">The action to be called when the coroutine is completed</param>
        /// <param name="args">The arguments to be passed into the first call of the coroutine</param>
        /// <returns></returns>
        public IEnumerator AsUnityCoroutine(Action callback, params object[] args)
        {
            if (!IsCoroutine)
            {
                Execute(args);
                callback.Invoke();
                yield break;
            }
            else
            {
                CoroutineState state = CoroutineState.NotStarted;
                var func = new ScriptFunction(this);
                func._autoResetCoroutine = false;
                func.FuncType = LuaFuncType.SingleUseCoroutine;
                //Pull the coroutine reference first so we don't convert every time
                Coroutine co = func.Coroutine.Coroutine;
                while (state != CoroutineState.Dead)
                {
                    state = RunAsUnityCoroutine(co, callback, args);
                    yield return null;
                }
            }
        }

        //Assumes it is a coroutine
        private CoroutineState RunAsUnityCoroutine(Coroutine co, Action callback, object[] args)
        {
            if (co.State == CoroutineState.Dead)
            {
                return CoroutineState.Dead;
            }
            else if (!CheckYieldStatus())
            {
                return CoroutineState.Suspended;
            }
            else
            {
                //Do coroutine
                DynValue ret;
                if (co.State == CoroutineState.NotStarted)
                {
                    ret = co.Resume(args);
                }
                else
                {
                    ret = co.Resume();
                }

                switch (co.State)
                {
                    case CoroutineState.Suspended:

                        if (ret.IsNotNil())
                        {
                            //Todo: try catch?
                            Yielder yielder = ret.ToObject<Yielder>();
                            CurYielder = yielder;
                        }
                        return CoroutineState.Suspended;
                    case CoroutineState.Dead:
                        callback?.Invoke();
                        if (AutoResetCoroutine)
                        {
                            ResetCoroutine();
                        }
                        return CoroutineState.Dead;
                    default:
                        //ForceSuspended, Running, etc
                        return co.State;
                }
            }
        }

    }
}
