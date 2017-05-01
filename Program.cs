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
        public interface IConfig
        {
            string getProperty(string param);
            void setProperty(string param);
            IDictionary<string,string> getConfig();            
        }

        
        static void Main(string[] args)
        {

                        
               
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
        static void testSet_elementError(IElement sender, TestEventArgs args)
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
        static void testSet_blockError(IBlock sender, TestEventArgs args)
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
        static void testSet_testError(ITest sender, TestEventArgs args)
        {
            args.cancel = true;
            Console.WriteLine("TEST:{0} error: {1}", sender.Name, args.e);
        }

        
    }
}
