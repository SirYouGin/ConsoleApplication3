using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConsoleApplication3.Interfaces;

namespace ConsoleApplication3.Implementations
{
    class Context : IContext
    {
        Dictionary<string, string> dict = new Dictionary<string, string>();


        public string get(string prop, string def_value = null)
        {
            return dict.ContainsKey(prop.ToLower()) ? dict[prop.ToLower()] : def_value;
        }

        public void set(string prop, string val)
        {
            dict[prop.ToLower()] = val;
        }

        public IDictionary<string, string> getContext()
        {
            return new Dictionary<string, string>(dict);
        }


        public void updateFrom(IDictionary<string, string> _ctx)
        {
            dict.updateFrom(_ctx);
        }
    }
}
