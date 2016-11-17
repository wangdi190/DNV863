using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DataLayer
{
    ///<summary>用于保存sql和sim语句的类</summary>
    class SentenceCollection
    {
        public MyClassLibrary.SerializableDictionary<string, SqlSimDesc> sentences=new MyClassLibrary.SerializableDictionary<string,SqlSimDesc>();
    }


    ///<summary>用于保存sql和sim语句的类</summary>
    class SqlSimDesc
    {
        public string sql { get; set; }
        public string sim { get; set; }
    }
}
