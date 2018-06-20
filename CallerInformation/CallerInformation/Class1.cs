using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CallerInformation
{
    public static class Class1
    {
        public static void Do1()
        {
            Do2();
        }
        public static void Do2()
        {
            Program.WriteInformation<Program>();
            Program.WriteInformationSimple();
            Program.WriteInformationStackTrace();
        }
    }
}
