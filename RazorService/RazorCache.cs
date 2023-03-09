using Microsoft.Extensions.Caching.Memory;
using System.IO;
using System.Text;
using System.Threading.Channels;

namespace RazorService;

// 文件缓存10天过期,内存2小时滑动过期
internal static class RazorCache
{
    private static readonly IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
    private static readonly System.Timers.Timer ClearWorkerTimer = new(10_000);
    private static readonly string CsKey = "cs";
    private static readonly string DllKey = "dll";
    /// <summary>
    /// 选项: 滑动过期时间2小时
    /// </summary>
    private static readonly MemoryCacheEntryOptions Ops = new()
    {
        SlidingExpiration = TimeSpan.FromMinutes(120)
    };

    static RazorCache()
    {
        ClearWorkerTimer.Elapsed += ClearWorkerTimer_Elapsed;
        ClearWorkerTimer.Start();
    }

    /// <summary>
    /// 停止缓存清理定时器
    /// </summary>
    public static void Close()
    {
        ClearWorkerTimer.Stop();
    }

    /// <summary>
    /// 文件缓存清理,访问时间为10天前的文件删除掉
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void ClearWorkerTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        // 检查文件,访问时间为10天前的文件删除掉
        DirectoryInfo directory = new(Cfg.cacheDir);

        var files = directory.EnumerateFileSystemInfos()
            .Where(item => item.Extension.EndsWith($".{CsKey}") || item.Extension.EndsWith($".{DllKey}"));
#if DEBUG
        Helper.Log($"执行缓存文件检查,总数: {files.Count()}");
#endif
        //
        foreach (FileSystemInfo file in files)
        {
            TimeSpan difference = DateTime.Now - file.LastAccessTime;
            if (difference.TotalDays >= 10)
            {
                file.Delete();
#if DEBUG
                Helper.Log($"已删除文件缓存:{file.FullName}, 最后访问超过10天: {file.LastAccessTime}");
#endif
            }
        }
    }

    private static object Get(string key, string keyType)
    {
        // md5.cs / md5.dll
        var _key = $"{key}.{keyType}";
        return memoryCache.Get(_key);
    }

    private static void Set(object value, string key, string keyType)
    {
        // md5.cs / md5.dll
        var _key = $"{key}.{keyType}";
        memoryCache.Set(_key, value, Ops);
    }

    /// <summary>
    /// 查找cs源码 key是MD5
    /// 缓存先找内存,再找文件. 如果文件找到了,那么放入内存
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    internal static string GetCsSrc(string key)
    {
        var cs = Get(key, CsKey);
        if (cs == null)
        {
            string path = $"{Cfg.cacheDir}/{key}.{CsKey}";
            if (File.Exists(path))
            {
                string txt = File.ReadAllText(path);
                Set(txt, key, CsKey);
#if DEBUG
                Helper.Log($"已从文件获取,并且缓存cs: {key}");
#endif
                return txt;
            }
            return null;
        }
#if DEBUG
        Helper.Log($"已从缓存获取cs: {key}");
#endif
        return (string)cs;
    }

    /// <summary>
    /// 查找dll key是MD5
    /// 缓存先找内存,再找文件. 如果文件找到了,那么放入内存
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    internal static byte[] GetDll(string key)
    {
        var dll = Get(key, DllKey);
        if (dll == null)
        {
            string path = $"{Cfg.cacheDir}/{key}.{DllKey}";
            if (File.Exists(path))
            {
                byte[] bytes = File.ReadAllBytes(path);
                Set(bytes, key, DllKey);
#if DEBUG
                Helper.Log($"已从文件获取,并且缓存dll: {key}");
#endif
                return bytes;
            }
            return null;
        }
#if DEBUG
        Helper.Log($"已从缓存获取dll: {key}");
#endif
        return (byte[])dll;
    }

    /// <summary>
    /// 缓存dll到, 文件(10天过期)和内存(30分过期) key是MD5,也是文件名 md5.cs
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dll"></param>
    /// <returns></returns>
    internal static void SaveDll(string key, byte[] dll)
    {
        Set(dll, key, DllKey);
        string path = $"{Cfg.cacheDir}/{key}.{DllKey}";
        File.WriteAllBytes(path, dll);
    }

    /// <summary>
    /// 缓存cs到, 文件(10天过期)和内存(30分过期) key是MD5,也是文件名 md5.cs
    /// </summary>
    /// <param name="key"></param>
    /// <param name="src"></param>
    /// <returns></returns>
    internal static void SaveCsSrc(string key, string src)
    {
        Set(src, key, CsKey);
        string path = $"{Cfg.cacheDir}/{key}.{CsKey}";
        File.WriteAllText(path, src, Encoding.UTF8);
    }
}