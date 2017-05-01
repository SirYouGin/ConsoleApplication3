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
        void OnTestError(ITest test, TestEventArgs args);
            
    }

    public interface IBlockEvents
    {        
        void OnBlockStart(IBlock block);
        void OnBlockFinish(IBlock block);
        void OnBlockError(IBlock block, TestEventArgs args);        
    }

    public interface IElementEvents
    {
        void OnElementStart(IElement element);
        void OnElementFinish(IElement element);
        void OnElementError(IElement element, TestEventArgs args);
    }

    public interface IEvents :ITestEvents, IBlockEvents, IElementEvents
    {
        event TestStartEvent testStart;
        event TestFinishEvent testFinish;
        event TestErrorEvent testError;
        event BlockStartEvent blockStart;
        event BlockFinishEvent blockFinish;
        event BlockErrorEvent blockError;
        event ElementStartEvent elementStart;
        event ElementFinishEvent elementFinish;
        event ElementErrorEvent elementError;
    
    }
}
