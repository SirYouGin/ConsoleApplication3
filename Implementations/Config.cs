using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration;
using System.Threading.Tasks;

using ConsoleApplication3.Interfaces;
using System.Collections.Specialized;

namespace ConsoleApplication3.Implementations
{
    class Config : IConfig
    {
        protected readonly Dictionary<string, string> m_params;

        public Config()
        {
            m_params = new Dictionary<string, string>();
        }
        public string this[string index]
        {
            get
            {
                return m_params.ContainsKey(index.ToLower()) ? m_params[index.ToLower()] : null;
            }
        }

        public bool hasProperty(string name) { return m_params.ContainsKey(name.ToLower()); }
        public IDictionary<string, string> toDictionary()
        {
            return new Dictionary<string,string>(m_params);
        }

        public void updateFrom(IDictionary<string, string> src)
        {
            m_params.updateFrom(src);
        }

       
    }
    
}
