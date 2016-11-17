using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DistNetLibrary.Edit
{
    /// <summary>
    /// WinSimNote.xaml 的交互逻辑
    /// </summary>
    public partial class WinSimNote : Window
    {
        public WinSimNote()
        {
            InitializeComponent();
        }

        private void Window_Initialized(object sender, EventArgs e)
        {
            txt.Text = @"sim语句结构与标准sql语句类似，以
select top 100 i as id, dmv2004e1 as edate, s描*述as desc, s{甲;乙;丙} as etype, fmax1000 as value1
为例说明如下：
1.select 必填，无特殊意义。
2.top 100 选填，表示模拟100条记录，若不填写则随机返回10-100条记录。
3.字段部分以西文逗号,分隔，分隔后的每一部分表示一个字段，格式为 XXXXXX as fieldname, XXXXXX为字段数据模拟描述，as 后的fieldname为返回数据表中的字段名。
4.上例中s描*述, 表示返回诸如""描A述""，""描C述""的字串。
5.s{甲;乙;丙}  表示返回甲;乙;丙三个中随机的一个。
6.XXXXXX字段数据模拟描述约定如下：
	第一个字母固定表示数据类型，现支持i（整型）s（字符串型）d（日期型）f（数字型）四种类型。

字段关键字：
min：随机生成数据的最小值，默认值为0，例min100指生成数据不小于100。
max：随机生成数据的最大值，默认值为无穷大，例max 1000指生成数据不大于1000。
v：指定数据初始值，此关键字比min，max具有更高优先级，指定v后minmax无效，无v则随机生成数据，例v2004，初始为2004。
e：步进值，每条数据为前一条数据加e的值，默认为没有步进值，当指定e后，minmax和v关键字只对第一条数据产生作用，例e2，步进2，在getDataTable(string key，DataTable olddt)方法中时，为原值基础上的增减，日期和数字型有效。
r：随机，只在getDataTable(string key，DataTable olddt)方法中生效，即该字段将在原值的0.9-1.1范围变化，但不超出minmax限定的范围，仅数字类型字段有效。
now：特定关键字，仅日期型生效，指该字段取当前时间。
y：样条函数，例yspyear(edate)，暂未实现。
";
        }
    }
}
