using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DistNetLibrary.Edit
{
    class WinTableRelationSetupViewModel
    {
        public WinTableRelationSetupViewModel(List<TableDesc> Tables, TableRelation tRelation)
        {
            tables = Tables;
            tableRelation = tRelation;
        }


        public TableRelation tableRelation { get; set; }

        ///<summary>所有数据表列表</summary>
        public List<TableDesc> tables { get; set; }


    }
}
