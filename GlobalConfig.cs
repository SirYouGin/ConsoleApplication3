using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;
//using NLog;
using System.Reflection;

namespace ConsoleApplication3.Implementations
{
    /// <summary>
    /// Глобальный конфиг сессии выполнения
    /// </summary>
    class GlobalConfig : Config
    {

        public void readConfig()
        {
            string activeProfile = String.Empty;
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                m_params.updateFrom(config.AppSettings.Settings.ToDictionary());
                activeProfile = this["ActiveProfile"];
                if (String.IsNullOrWhiteSpace(activeProfile)) throw new ArgumentNullException("Не задан активный профиль настроек");
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка при чтении файла конфигурации ", e);
            }
            if (!String.IsNullOrEmpty(activeProfile))
            {
                NameValueCollection nvc = ConfigurationManager.GetSection(activeProfile) as NameValueCollection;
                m_params.updateFrom(nvc.ToDictionary());
            }
            //Явно заданые параметры из командной строки перезатирают параметры из файла конфигурации
            m_params.updateFrom(parseArgs(Environment.GetCommandLineArgs()));
        }

        #region Command line predefined params
        private static Dictionary<string, string> flags = new Dictionary<string, string>() { { "-l <log folder path>", "path to logs folder, not null" }
                                                                                            ,{ "-x                  ", "failures only mode if present" }
                                                                                            ,{ "-user <uname>       ", "username for testset search, not null" }
                                                                                            ,{ "-host <vmname>      ", "virtual host name for testset search, not null"}
                                                                                            ,{ "-debug_level <level>", "level of debug. 0 - no debug, 10 - max details, not null"}
                                                                                            ,{ "-encode <encoding>  ", "predefined .NET encoding name like: cp866, windows-1251, utf-8, iso-8859-5, etc"}
                                                                                            ,{ "-h                  ", "print this message"}
                                                                                            };
        public static void printUsage()
        {
            string usage = "usage: runner <flags>\n\n\tflags:\n";
            Console.WriteLine(usage);
            foreach (KeyValuePair<string, string> kvp in flags)
            {
                usage = String.Format("\t{0}\t{1}", kvp.Key, kvp.Value);
                Console.WriteLine(usage);
            }
        }
        private static Dictionary<string, string> parseArgs(string[] args)
        {
            Dictionary<string, string> local = new Dictionary<string, string>();

            string s = String.Join(" ", args);
            string[] p = s.Split(new string[] { " -" }, StringSplitOptions.None);

            bool found = false;

            foreach (KeyValuePair<string, string> kvp in flags)
            {
                string flag = kvp.Key.Substring(1, kvp.Key.IndexOf(' ') - 1).ToLower(); //safe

                for (int i = 0; i < p.Length; i++)
                {
                    found = false;
                    string[] pp = p[i].TrimStart('-').Split(' ');
                    for (int j = 0; j < pp.Length; j++)
                    {
                        if (pp[j].Equals(flag, StringComparison.CurrentCultureIgnoreCase))
                        {
                            if ((j + 1) < pp.Length)
                            {
                                if (!String.IsNullOrWhiteSpace(pp[j + 1]))
                                    local.Add(flag, pp[j + 1]);
                                else
                                {
                                    if (kvp.Value.Contains("not null"))
                                        throw new ArgumentNullException("Не указано значение для параметра -" + flag);
                                    else
                                    {
                                        local.Add(flag, "");
                                    }
                                }

                            }
                            else
                            {
                                if (kvp.Value.Contains("required"))
                                    throw new ArgumentNullException("/" + flag);
                                else
                                {
                                    local.Add(flag, "");
                                }
                            }
                            found = true;
                            break;
                        }
                    }
                    if (found) break;
                }
                if (!found && kvp.Value.Contains("required"))
                    throw new ArgumentException("Не указан обязательный параметр: " + kvp.Key.Trim());
            }
            return local;
        }
        #endregion

    }


}
