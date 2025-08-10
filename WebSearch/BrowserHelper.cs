using Microsoft.Win32;
using System.Diagnostics;

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
                var regDefault = Registry.CurrentUser.OpenSubKey(Constants.RegKeyUserChoice, false);
                var stringDefault = regDefault?.GetValue("ProgId") as string;

                if (!string.IsNullOrEmpty(stringDefault))
                {
                    regKey = Registry.ClassesRoot.OpenSubKey(stringDefault + "\\" + Constants.RegKeyShellOpenCommand, false);
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
                name = string.Format(Constants.ErrorMessageFormat, ex.GetType(), ex.TargetSite, typeof(BrowserHelper));
            }
            finally
            {
                if (regKey != null)
                    regKey.Close();
            }

            return name;
        }


        public static void searchInANewTab(string defaultBrowserURL, string searchValue, bool website)
        {
            var psi = new ProcessStartInfo();

            WebSearch.recentSites.Add(new RecentItem
            {
                Title = searchValue,
                Url = $"{Constants.GoogleSearchUrl}{Uri.EscapeDataString(searchValue)}"
            });

            if (website)
            {
                psi = new ProcessStartInfo
                {
                    FileName = searchValue,
                    CreateNoWindow = true,
                    UseShellExecute = true
                };

            } else
            {
                psi = new ProcessStartInfo
                {
                    FileName = defaultBrowserURL,
                    Arguments = $"{Constants.GoogleSearchUrl}{Uri.EscapeDataString(searchValue)}",
                    CreateNoWindow = true,
                    UseShellExecute = true
                };
            }
            Process.Start(psi);
        }
    }
}
