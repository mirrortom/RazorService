using System.Dynamic;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

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
    /// 匿名类型转为动态类型.支持匿名类型的model.只能转换简单的,例如var m={name="mirror",复杂的属性不行}
    /// 还是使用ExpandoObject靠谱
    /// 匿名类型作用域范围太小,在一个方法内可以用,出了方法或者不在一个程序集时会出错.这不方便传递model
    /// </summary>
    /// <param name="model"></param>
    internal static dynamic AnonymousTypeToExpandoObject(dynamic model)
    {
        if (model == null) return model;
        TypeInfo t = model.GetType();
        if (t == null)
            return model;
        // 这个检查匿名的方式是判断类型名字含有这个字符串,可能不靠谱
        if (t.Name.Contains("AnonymousType"))
        {
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(model), typeof(ExpandoObject));
        }
        // 其它情况都直接返回
        return model;
    }
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
        // 将path弄成全路径名字

        // 1.支持省略扩展名
        string p = path;
        if (!Path.HasExtension(path))
        {
            p = $"{path}.{RazorCfg.ExtName}";
        }

        // 2.相对路径(不是带盘符的那种全路径)
        if (!Path.IsPathFullyQualified(p))
        {
            // 不带盘符的路径分为2种情况 aa/bb.txt和/aa/bb.txt,如果是/或\开头的,算是绝对路径
            // 要将/或\去掉后,变成"相对路径"才能用Path.Combine()
            if (Path.IsPathRooted(p))
            {
                p = p.TrimStart('/', '\\');
            }
            // 先从搜索目录查找,这个是正规情况
            foreach (var dir in RazorCfg.SearchDirs)
            {
                string full = Path.Combine(dir, p);
                if (File.Exists(full))
                    return File.ReadAllText(full);
            }
            // 从程序运行目录查找,这种情况不正规
            string full2 = Path.Combine(AppContext.BaseDirectory, p);
            if (File.Exists(full2))
                return File.ReadAllText(full2);
        }

        // 3.带盘符全路径
        if (File.Exists(p))
            return File.ReadAllText(p);

        // 没找到文件,程序终止
        throw new RazorServeException($"Razor file not found! Check the path: [{path}]");
    }
}