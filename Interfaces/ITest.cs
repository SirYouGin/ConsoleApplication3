using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3.Interfaces
{
    public interface ITest : IReadOnlyCollection<IBlock>, IBlockEvents, IElementEvents
    {
        string Name {get;set;}
        string Overlap { get; set; }
        string WinId { get; set; }
        string Session { get; }        
        IContext ctx { get; }
        void Execute();
    }
}
