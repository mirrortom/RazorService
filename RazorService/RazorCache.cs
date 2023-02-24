using Microsoft.Extensions.Caching.Memory;
using System.Text;

namespace RazorService;

// 文件缓存10天过期,内存2小时过期
internal static class RazorCache
{
    private static readonly IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
    private static readonly System.Timers.Timer ClearWorkerTimer = new(10_000);
    private static TimeSpan LifeTime = TimeSpan.FromMinutes(30);
    private static string CsKey = "cs";
    private static string DllKey = "dll";

    static RazorCache()
    {
        ClearWorkerTimer.Elapsed += ClearWorkerTimer_Elapsed;
        //ClearWorkerTimer.Start();
    }

    /// <summary>
    /// 文件缓存清理,访问时间为10天前的文件删除掉
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void ClearWorkerTimer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
    {
        throw new NotImplementedException();
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
        memoryCache.Set(_key, value, LifeTime);
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