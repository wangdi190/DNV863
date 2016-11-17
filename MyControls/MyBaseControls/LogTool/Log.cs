using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyBaseControls.LogTool
{
    public enum ELogType {记录,告警,错误,重要}

    public static class Log
    {
        static Log()
        {
            logfile = Directory.GetCurrentDirectory() + "\\log.txt";
            delLogFile();
        }

        static string logfile;
        static Dictionary<int, bool> dictinfo = new Dictionary<int, bool>();

        static void delLogFile()
        {
            if (System.IO.File.Exists(logfile))
                System.IO.File.Delete(logfile);
        }

        ///<summary>允许日志最大行数，缺省1000</summary>
        public static int maxLines = 1000;
        ///<summary>是否写日志</summary>
        public static bool isEnabled = true;

        ///<summary>尝试写入一行日志，记录类型日志自动加时间为行头，其它类型日志自动加相应行头</summary>
        public static void addLog(string s, ELogType logtype)
        {
            if (dictinfo.Count > maxLines || !isEnabled) return;
            if (logtype == ELogType.记录)
            {
                string info = string.Format("[{0}]{1}", DateTime.Now, s);
                int hashcode = info.GetHashCode();
                dictinfo.Add(hashcode, false);
                StreamWriter sw = File.AppendText(logfile);
                sw.WriteLine(info);
                if (dictinfo.Count == maxLines)
                    sw.WriteLine("【注意】日志文件记录行数已超过maxLines限定的行数，后续信息将不再写入日志！");
                sw.Close();
            }
            else
            {
                int hashcode = s.GetHashCode();
                if (!dictinfo.ContainsKey(hashcode))
                {
                    dictinfo.Add(hashcode, false);
                    string title = "";
                    switch (logtype)
                    {
                        case ELogType.告警:
                            title = "〖告警〗";
                            break;
                        case ELogType.错误:
                            title = "【错误】";
                            break;
                        case ELogType.重要:
                            title = "★重要：";
                            break;
                    }
                    StreamWriter sw = File.AppendText(logfile);
                    sw.WriteLine(string.Format("{0}{1}", title, s));
                    if (dictinfo.Count == maxLines)
                        sw.WriteLine("★★★========= 日志文件记录行数已超过maxLines限定的行数，后续信息将不再写入日志！==========★★★");
                    sw.Close();
                }
            }
        }

       
    }
}
