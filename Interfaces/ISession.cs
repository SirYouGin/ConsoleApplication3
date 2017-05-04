using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3.Interfaces
{
    public interface ISession
    {
        IConfig conf { get; }
        int Timeout { get; }
        string compName { get; }        
        string userName { get; }
        string failuresOnly { get; }
        string logPath { get; }
    }
}
