using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSearch
{
    public class Logger
    {
        public static void Print(string message)
        {
            Debug.WriteLine("[INFO]: "+ message);
        }
    }
}
