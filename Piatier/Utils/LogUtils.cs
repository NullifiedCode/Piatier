using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Piatier.Utils
{
    internal class LogUtils
    {
        private static int logNumber = 0;
        public static string FormatLog(string text)
        {
            logNumber++;
            return $"[{logNumber}] {text}\n";
        }
    }
}
