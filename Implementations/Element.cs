using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Threading.Tasks;

using ConsoleApplication3.Events;
using ConsoleApplication3.Interfaces;
using ConsoleApplication3.Implementations;


namespace ConsoleApplication3.Elements
{
    public abstract class Element : IElement
    {
        protected IConfig m_conf = new Config();
        public string Name {get;set;}
        public string Id {get;set;}
        public IBlock Block { get; set; }
        
        public abstract void Execute();
        public virtual void Initialize(IDictionary<string, string> dict) { m_conf.updateFrom(dict); }

        public event ElementStartEvent elementStart;
        public event ElementFinishEvent elementFinish;
        public event ElementErrorEvent elementError;

        public void OnElementStart(IElement element)
        {
            if (elementStart != null)
            {
                ElementStartEvent evnt = elementStart; //avoid race condition
                evnt(element);
            }
        }
        public void OnElementFinish(IElement element)
        {
            if (elementFinish != null)
            {
                ElementFinishEvent evnt = elementFinish; //avoid race condition
                evnt(element);
            }
        }
        public void OnElementError(IElement element, Exception e)
        {
            if (elementError != null)
            {
                ElementErrorEvent evnt = elementError; //avoid race condition
                evnt(element, e);
            }
        }
    }
}
