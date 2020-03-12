using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace AIThermometer.Cores
{
    class Common
    {
        public static bool FileExists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public static void CreateDir(string path)
        {
            if (false == System.IO.Directory.Exists(path))
            {
                //创建pic文件夹
                System.IO.Directory.CreateDirectory(path);
            }
        }
        public static T DeepCopyByReflection<T>(T obj)
        {
            if (obj is string || obj.GetType().IsValueType)
                return obj;

            object retval = Activator.CreateInstance(obj.GetType());
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
            foreach (var field in fields)
            {
                try
                {
                    field.SetValue(retval, DeepCopyByReflection(field.GetValue(obj)));
                }
                catch { }
            }

            return (T)retval;
        }

        public static bool IPMatch(string ip)
        {
            string pattern = @"^(([1-9]\d?)|(1\d{2})|(2[01]\d)|(22[0-3]))(\.((1?\d\d?)|(2[04]/d)|(25[0-5]))){3}$";
            if (Regex.IsMatch(ip, pattern))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void CopyDirectory(string srcPath, string desPath)
        {
            string folderName = srcPath.Substring(srcPath.LastIndexOf("\\") + 1);
            string desfolderdir = desPath + "\\" + folderName;
            if (srcPath.LastIndexOf("\\") == (desPath.Length - 1))
            {
                desfolderdir = desPath + folderName;
            }
            string[] filenames = Directory.GetFileSystemEntries(srcPath);
            foreach (string file in filenames)
            {
                if (Directory.Exists(file))
                {
                    string currentdir = desfolderdir + "\\" + file.Substring(file.LastIndexOf("\\") + 1);
                    if (!Directory.Exists(currentdir))
                    {
                        Directory.CreateDirectory(currentdir);
                    }
                    CopyDirectory(file, desfolderdir);
                }
                else
                {
                    string srcfileName = file.Substring(file.LastIndexOf("\\") + 1);
                    srcfileName = desfolderdir + "\\" + srcfileName;
                    if (!Directory.Exists(desfolderdir))
                    {
                        Directory.CreateDirectory(desfolderdir);
                    }

                    File.Copy(file, srcfileName);
                }
            }
        }

        public static void ClearAllFiles(string path)
        {
            try
            {
                if (Directory.Exists(path))
                {
                    /** 删除文件夹下所有文件 */

                    //方法一：
                    //Directory.GetFiles(path).ToList().ForEach(
                    //a => File.Delete(a));

                    //方法二：
                    //DirectoryInfo dirInfo = new DirectoryInfo(path);
                    //dirInfo.GetFiles().ToList().ForEach(a => a.Delete());

                    //方法三：
                    //DirectoryInfo dirInfo = new DirectoryInfo(path);
                    //dirInfo.GetFiles().ToList().ForEach(
                    //a => File.Delete(a.FullName));

                    /** 删除文件夹下所有文件夹 */

                    //方法一：
                    //Directory.GetDirectories(path).ToList().ForEach(
                    //a => Directory.Delete(a, true));

                    //方法二：
                    //DirectoryInfo dirInfo = new DirectoryInfo(path);
                    //dirInfo.GetDirectories().ToList().ForEach(a => a.Delete());

                    //方法三：
                    //DirectoryInfo dirInfo = new DirectoryInfo(path);
                    //dirInfo.GetDirectories().ToList().ForEach(
                    //a => Directory.Delete(a.FullName, true));

                    /** 删除文件夹下所有文件与文件夹 */
                    DirectoryInfo dirInfo = new DirectoryInfo(path);
                    FileSystemInfo[] fileSysInfo = dirInfo.GetFileSystemInfos();
                    foreach (FileSystemInfo fsi in fileSysInfo)
                    {
                        if (fsi is DirectoryInfo)
                        {
                            Directory.Delete(fsi.FullName, true);
                        }
                        else
                        {
                            File.Delete(fsi.FullName);
                        }
                    }
                }
                else
                {
                    Directory.CreateDirectory(path);
                }
            }
            catch (Exception e)
            {
                //Debug.Log("Exception: " + e.ToString());
            }
        }

    }
}
