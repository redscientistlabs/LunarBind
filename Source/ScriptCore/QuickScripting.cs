namespace ScriptCore
{
    using System.Collections.Generic;
    /// <summary>
    /// This class is for quick testing of scripts using global bindings. If you do not need bindings, use <see cref="MoonSharp.Interpreter.Script.RunString(string)"/>
    /// </summary>
    public static class QuickScripting
    {
        public static BasicScriptRunner Basic { get; private set; } = new BasicScriptRunner();

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

    }
}
