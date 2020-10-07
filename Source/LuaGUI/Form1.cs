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
        Script testScript = null;

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
            Scripts.RegisterFunc("printA", null, GetType().GetMethod("PrintPlusA", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            Scripts.RegisterFunc("peekByte", null, typeof(CSharpClass).GetMethod("PeekByte", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public));
            //TODO: ADD TYPES
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
           

            Script script = Scripts.Create(tbScript.Text);
            Console.WriteLine("Script created");

            Stopwatch w = new Stopwatch();
            w.Start();
            for (int j = 0; j < 60; j++)
            {
                Console.WriteLine($"LOOP {j} ============================");
                Console.WriteLine($"(C#) PreExecute {j}");
                script.PreExecute();
                Console.WriteLine($"\r\n(C#) Execute {j}");
                script.Execute();
                Console.WriteLine($"\r\n(C#) PostExecute {j}");
                script.PostExecute();
            }
            Console.WriteLine(w.Elapsed);
            w.Stop();

            script.Dispose();
        }

        private void bStart_Click(object sender, EventArgs e)
        {
            testScript?.Dispose();
            testScript = Scripts.Create(tbScript.Text);
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
                testScript.PreExecute();
                Console.WriteLine($"\r\n(C#) Execute");
                testScript.Execute();
                Console.WriteLine($"\r\n(C#) PostExecute");
                testScript.PostExecute();
            }
            catch 
            {
                Console.WriteLine("ERROR: Script was aborted!");
                bDispose_Click(null, null);
            }
        }

        private void bDispose_Click(object sender, EventArgs e)
        {
            testScript.Dispose();
            testScript = null;
            bExecute.Enabled = false;
            bDispose.Enabled = false;
            bAbort.Enabled = false;
            bStart.Enabled = true;
        }

        private void bAbort_Click(object sender, EventArgs e)
        {
            testScript.Abort();
        }
    }
}
