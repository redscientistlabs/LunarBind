using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaGUI
{
    public static class CSharpClass
    {
        public static byte PeekByte(string domain, long address)
        {
            Console.WriteLine("(From C# Method) " + domain);
            return (byte)address;
        }
        public static short PeekShort(string domain, long address)
        {
            Console.WriteLine("From C# " + domain);
            return (short)address;
        }

    }
}
