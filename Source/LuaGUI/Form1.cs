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

namespace LuaGUI
{
    public partial class Form1 : Form
    {
        ScriptRunner testScriptRunner = null;

        //Console show stuff
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;

            //ADD FULL NAMESPACES
            //Scripts.AddAssemblyAndNamespaces(GetType().Assembly.FullName, nameof(LuaGUI));

            //ADD INDIVIDUAL FUNCTIONS 
            ScriptInitializer.RegisterFunc("printA", null, GetType().GetMethod("PrintPlusA", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            ScriptInitializer.RegisterFunc("peekByte", null, typeof(CSharpClass).GetMethod("PeekByte", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            AllocConsole();
            Console.WriteLine("Is 64: " + Environment.Is64BitProcess.ToString());
        }

        public static void PrintPlusA(string s)
        {
            Console.WriteLine("(A) " + s);
        }

        private void bTest_Click(object sender, EventArgs e)
        {
            ScriptRunner scriptRunner = new ScriptRunner();
            scriptRunner.LoadScripts(null,null,tbScript.Text);
            scriptRunner.SetCurrentScript(1);

            Console.WriteLine("Script created");

            Stopwatch w = new Stopwatch();
            w.Start();
            for (int j = 0; j < 60; j++)
            {
                Console.WriteLine($"LOOP {j} ============================");
                Console.WriteLine($"(C#) PreExecute {j}");
                scriptRunner.PreExecute();
                Console.WriteLine($"\r\n(C#) Execute {j}");
                scriptRunner.Execute();
                Console.WriteLine($"\r\n(C#) PostExecute {j}");
                scriptRunner.PostExecute();
            }
            Console.WriteLine(w.Elapsed);
            w.Stop();

            scriptRunner.Dispose();
        }

        private void bStart_Click(object sender, EventArgs e)
        {
            testScriptRunner?.Dispose();

            testScriptRunner = new ScriptRunner();
            testScriptRunner.LoadScripts(tbScript.Text);
            bExecute.Enabled = true;
            bDispose.Enabled = true;
            bAbort.Enabled = true;

            bStart.Enabled = false;
        }

        private void bExecute_Click(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine($"LOOP ============================");
                Console.WriteLine($"(C#) PreExecute");
                testScriptRunner.PreExecute();
                Console.WriteLine($"\r\n(C#) Execute");
                testScriptRunner.Execute();
                Console.WriteLine($"\r\n(C#) PostExecute");
                testScriptRunner.PostExecute();
            }
            catch 
            {
                Console.WriteLine("ERROR: Script was aborted!");
                bDispose_Click(null, null);
            }
        }

        private void bDispose_Click(object sender, EventArgs e)
        {
            testScriptRunner.Dispose();
            testScriptRunner = null;
            bExecute.Enabled = false;
            bDispose.Enabled = false;
            bAbort.Enabled = false;
            bStart.Enabled = true;
        }

        private void bAbort_Click(object sender, EventArgs e)
        {
            testScriptRunner.Abort();
        }
    }
}
