﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApplication3.Interfaces
{
    public interface ITestSet 
    {        
        void Run();
        ISession Session { get; }
    }
}
