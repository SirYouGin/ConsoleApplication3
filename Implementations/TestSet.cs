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
            private readonly HashSet<ITest> m_testList = new HashSet<ITest>();

            public event TestStartEvent testStart;            
            public event TestFinishEvent testFinish;            
            public event TestErrorEvent testError;

            private IContext m_ctx;

            public string Session { get { return m_ctx.get("Session","undefined"); } }
            public IContext ctx 
            { 
                get 
                {
                    if (m_ctx == null) throw new NullReferenceException("Контекст сессии не установлен");
                    return m_ctx;
                }
                set 
                {
                    m_ctx = value;
                } 
            }

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
            public void OnTestError(ITest test, TestEventArgs args)
            {
                if (testError != null)
                {
                    TestErrorEvent evnt = testError; //avoid race condition
                    evnt(test, args);
                }
            }

            public event BlockStartEvent blockStart;
            public event BlockFinishEvent blockFinish;
            public event BlockErrorEvent blockError;

            void IBlockEvents.OnBlockStart(IBlock block)
            {
                if (blockStart != null)
                {
                    BlockStartEvent evnt = blockStart; //avoid race condition
                    evnt(block);
                }
            }
            void IBlockEvents.OnBlockFinish(IBlock block)
            {
                if (blockFinish != null)
                {
                    BlockFinishEvent evnt = blockFinish; //avoid race condition
                    evnt(block);
                }
            }
            void IBlockEvents.OnBlockError(IBlock block, TestEventArgs args)
            {
                if (blockError != null)
                {
                    BlockErrorEvent evnt = blockError; //avoid race condition
                    evnt(block, args);
                }
            }

            public event ElementStartEvent elementStart;
            public event ElementFinishEvent elementFinish;
            public event ElementErrorEvent elementError;

            void IElementEvents.OnElementStart(IElement element)
            {
                if (elementStart != null)
                {
                    ElementStartEvent evnt = elementStart; //avoid race condition
                    evnt(element);
                }
            }
            void IElementEvents.OnElementFinish(IElement element)
            {
                if (elementFinish != null)
                {
                    ElementFinishEvent evnt = elementFinish; //avoid race condition
                    evnt(element);
                }
            }
            void IElementEvents.OnElementError(IElement element, TestEventArgs args)
            {
                if (elementError != null)
                {
                    ElementErrorEvent evnt = elementError; //avoid race condition
                    evnt(element, args);
                }
            }

            public TestSet(XmlNode parent, IContext ctx)
            {
                m_ctx = ctx;
                foreach (XmlNode child in parent.ChildNodes)
                    m_testList.Add(new Test(this, child));
            }

            public TestSet(XmlDocument xml, IContext ctx) : this(xml.GetElementsByTagName("root")[0], ctx) { }
            
            public int Count
            {
                get
                {
                    return m_testList.Count;
                }
            }            

            
            public void Add(ITest item)
            {
                throw new NotSupportedException();
            }

            public void Clear()
            {
                throw new NotSupportedException();
            }

            public bool Contains(ITest item)
            {
                return m_testList.Contains(item);
            }

            public void CopyTo(ITest[] array, int arrayIndex)
            {
                foreach (ITest t in m_testList)
                    array[arrayIndex++] = t;
            }

            public IEnumerator<ITest> GetEnumerator()
            {
                return m_testList.GetEnumerator();
            }

            public bool Remove(ITest item)
            {
                throw new NotSupportedException();
            }

            public bool Run()
            {
                uint executed = 0;
                foreach (ITest t in m_testList)
                {
                    try
                    {
                        
                        OnTestStart(t);
                        t.Execute();
                        executed++;
                        OnTestFinish(t);
                    }
                    catch (Exception e)
                    {
                        ExceptionDispatchInfo edi = ExceptionDispatchInfo.Capture(e); 
                        TestEventArgs args = new TestEventArgs(e);
                        OnTestError(t, args);
                        if (!args.cancel) edi.Throw();
                    }
                }
                return (m_testList.Count == executed);
            }



            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            
        }
}
