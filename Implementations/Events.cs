using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConsoleApplication3.Interfaces;

namespace ConsoleApplication3.Events
{
    class TestRunException : Exception
    {
        public TestRunException(string message, Exception inner) : base(message, inner) { }
        public TestRunException(string message) : base(message) { }
        public TestRunException() : base() { }
    }
    class TestException : Exception
    {
        public TestException(string message, Exception inner) : base(message, inner) { }
        public TestException(string message) : base(message) { }
        public TestException() : base() { }
    }
    class BlockException : Exception
    {
        public BlockException(string message, Exception inner) : base(message, inner) { }
        public BlockException(string message) : base(message) { }
        public BlockException() : base() { }
    }
    class ElementException : Exception
    {
        public ElementException(string message, Exception inner) : base(message, inner) { }
        public ElementException(string message) : base(message) { }
        public ElementException() : base() { }
    }

    public delegate void TestStartEvent(ITest sender);
    public delegate void TestFinishEvent(ITest sender);
    public delegate void TestErrorEvent(ITest sender, Exception e);
    public delegate void BlockStartEvent(IBlock sender);
    public delegate void BlockFinishEvent(IBlock sender);
    public delegate void BlockErrorEvent(IBlock sender, Exception e);
    public delegate void ElementStartEvent(IElement sender);
    public delegate void ElementFinishEvent(IElement sender);
    public delegate void ElementErrorEvent(IElement sender, Exception e);
    

    public delegate void LogEvent(object sender, LogEventArgs args);

    
    public class LogEventArgs : EventArgs
    {
        public Exception e;
        public string message;
        public string category;

        public LogEventArgs(string Category, string m, Exception ex = null) { e = ex; category = Category; message = m; }

    }
}
