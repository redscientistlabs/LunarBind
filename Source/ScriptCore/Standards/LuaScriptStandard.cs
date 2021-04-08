namespace ScriptCore.Standards
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MoonSharp.Interpreter;

    /// <summary>
    /// A class for defining and extracting a standard for lua functions
    /// </summary>
    public class LuaScriptStandard
    {
        public List<LuaFuncStandard> Standards { get; private set; } = new List<LuaFuncStandard>();
        
        public LuaScriptStandard()
        {
        }
        public LuaScriptStandard(params LuaFuncStandard[] standards)
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
        public LuaScriptStandard(string path, bool isCoroutine = false, bool autoResetCoroutine = false)
        {
            Standards.Add(new LuaFuncStandard(path, isCoroutine, autoResetCoroutine));
        }

        public void AddStandard(string path, bool isCoroutine = false, bool autoResetCoroutine = false)
        {
            Standards.Add(new LuaFuncStandard(path, isCoroutine, autoResetCoroutine));
        }

        public void AddStandard(LuaFuncStandard standard)
        {
            Standards.Add(standard);
        }
        public void AddStandards(params LuaFuncStandard[] standards)
        {
            if (standards != null)
            {
                Standards.AddRange(standards);
            }
        }

        public void AddStandards(IEnumerable<LuaFuncStandard> standards)
        {
            if (standards != null)
            {
                Standards.AddRange(standards);
            }
        }

        public Dictionary<string, ScriptFunction> ExtractFunctions(Script script, List<string> errors = null)
        {
            Dictionary<string, ScriptFunction> hooks = new Dictionary<string, ScriptFunction>();
            var g = script.Globals;
            for (int i = 0; i < Standards.Count; i++)
            {
                try
                {
                    var globalItem = g.Get(Standards[i].Path);
                    if (globalItem.Type != DataType.Function)
                    {
                        errors?.Add($"Script contains [{Standards[i].Path}], but it is not a lua function");
                    }
                    else
                    {
                        hooks[Standards[i].Path] = new ScriptFunction(script, globalItem, 
                            !Standards[i].FuncType.HasFlag(LuaFuncType.Function), 
                            Standards[i].FuncType.HasFlag(LuaFuncType.AutoCoroutine));
                    }
                }
                catch
                {
                    errors?.Add($"Script does not contain [{Standards[i].Path}]");
                }
            }
            return hooks;
        }

        public Dictionary<string, DynValue> ExtractFunctionsRaw(Script script, List<string> errors = null)
        {
            Dictionary<string, DynValue> hooks = new Dictionary<string, DynValue>();
            var g = script.Globals;
            for (int i = 0; i < Standards.Count; i++)
            {
                try
                {
                    var globalItem = g.Get(Standards[i].Path);
                    if (globalItem.Type != DataType.Function)
                    {
                        errors?.Add($"Script contains [{Standards[i].Path}], but it is not a lua function");
                    }
                    else
                    {
                        hooks[Standards[i].Path] = globalItem;
                    }
                }
                catch
                {
                    errors?.Add($"Script does not contain [{Standards[i].Path}]");
                } 
            }
            return hooks;
        }

        public bool ApplyStandard(Script script, HookedScriptContainer container, List<string> messages = null)
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
                                messages?.Add($"Script does not contain [{Standards[i].Path}]");
                            }
                            else
                            {
                                //It is not required, add warning
                                messages?.Add($"WARNING: Script does not contain [{Standards[i].Path}], but it is not required");
                                continue;
                            }
                        }
                        else if (globalItem.Type != DataType.Function)
                        {
                            //It is required, and is not a lua function
                            if (Standards[i].Required)
                            {
                                ok = false;
                                messages?.Add($"Script contains [{Standards[i].Path}], but it is not a lua function");
                            }
                        }
                        else
                        {
                            var funcType = Standards[i].FuncType;
                            if (funcType.HasFlag(LuaFuncType.Function))
                            {
                                container.AddHook(Standards[i].Path, new ScriptFunction(script, globalItem, false));
                            }
                            else if (funcType.HasFlag(LuaFuncType.SingleUseCoroutine))
                            {
                                container.AddHook(Standards[i].Path, new ScriptFunction(script, globalItem, true, false));

                            }
                            else if (funcType.HasFlag(LuaFuncType.AutoCoroutine))
                            {
                                container.AddHook(Standards[i].Path, new ScriptFunction(script, globalItem, true, true));
                            }
                        }

                    }
                    else
                    {
                        //Is it already set up?
                        var funcType = Standards[i].FuncType;
                        if (funcType.HasFlag(LuaFuncType.AllowAny))
                        {
                            continue;
                        }
                        else if (funcType.HasFlag(LuaFuncType.AllowAnyCoroutine))
                        {
                            if (hook.IsCoroutine) { continue; }
                            else
                            {
                                ok = false;
                                messages?.Add($"User defined hook contains [{Standards[i].Path}], but it is not a coroutine");
                            }
                        }
                        else if (funcType == LuaFuncType.Function)
                        {
                            if (!hook.IsCoroutine) { continue; }
                            else
                            {
                                ok = false;
                                messages?.Add($"User defined hook contains [{Standards[i].Path}], but it is not a normal function");
                            }
                        }
                        else if (funcType == LuaFuncType.SingleUseCoroutine)
                        {
                            if (!hook.IsCoroutine || hook.AutoResetCoroutine)
                            {
                                ok = false;
                                messages?.Add($"User defined hook contains [{Standards[i].Path}], but it is not a single-use coroutine");
                            }
                            else { continue; }
                        }
                        else if (funcType == LuaFuncType.AutoCoroutine)
                        {
                            if (!hook.IsCoroutine || !hook.AutoResetCoroutine)
                            {
                                ok = false;
                                messages?.Add($"User defined hook contains [{Standards[i].Path}], but it is not an automatic coroutine");
                            }
                        }
                    }
                }
                catch
                {
                    ok = false;
                    messages?.Add($"Script does not contain [{Standards[i].Path}]");
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


    public class LuaFuncStandard
    {
        public string Path;
        public bool Required { get; set; }

        internal LuaFuncType FuncType { get; private set; }

        /// <summary>
        /// Creates a <see cref="LuaFuncStandard"/>
        /// </summary>
        /// <param name="path">The global function name. Paths not yet implemented, but this can be bypassed by registering the function manually</param>
        /// <param name="isCoroutine">Is it a coroutine</param>
        /// <param name="autoResetCoroutine">Should the coroutine automatically reset? (create a new coroutine when it dies?)</param>
        /// <param name="required">Does this function have to be implemented?</param>
        public LuaFuncStandard(string path, bool isCoroutine = false, bool autoResetCoroutine = false, bool required = true)
        {
            Path = path;
            Required = required;

            if (isCoroutine)
            {
                if (autoResetCoroutine)
                {
                    FuncType = LuaFuncType.AutoCoroutine;
                }
                else
                {
                    FuncType = LuaFuncType.SingleUseCoroutine;
                }
            }
            else
            {
                FuncType = LuaFuncType.Function;
            }

        }

        public LuaFuncStandard(string path, LuaFuncType type, bool required = true)
        {
            Path = path;
            FuncType = type;
            Required = required;

            if(FuncType.HasFlag(LuaFuncType.AllowAny) && (((int)FuncType & 0b0111) < 1))
            {
                throw new ArgumentException("Type must also contain FuncType.Function, FuncType.SingleUseCoroutine, or FuncType.AutoCoroutine", "type");
            }

            if (FuncType.HasFlag(LuaFuncType.AllowAnyCoroutine) && (((int)FuncType & 0b0110) < 1))
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
