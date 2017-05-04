using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ConsoleApplication3.Interfaces;
using System.Data;

namespace ConsoleApplication3.Implementations
{
    class Session : ISession
    {
        private IConfig m_conf;
        
        public Session(IConfig conf)
        {
            m_conf = conf;
        }

        public IConfig conf { get { return m_conf; } }

        public string compName { get { return m_conf["host"] ?? Environment.MachineName; } }
        public string userName { get { return m_conf["user"] ?? Environment.UserName; } }
        public string failuresOnly { get { return m_conf.hasProperty("x") ? "1" : "0";  } }
        public string logPath { get { return m_conf["l"] ?? compName + "_" + DateTime.Now.Ticks;} }
        public int Timeout { get { return Int32.Parse(m_conf["req_timeout"] ?? "5000"); } }
        
        public IList<IDictionary<string,string>> getCases()
        {
            const string procName = "Z$att_test_script_lib.GETTESTS";


            List<IDictionary<string,string>> list = new List<IDictionary<string,string>>();

            Dictionary<string, string> dict = null;

            DataTable dt = new DataTable();

            using (IbsoConnection conn = new IbsoConnection(m_conf["db_user"], m_conf["db_password"], m_conf["db_name"]))
            {
                conn.Open();
                IDBCommand cmd = conn.getCommand(procName, Timeout);
                cmd.withParam("P_COMP", compName);
                cmd.withParam("P_USER", userName);
                cmd.withParam("P_RUN_ERR", failuresOnly);
                cmd.withCursor("P_RECORDSET");
                dt = (DataTable)cmd.Execute();
                conn.Commit();
                conn.Close();
            }
            
            foreach (DataRow r in dt.Rows)
            {
                dict = new Dictionary<string, string>();

                foreach (DataColumn c in dt.Columns)
                    dict.Add(c.ColumnName, r[c].ToString());

                list.Add(dict);
            }

            /*
            if (dt.Columns.Count < 6)
            {
                throw new Exception("Выполнение тест сета невозможно. При получении списка тестов получено " + dt.Columns.Count + " полей. Ожидалось не менее 6 полей");
            }

            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    ScenarioRunConfig config = new ScenarioRunConfig();
                    config.TestSetId = dt.Rows[i][0].ToString();
                    config.ScenarioId = dt.Rows[i][1].ToString();
                    config.ProfileId = dt.Rows[i][2].ToString();
                    config.ConturId = dt.Rows[i][3].ToString();
                    string tuneErrString = dt.Rows[i][4].ToString();
                    if (tuneErrString == null || tuneErrString.Equals("")) tuneErrString = "0";
                    config.TuneErrRun = int.Parse(tuneErrString);
                    config.JournalId = dt.Rows[i][5].ToString();

                    if (dt.Columns.Count > 6)
                    {
                        List<string> additionalValues = new List<string>();
                        for (int k = 6; k < dt.Columns.Count; k++)
                        {
                            additionalValues.Add(dt.Rows[i][k].ToString());
                        }
                        config.AdditionalValues = additionalValues.ToArray();
                    }
                    else
                    {
                        config.AdditionalValues = new string[] { };
                    }

                    res.Add(config);
                }
            }*/

            return list;
        }        

        
     }
}
