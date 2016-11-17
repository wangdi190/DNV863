using System;
using System.Windows.Input;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;
using System.Xml.Serialization;
using MyClassLibrary.MVVM;

namespace MyClassLibrary
{

    public static class EmailHelper
    {
        static EmailHelper()
        {
            paras = (EmailParas)XmlHelper.readFromXml("datas/email.xml", typeof(EmailParas));

        }

        static EmailParas paras = new EmailParas();


        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="toAddr">收件人人地址</param>
        /// <param name="Cc">抄送地址</param>
        /// <param name="Mcc">密送地址</param>
        /// <param name="from">发送人地址</param>
        /// <param name="content">邮件内容</param>
        /// <param name="subject">邮件标题</param>
        /// <param name="attach">附件内容</param>
        /// <param name="Pwd">发送人邮件密码</param>
        public static void SendMailByPlainFormat(string toAddr, string Cc, string Mcc, string from, string content, string subject, string attach, string Pwd)
        {
            MailMessage mailobj = new MailMessage();
            mailobj.From = new MailAddress(from);//发件人
            mailobj.To.Add(toAddr); //收件人
            if (Cc != "")
                mailobj.CC.Add(Cc);  //抄送
            if (Mcc != "")
                mailobj.Bcc.Add(Mcc);  //密送
            mailobj.Priority = MailPriority.High;  //发送优先级
            mailobj.Subject = subject; //主题
            mailobj.Body = content;   //内容
            mailobj.IsBodyHtml = true; //内容是否可以为html形式
            mailobj.BodyEncoding = Encoding.Default;
            if (attach != "")
            {
                char[] delim = new char[] { ';' };
                foreach (string substr in attach.Split(delim))
                {
                    Attachment MyAttach = new Attachment(substr);
                    //MailAttachment MyAttach = new MailAttachment(substr);
                    mailobj.Attachments.Add(MyAttach);

                }
            }

            SmtpClient smtp = new SmtpClient();
            smtp.Host =paras.server;  //服务器
            smtp.Port =paras.port;    //端口        
            smtp.Credentials = new System.Net.NetworkCredential(paras.user, paras.password); //用户名和密码
            smtp.Send(mailobj);
        }



        public static void SavePara()
        {
            XmlHelper.saveToXml("datas/email.xml", paras);
        }

        [Serializable]
        class EmailParas:NotificationObject
        {
            public string server { get; set; }
            public int port { get; set; }
            public string user { get; set; }
            public string password { get; set; }

            public BindingList<Contactor> contactors { get; set; }


            public ICommand SaveCommand { get { return new DelegateCommand(OnSave); } }
            private void OnSave()
            {
                XmlHelper.saveToXml("datas/email.xml", this);
            }

            int _curItemIdx = -1;
            [XmlIgnore]
            public int curItemIdx
            {
                get { return _curItemIdx; }
                set
                {
                    if (value < contactors.Count)
                        _curItemIdx = value;
                    else
                        _curItemIdx = -1;
                    RaisePropertyChanged(() => curItem);
                }
            }

            [XmlIgnore]
            public Contactor curItem
            {
                get { return curItemIdx < 0 ? null : contactors[curItemIdx]; }
            }


        }

        [Serializable]
        class Contactor:NotificationObject
        {
            public string name { get; set; }
            public string sort { get; set; }
            public string email { get; set; }
            public string lasttime { get; set; }
        }


    }
}
