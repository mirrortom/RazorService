using System.Security.Cryptography;
using System.Text;

namespace RazorService;

/// <summary>
/// 一些工具方法
/// </summary>
internal static class Helper
{
#if DEBUG

    public static void Log(string info)
    {
        Console.WriteLine(info);
    }

#endif

    /// <summary>
    /// 获取文本的md5,以32位16进制字符串形式
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    internal static string GetHex32Md5(string plain)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(plain);
        var data = MD5.Create().ComputeHash(buffer);
        return Convert.ToHexString(data).ToLower();
    }

    /// <summary>
    /// 从搜索目录查找razor文件,返回文件内容,没找到返回null
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    internal static string FindRazorFile(string path)
    {
        // 直接判断文件是否存在,可以是相对或者绝对路径
        if (File.Exists(path))
            return File.ReadAllText(path);

        // 没有写扩展名的情况,加上扩展名后再次判断
        string p = path;
        if (!Path.HasExtension(path))
        {
            p = $"{path}.{RazorCfg.ExtName}";
            if (File.Exists(p))
                return File.ReadAllText(p);
        }

        // 从搜索目录查找
        // 取得文件名字,加上缓存目录
        string name = Path.GetFileName(p);
        foreach (var dir in RazorCfg.SearchDirs)
        {
            string file = $"{dir}/{name}";
            if (File.Exists(file))
                return File.ReadAllText(file);
        }

        // 仍然没有
        return null;
    }
}