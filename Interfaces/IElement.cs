using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ConsoleApplication3.Interfaces
{
    public interface IElement
    {        
        string Name {get; set;}
        string Id { get; set; }
        string Session { get; }
        IBlock Owner {get;set;}        
        IContext ctx {get;}        
        void Execute();
        void Initialize(IDictionary<string, string> dict);
    }
}
