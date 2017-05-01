using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConsoleApplication3.Interfaces;

namespace ConsoleApplication3.Events
{
    public delegate void TestStartEvent(ITest sender);
    public delegate void TestFinishEvent(ITest sender);
    public delegate void TestErrorEvent(ITest sender, TestEventArgs args);
    public delegate void BlockStartEvent(IBlock sender);
    public delegate void BlockFinishEvent(IBlock sender);
    public delegate void BlockErrorEvent(IBlock sender, TestEventArgs args);
    public delegate void ElementStartEvent(IElement sender);
    public delegate void ElementFinishEvent(IElement sender);
    public delegate void ElementErrorEvent(IElement sender, TestEventArgs args);

    public class TestEventArgs : EventArgs
    {
        public Exception e;
        public bool cancel;
        public TestEventArgs(Exception ex) { e = ex; cancel = false; }

    }

    public delegate void LogEvent(object sender, LogEventArgs args);

    
    public class LogEventArgs : EventArgs
    {
        public Exception e;
        public string message;
        public string category;

        public LogEventArgs(string Category, string m, Exception ex = null) { e = ex; category = Category; message = m; }

    }
}
