//********************************************************************************************
//Author: Sergey Stoyan
//        sergey.stoyan@gmail.com
//        sergey_stoyan@yahoo.com
//        http://www.cliversoft.com
//        26 September 2006
//Copyright: (C) 2006-2013, Sergey Stoyan
//********************************************************************************************
using System;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Cliver
{
    public partial class Log
    {
        public class ThreadLog : Thread
        {
            ThreadLog(int id, string file_name)
                : base(id.ToString(), file_name)
            {
                this.Id = id;
            }
            
            internal const int MAIN_THREAD_LOG_ID = -1;
            
            override protected string get_directory()
            {
                switch (Log.mode)
                {
                    case Log.Mode.ONLY_LOG:
                        return Log.WorkDir + @"\";
                    //case Log.Mode.SINGLE_SESSION:
                    case Log.Mode.SESSIONS:
                        return Log.SessionDir + @"\";
                    //case Log.Mode.SESSIONS:
                    //    throw new Exception("ThreadLog cannot be used in Mode.SESSIONS.");
                    default:
                        throw new Exception("Unknown LOGGING_MODE:" + Log.mode);
                }
            }
            
            /// <summary>
            /// Log belonging to the first (main) thread of the process.
            /// </summary>
            public static ThreadLog Main
            {
                get
                {
                    return get_thread_log(Log.MainThread);
                }
            }

            ///// <summary>
            ///// Log belonging to the first (main) thread of the process.
            ///// </summary>
            //public static ThreadLog Main
            //{
            //    get
            //    {
            //        return Session.Main;
            //    }
            //}

            ///// <summary>
            ///// Log beloning to the current thread.
            ///// </summary>
            //public static ThreadLog This
            //{
            //    get
            //    {
            //        return get_log_thread(System.Threading.Thread.CurrentThread);
            //    }
            //}

            /// <summary>
            /// Log beloning to the current thread.
            /// </summary>
            public static ThreadLog This
            {
                get
                {
                    return get_thread_log(System.Threading.Thread.CurrentThread);
                }
            }

            public static int TotalErrorCount
            {
                get
                {
                    lock (thread2tls)
                    {
                        int ec = 0;
                        foreach (Thread tl in thread2tls.Values)
                            ec += tl.ErrorCount;
                        return ec;
                    }
                }
            }

            public static void CloseAll()
            {
                lock (thread2tls)
                {
                    foreach (Thread tl in thread2tls.Values)
                        tl.Close();
                    thread2tls.Clear();

                    exiting_thread = null;
                }
            }

            /// <summary>
            /// Log id that is used for logging and browsing in GUI
            /// </summary>
            public readonly int Id = MAIN_THREAD_LOG_ID;

            static Dictionary<System.Threading.Thread, ThreadLog> thread2tls = new Dictionary<System.Threading.Thread, ThreadLog>();

            static ThreadLog get_thread_log(System.Threading.Thread thread)
            {
                lock (thread2tls)
                {
                    ThreadLog tl;
                    if (!thread2tls.TryGetValue(thread, out tl))
                    {
                        try
                        {
                            //cleanup for dead thread logs
                            List<System.Threading.Thread> old_log_keys = (from t in thread2tls.Keys where !t.IsAlive select t).ToList();
                            foreach (System.Threading.Thread t in old_log_keys)
                            {
                                if (t.ThreadState != System.Threading.ThreadState.Stopped)
                                {
                                    thread2tls[t].Error("This thread is detected as not alive. Aborting it...");
                                    t.Abort();
                                }
                                thread2tls[t].Close();
                                thread2tls.Remove(t);
                            }

                            int log_id;
                            if (thread == Log.MainThread)
                                log_id = MAIN_THREAD_LOG_ID;
                            else
                            {
                                log_id = 1;
                                var ids = from x in thread2tls.Keys orderby thread2tls[x].Id select thread2tls[x].Id;
                                foreach (int id in ids)
                                    if (log_id == id) log_id++;
                            }

                            string log_name = Log.EntryAssemblyName;
                            if (log_id < 0)
                                log_name = "_" + Log.TimeMark + ".log";
                            else
                                log_name += "_" + log_id.ToString() + "_" + Log.TimeMark + ".log";

                            tl = new ThreadLog(log_id, log_name);
                            thread2tls.Add(thread, tl);
                        }
                        catch (Exception e)
                        {
                            Log.Main.Error(e);
                        }
                    }
                    return tl;
                }
            }
        }
    }
}