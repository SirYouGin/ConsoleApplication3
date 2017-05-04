using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3.Interfaces
{
    public interface IConfig
    {
        string this[string index] { get; }
        bool hasProperty(string name);
        IDictionary<string, string> toDictionary();
        void updateFrom(IDictionary<string, string> src);
    }
}
