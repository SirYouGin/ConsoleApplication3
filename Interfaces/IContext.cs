using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3.Interfaces
{
    public interface IContext
    {
        string get(string prop, string def_value = null);
        void set(string prop, string val);
        IDictionary<string, string> getContext();
        void updateFrom(IDictionary<string, string> _ctx);
    }
}
