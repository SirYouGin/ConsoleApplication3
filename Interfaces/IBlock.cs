using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3.Interfaces
{
    public interface IBlock : IReadOnlyCollection<IElement>, IBlockEvents
    {
        string Name { get; }
        string Num { get; }
        string Code { get; }
        void Execute();
        ITest test { get; }
    }
}
