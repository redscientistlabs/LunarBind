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
using ScriptCore;
using System.Runtime.InteropServices;
using ScriptCore.Attributes;

namespace LuaGUI
{
    public partial class Form1 : Form
    {
        //Console show stuff
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        ScriptRunner testScriptRunner = null;
        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;

            //Start script initializer
            ScriptInitializer.Start();
            //Register types you cannot put an attribute on
            //ScriptInitializer.RegisterType(typeof(Control));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
            Console.WriteLine("Is 64 bit process: " + Environment.Is64BitProcess.ToString());
        }

        [LuaCallback("PrintPlusA")]
        public static void PrintPlusA(string s)
        {
            Console.WriteLine("(A) " + s);
        }

        private void bTest_Click(object sender, EventArgs e)
        {
           
        }

        private void bStart_Click(object sender, EventArgs e)
        {
            testScriptRunner = new ScriptRunner();
            //testScriptRunner.LoadScript(tbStashkey1.Text);
            //testScriptRunner.SetCurrentScript(0);
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

        private void bAbort_Click(object sender, EventArgs e)
        {
            testScriptRunner.Abort();
        }

        private void bSetStashkey_Click(object sender, EventArgs e)
        {
            //testScriptRunner?.SetCurrentScript((int)nmStashkey.Value);
            testScriptRunner?.LoadScript(tbScript.Text);
            Console.WriteLine($"Loaded script");
            //Console.WriteLine($"Set Stashkey Script to index {((int)nmStashkey.Value)}");
        }

        private void bCallHook_Click(object sender, EventArgs e)
        {
            Console.WriteLine($"Executing {tbHook.Text}");
            testScriptRunner?.Execute(tbHook.Text);
        }
    }
}
