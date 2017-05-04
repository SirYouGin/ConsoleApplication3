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
                IDBCommand cmd = session.getCommand("Z$SCRIPTS_LIB_API.mt_exec");
                cmd.withParam("P_SESSION_FILE_NAME", m_conf["SessionId"]);
                cmd.withParam("P_TEST_MT", m_conf["mt_test"]);
                cmd.withParam("P_CONDITION", m_conf["condition"]);
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
