using System;
using System.IO;
using System.Net;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyClassLibrary
{

    public delegate void ShowError(string content, string title);
    public class Ftp : IDisposable
    {
        string ftpHost;//FTP HostName or IPAddress  
        string ftpUserID;
        string ftpPassword;
        int port = 21;
        string ftpName;
        /// <summary>  
        /// FTP根目录URI  
        /// </summary>  
        string ftpRootURI;
        /// <summary>  
        /// FTP的IP地址  
        /// </summary>  
        string ftpIPAddress;
        FtpWebRequest reqFTP;
        //  
        bool isconnect = false;
        #region 属性
        /// <summary>  
        /// FTP根目录URI  
        /// </summary>  
        public string FTPRootURI
        {
            get { return ftpRootURI; }
        }
        #endregion
        #region 事件
        public event ShowError ShowErrorEvent;
        private void ErrorNotify(string content, string title)
        {
            if (ShowErrorEvent != null)
                ShowErrorEvent(content, title);
        }
        #endregion
        #region 构造函数
        public Ftp(string hostName, string userID, string passWord)
        {
            ftpHost = hostName;
            ftpUserID = userID;
            ftpPassword = passWord;
            PrepareFTPInfo();
        }
        public Ftp(string hostName, string userID, string passWord, int port)
            : this(hostName, userID, passWord)
        {
            this.port = port;
            PrepareFTPInfo();
        }
        public Ftp(string hostName, string ftpName, string userID, string passWord)
            : this(hostName, userID, passWord)
        {
            this.ftpName = ftpName;
            PrepareFTPInfo();
        }
        public Ftp(string hostName, string ftpName, string userID, string passWord, int port)
            : this(hostName, ftpName, userID, passWord)
        {
            this.port = port;
            this.ftpName = ftpName;
            PrepareFTPInfo();
        }
        #endregion
        public bool PrepareFTPInfo()
        {
            try
            {
                IPEndPoint remoteEndPoint = new IPEndPoint
                        (IPAddress.Parse(ftpHost), port);
                ftpIPAddress = remoteEndPoint.Address.ToString();
                ftpRootURI = "ftp://" + ftpIPAddress + "/";
                if (!string.IsNullOrEmpty(ftpName))
                    ftpRootURI += ftpName + "/";
            }
            catch (FormatException)
            {
                ErrorNotify("FTP地址的格式不正确，或者连接不到FTP主机。", "FTP地址错误");
                return false;
            }
            catch (Exception ex)
            {
                ErrorNotify(ex.Message, "连接错误");
                return false;
            }
            return true;
        }
        private bool Connect(string path)
        {
            try
            {
                if (!PrepareFTPInfo())
                    return false;
                if (string.IsNullOrEmpty(path))
                    path = ftpRootURI;
                //connect to ftp  
                // 根据uri创建FtpWebRequest对象  
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
                // 指定数据传输类型  
                reqFTP.UseBinary = true;
                // ftp用户名和密码  
                if (!string.IsNullOrWhiteSpace(ftpUserID))
                    reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
                //  
                isconnect = true;

            }
            catch (FormatException)
            {
                DisConnect();

                ErrorNotify("FTP地址的格式不正确，或者连接不到FTP主机。", "FTP地址错误");
                return false;
            }
            catch (Exception ex)
            {
                DisConnect();

                ErrorNotify(ex.Message, "连接错误");
                return false;
            }
            return true;
        }
        private void DisConnect()
        {
            isconnect = false;
        }
        #region Methods
        #region 判断是否存在
        /// <summary>  
        /// FTP测试  
        /// </summary>  
        /// <returns></returns>  
        public bool ExistFTP()
        {
            if (!Connect(""))
            {
                return false;
            }
            try
            {
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader
                    (response.GetResponseStream(), GlobalInfo.FtpEncode);
                string line = reader.ReadLine();
                reader.Close();
                response.Close();
                return true;
            }
            catch (Exception e1)
            {
                DisConnect();
                ErrorNotify(e1.Message, "连接失败");
                return false;
            }
        }
        /// <summary>  
        /// FTP是否存在<paramref name="path"/>的路径  
        /// </summary>  
        /// <param name="path">ftp路径</param>  
        /// <returns></returns>  
        public bool ExistPath(string path)
        {
            if (!Connect(path))
                return false;
            try
            {
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectory;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader
                    (response.GetResponseStream(), GlobalInfo.FtpEncode);
                string line = reader.ReadLine();
                DisConnect();
                return true;
            }
            catch (Exception e1)
            {
                ErrorNotify("FTP路径不存在!\r\n" + e1.Message, "连接失败");
                DisConnect();
                return false;
            }
        }
        /// <summary>  
        /// 是否存在文件  
        /// </summary>  
        /// <param name="path"></param>  
        /// <param name="fileName"></param>  
        /// <param name="info"></param>  
        /// <returns></returns>  
        public bool ExistFile(string path, string fileName, out string info)
        {
            return GetFileList(path, out info).Contains(fileName);
        }
        public bool ExistFile(string path, string fileName)
        {
            string info;
            return GetFileList(path, out info).Contains(fileName);
        }
        /// <summary>  
        /// 是否存在文件夹  
        /// </summary>  
        /// <param name="path"></param>  
        /// <param name="folderName"></param>  
        /// <param name="info"></param>  
        /// <returns></returns>  
        public bool ExistFolder(string path, string folderName, out string info)
        {
            return GetFolderList(path, out info).Contains(folderName);
        }
        #endregion
        #region 获取信息
        /// <summary>  
        /// 获取文件列表  
        /// </summary>  
        /// <param name="path">ftp目录路径</param>  
        /// <param name="info">0 failed 1 success</param>  
        /// <returns>文件名称列表</returns>  
        public List<string> GetFileList(string path, out string info)
        {
            return GetFTPPathList(path, "file", out info);
        }
        /// <summary>  
        /// 获取目录的列表  
        /// </summary>  
        /// <param name="path">ftp目录路径</param>  
        /// <param name="info">0 failed 1 success</param>  
        /// <returns>目录的列表</returns>  
        public IList<string> GetFolderList(string path, out string info)
        {
            return GetFTPPathList(path, "folder", out info);
        }

        /// <summary>  
        /// 获取FTP<paramref name="path"/>下的所有内容  
        /// </summary>  
        /// <param name="path">Ftp路径</param>  
        /// <param name="getType">0/file is File 1/Folder is Folder </param>  
        /// <param name="info">0 failed 1 success</param>  
        /// <returns>文件或文件夹列表</returns>  
        public List<string> GetFTPPathList(string path, string getType, out string info)
        {
            List<string> fileList = new List<string>();
            //  
            if (!Connect(path))
            {
                info = "0";
                return fileList;
            }
            try
            {
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                WebResponse response = reqFTP.GetResponse();
                StreamReader reader = new StreamReader
                    (response.GetResponseStream(), GlobalInfo.FtpEncode);
                string line = reader.ReadLine();
                while (line != null)
                {
                    if (!string.IsNullOrEmpty(line))
                    {
                        bool flag;//ture is folder false is file  
                        string value = GetListInfo(line, out flag);
                        if (flag == (getType == "1" || getType.Trim().ToLower() == "folder"))
                            fileList.Add(value);
                    }
                    line = reader.ReadLine();
                }
                // to remove the trailing '/n'  
                DisConnect();
                reader.Close();
                response.Close();
                info = "1";
                return fileList;
            }
            catch (Exception ex)
            {
                //System.Windows.Forms.MessageBox.Show(ex.Message);  
                ErrorNotify("路径不存在\r\n" + ex.Message, "获取文件夹");
                fileList = new List<string>();
                info = "0";
                DisConnect();
                return fileList;
            }
        }
        /// <summary>  
        /// 处理得到的信息流，判断是文件还是文件夹  
        /// </summary>  
        /// <param name="line"></param>  
        /// <param name="isfloder"></param>  
        /// <returns></returns>  
        private string GetListInfo(string line, out bool isfloder)
        {
            string processstr = line.Trim();
            string dateStr = processstr.Substring(0, 8);
            processstr = (processstr.Substring(8, processstr.Length - 8)).Trim();
            string timeStr = processstr.Substring(0, 7);
            processstr = (processstr.Substring(7, processstr.Length - 7)).Trim();
            DateTimeFormatInfo myDTFI = new CultureInfo("en-US", false).DateTimeFormat;
            myDTFI.ShortTimePattern = "t";
            DateTime CreateTime = DateTime.Parse(dateStr + " " + timeStr, myDTFI);

            if (processstr.Substring(0, 5) == "<DIR>")
            {
                isfloder = true;
                processstr = (processstr.Substring(5, processstr.Length - 5)).Trim();
                return processstr;
            }
            else
            {
                isfloder = false;
                int idx = processstr.IndexOf(" ");
                processstr = processstr.Substring(idx, processstr.Length - idx).Trim();
                //string[] strs = processstr.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);  // true);       
                //processstr = strs[1];  
                return processstr;
            }
        }
        #endregion
        #region 文件操作
        /// <summary>  
        /// 上传文件  
        /// </summary>  
        /// <param name="path">文件上传到的ftp路径</param>  
        /// <param name="oriFilePath">源文件地址</param>  
        /// <param name="ftpFileName">ftp中此文件的名称</param>  
        /// <returns>是否成功</returns>  
        public bool UploadFile(string path, string oriFilePath, string ftpFileName)
        {
            string info;
            if (ExistFile(path, ftpFileName, out info))
            {
                info = "0";
                ErrorNotify("文件已存在，请重新命名文件！", "上传文件");
                return false;
            }
            else
            {
                if (info != "1")
                    return false;
            }
            FileInfo fileInf = new FileInfo(oriFilePath);
            //上传文件路径名称  
            string ftpFilePath = (path.EndsWith("/") ? path : path + "/") + ftpFileName;
            //  
            if (!Connect(ftpFilePath))
            {
                info = "0";
                return false;
            }
            reqFTP.KeepAlive = false;
            reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
            reqFTP.UseBinary = true;
            reqFTP.ContentLength = fileInf.Length;
            int buffLength = 2048;
            byte[] buff = new byte[buffLength];
            int contentLen;
            FileStream fs = fileInf.OpenRead();
            try
            {
                Stream strm = reqFTP.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                while (contentLen != 0)
                {
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                strm.Close();
                fs.Close();
                DisConnect();
                info = "1";
                return true;
            }
            catch (Exception ex)
            {
                ErrorNotify(ex.Message, "上传文件");
                info = "0";
                DisConnect();
                return false;
            }
        }
        /// <summary>  
        /// 上传文件  
        /// </summary>  
        /// <param name="path">文件上传到的ftp路径</param>  
        /// <param name="oriFilePath">源文件地址</param>  
        /// <returns>是否成功</returns>  
        public bool UploadFile(string path, string oriFilePath)
        {
            string ftpFileName = System.IO.Path.GetFileName(oriFilePath);
            return UploadFile(path, oriFilePath, ftpFileName);
        }
        /// <summary>  
        /// 删除文件  
        /// </summary>  
        /// <param name="filePath"></param>  
        /// <returns></returns>  
        public bool DeleteFile(string filePath)
        {
            if (!Connect(filePath))
            {
                return false;
            }
            try
            {
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
                //  
                string result = String.Empty;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                long size = response.ContentLength;
                Stream datastream = response.GetResponseStream();
                StreamReader sr = new StreamReader(datastream);
                result = sr.ReadToEnd();
                sr.Close();
                datastream.Close();
                response.Close();
                DisConnect();
                return true;
            }
            catch (Exception ex)
            {
                ErrorNotify("删除文件失败!\r\n" + ex.Message, "删除文件");
                DisConnect();
                return false;
            }
        }
        /// <summary>  
        /// 下载文件  
        /// </summary>  
        /// <param name="ftpFilePath">ftp中文件的路径，包含文件名</param>  
        /// <param name="savePath">保存下载文件的路径</param>  
        /// <returns></returns>  
        public bool DownLoadFile(string ftpFilePath, string savePath)
        {
            string fileName = System.IO.Path.GetFileName(ftpFilePath);
            return DownLoadFile(ftpFilePath, savePath, fileName);
        }
        /// <summary>  
        ///   
        /// </summary>  
        /// <param name="ftpFilePath">ftp中文件的路径，包含文件名</param>  
        /// <param name="savePath">保存下载文件的路径</param>  
        /// <param name="downloadFileName">保存文件的名称</param>  
        /// <returns></returns>  
        public bool DownLoadFile(string ftpFilePath, string savePath, string downloadFileName)
        {
            if (!Connect(ftpFilePath))
            {
                return false;
            }
            FileStream outputStream = null;// = new FileStream(savePath + "\\" + downloadFileName, FileMode.Create);  
            try
            {
                outputStream = new FileStream(savePath + "\\" + downloadFileName, FileMode.Create);
                reqFTP.Method = WebRequestMethods.Ftp.DownloadFile;
                reqFTP.UseBinary = true;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                long cl = response.ContentLength;
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];

                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    outputStream.Write(buffer, 0, readCount);
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Close();
                outputStream.Close();
                response.Close();
                DisConnect();
                return true;
            }
            catch (Exception ex)
            {
                if (outputStream != null)
                {
                    outputStream.Close();
                    if (File.Exists(savePath + "\\" + downloadFileName))
                        File.Delete(savePath + "\\" + downloadFileName);
                }
                ErrorNotify("下载文件失败!\r\n" + ex.Message, "下载文件");
                DisConnect();
                return false;
            }
        }
        #endregion
        #region 创建目录
        /// <summary>  
        /// 创建目录  
        /// </summary>  
        /// <param name="path">当前FTP目录</param>  
        /// <param name="folderName">文件夹名称</param>  
        /// <returns></returns>  
        public bool CreateFolder(string path, string folderName)
        {
            string info;
            if (ExistFolder(path, folderName, out info))
            {
                info = "1";
                ErrorNotify("文件夹已存在！", "创建文件夹");
                return true;
            }
            else
            {
                if (info != "1")
                    return false;
            }
            //  
            string ftpFolderpath = (path.EndsWith("/") ? path : path + "/") + folderName;
            //  
            if (!Connect(ftpFolderpath))
            {
                info = "0";
                return false;
            }
            try
            {
                reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
                reqFTP.UseBinary = true;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();
                //  
                ftpStream.Close();
                response.Close();
                info = "1";
                DisConnect();
                return true;
            }
            catch (Exception e1)
            {
                ErrorNotify(e1.Message, "创建文件夹");
                info = "0";
                DisConnect();
                return false;
            }
        }
        /// <summary>  
        /// 创建目录  
        /// </summary>  
        /// <param name="folderPath">FTP目录</param>  
        /// <returns></returns>  
        public bool CreateFolder(string folderPath)
        {
            int idx = folderPath.LastIndexOf("/");
            string path = folderPath.Substring(0, idx);
            string folderName = folderPath.Substring(idx, folderPath.Length - idx);
            return CreateFolder(path, folderName);
        }

        public bool DeleteFolder(string ftpPath)
        {
            if (ftpPath == ftpRootURI)
            {
                ErrorNotify("FTP根目录无法删除!", "删除文件夹");
                return false;
            }
            //遍历所有的子文件夹  
            string info;
            IList<string> folderList = GetFolderList(ftpPath, out info);
            if (info == "1")
            {
                foreach (string folderName in folderList)
                {
                    string newPath = (ftpPath.EndsWith("/") ? ftpPath : ftpPath + "/") + folderName;
                    if (!DeleteFolder(newPath))
                        return false;
                }
            }
            else//连接出错  
                return false;
            //删除当前目录下的文件  
            IList<string> fileList = GetFileList(ftpPath, out info);
            if (info == "1")
            {
                foreach (string fileName in fileList)
                    if (!DeleteFile((ftpPath.EndsWith("/") ? ftpPath : ftpPath + "/") + fileName))
                        return false;
            }
            else return false;
            //删除自己  
            return DeleteOnlyFolder(ftpPath);

        }
        /// <summary>  
        /// 只删除文件夹  
        /// </summary>  
        /// <param name="ftpPath"></param>  
        /// <returns></returns>  
        private bool DeleteOnlyFolder(string ftpPath)
        {
            if (!Connect(ftpPath))
            {
                return false;
            }
            try
            {
                reqFTP.UseBinary = true;
                //reqFTP.UsePassive = false;  
                reqFTP.KeepAlive = false;
                reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                response.Close();
                DisConnect();
                return true;
            }
            catch (Exception ex)
            {
                ErrorNotify("删除文件夹失败！\r\n" + ex.Message, "删除文件夹");
                DisConnect();
                return false;
            }
        }
        #endregion
        /// <summary>  
        /// 重命名文件名或文件夹名称  
        /// </summary>  
        /// <param name="currentFtpFilePath"></param>  
        /// <param name="newFilename">新的文件名或文件夹名称</param>  
        public bool ReName(string currentFtpFilePath, string newFilename)
        {
            if (!Connect(currentFtpFilePath))
            {
                return false;
            }
            try
            {
                reqFTP.Method = WebRequestMethods.Ftp.Rename;
                reqFTP.RenameTo = newFilename;
                reqFTP.UseBinary = true;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                ftpStream.Close();
                response.Close();
                DisConnect();
                return true;
            }
            catch (Exception ex)
            {
                ErrorNotify("重命名失败!\r\n" + ex.Message, "重命名");
                DisConnect();
                return false;
            }
        }
        #endregion

        #region IDisposable 成员

        public void Dispose()
        {
            if (reqFTP != null)
                reqFTP.Abort();
        }

        #endregion
    }

}
