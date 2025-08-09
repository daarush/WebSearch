using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSearch
{
    public class BrowserHelper
    {
        public static string GetSystemDefaultBrowser()
        {
            string name = string.Empty;
            RegistryKey? regKey = null;

            try
            {
                var regDefault = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\.htm\\UserChoice", false);
                var stringDefault = regDefault?.GetValue("ProgId") as string;

                if (!string.IsNullOrEmpty(stringDefault))
                {
                    regKey = Registry.ClassesRoot.OpenSubKey(stringDefault + "\\shell\\open\\command", false);
                    var value = regKey?.GetValue(null);
                    if (value != null)
                    {
                        var valueStr = value.ToString();
                        if (!string.IsNullOrEmpty(valueStr))
                        {
                            name = valueStr.ToLower().Replace("" + (char)34, "");

                            if (!name.EndsWith("exe"))
                                name = name.Substring(0, name.LastIndexOf(".exe") + 4);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                name = string.Format("ERROR: An exception of type: {0} occurred in method: {1} in the following module: {2}", ex.GetType(), ex.TargetSite, typeof(BrowserHelper));
            }
            finally
            {
                if (regKey != null)
                    regKey.Close();
            }

            return name;
        }


        public static void searchInANewTab(string defaultBrowserURL, string searchValue)
        {
            var psi = new ProcessStartInfo
            {
                FileName = defaultBrowserURL,
                Arguments = $"https://google.com/search?q={Uri.EscapeDataString(searchValue)}",
                CreateNoWindow = true,
                UseShellExecute = true
            };
            Process.Start(psi);
        }
    }
}
