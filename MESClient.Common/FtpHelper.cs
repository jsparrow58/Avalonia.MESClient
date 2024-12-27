using System;
using System.IO;
using System.Net;
using System.Text;

namespace MESClient.Common;

/// <summary>
/// FTP文件上传、下载帮助类
/// </summary>
public class FtpHelper
{
    #region 变量
    private string ftpServerIP;     //ftpIP地址
    private string ftpUserID;       //ftp用户名
    private string ftpPassword;     //ftp密码
    private FtpWebRequest reqFTP;   //ftpweb服务
    #endregion

    #region 属性
    /// <summary>
    /// ftpIP地址
    /// </summary>
    public string FtpServerIP
    {
        get { return ftpServerIP; }
        set { ftpServerIP = value; }
    }

    /// <summary>
    /// ftp用户名
    /// </summary>
    public string FtpUserID
    {
        get { return ftpUserID; }
        set { ftpUserID = value; }
    }

    /// <summary>
    /// ftp密码
    /// </summary>
    public string FtpPassword
    {
        get { return ftpPassword; }
        set { ftpPassword = value; }
    }
    #endregion

    #region 构造方法
    /// <summary>
    /// 构造方法
    /// </summary>
    /// <param name="ftpServerIP">ftpIP地址</param>
    /// <param name="ftpUserID">ftp用户名</param>
    /// <param name="ftpPassword">ftp密码</param>
    public FtpHelper(string ftpServerIP, string ftpUserID, string ftpPassword)
    {
        this.ftpServerIP = ftpServerIP;
        this.ftpUserID = ftpUserID;
        this.ftpPassword = ftpPassword;
    }
    #endregion

    #region 连接
    /// <summary>
    /// 连接FTP
    /// </summary>
    /// <param name="path">路径</param>
    private void Connect(String path)
    {
        // 根据uri创建FtpWebRequest对象  
        reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(path));
        // 指定数据传输类型  
        reqFTP.UseBinary = true;
        // ftp用户名和密码  
        reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
    }
    #endregion

    #region 从ftp服务器上获得文件列表
    /// <summary>
    /// 从ftp服务器上获得根目录文件列表 
    /// </summary>
    /// <returns></returns>
    public string[] GetFileList()
    {
        return GetFileList("ftp://" + ftpServerIP + "/", WebRequestMethods.Ftp.ListDirectory);
    }

    /// <summary>
    /// 从ftp服务器上获得文件列表
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public string[] GetFileList(string path)
    {
        return GetFileList("ftp://" + ftpServerIP + "/" + path, WebRequestMethods.Ftp.ListDirectory);
    }

    /// <summary>
    /// 从ftp服务器上获得文件列表
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="WRMethods">文件操作类型</param>
    /// <returns></returns>
    private string[] GetFileList(string path, string WRMethods)
    {
        string[] downloadFiles;
        StringBuilder result = new StringBuilder();
        try
        {
            Connect(path);
            reqFTP.Method = WRMethods;
            WebResponse response = reqFTP.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream(), System.Text.Encoding.Default);//中文文件名  
            string line = reader.ReadLine();
            while (line != null)
            {
                result.Append(line);
                result.Append("/n");
                line = reader.ReadLine();
            }
            // to remove the trailing '/n'  
            result.Remove(result.ToString().LastIndexOf('\n'), 1);
            reader.Close();
            response.Close();
            return result.ToString().Split('\n');
        }
        catch (Exception)
        {
            downloadFiles = null;
            return downloadFiles;
        }
    }
    #endregion

    #region 上传下载
    /// <summary>
    /// 上传文件到FTP
    /// </summary>
    /// <param name="filename">文件名称</param>
    /// <param name="ftpfilepath">上传目录</param>
    /// <returns></returns>
    public string Upload(string filename, string ftpfilepath)
    {
        string strResult = string.Empty;
        Stream strm = null;
        FileInfo fileInf = new FileInfo(filename);
        string uri = "ftp://" + ftpServerIP + "/" + ftpfilepath + "/" + fileInf.Name;
        Connect(uri);//连接           
        // 默认为true，连接不会被关闭  
        // 在一个命令之后被执行  
        reqFTP.KeepAlive = false;
        // 指定执行什么命令
        reqFTP.Method = WebRequestMethods.Ftp.UploadFile;
        // 上传文件时通知服务器文件的大小
        reqFTP.ContentLength = fileInf.Length;
        // 缓冲大小设置为kb
        int buffLength = 2048;
        byte[] buff = new byte[buffLength];
        int contentLen;
        // 打开一个文件流(System.IO.FileStream) 去读上传的文件  
        FileStream fs = fileInf.OpenRead();
        try
        {
            // 把上传的文件写入流  
            strm = reqFTP.GetRequestStream();
            // 每次读文件流的kb  
            contentLen = fs.Read(buff, 0, buffLength);
            // 流内容没有结束  
            while (contentLen != 0)
            {
                // 把内容从file stream 写入upload stream  
                strm.Write(buff, 0, contentLen);
                contentLen = fs.Read(buff, 0, buffLength);
            }
        }
        catch (Exception ex)
        {
            strResult = "上传出错：" + ex.Message;
        }
        finally
        {
            // 关闭两个流  
            strm.Close();
            fs.Close();
        }
        return strResult;
    }

    /// <summary>
    /// 从ftp服务器下载文件
    /// </summary>
    /// <param name="serverPath">远程目录</param>
    /// <param name="fileName">名称</param>
    /// <param name="localPath">本地保存目录</param>
    /// <returns></returns>
    public string Download(string serverPath, string fileName, string localPath)
    {
        try
        {
            String onlyFileName = Path.GetFileName(fileName);
            string newFileName = localPath + "//" + onlyFileName;
            if (File.Exists(newFileName))
            {
                return "本地文件" + newFileName + "已存在,无法下载";
            }
            string url = "ftp://" + ftpServerIP + "/" + serverPath + "/" + fileName;
            Connect(url);//连接   
            reqFTP.Credentials = new NetworkCredential(ftpUserID, ftpPassword);
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            Stream ftpStream = response.GetResponseStream();
            long cl = response.ContentLength;
            int bufferSize = 2048;
            int readCount;
            byte[] buffer = new byte[bufferSize];
            readCount = ftpStream.Read(buffer, 0, bufferSize);
            FileStream outputStream = new FileStream(newFileName, FileMode.Create);
            while (readCount > 0)
            {
                outputStream.Write(buffer, 0, readCount);
                readCount = ftpStream.Read(buffer, 0, bufferSize);
            }
            ftpStream.Close();
            outputStream.Close();
            response.Close();
            return "";
        }
        catch (Exception ex)
        {
            return "下载出错：" + ex.Message;
        }
    }
    #endregion

    #region 文件及目录操作
    /// <summary>
    /// 删除文件
    /// </summary>
    /// <param name="fileName">名称</param>  
    public string DeleteFileName(string path, string fileName)
    {
        try
        {
            FileInfo fileInf = new FileInfo(fileName);
            string uri = "ftp://" + ftpServerIP + "/" + path + "/" + fileInf.Name;
            Connect(uri);//连接           
            // 默认为true，连接不会被关闭  
            // 在一个命令之后被执行  
            reqFTP.KeepAlive = false;
            // 指定执行什么命令  
            reqFTP.Method = WebRequestMethods.Ftp.DeleteFile;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            response.Close();
        }
        catch (Exception ex)
        {
            return "删除出错：" + ex.Message;
        }
        return "";
    }

    /// <summary>
    /// 创建目录
    /// </summary>
    /// <param name="dirName">目录名称</param>
    /// <returns></returns>
    public string MakeDir(string dirName)
    {
        try
        {
            string uri = "ftp://" + ftpServerIP + "/" + dirName;
            Connect(uri);//连接        
            reqFTP.Method = WebRequestMethods.Ftp.MakeDirectory;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            response.Close();
            return "";
        }
        catch (WebException ex)
        {
            return "创建目录出错：" + ex.Message;
        }
    }

    /// <summary>
    /// 删除目录
    /// </summary>
    /// <param name="dirName">目录名称</param>
    public string delDir(string dirName)
    {
        try
        {
            string uri = "ftp://" + ftpServerIP + "/" + dirName;
            Connect(uri);//连接        
            reqFTP.Method = WebRequestMethods.Ftp.RemoveDirectory;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            response.Close();
        }
        catch (Exception ex)
        {
            return "删除目录出错：" + ex.Message;
        }
        return "";
    }
    /// <summary>
    /// 获得文件大小
    /// </summary>
    /// <param name="filename">名称</param>
    /// <returns></returns>  
    public long GetFileSize(string path, string filename)
    {
        long fileSize = 0;
        try
        {
            FileInfo fileInf = new FileInfo(filename);
            string uri = "ftp://" + ftpServerIP + "/" + path + "/" + fileInf.Name;
            Connect(uri);//连接        
            reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            fileSize = response.ContentLength;
            response.Close();
        }
        catch (Exception)
        {
            return fileSize;
        }

        return fileSize;
    }

    /// <summary>
    /// 文件改名
    /// </summary>
    /// <param name="currentFilename">旧名称（包含目录路径）</param>
    /// <param name="newFilename">新名称（包含目录路径）</param>
    public string Rename(string path, string currentFilename, string newFilename)
    {
        try
        {
            FileInfo fileInf = new FileInfo(currentFilename);
            string uri = "ftp://" + "/" + ftpServerIP + "/" + path + "/" + fileInf.Name;
            Connect(uri);//连接  
            reqFTP.Method = WebRequestMethods.Ftp.Rename;
            reqFTP.RenameTo = newFilename;
            FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
            //Stream ftpStream = response.GetResponseStream();  
            //ftpStream.Close();  
            response.Close();
        }
        catch (Exception ex)
        {
            return "文件改名出错：" + ex.Message;
        }
        return "";
    }
    #endregion

    #region 获取文件信息
    /// <summary>
    /// 获得根目录文件明细
    /// </summary>
    /// <returns></returns>  
    public string[] GetFilesDetailList()
    {
        return GetFileList("ftp://" + ftpServerIP + "/", WebRequestMethods.Ftp.ListDirectoryDetails);
    }

    /// <summary>
    /// 获得指定目录文件明细
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string[] GetFilesDetailList(string path)
    {
        return GetFileList("ftp://" + ftpServerIP + "/" + path, WebRequestMethods.Ftp.ListDirectoryDetails);
    }
    #endregion
}
