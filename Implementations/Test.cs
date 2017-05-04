using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Threading.Tasks;

using ConsoleApplication3.Events;
using ConsoleApplication3.Interfaces;

namespace ConsoleApplication3.Implementations
{
    class Test : ITest
    {
        private readonly List<IBlock> m_blockList = new List<IBlock>();
        private readonly IConfig m_conf = new Config();
        private readonly ITestRun m_testRun;       
        public string Name { get; set; }
        public string Overlap { get; set; }
        public string WinId { get; set; }
        public ITestRun testRun { get { return m_testRun; } }
        public Test(ITestRun tr, Dictionary<string,string> param_set, XmlNode root)
        {
            m_testRun = tr;
            m_conf.updateFrom(param_set);
            foreach (XmlNode node in root.FirstChild.ChildNodes)
            {
                Dictionary<string, string> p = new Dictionary<string, string>();
                foreach (XmlAttribute attrib in node.Attributes)
                    p.Add(attrib.Name, attrib.Value);
                IBlock b = new Block(this, p, node);
                m_blockList.Add(b);
            }
        }

        public int Count
        {
            get
            {
                return m_blockList.Count;
            }
        }

        

        public event TestStartEvent testStart;
        public event TestFinishEvent testFinish;
        public event TestErrorEvent testError;

        public void OnTestStart(ITest test)
        {
            if (testStart != null)
            {
                TestStartEvent evnt = testStart; //avoid race condition
                evnt(test);
            }
        }
        public void OnTestFinish(ITest test)
        {
            if (testFinish != null)
            {
                TestFinishEvent evnt = testFinish; //avoid race condition
                evnt(test);
            }
        }
        public void OnTestError(ITest test, Exception e)
        {
            if (testError != null)
            {
                TestErrorEvent evnt = testError; //avoid race condition
                evnt(test, e);
            }
        }
        public void Execute()
        {

            try
            {
                OnTestStart(this);
                internalExecute();
                OnTestFinish(this);
            }
            catch (BlockException e)
            {
                OnTestError(this, e);
            }
        }
        protected void internalExecute()
        {
            foreach (IBlock elem in m_blockList)
            {
                elem.Execute();
            }
        }

        public IEnumerator<IBlock> GetEnumerator()
        {
            return m_blockList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
