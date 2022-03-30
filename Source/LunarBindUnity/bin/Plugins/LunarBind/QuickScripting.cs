namespace LunarBind
{
    using MoonSharp.Interpreter;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// This class is for quick testing of scripts using global bindings. If you do not need bindings, use <see cref="MoonSharp.Interpreter.Script.RunString(string)"/>
    /// </summary>
    public static class QuickScripting
    {
        public static BasicScriptRunner Basic { get; private set; } = new BasicScriptRunner();
        public static HookedScriptRunner Hooked { get; private set; } = new HookedScriptRunner();

        public static void Run(string script)
        {
            Basic = new BasicScriptRunner();
            Basic.Run(script);
        }

        public static void Run(string script, ScriptBindings bindings)
        {
            Basic = new BasicScriptRunner();
            Basic.AddBindings(bindings);
            Basic.Run(script);
        }

        public static void Run(string script, object target)
        {
            var bindings = new ScriptBindings(target);
            Basic = new BasicScriptRunner(bindings);
            Basic.Run(script);
        }

        /// <summary>
        /// Creates a MoonSharp script with bindings
        /// </summary>
        /// <param name="bindings"></param>
        /// <returns></returns>
        public static Script CreateBoundScript(ScriptBindings bindings = null, CoreModules modules = CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time)
        {
            Script script = new Script(modules);
            GlobalScriptBindings.Initialize(script);
            bindings.Initialize(script);
            return script;
        }
    }

    public static class Optimizers
    {
        public static DynValue CreateOptimizedAutoCoroutine(Script script, string functionName, int numArgs = 0)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < numArgs; i++)
            {
                if(i == 0) { sb.Append("arg0"); }
                else { sb.Append(",arg"); sb.Append(i); }
            }
            string guid = "_OriginalOptimizedFunction_"+ Guid.NewGuid().ToString().Replace('-', '_');
            string paramStr = sb.ToString();
            string overrideStr = $"{guid} = {functionName} function {functionName}({paramStr}){Environment.NewLine} while true do{Environment.NewLine}  {guid}({paramStr}){Environment.NewLine}  coroutine.yield(){Environment.NewLine}  end{Environment.NewLine}end return{functionName}";
            return script.DoString(overrideStr);
        }
    }

}
