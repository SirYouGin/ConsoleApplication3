using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3.Interfaces
{
    interface ITestSet : IReadOnlyCollection<ITest>, IEvents
    {
        string Session { get; }
        IContext ctx { get; set; }

        bool Run();
    }
}
