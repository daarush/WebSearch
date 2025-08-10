using System.Diagnostics;

namespace WebSearch
{
    public class Logger
    {
        public static void Print(string message)
        {
            Debug.WriteLine("[INFO]: " + message);
        }
    }
}
