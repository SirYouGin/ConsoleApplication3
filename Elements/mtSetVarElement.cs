using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConsoleApplication3.Interfaces;

namespace ConsoleApplication3.Elements
{
    class mtSetVarElement : Element, IElement
    {

        public override void Execute()
        {
            Console.WriteLine("{0}: Execute", GetType());
        }

        public override void Initialize(IDictionary<string, string> dict)
        {
            base.Initialize(dict);
            Console.WriteLine("{0}: Initialize", GetType());
        }
    }
}
