using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Runtime.ExceptionServices;

using ConsoleApplication3.Events;
using ConsoleApplication3.Interfaces;

namespace ConsoleApplication3.Implementations
{
    class Block : IBlock, IBlockEvents
    {
        private readonly List<IElement> m_elementList = new List<IElement>();
        private readonly Config m_conf = new Config();
        private readonly ITest m_Test;

        public string Name { get { return m_conf["name"]; } }
        public string Code { get { return m_conf ["code"]; } }
        public string Num { get { return m_conf["num"]; } }
        public ITest test { get { return m_Test; } }
        public Block(ITest t, Dictionary<string, string> param_set, XmlNode root)
        {
            m_Test = t;
            m_conf.updateFrom(param_set);
            foreach (XmlNode node in root.FirstChild.ChildNodes)
            {
                Dictionary<string, string> p = new Dictionary<string, string>();
                foreach (XmlAttribute attrib in node.Attributes)
                    p.Add(attrib.Name, attrib.Value);
                IElement el = ElementFactory.makeElement(this, p, node);
                m_elementList.Add(el);
            }
        }
        
        public int Count
        {
            get
            {
                return m_elementList.Count;
            }
        }

        public event BlockStartEvent blockStart;
        public event BlockFinishEvent blockFinish;
        public event BlockErrorEvent blockError;

        public void OnBlockStart(IBlock block)
        {
            if (blockStart != null)
            {
                BlockStartEvent evnt = blockStart; //avoid race condition
                evnt(block);
            }
        }
        public void OnBlockFinish(IBlock block)
        {
            if (blockFinish != null)
            {
                BlockFinishEvent evnt = blockFinish; //avoid race condition
                evnt(block);
            }
        }
        public void OnBlockError(IBlock block, Exception e)
        {
            if (blockError != null)
            {
                BlockErrorEvent evnt = blockError; //avoid race condition
                evnt(block, e);
            }
        }

        public void Execute()
        {
           
            try
            {
                OnBlockStart(this);
                internalExecute();
                OnBlockFinish(this);
            }
            catch (BlockException e)
            {
                OnBlockError(this, e);
            }
        }
        protected void internalExecute()
        {
            foreach (IElement elem in m_elementList)
            {
                elem.Execute();                
            }
        }

        public IEnumerator<IElement> GetEnumerator()
        {
            return m_elementList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }        
    }
}
