using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using ConsoleApplication3.Events;
using ConsoleApplication3.Interfaces;
using ConsoleApplication3.Implementations;

namespace ConsoleApplication3
{
    class Program
    {                
        static void Main(string[] args)
        {

            GlobalConfig conf = new GlobalConfig();

            conf.readConfig();

            Session session = new Session(conf);

            IList<IDictionary<string,string>> cases = session.getCases();

            if (cases.Count == 0) throw new Exception("No tests found.");

            TestSet ts = new TestSet(session, cases);

            ts.Run();

            /*   
            XmlDocument doc = new XmlDocument();
            doc.Load("tests.xml");

            try
            {
                IContext ctx = new Context();
                ctx.updateFrom(GlobalConfig.getProperties());

                ConnectionFactory.Init();

                ITestSet testSet = new TestSet(doc, ctx);


                testSet.testStart += testSet_testStart;
                testSet.testError += testSet_testError;
                testSet.testFinish += testSet_testFinish;
            
                testSet.blockStart +=  testSet_blockStart;
                testSet.blockError += testSet_blockError;
                testSet.blockFinish += testSet_blockFinish;

                testSet.elementStart += testSet_elementStart;
                testSet.elementFinish += testSet_elementFinish;
                testSet.elementError += testSet_elementError;
            
                
                bool r = testSet.Run();
                Console.WriteLine("Test count: {0}, Run:{1}", testSet.Count, r);
            }
            catch (Exception e)
            {
                Console.WriteLine("TestSet Error: {0}", e);
            }

            */
            Console.WriteLine("Press any key to close this window");
            Console.ReadKey();
        }


        static void testSet_elementStart(IElement sender)
        {
            Console.WriteLine("\t\tELEMENT:{0} start", sender.Name);
        }
        static void testSet_elementFinish(IElement sender)
        {
            Console.WriteLine("\t\tELEMENT:{0} finish", sender.Name);
        }
        static void testSet_elementError(IElement sender, Exception e)
        {
            //Console.WriteLine("\t\tELEMENT:{0} error: {1}", sender.Name, args.e.Message);
            //throw args.e;
        }
        static void testSet_blockStart(IBlock sender)
        {
            Console.WriteLine("\tBLOCK:{0} start", sender.Name);
        }
        static void testSet_blockFinish(IBlock sender)
        {
            Console.WriteLine("\tBLOCK:{0} finish", sender.Name);
        }
        static void testSet_blockError(IBlock sender,Exception e)
        {
            //Console.WriteLine("\tBLOCK:{0} error: {1}", sender.Name, args.e.Message);
            //throw args.e;
        }
        static void testSet_testStart(ITest sender)
        {
            Console.WriteLine("TEST:{0} start", sender.Name);
        }
        static void testSet_testFinish(ITest sender)
        {
            Console.WriteLine("TEST:{0} finish", sender.Name);
        }
        static void testSet_testError(ITest sender, Exception e)
        {
           
            Console.WriteLine("TEST:{0} error: {1}", sender.Name,e);
        }

        
    }
}
