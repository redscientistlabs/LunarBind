namespace ScriptCore
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MoonSharp.Interpreter;

    /// <summary>
    /// A class for defining and extracting a standard for lua functions
    /// </summary>
    public class HookStandard
    {
        public List<FuncStandard> Standards { get; private set; } = new List<FuncStandard>();
        
        public HookStandard()
        {
        }
        public HookStandard(params FuncStandard[] standards)
        {
            if (standards != null)
            {
                Standards.AddRange(standards);
            }
        }

        //public bool ScriptComplies(Script script, List<string> errors = null)
        //{
        //    if(script.)
        //}

        /// <summary>
        /// Constructor for a single function
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isCoroutine"></param>
        /// <param name="autoResetCoroutine"></param>
        public HookStandard(string path, bool isCoroutine = false, bool autoResetCoroutine = false)
        {
            Standards.Add(new FuncStandard(path, isCoroutine, autoResetCoroutine));
        }

        public void AddStandard(string path, bool isCoroutine = false, bool autoResetCoroutine = false)
        {
            Standards.Add(new FuncStandard(path, isCoroutine, autoResetCoroutine));
        }

        public void AddStandard(FuncStandard standard)
        {
            Standards.Add(standard);
        }
        public void AddStandards(params FuncStandard[] standards)
        {
            if (standards != null)
            {
                Standards.AddRange(standards);
            }
        }

        public Dictionary<string, DynValue> ExtractHooks(Script script, List<string> errors = null)
        {
            Dictionary<string, DynValue> hooks = new Dictionary<string, DynValue>();
            var g = script.Globals;
            bool ok = true;
            for (int i = 0; i < Standards.Count; i++)
            {
                try
                {
                    //foreach (var key in g.Keys)
                    //{
                    //    if (key.ToString() == Standards[i].Path) {
                    //        var globalItem = g.Get(key);
                    //        if (globalItem.Type != DataType.Function)
                    //        {
                    //            ok = false;
                    //            errors.Add($"Script contains [{Standards[i].Path}], but it is not a function");
                    //        }
                    //        else
                    //        {
                    //            hooks[Standards[i].Path] = globalItem;
                    //        }
                    //    }
                    //}

                    var globalItem = g.Get(Standards[i].Path);
                    if (globalItem.Type != DataType.Function)
                    {
                        ok = false;
                        errors?.Add($"Script contains [{Standards[i].Path}], but it is not a lua function");
                    }
                    else
                    {
                        hooks[Standards[i].Path] = globalItem;
                    }
                }
                catch
                {
                    ok = false;
                    errors?.Add($"Script does not contain [{Standards[i].Path}]");
                } 

            }
            return hooks;
        }

        public bool ApplyStandard(Script script, HookedScriptContainer container, List<string> errors = null)
        {
            var g = script.Globals;
            bool ok = true;
            for (int i = 0; i < Standards.Count; i++)
            {
                try
                {
                    //Try to see a manually added one exists
                    var hook = container.GetHook(Standards[i].Path);
                    if (hook == null)
                    {
                        //It was not already added
                        var globalItem = g.Get(Standards[i].Path);
                        if (globalItem.Type == DataType.Nil)
                        {
                            if (Standards[i].Required)
                            {
                                //It is required, add error
                                ok = false;
                                errors?.Add($"Script does not contain [{Standards[i].Path}]");
                            }
                            else
                            {
                                continue;
                            }
                        }
                        else if (globalItem.Type != DataType.Function)
                        {
                            //It is required, and is not a lua function
                            if (Standards[i].Required)
                            {
                                ok = false;
                                errors?.Add($"Script contains [{Standards[i].Path}], but it is not a lua function");
                            }
                        }
                        else
                        {
                            var funcType = Standards[i].FuncType;
                            if (funcType.HasFlag(FuncType.Function))
                            {
                                container.AddHook(Standards[i].Path, new ScriptHook(globalItem));
                            }
                            else if (funcType.HasFlag(FuncType.SingleUseCoroutine))
                            {
                                container.AddHook(Standards[i].Path, new ScriptHook(globalItem, script.CreateCoroutine(globalItem), false));

                            }
                            else if (funcType.HasFlag(FuncType.AutoCoroutine))
                            {
                                container.AddHook(Standards[i].Path, new ScriptHook(globalItem, script.CreateCoroutine(globalItem), true));
                            }
                        }

                    }
                    else
                    {
                        //Is it already set up?
                        var funcType = Standards[i].FuncType;
                        if (funcType.HasFlag(FuncType.AllowAny))
                        {
                            continue;
                        }
                        else if (funcType.HasFlag(FuncType.AllowAnyCoroutine))
                        {
                            if (hook.IsCoroutine) { continue; }
                            else
                            {
                                ok = false;
                                errors?.Add($"User defined hook contains [{Standards[i].Path}], but it is not a coroutine");
                            }
                        }
                        else if (funcType == FuncType.Function)
                        {
                            if (!hook.IsCoroutine) { continue; }
                            else
                            {
                                ok = false;
                                errors?.Add($"User defined hook contains [{Standards[i].Path}], but it is not a normal function");
                            }
                        }
                        else if (funcType == FuncType.SingleUseCoroutine)
                        {
                            if (!hook.IsCoroutine || hook.AutoResetCoroutine)
                            {
                                ok = false;
                                errors?.Add($"User defined hook contains [{Standards[i].Path}], but it is not a single-use coroutine");
                            }
                            else { continue; }
                        }
                        else if (funcType == FuncType.AutoCoroutine)
                        {
                            if (!hook.IsCoroutine || !hook.AutoResetCoroutine)
                            {
                                ok = false;
                                errors?.Add($"User defined hook contains [{Standards[i].Path}], but it is not an automatic coroutine");
                            }
                        }
                    }
                }
                catch
                {
                    ok = false;
                    errors?.Add($"Script does not contain [{Standards[i].Path}]");
                }

                //Todo: Eventually do Tables too
                //int len = Standards[i].SplitPath.Length;
                //if(len == 1)
                //{
                //    //Check standard
                //}
                //else
                //{
                //    try
                //    {
                //        DynValue cur = script.Globals.Get(Standards[i].SplitPath[0]);
                //        for (int j = 1; j < len; j++)
                //        {
                //            if(cur.)
                //        }
                //    }
                //    catch
                //    {
                //    }
                //}     

            }
            return ok;
        }
        

    }

    [System.Flags]
    public enum FuncType
    {
        /// <summary>
        /// A non-coroutine function
        /// </summary>
        Function = 1,
        /// <summary>
        /// A coroutine which automatically restarts after completing
        /// </summary>
        AutoCoroutine = 2,
        /// <summary>
        /// A coroutine which does not automatically restart after completing
        /// </summary>
        SingleUseCoroutine = 4,
        /// <summary>
        /// Allow User-Registered hooks, which must be coroutines.<para/> Must also provide either <see cref="AutoCoroutine"/> or <see cref="SingleUseCoroutine"/> for automatic hooking if you use this flag
        /// </summary>
        AllowAnyCoroutine = 8,
        /// <summary>
        /// Allow user-registered hooks of any type.<para/> Must provide another type besides <see cref="AllowAnyCoroutine"/> for automatic hooking if you use this flag.
        /// </summary>
        AllowAny = 16
    }

    public class FuncStandard
    {
        public string Path;
        //public string[] SplitPath;
        //public bool IsCoroutine;
        //public bool AutoResetCoroutine;
        public bool Required { get; set; }

        internal FuncType FuncType { get; private set; }

        public FuncStandard(string path, bool isCoroutine = false, bool autoResetCoroutine = false, bool required = true)
        {
            Path = path;
            Required = required;

            //SplitPath = path.Split('.');
            //IsCoroutine = isCoroutine;
            // AutoResetCoroutine = autoResetCoroutine;

            if (isCoroutine)
            {
                if (autoResetCoroutine)
                {
                    FuncType = FuncType.AutoCoroutine;
                }
                else
                {
                    FuncType = FuncType.SingleUseCoroutine;
                }
            }
            else
            {
                FuncType = FuncType.Function;
            }

        }

        public FuncStandard(string path, FuncType type, bool required = true)
        {
            Path = path;
            FuncType = type;
            Required = required;

            if(FuncType.HasFlag(FuncType.AllowAny) && (((int)FuncType & 0b0111) < 1))
            {
                throw new ArgumentException("Type must also contain FuncType.Function, FuncType.SingleUseCoroutine, or FuncType.AutoCoroutine", "type");
            }

            if (FuncType.HasFlag(FuncType.AllowAnyCoroutine) && (((int)FuncType & 0b0110) < 1))
            {
                throw new ArgumentException("Type must also contain FuncType.SingleUseCoroutine or FuncType.AutoCoroutine", "type");
            }

            //switch (type)
            //{
            //    case LuaFuncType.Function:
            //        IsCoroutine = false;
            //        AutoResetCoroutine = false;
            //        break;
            //    case LuaFuncType.Coroutine:
            //        IsCoroutine = true;
            //        AutoResetCoroutine = false;
            //        break;
            //    case LuaFuncType.AutoCoroutine:
            //        IsCoroutine = true;
            //        AutoResetCoroutine = true;
            //        break;
            //    case LuaFuncType.AnyCoroutine:
            //        IsCoroutine = true;
            //        AutoResetCoroutine = true;
            //        break;
            //    case LuaFuncType.Any:
            //        break;
            //    default:
            //        break;
            //}

            //SplitPath = path.Split('.');

        }

    }

}
