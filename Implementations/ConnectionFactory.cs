using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3.Implementations
{
    public static class ConnectionFactory
    {        
        public static IConnection makeConnection(string connectStr) { return new IbsoConnection(connectStr); }
        public static IConnection makeConnection(string user, string pwd, string dbname) { return new IbsoConnection(user, pwd, dbname); }
        public static IConnection makeConnection() { return new IbsoConnection(); }
    }
}
