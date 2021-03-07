
namespace ScriptCore
{
    using System.Collections.Generic;

    public static class QuickScripting
    {
        public static BasicScriptRunner Basic { get; private set; } = new BasicScriptRunner();

        //TODO: make a quick dictionary of basic script runners
        //public static Dictionary<string, BasicScriptRunner> Runners { get; private set; } = new Dictionary<string, BasicScriptRunner>();

        public static void AddBindings(ScriptBindings bindings)
        {
            Basic.AddBindings(bindings);
        }

        public static void RemoveBindings(ScriptBindings bindings)
        {
            Basic.RemoveBindings(bindings);
        }

        public static void Reset()
        {
            Basic.Globals.Clear();
            Basic = new BasicScriptRunner();
        }

        public static void Run(string script)
        {
            Basic.Run(script);
        }

        public static void Run(string script, ScriptBindings bindings)
        {
            Basic.AddBindings(bindings);
            Basic.Run(script);
            Basic.RemoveBindings(bindings);
        }

        public static void Run(string script, object target)
        {
            var bindings = new ScriptBindings(target);
            Basic.AddBindings(bindings);
            Basic.Run(script);
            Basic.RemoveBindings(bindings);
        }

    }
}
