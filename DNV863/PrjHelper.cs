using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DNV863
{
        internal static class PrjHelper
        {
            ///<summary>生成当前方案及逐基于方案的串表达式, 如(prjid=1 || prjid=2)</summary>
            public static string genProjectsExpress(int prjid, DataTable dtproject, string prefix = "")
            {
                //逐基表读取
                List<int> ids = new List<int>();
                int tmpid = prjid;
                while (true)
                {
                    DataRow dr = dtproject.AsEnumerable().FirstOrDefault(p => p.Field<int>("id") == tmpid);
                    int? tmpbaseid = dr.Field<int?>("baseonid");
                    bool tmpisbase = dr.Field<bool>("isbase");
                    ids.Add(tmpid);
                    if (tmpisbase)
                        break;
                    dr = dtproject.AsEnumerable().FirstOrDefault(p => p.Field<int>("id") == tmpbaseid);
                    if (dr == null)
                        break;
                    tmpid = (int)tmpbaseid;
                }

                string sqladd = "";
                for (int i = 0; i < ids.Count; i++)
                {
                    if (i == 0)
                        sqladd += " " + prefix + "prjid=" + ids[i];
                    else
                        sqladd += " or " + prefix + "prjid=" + ids[i];
                }

                return "(" + sqladd + ")";
            }

            ///<summary>返回当前方案的有效prjid列表</summary>
            public static List<int> getPrjIDList(int prjid, DataTable dtproject)
            {
                //逐基表读取
                List<int> ids = new List<int>();
                int tmpid = prjid;
                while (true)
                {
                    DataRow dr = dtproject.AsEnumerable().FirstOrDefault(p => p.Field<int>("id") == tmpid);
                    int? tmpbaseid = dr.Field<int?>("baseonid");
                    bool tmpisbase = dr.Field<bool>("isbase");
                    ids.Add(tmpid);
                    if (tmpisbase)
                        break;
                    dr = dtproject.AsEnumerable().FirstOrDefault(p => p.Field<int>("id") == tmpbaseid);
                    if (dr == null)
                        break;
                    tmpid = (int)tmpbaseid;
                }
                return ids;
            }


            ///<summary>返回当前方案和所有基于当前方案的prjid列表</summary>
            public static List<int> getAllIDBaseONCurprj(int prjid, DataTable dtproject)
            {
                //逐基表读取
                List<int> ids = new List<int>();
                List<int> tmps = new List<int>();
                List<int> tmps2 = new List<int>();

                ids.Add(prjid);
                tmps.Add(prjid);

                while (true)
                {
                    tmps2 = dtproject.AsEnumerable().Where(p => p.Field<int?>("baseonid") != null && tmps.Contains(p.Field<int>("baseonid"))).Select(p => p.Field<int>("id")).ToList();

                    if (tmps2.Count > 0)
                    {
                        ids = ids.Union(tmps2).ToList();

                        tmps = tmps2;
                    }
                    else
                        break;
                }
                return ids;
            }

        
    }
}
