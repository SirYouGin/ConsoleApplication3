using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.ExceptionServices;

using ConsoleApplication3.Interfaces;
using ConsoleApplication3.Events;
using System.Data;

namespace ConsoleApplication3.Implementations
{
    class TestRun : ITestRun
    {
        private readonly List<ITest> m_testList = new List<ITest>();
        private readonly ITestSet m_testSet;
        private readonly IConfig m_conf;
        
        public TestRun(ITestSet owner, IDictionary<string, string> param_set)
        {
            m_testSet = owner;
            m_conf = new Config();
            m_conf.updateFrom(param_set);
                
        }
        public int Count
        {
            get
            {
                return m_testList.Count;
            }
        }

        public void Execute()
        {

            internalInit();
            internalExecute();                            
        }
        protected void internalExecute()
        {
            foreach (ITest elem in m_testList)
            {
                elem.Execute();
            }
        }
        public ITestSet testSet { get { return m_testSet; } }
        public string Id { get { return m_conf["testRunId"]; } }
        public string ScenarioId { get { return m_conf["test_params"]; } }
        public string ConturId { get { return m_conf["Contur"]; } }
        public string ProfileId { get { return m_conf["Profile"]; } }
        public string TestSetId { get { return m_conf["TestSet"]; } }
        public string JournalId { get { return m_conf["Journal_Id"]; } }
        public string logsFolder { get { return testSet.Session.logPath; } }
        public string retries { get { return m_conf["tune_errrun"]; } }
       

        protected void internalInit()
        {
            string procName = "Z$ATT_TEST_SCRIPT_LIB.GenNewSession";

            DataTable dt = null;

            using (IbsoConnection conn = new IbsoConnection())
            {
                conn.Open();
                IDBCommand cmd = conn.getCommand(procName, testSet.Session.Timeout);
                cmd.withParam("p_ext_scr_code", ScenarioId);
                cmd.withCursor("P_XML_CLOB");
                cmd.withParam("p_contur", ConturId);
                cmd.withParam("p_profile", ProfileId);
                cmd.withParam("p_testset", TestSetId);
                cmd.withParam("p_jour_id", JournalId);
                cmd.withParam("p_log_path", logsFolder);
                dt = (DataTable)cmd.Execute();
                conn.Commit();
                conn.Close();
            }

            if (dt.Rows.Count != 1 || dt.Columns.Count != 2) throw new TestRunException(String.Format("Вызов процедуры \"{0}\" вернул неверный набор данных. Вернулось строк: {1}, стобцов: {2}. Ожидалось строк: {3}, столбцов: {4}", procName, dt.Rows.Count, dt.Columns.Count,1,2));

            string testRunId = dt.Rows[0][1].ToString();
            if (String.IsNullOrEmpty(testRunId)) throw new TestRunException("Не заполнено обязательное поле \"Имя файла сценария\"");

            IDictionary<string,string> dict = new Dictionary<string, string>();            
            dict.Add("testRunId", dt.Rows[0][1].ToString());
            m_conf.updateFrom(dict);

            XmlDocument doc = new XmlDocument();
            try
            {                
                doc.LoadXml(dt.Rows[0][0].ToString());
            }
            catch (Exception e)
            {
                throw new TestRunException("Не удалось разобрать xml-содержимое файла сценария", e);
            }

            XmlNode test_node = doc.DocumentElement.FirstChild;
            while (test_node != null)
            {
                Dictionary<string, string> p = new Dictionary<string, string>();
                foreach (XmlAttribute attrib in test_node.Attributes)
                {                    
                    p.Add(attrib.Name, attrib.Value);
                }
                ITest t = new Test(this, p, test_node);                
                m_testList.Add(t);
                test_node = test_node.NextSibling;
            }

        }

        public IEnumerator<ITest> GetEnumerator()
        {
            return m_testList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
