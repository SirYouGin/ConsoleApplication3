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
    class Block : IBlock
    {
        private readonly HashSet<IElement> m_elementList = new HashSet<IElement>();
        private readonly ITest Owner;

        public string Name { get; set; }
        public string Code { get; set; }
        public string Num { get; set; }

        public string Session { get { return Owner.Session; } }
        public IContext ctx { get { return Owner.ctx; } }

        public Block(ITest owner, XmlNode parent)
        {
            Owner = owner;
            XmlAttributeCollection attrib = parent.Attributes;
            Name = attrib["name"].Value;
            Code = attrib["code"].Value;
            Num = attrib["num"].Value;

            foreach (XmlNode child in parent.FirstChild.ChildNodes)
                m_elementList.Add(ElementFactory.makeElement(this, child));
        }
        public int Count
        {
            get
            {
                return m_elementList.Count;
            }
        }

        public void Execute()
        {
            foreach (IElement elem in m_elementList)
            {
                try
                {
                    OnElementStart(elem);
                    elem.Execute();
                    OnElementFinish(elem);
                }
                catch (Exception e)
                {
                    ExceptionDispatchInfo edi = ExceptionDispatchInfo.Capture(e); 
                    TestEventArgs args = new TestEventArgs(e);
                    OnElementError(elem, args);
                    if (!args.cancel) edi.Throw();
                }
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
    }
}
