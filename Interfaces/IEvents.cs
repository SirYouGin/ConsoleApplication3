using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConsoleApplication3.Events;

namespace ConsoleApplication3.Interfaces
{
    public interface ITestEvents
    {
        
        void OnTestStart(ITest test);
        void OnTestFinish(ITest test);
        void OnTestError(ITest test, Exception e);
            
    }

    public interface IBlockEvents
    {        
        void OnBlockStart(IBlock block);
        void OnBlockFinish(IBlock block);
        void OnBlockError(IBlock block, Exception e);        
    }

    public interface IElementEvents
    {
        void OnElementStart(IElement element);
        void OnElementFinish(IElement element);
        void OnElementError(IElement element, Exception e);
    }    
}
