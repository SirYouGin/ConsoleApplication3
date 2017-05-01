using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Specialized;
using System.Configuration;

namespace ConsoleApplication3
{
    public static class SystemUtil
    {
        public static void CloseProcessByName(String name)
        {
            System.Diagnostics.Process[] local_procs = System.Diagnostics.Process.GetProcesses();
            IEnumerable<System.Diagnostics.Process> target_procs;
            if (name.EndsWith("*"))
            {
                target_procs = local_procs.Where(p => p.ProcessName.StartsWith(name.Substring(0, name.Length-1)));
            }
            else
            {
                target_procs = local_procs.Where(p => p.ProcessName == name);
            }
            foreach (System.Diagnostics.Process target_proc in target_procs)
            {
                try
                {
                    target_proc.Kill();
                }
                catch (Exception) { }
            }

        }

        public static void Run(String name)
        {
            //System.Diagnostics.Process pr = 
            System.Diagnostics.Process.Start(name);
        }

        public static IDictionary<string, string> ToDictionary(this NameValueCollection nvc, IDictionary<string, string> copyFrom = null)
        {
            var dict = nvc.AllKeys.ToDictionary(k => k, k => nvc[k]);
            if (copyFrom != null)
                foreach (var elem in copyFrom) if (!dict.ContainsKey(elem.Key.ToLower())) dict.Add(elem.Key.ToLower(), elem.Value);                
            return dict;
        }

        public static IDictionary<string, string> ToDictionary(this KeyValueConfigurationCollection kvc)
        {
            return kvc.AllKeys.ToDictionary(k => k.ToLower(), k => kvc[k].Value);
        }

        public static IDictionary<string, string> updateFrom(this IDictionary<string, string> dict, IDictionary<string, string> copyFrom = null)
        {
            if (copyFrom != null)
                foreach (var elem in copyFrom)
                    if (!dict.ContainsKey(elem.Key.ToLower())) dict.Add(elem.Key.ToLower(), elem.Value);
                    else dict[elem.Key.ToLower()] = elem.Value;
            return dict;
        }
    }
}
