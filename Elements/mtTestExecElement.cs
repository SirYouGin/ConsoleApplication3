using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConsoleApplication3.Interfaces;
using ConsoleApplication3.Implementations;

namespace ConsoleApplication3.Elements
{
    class mtTestExecElement : Element, IElement
    {

        public override void Execute()
        {
            Console.WriteLine("{0}: Execute", GetType());

            using (IConnection session = ConnectionFactory.makeConnection())
            {
                IDBCommand cmd = session.getCommand("Z$SCRIPRS_LIB_API.mt_test_exec");
                cmd.withParam("P_SESSION_FILE_NAME", Owner.Session);
                cmd.withParam("P_TEST_MT", prop["mt_test"]);
                cmd.withParam("P_CONDITION", prop["condition"]);
                cmd.Execute();

                session.Commit();
            };
        }

        public override void Initialize(IDictionary<string, string> dict)
        {
            base.Initialize(dict);
            Console.WriteLine("{0}: Initialize", GetType());            
        }
    }
}
