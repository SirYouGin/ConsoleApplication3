using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleApplication3.Interfaces
{
    public interface IElement : IElementEvents
    {        
        string Name {get; set;}
        string Id { get; set; }
        IBlock Block { get; set; }        
        void Execute();
        void Initialize(IDictionary<string, string> dict);
    }
}
