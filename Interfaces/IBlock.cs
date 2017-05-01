using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3.Interfaces
{
    public interface IBlock : IReadOnlyCollection<IElement>, IElementEvents
    {
        string Name { get; set;}
        string Num { get; set; }
        string Code { get; set; }
        string Session { get; }
        IContext ctx { get; }
        void Execute();
    }
}
