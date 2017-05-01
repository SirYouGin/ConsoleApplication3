using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3
{
    public interface IDBCommand
    {
        void withParam(string param, string value);
        void withOutputParam(string param, string value = null);
        void withCursor(string param);
        void asPLSQLBlock();
        object Execute();        
    }
    public interface ICommandSupport
    {
        int Timeout { get; set; }
        IDBCommand getCommand(string procName, int Timeout = 0);        
    }
    public interface IConnection : IDisposable, ICommandSupport
    {
        void Open();
        void Close(bool Force = false);        
        void Commit();
        void Rollback();
        string SessionInfo();
    }
}
