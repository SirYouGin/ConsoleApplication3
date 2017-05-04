using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

using ConsoleApplication3.Interfaces;
using ConsoleApplication3.Events;

namespace ConsoleApplication3.Implementations
{
    class TestSet : ITestSet
        {
            private readonly IList<IDictionary<string,string>> m_cases;
            private readonly ISession m_session;

            public ISession Session { get { return m_session; } }
            public TestSet(ISession s, IList<IDictionary<string,string>> cases)
            {
                m_session = s;
                m_cases = new List<IDictionary<string,string>>(cases);                
            }
                        
            public void Run()
            {
                foreach (IDictionary<string,string> dict in m_cases)
                {
                    ITestRun t = new TestRun(this, dict);                    
                    t.Execute();                
                }                                
            }                        
        }
}
