using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using LunarBind;
using System.Runtime.InteropServices;
using LunarBind.Yielding;
using LunarBind.Standards;
namespace LuaGUI
{
    public partial class Form1 : Form
    {
        //Console show stuff
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        HookedScriptRunner testScriptRunner = null;

        BasicScriptRunner basicRunner = null;

        //[LunarBindFunction("TestProp2")]
        //public static Action<int> TestProp => (t) =>
        //{
        //    Console.WriteLine($"Prop OK, passed: {t}");
        //};

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            FormClosed += Form1_FormClosed;
            GlobalScriptBindings.AddYieldableType<MyYielder>("MyYielder");
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //QuickScripting.RemoveBindings(new ScriptBindings(this));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
            Console.WriteLine("Is 64 bit process: " + Environment.Is64BitProcess.ToString());
            //Basic runner test
            basicRunner = new BasicScriptRunner((Action<string>)PrintPlusA);
            basicRunner.AddBindings(new ScriptBindings(this));
            basicRunner.Run("PrintPlusA('Hi this is a test from Lua!')");
        }

        [LunarBindFunction("PrintPlusA")]
        public void PrintPlusA(string s)
        {
            Console.WriteLine("(A) " + s);
        }

        [LunarBindFunction("Test.MyTables.PrintPlusA")]
        public void PrintPlusA(string s, string s2)
        {
            Console.WriteLine("(A) " + s + " num two: " + s2);
        }

        [LunarBindFunction("Test.MyTables.YieldPls")]
        public MyYielder YieldPls(string a)
        {
            Console.WriteLine($"My Yielder Go: {a}");
            return new MyYielder(1, "Test");
        }

        private void bTest_Click(object sender, EventArgs e)
        {
           
        }

        private void bStart_Click(object sender, EventArgs e)
        {
            testScriptRunner = new HookedScriptRunner(new ScriptBindings(this));// { AutoRefreshInterval = 1 };
            bExecute.Enabled = true;
            bDispose.Enabled = true;
            bAbort.Enabled = true;
            bSetStashkey.Enabled = true;
            bStart.Enabled = false;
            bCallHook.Enabled = true;
        }

        private void bExecute_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine($"LOOP ============================");
                Console.WriteLine($"(C#) PreExecute");
                testScriptRunner.Execute("PreExecute");
                Console.WriteLine($"\r\n(C#) Execute");
                testScriptRunner.Execute("Execute");
                Console.WriteLine($"\r\n(C#) PostExecute");
                testScriptRunner.Execute("PostExecute");
            }
            catch 
            {
                Console.WriteLine("ERROR: Script was aborted!");
                bDispose_Click(null, null);
            }
        }

        private void bDispose_Click(object sender, EventArgs e)
        {
            testScriptRunner = null;
            bExecute.Enabled = false;
            bDispose.Enabled = false;
            bAbort.Enabled = false;
            bStart.Enabled = true;
            bSetStashkey.Enabled = false;
            bCallHook.Enabled = false;

        }
        int a = 0;
        [LunarBindFunction("AutoCo")]
        public WaitUntil AutoCoroutineTest(int amt, string test)
        {
            Console.WriteLine("Auto : " + test);
            a = 0;
            return new WaitUntil(() => { if (++a > amt) { a = 0; return true; } else { return false; } });
        }

        private void bAbort_Click(object sender, EventArgs e)
        {
            //testScriptRunner.Abort();
        }

        private void bloadScript_Click(object sender, EventArgs e)
        {
            testScriptRunner?.LoadScript(tbScript.Text);
            Console.WriteLine($"Loaded script");
        }

        private void bCallHook_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"Executing {tbHook.Text}");
            testScriptRunner?.Execute(tbHook.Text);
        }

        private void bLoadKeyCoroutine_Click(object sender, EventArgs e)
        {
            testScriptRunner?.Execute("OnStashkeyLoad", 1, "help");
        }

        private void bArrayParamTest_Click(object sender, EventArgs e)
        {
            testScriptRunner?.Execute("ArrayTest", "a", new string[]{ "hi", "bye" } );
            //testScriptRunner?.Execute("ArrayTest", new string[] { "hi", "bye" });
        }
        private void CallbackPass()
        {
            Console.WriteLine("Call C# callback in lua");
        }

        private void bTest0_Click(object sender, EventArgs e)
        {

            string s = "function A() Test.MyTables.PrintPlusA('one', 'two') end " +
                            "function B(l,m)" +
                            "print(l) " +
                            "print(m) " +
                            "coroutine.yield() " +
                            "print(2) " +
                            "coroutine.yield() " +
                            "print(3) " +
                            "end " +
                            "RegisterCoroutine(A, 'A', true) " +
                            "RegisterCoroutine(B, 'B', true) " +
                            "";

            string s2 =     " function() " +
                            "print(4)" +
                            "coroutine.yield()" +
                            "print(5)" +
                            "coroutine.yield()" +
                            "print(6)" +
                            "end ";
            


            bool isCoroutine = true;
            bool autoResetCoroutine = true;
            LuaScriptStandard standard = new LuaScriptStandard(
                new LuaFuncStandard("A", LuaFuncType.AutoCoroutine | LuaFuncType.AllowAny),
                new LuaFuncStandard("B", LuaFuncType.AutoCoroutine | LuaFuncType.AllowAnyCoroutine)
                );

            HookedScriptRunner hsr = new HookedScriptRunner(standard);
            ScriptBindings b = new ScriptBindings(this);
            b.AddDelegate("Test.YieldPls", (Func<int, string, WaitUntil>)AutoCoroutineTest, "", "");
            hsr.AddBindings(b);
            hsr["text"] = "test";
            hsr.LoadScript(s);


            //ScriptHook hook = new ScriptHook(hsr.Lua, s2, true, true);
            //for (int j = 0; j < 30; j++)
            //{
            //    hook.Execute();
            //}
            //ScriptHook shb = hsr.GetHook("B");
            //for (int j = 0; j < 30; j++)
            //{
            //    shb.Execute();
            //}
            //Console.WriteLine("Clone:");
            //ScriptHook shb2 = new ScriptHook(shb) { AutoResetCoroutine = false };
            //for (int j = 0; j < 30; j++)
            //{
            //    shb2.Execute();
            //}

            hsr.LoadScript(s);

            //hsr.RefreshLua();

            hsr.Execute("A");

            for (int j = 0; j < 30; j++)
            {
                Console.WriteLine($"==========C# {j + 1}");
                hsr.Execute("B", j+1, j+2);
            }

            new object();

            //Script s = new Script();
            //s.Globals["PrintPlusA"]
            //var a = CallbackFunction.FromDelegate(s, (Action<string>)PrintPlusA);

            //a.
            //var b = CallbackFunction.FromDelegate(s, (Action<string, string>)PrintPlusA);
            //s.DoString(@"PrintPlusA(")

            //DynValue.NewCallbackFu(CallbackFunction)
            //DynValue.NewYieldReq()
            //Closure c = new Closure();
            //DynValue.FromObject(s.Globals["test"]).Function.GetUpvalue
        }

        private void bRefreshLua_Click(object sender, EventArgs e)
        {
            //testScriptRunner.RefreshLua();
            GC.Collect();
            //Task.Run(async () => { await Task.Delay(1000); GC.Collect(); });
        }
    }

    public class MyYielder : Yielder
    {
        public MyYielder(int a, string b)
        {
            Console.WriteLine("Custom Int" + a.ToString());
            Console.WriteLine("Custom String" + b);
        }

        public override bool CheckStatus()
        {
            return true;
        }
    }

}
