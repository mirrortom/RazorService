using Microsoft.Extensions.Caching.Memory;
using System.Text;
using Timer = System.Threading.Timer;

namespace RazorService;

// 默认文件缓存10天过期,内存2小时滑动过期
internal static class RazorCache
{
    private static readonly IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());
    private static readonly Timer ClearWorkerTimer = new(ClearWorkerTimerCallback);

    /// <summary>
    /// CS类缓存文件扩展名
    /// </summary>
    private const string CsKey = "cs";

    /// <summary>
    /// dll缓存文件扩展名
    /// </summary>
    private const string DllKey = "dll";

    /// <summary>
    /// 打开计时器. 定时清除过期的缓存文件
    /// </summary>
    internal static void Start()
    {
        // 文件缓存目录建立
        Directory.CreateDirectory(RazorCfg.CacheDir);
        ClearWorkerTimer.Change(0, 10_000);
#if DEBUG
        Helper.Log($"缓存定时器开启!");
#endif
    }

    /// <summary>
    /// 停止计算器. 停止扫描过期文件
    /// </summary>
    internal static void Stop()
    {
        ClearWorkerTimer.Change(Timeout.Infinite, 0);
#if DEBUG
        Helper.Log($"缓存定时器停止!");
#endif
    }

    /// <summary>
    /// 文件缓存清理,访问时间为10天前的文件删除掉
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <exception cref="NotImplementedException"></exception>
    private static void ClearWorkerTimerCallback(object? state)
    {
        // 检查文件,访问时间为10天前的文件删除掉
        DirectoryInfo directory = new(RazorCfg.CacheDir);

        var files = directory.EnumerateFileSystemInfos()
            .Where(item => item.Extension.EndsWith($".{CsKey}") || item.Extension.EndsWith($".{DllKey}"));
#if DEBUG
        Helper.Log($"执行缓存文件检查,总数: {files.Count()}");
#endif
        //
        foreach (FileSystemInfo file in files)
        {
            TimeSpan difference = DateTime.Now - file.LastAccessTime;
            if (difference.TotalDays >= RazorCfg.DeadDays)
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
        memoryCache.Set(_key, value,
            new MemoryCacheEntryOptions() { SlidingExpiration = TimeSpan.FromMinutes(RazorCfg.DeadMinutes) }); ;
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
            string path = $"{RazorCfg.CacheDir}/{key}.{CsKey}";
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
            string path = $"{RazorCfg.CacheDir}/{key}.{DllKey}";
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
    /// 缓存dll
    /// </summary>
    /// <param name="key"></param>
    /// <param name="dll"></param>
    /// <returns></returns>
    internal static void SaveDll(string key, byte[] dll)
    {
        Set(dll, key, DllKey);
        string path = $"{RazorCfg.CacheDir}/{key}.{DllKey}";
        File.WriteAllBytes(path, dll);
    }

    /// <summary>
    /// 缓存cs
    /// </summary>
    /// <param name="key"></param>
    /// <param name="src"></param>
    /// <returns></returns>
    internal static void SaveCsSrc(string key, string src)
    {
        Set(src, key, CsKey);
        string path = $"{RazorCfg.CacheDir}/{key}.{CsKey}";
        File.WriteAllText(path, src, Encoding.UTF8);
    }
}