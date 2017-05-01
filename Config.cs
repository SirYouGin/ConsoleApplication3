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
    public class GlobalConfig
    {

        public const string DB_INSTANCE_NAME    = "db_instance_name";
        public const string DB_NAME             = "db_name";
        public const string DB_USER_NAME        = "db_user";
        public const string DB_PWD_NAME         = "db_password";
        public const string LOCAL_DIR_NAME      = "local_dir";
        public const string DEBUG_LEVEL_NAME    = "debug_level";
        public const string SYNC                = "sync";
        public const string REQ_TIMEOUT         = "req_timeout";
        public const string DB_OWNER            = "db_owner";
        public const string ADDONS_FOLDER       = "AddonsFolder";
        
        private static IDictionary<string, string> _currentConfig = new Dictionary<string, string>();

        //private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static List<Assembly> _addonAssemblies = new List<Assembly>();

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
        static GlobalConfig()
        {
            Interactive = Environment.UserInteractive;
               
            string activeProfile = String.Empty;            
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                _currentConfig.updateFrom(config.AppSettings.Settings.ToDictionary());
                activeProfile = getProperty("ActiveProfile");
                if (String.IsNullOrWhiteSpace(activeProfile)) throw new ArgumentNullException("Не задан активный профиль настроек");
            }
            catch (Exception e)
            {
                throw new Exception("Ошибка при чтении файла конфигурации ", e);
            }
            if (!String.IsNullOrEmpty(activeProfile))
            {
                NameValueCollection nvc = ConfigurationManager.GetSection(activeProfile) as NameValueCollection;
                _currentConfig.updateFrom(nvc.ToDictionary());
            }

            _currentConfig.updateFrom(parseArgs(Environment.GetCommandLineArgs()));
        }        
        
        public static string getProperty(string name, string def_value = null)
        {
            return _currentConfig.ContainsKey(name.ToLower()) ? _currentConfig[name.ToLower()] : def_value;
        }

        public static bool hasProperty(string name)
        {
            return _currentConfig.ContainsKey(name.ToLower());
        }

        public static void AddAddonAssembly(Assembly assembly)
        {
            _addonAssemblies.Add(assembly);
        }

        public static List<Assembly> AddonAssemblies { get { return _addonAssemblies; } }

        public static void ResetCurrentConfig(Dictionary<string, string> mergeFromConfig)
        {
            _currentConfig.updateFrom(mergeFromConfig);            
        }
        
        /// <summary>
        /// Проверка параметров конфигурации на полноту
        /// </summary>
        /// <returns>true, если все необходимые параметры переданы</returns>
        public static void Check()
        {
            
            if (!hasProperty(DB_NAME)) throw new ArgumentNullException("Отсутствует обязательный параметр '" + DB_NAME + "'");
            if (!hasProperty(DB_USER_NAME)) throw new ArgumentNullException("Отсутствует обязательный параметр '" + DB_USER_NAME + "'");
            if (!hasProperty(DB_PWD_NAME)) throw new ArgumentNullException("Отсутствует обязательный параметр '" + DB_PWD_NAME + "'");
            if (!hasProperty(LOCAL_DIR_NAME)) throw new ArgumentNullException("Отсутствует обязательный параметр '" + LOCAL_DIR_NAME + "'");
            if (!hasProperty(REQ_TIMEOUT)) throw new ArgumentNullException("Отсутствует обязательный параметр '" + REQ_TIMEOUT + "'");
            if (!hasProperty(DEBUG_LEVEL_NAME)) throw new ArgumentNullException("Отсутствует обязательный параметр '" + DEBUG_LEVEL_NAME + "'");

            int lvl = int.Parse(getProperty(DEBUG_LEVEL_NAME));
            if (!(0 <= lvl && lvl <= 10)) throw new ArgumentOutOfRangeException("Параметр '" + DEBUG_LEVEL_NAME + "' должен быть числом от 0 до 10");

            if (!Directory.Exists(AddonsPath)) throw new ArgumentException("Папка с addon-ами '" + AddonsPath + "' не найдена");
            
        }

        public static string AddonsPath { get { return  getProperty(ADDONS_FOLDER, "Addons"); } }
        public static string DBOwner { get { return getProperty(DB_OWNER); } }
        
        /// <summary>
        /// Наименование БД
        /// </summary>
        public static string DBName { get { return getProperty(DB_NAME); } }

        /// <summary>
        /// таймаут
        /// </summary>
        public static int Timeout
        {
            get
            {
                return int.Parse(getProperty(SYNC, "0"));                
            }
        }

        public static int RequestTimeout { get { return int.Parse(getProperty(REQ_TIMEOUT)); } }

        /// <summary>
        /// Пользователь БД
        /// </summary>
        public static string DBUserName { get { return getProperty(DB_USER_NAME); } }
        /// <summary>
        /// Пароль пользователя
        /// </summary>
        public static string DBUserPasswrod { get { return getProperty(DB_PWD_NAME); } }
        /// <summary>
        /// Папка для ресурсов
        /// </summary>
        public static string LocalDir { get { return getProperty(LOCAL_DIR_NAME); } }
        /// <summary>
        /// Уровень логирования ЦФТ
        /// </summary>
        public static int CFTLogLevel { get { return int.Parse(getProperty(DEBUG_LEVEL_NAME)); } }

        public static bool Interactive { get { return (getProperty("interactive", "0") == "1");  } set { _currentConfig["interactive"] = (value ? "1" : "0"); } }
        public static Dictionary<string,string> getProperties()
        {
            return new Dictionary<string, string>(_currentConfig); //return copy
        } 
        
        public static new string ToString()
        {
            string res = "";

            foreach (var key in _currentConfig.Keys)
            {
                res += key + ": '" + _currentConfig[key] + "'\n";
            }

            return res;
        }        

    }


}
