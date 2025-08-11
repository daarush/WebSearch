using System.Diagnostics;

namespace WebSearch
{
    public class Logger
    {
        public static void Print(string message)
        {
            string logMessage = $"[{DateTime.Now:HH:mm:ss}] [INFO]: {message}";
            Debug.WriteLine(logMessage);
            File.AppendAllText(Path.Combine(Constants.AppDataFolder, Constants.LoggerFileName), logMessage + Environment.NewLine);
        }
    }
}
