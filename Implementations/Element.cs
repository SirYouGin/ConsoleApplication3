using System;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Threading.Tasks;

using ConsoleApplication3.Interfaces;

namespace ConsoleApplication3.Elements
{
    public abstract class Element : IElement
    {
        protected Dictionary<string, string> prop = new Dictionary<string, string>();
        public string Name {get;set;}
        public string Id {get;set;}
        public IBlock Owner { get; set; }
        public string Session { get { return Owner.Session;  } }
        public IContext ctx { get { return Owner.ctx; } }        
        public abstract void Execute();
        public virtual void Initialize(IDictionary<string, string> dict) { prop.updateFrom(dict); }
    }
}
