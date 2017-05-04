using System;
using System.Collections.Generic;
using System.Data;
using Oracle.DataAccess.Client;
using System.Threading;
using System.Diagnostics;
using System.Linq;

using ConsoleApplication3.Events;

namespace ConsoleApplication3.Implementations
{
    

    //public enum ConnectionCacheStrategy { Off, On };
    public sealed class IbsoConnection :IDisposable, IConnection
    {
        private Dictionary<string, string> m_sessionInfo;
        private static Dictionary<string, OracleConnection> m_pool;
        private int m_waitTime;
        private bool isOpen;
        private bool releaseLock;        
        private string m_connStr;
        private OracleConnection m_conn;
        private OracleTransaction m_trx;
        //private OracleCommand m_cmd;        
        
        private static string m_defaultConnectionString;

        public event LogEvent evnt;

        private void ConnectionStart(string category, string msg)
        {
            if (evnt != null)
            {
                LogEvent ev = evnt;
                ev(this, new LogEventArgs(category, msg));
            }

        }
        private void ConnectionEnd(string category, string msg)
        {
            if (evnt != null)
            {
                LogEvent ev = evnt;
                ev(this, new LogEventArgs(category, msg));
            }
        }

        public static void setDefaultConnectionString(string connStr)
        {
            m_defaultConnectionString = connStr;
        }

        public static string makeConnectionString(string DBUserName, string DBUserPassword, string DBName)
        {
            OracleConnectionStringBuilder b = new OracleConnectionStringBuilder();
            b.Add("User Id", DBUserName);
            b.Add("Password", DBUserPassword);
            b.Add("Data Source", DBName);
            b.Add("Pooling", false);                
            return b.ConnectionString;          
        }
        //public static ConnectionCacheStrategy CacheConnections { get; set; }

        static IbsoConnection()
        {
            m_pool = new Dictionary<string, OracleConnection>();
            //CacheConnections = ConnectionCacheStrategy.Off;
        }
        private void lockOpen()
        {
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = m_conn;
                cmd.CommandText = "ibs.executor.lock_open";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("retVal", OracleDbType.Varchar2,32000, null, ParameterDirection.ReturnValue);
                cmd.ExecuteNonQuery();
                if (String.IsNullOrEmpty(cmd.Parameters["retVal"].Value.ToString())) throw new Exception("lock_open failed");
                Trace.WriteLine("lockOpen=" + cmd.Parameters["retVal"].Value.ToString());                
            }
        }

        private void lockClear()
        {
            Trace.WriteLine("lockClear");
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = m_conn;
                cmd.CommandText = "ibs.executor.lock_clear";
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
            }
        }

        private void getSessionInfo()
        {
            m_sessionInfo.Clear();
            using (OracleCommand cmd = new OracleCommand())
            {
                cmd.Connection = m_conn;
                cmd.CommandText = "select sys_context('userenv','db_unique_name') dbname, s.* from v$session s where s.audsid = sys_context('userenv','sessionid')";
                cmd.CommandType = CommandType.Text;
                OracleDataReader r = cmd.ExecuteReader();
                try
                {
                    if (r.HasRows)
                    {
                        r.Read();
                        for (int i = 0; i < r.FieldCount; i++)
                        {
                            if (r.IsDBNull(i)) continue;
                            string field = r.GetName(i);
                            string val = String.Empty;
                            switch (r.GetDataTypeName(i))
                            {
                                case "Decimal": val = Convert.ToString(r.GetDecimal(i)); break;
                                case "Varchar2": val = r.GetString(i); break;
                                case "Date": val = Convert.ToString(r.GetDateTime(i)); break;
                                default: val = r.GetDataTypeName(i); break;
                            }
                            if (!String.IsNullOrWhiteSpace(val))
                                m_sessionInfo.Add(field, val);
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally
                {
                    r.Close();
                }
            }
        }
        public void killSession()
        {
            if (m_conn == null) return;
            if (disposedValue) return;
            Trace.WriteLine("killSession");
            //if (CacheConnections == ConnectionCacheStrategy.On)
                resetCache(m_conn);            
            string killinfo = m_sessionInfo["SID"];
            isOpen = false;
            if (String.IsNullOrWhiteSpace(killinfo)) return;
            killinfo = killinfo + "," + m_sessionInfo["SERIAL#"];
            try
            {
                using (OracleConnection c = new OracleConnection(m_connStr))
                {
                    c.Open();
                    using (OracleCommand cmd = new OracleCommand())
                    {
                        cmd.Connection = c;
                        //cmd.CommandText = "begin execute immediate 'alter system kill session ''" + killinfo + "'' immediate'; end;";
                        //cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "AT_TARGET_LIB_SYN.kill_session";
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("sid_serial", OracleDbType.Varchar2, killinfo.Length, killinfo, ParameterDirection.Input);
                        cmd.ExecuteNonQuery();

                    }
                    c.Close();
                }
            }
            catch (Exception e)
            {                
                Debug.WriteLine(e);
            }
        }

        public void Open()
        {
            if (m_conn != null)
            {
                if (m_conn.State == ConnectionState.Closed)
                {
                    m_conn.Open();
                    lockOpen();
                    isOpen = true;
                }
                getSessionInfo();
            }
        }

        public string SessionInfo()
        {
            if (m_sessionInfo.Count == 0) return String.Empty;
            return String.Format("DB: {0}, USER: {1}, SID: {2}, SERIAL#: {3}", m_sessionInfo["DBNAME"], m_sessionInfo["USERNAME"], m_sessionInfo["SID"], m_sessionInfo["SERIAL#"]);
        }
        
        private OracleConnection getConnection(string key = null)
        {
            //if (CacheConnections != ConnectionCacheStrategy.On)
            //    return new OracleConnection(m_connStr);
            string m_key = String.IsNullOrWhiteSpace(key) ? m_connStr.ToUpper() : key.ToUpper();            
            if (!m_pool.ContainsKey(m_key))
                m_pool.Add(m_key, new OracleConnection(m_connStr));            
            return m_pool[m_key];
        }
        private void Reset(string tag = null)
        {
            killSession();
            m_conn = getConnection(tag);
            m_trx = null;

        }
        public void Close(bool clearLocks = false )
        {
            m_trx = null;
            //if (CacheConnections == ConnectionCacheStrategy.On)
            //{
                if (clearLocks) releaseLock = true;
                return;
            //}
            /*
            Trace.WriteLine("Close active connection "+m_conn.ConnectionString);
            if (releaseLock) lockClear();
            m_conn.Close();
            isOpen = false;
            */
        }

        public void resetCache(OracleConnection conn)
        {
            Trace.WriteLine("resetCache");
            //if (CacheConnections != ConnectionCacheStrategy.On) return;
            foreach (var item in m_pool.Where(kvp => kvp.Value == conn).ToList())
            {                
                if (item.Value != null)
                {
                    Trace.WriteLine("Close connection "+m_conn.ConnectionString + " for " + item.Key);
                    // Нельзя дергать, если сессия висит
                    //item.Value.Close();                    
                }
              
                m_pool.Remove(item.Key);
            }            
        }
        public static void flashCache()
        {
            //if (CacheConnections != ConnectionCacheStrategy.On) return;
            Trace.WriteLine("flashCache");
            foreach (KeyValuePair<string, OracleConnection> kvp in m_pool)            
            {
                if (kvp.Value != null)
                {
                    Trace.WriteLine("Close connection "+kvp.Value.ConnectionString+" for "+kvp.Key);
                    kvp.Value.Close();
                }
                
            }            
            m_pool.Clear();
        }
        public void Commit()
        {
            m_trx.Commit();
        }
        public void Rollback()
        {
            m_trx.Rollback();
        }

        public int Timeout { get { return m_waitTime; } set { m_waitTime = value; } }

        public IbsoConnection(string user, string pwd, string dbname, string tag = null) : this(makeConnectionString(user, pwd, dbname), tag) { }

        public IbsoConnection(string connectionString = null, string tag = null)
        {
            m_sessionInfo = new Dictionary<string, string>();
            m_connStr = String.IsNullOrWhiteSpace(connectionString)? m_defaultConnectionString : connectionString;
            m_defaultConnectionString = m_defaultConnectionString ?? connectionString;
            m_waitTime = 5000;
            Reset(tag);
        }

        public IDBCommand getCommand(string procName, int iTimeout = 0)
        {
            IbsoCommand m_cmd = new IbsoCommand(this, procName, iTimeout);
            return m_cmd;
        }
        internal class IbsoCommand : IDBCommand
        {
            internal class ThreadParams
            {
                public OracleCommand cmd;
                public object ResultValue;
                public Exception e;
                public ThreadParams(OracleCommand c) { cmd = c; }
            }

            private OracleCommand m_cmd;
            private IbsoConnection parent;
            public int Timeout { get; set; }

            private void ExecCmd(object p)
            {
                ThreadParams tp = (ThreadParams)p;

                string readCursor = null;
                bool hasOutParams = false;
                foreach (OracleParameter param in tp.cmd.Parameters)
                {
                    if (param.OracleDbType == OracleDbType.RefCursor)
                    {
                        readCursor = param.ParameterName;
                        break;
                    }
                    else
                    {
                        if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                        {
                            hasOutParams = true;
                            break;
                        }
                    }
                }

                try
                {
                    if (!String.IsNullOrEmpty(readCursor))
                    {
                        OracleDataAdapter da = new OracleDataAdapter(tp.cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        tp.ResultValue = dt;
                    }
                    else
                    {
                        if (hasOutParams)
                        {
                            tp.cmd.ExecuteNonQuery();
                            Dictionary<string, object> dict = new Dictionary<string, object>();
                            foreach (OracleParameter param in tp.cmd.Parameters)
                            {
                                if (param.Direction == ParameterDirection.Output || param.Direction == ParameterDirection.InputOutput)
                                {
                                    dict.Add(param.ParameterName, param.Value);
                                }
                            }
                            tp.ResultValue = dict;
                        }
                        else
                            tp.ResultValue = tp.cmd.ExecuteScalar();
                    }
                }
                catch (Exception e)
                {
                    tp.e = e;
                }
            }
            private object ExecCommand(OracleCommand cmd)
            {                
                if (!parent.isOpen) parent.Open();
                cmd.Connection = parent.getConnection();
                if (cmd.CommandTimeout > 0)
                    this.Timeout = cmd.CommandTimeout;
                cmd.CommandTimeout = 0; // не пытаться прервать операция по тайауту ORA-01013: пользователем запрошена отмена текущей операции
                
                parent.ConnectionStart("Session", String.Format("{0}, TIMEOUT(sec): {1}, THREAD: {2} ({3})", parent.SessionInfo(), Timeout, Thread.CurrentThread.Name, Thread.CurrentThread.ManagedThreadId));

                if (parent.m_trx == null || parent.m_trx.Connection == null)
                    parent.m_trx = parent.m_conn.BeginTransaction();

                parent.ConnectionStart("Session", String.Format("TRX: {0}", parent.m_trx.GetHashCode()));

                ThreadParams tp = new ThreadParams(cmd);
                Thread t = new Thread(new ParameterizedThreadStart(ExecCmd));
                t.Start(tp);
                if (!t.Join(this.Timeout * 1000))
                {
                    t.Abort();
                    parent.Reset();
                    throw new TimeoutException("Не удалось завершить выполнение команды за отведенное время: " + this.Timeout + " сек.");
                }
                if (tp.e != null) throw tp.e;
                return tp.ResultValue;
            }

            public IbsoCommand(IbsoConnection owner, string procName, int iTimeout)
            {
                parent = owner;
                m_cmd = new OracleCommand(procName);
                m_cmd.BindByName = true;
                m_cmd.CommandType = CommandType.TableDirect; //Вместо null используем CommandType.TableDirect
                this.Timeout = iTimeout;
                if (this.Timeout == 0) this.Timeout = parent.Timeout;
            }


            public void withParam(string param, string value)
            {
                if (m_cmd == null) throw new ArgumentNullException("OracleCommand");

                if (value != null)
                    m_cmd.Parameters.Add(param, OracleDbType.Varchar2, value.Length, value, ParameterDirection.Input);
                else
                    m_cmd.Parameters.Add(param, OracleDbType.Varchar2, ParameterDirection.Input);                
            }

            public void withOutputParam(string param, string value = null)
            {
                if (m_cmd == null) throw new ArgumentNullException("OracleCommand");

                string outvalue = String.Empty;

                if (value != null)
                    m_cmd.Parameters.Add(param, OracleDbType.Varchar2, 32767, value, ParameterDirection.InputOutput);
                else
                    m_cmd.Parameters.Add(param, OracleDbType.Varchar2, 32767, outvalue, ParameterDirection.Output);               
            }

            public void withCursor(string param)
            {
                if (m_cmd == null) throw new ArgumentNullException("OracleCommand");
                m_cmd.Parameters.Add(param, OracleDbType.RefCursor, ParameterDirection.Output);               
            }

            public void asPLSQLBlock()
            {
                if (m_cmd == null) throw new ArgumentNullException("OracleCommand");
                m_cmd.CommandType = CommandType.Text;                
            }

            public object Execute()
            {
                if (m_cmd == null) throw new ArgumentNullException("OracleCommand");
                if (m_cmd.CommandType == CommandType.TableDirect)
                    m_cmd.CommandType = CommandType.StoredProcedure;
                return ExecCommand(m_cmd);
            }            
        }

        #region IDisposable Support
        private bool disposedValue = false; // Для определения избыточных вызовов        

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: освободить управляемое состояние (управляемые объекты).
                    //if (m_cmd != null) m_cmd.Dispose();
                    if (m_trx != null) m_trx.Dispose();
                    if (m_conn != null)
                        //if (CacheConnections == ConnectionCacheStrategy.On)
                        //{
                            if (m_conn.State == ConnectionState.Open)
                                if (releaseLock) lockClear();
                            if (!m_pool.ContainsValue(m_conn))
                            {
                                m_conn.Dispose();
                            }
                            else
                                m_conn = null;
                        //}
                        //else
                        //{
                        //    m_conn.Dispose();
                        //}

                }

                // TODO: освободить неуправляемые ресурсы (неуправляемые объекты) и переопределить ниже метод завершения.
                // TODO: задать большим полям значение NULL.

                disposedValue = true;
            }
        }

        // TODO: переопределить метод завершения, только если Dispose(bool disposing) выше включает код для освобождения неуправляемых ресурсов.
        // ~IbsoConnection() {
        //   // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
        //   Dispose(false);
        // }

        // Этот код добавлен для правильной реализации шаблона высвобождаемого класса.
        public void Dispose()
        {
            // Не изменяйте этот код. Разместите код очистки выше, в методе Dispose(bool disposing).
            Dispose(true);
            // TODO: раскомментировать следующую строку, если метод завершения переопределен выше.
            GC.SuppressFinalize(this);
        }
        #endregion        


        
    }
}
