using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class Log
    {
        public static void Info(object msg)
        {
            Console.WriteLine(msg);
        }

        public static void Error(object msg)
        {
            Console.WriteLine(msg);
        }
    }
}
