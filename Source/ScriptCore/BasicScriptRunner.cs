using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp;
using MoonSharp.Interpreter;
namespace ScriptCore
{
    public class BasicScriptRunner
    {

        public Script lua { get; private set; }
        public Table Globals => lua.Globals;

        public BasicScriptRunner() {
            lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            GlobalScriptBindings.Initialize(lua);
        }

        public void AddBindings(ScriptBindings bindings)
        {
            bindings.Initialize(lua);
        }

        public void RemoveBindings(ScriptBindings bindings)
        {
            bindings.Clean(lua);
        }

        public BasicScriptRunner(ScriptBindings bindings) : this()
        {
            bindings.Initialize(lua);
        }
        
        public void Run(string script)
        {
            lua.DoString(script);
        }

        public DynValue Query(string script)
        {
            return lua.DoString(script);
        }

    }
}
