using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

public class GameUtils
{
    /// <summary>
    /// 到指定的目录下，找文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="extenstions">包含或者排除的文件的扩展名</param>
    /// <param name="include">true  包含   false 排除</param>
    /// <returns></returns>
    public static string[] GetAllFilesAtPath(string path, string[] extenstions = null, bool include = false)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        string[] allfiles = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
        ///如果没有排除或者包含的，那么返回所有文件。
        if (extenstions == null)
        {
            return allfiles;
        }
        ///获取到目录下所有的文件。
        if (include)
        {
            return allfiles.Where(file => extenstions.Contains(Path.GetExtension(file).ToLower())).ToArray();
        }
        else
        {
            return allfiles.Where(file => !extenstions.Contains(Path.GetExtension(file).ToLower())).ToArray();
        }
    }

    /// <summary>
    /// 获取一个文件的md5值
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetMD5(string path)
    {
        byte[] data = File.ReadAllBytes(path);
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] retVal = md5.ComputeHash(data);
        return BitConverter.ToString(retVal).Replace("-", "");
    }


}
