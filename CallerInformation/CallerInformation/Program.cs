using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CallerInformation
{
    class Program
    {
        static void Main(string[] args)
        {
            var date = new DateTime(2018,9,21);

            Console.WriteLine(date.AddDays(-45));
            

            //Class1.Do1();
            //WriteInformation<Program>();
            //WriteInformationSimple();
            //WriteInformationStackTrace();
        }

        public static void WriteInformation<T>([CallerMemberName]string method = "")
        {
            Console.WriteLine(DateTime.Now.ToString() + " => " + typeof(T).FullName + "." + method);
        }

        public static void WriteInformationSimple([CallerFilePath]string callerFilePath = "", [CallerMemberName]string callerMemberName = "")
        {
            Console.WriteLine(DateTime.Now.ToString() + " => " + callerFilePath + "." + callerMemberName);
        }


        public static void WriteInformationStackTrace()
        {
            var stackTrace = new StackTrace();
            foreach (var frame in stackTrace.GetFrames())
            {
                var methodBase = frame.GetMethod();
                var typeName = methodBase.DeclaringType.Name;
                var methodName = methodBase.Name;
                Console.WriteLine($"{typeName}.{methodName}");
            }
            //var methodBase = stackTrace.GetFrame(1).GetMethod();
            //var typeName = methodBase.DeclaringType.Name;
            //var methodName = methodBase.Name;
            //Console.WriteLine($"{typeName}.{methodName}");
        }
    }
}
