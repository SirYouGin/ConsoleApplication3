using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

using ConsoleApplication3.Interfaces;
using ConsoleApplication3.Events;

namespace ConsoleApplication3.Implementations
{
    class Test :ITest
    {
        private readonly HashSet<IBlock> m_blockList = new HashSet<IBlock>();
        private readonly ITestSet Owner;

        public string Name { get; set; }
        public string Overlap { get; set; }
        public string WinId { get; set; }

        public string Session { get { return Owner.Session; } }
        public IContext ctx { get { return Owner.ctx; } }

        public void OnBlockStart(IBlock block)
        {
            Owner.OnBlockStart(block);                
        }
        public void OnBlockFinish(IBlock block)
        {
           Owner.OnBlockFinish(block);                
        }
        public void OnBlockError(IBlock block, TestEventArgs args)
        {
            Owner.OnBlockError(block, args);
        }

        public void OnElementStart(IElement element)
        {
            Owner.OnElementStart(element);
        }
        public void OnElementFinish(IElement element)
        {
            Owner.OnElementFinish(element);
        }
        public void OnElementError(IElement element, TestEventArgs args)
        {
            Owner.OnElementError(element, args);
        }

        public Test(ITestSet owner, XmlNode parent)
        {
            Owner = owner;

            XmlAttributeCollection attrib = parent.Attributes;
            Name    = attrib["testname"].Value;
            Overlap = attrib["name"].Value;
            WinId   = attrib["GetWinID"].Value;

            foreach (XmlNode child in parent.FirstChild.ChildNodes)
                m_blockList.Add(new Block(this, child));
        }
        public int Count
        {
            get
            {
                return m_blockList.Count;
            }
        }

        public void Execute()
        {
            foreach (IBlock b in m_blockList)
            {
                try
                {
                    OnBlockStart(b);
                    b.Execute();
                    OnBlockFinish(b);
                }
                catch (Exception e)
                {
                    ExceptionDispatchInfo edi = ExceptionDispatchInfo.Capture(e); 
                    TestEventArgs args = new TestEventArgs(e);
                    OnBlockError(b, args);
                    if (!args.cancel) edi.Throw();
                }
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
