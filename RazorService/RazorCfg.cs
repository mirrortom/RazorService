﻿using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace RazorService;

public static class RazorCfg
{
    #region 缓存设置

    /// <summary>
    /// 获取或者设置缓存目录 (默认值: RazorTemplateCache)
    /// </summary>
    public static string CacheDir { get; set; } = Path.Combine(AppContext.BaseDirectory, "RazorTemplateCache");

    /// <summary>
    /// 获取或设置内存缓存过期时间(分钟,默认120分钟)
    /// </summary>
    public static uint DeadMinutes { get; set; } = 120;

    /// <summary>
    /// 获取或设置文件缓存过期时间(天,默认10天).设置后,超过该天数未有访问文件,会删除.
    /// </summary>
    public static uint DeadDays { get; set; } = 10;

    /// <summary>
    /// 开启缓存清理定时器
    /// </summary>
    public static void StartTimer()
    {
        RazorCache.Start();
    }

    /// <summary>
    /// 停止缓存清理定时器
    /// </summary>
    public static void StopTimer()
    {
        RazorCache.Stop();
    }

    #endregion 缓存设置

    /// <summary>
    /// razor源文件搜索目录
    /// </summary>
    internal static List<string> SearchDirs { get; } = new();

    /// <summary>
    /// 设置razor文件搜索目录 使用绝对或者相对程序运行目录
    /// 使用时发现问题,如果多个程序使用了服务,那么会加入自己的搜索目录,那么会乱,所以改为:
    /// 每次编译前可以更新搜索目录
    /// </summary>
    /// <param name="searchDirs"></param>
    public static void SetSearchDirs(params string[] searchDirs)
    {
        SearchDirs.Clear();
        foreach (var item in searchDirs)
        {
            if (SearchDirs.Contains(item))
                continue;
            SearchDirs.Add(item);
        }
    }

    /// <summary>
    /// razor文件默认扩展名
    /// </summary>
    public static string ExtName { get; } = "cshtml";

    /// <summary>
    /// razor源文件生成的CS类名,来自方法:Microsoft.AspNetCore.Razor.Language.RazorProjectEngine.AddDefaultFeatures()
    /// </summary>
    public static string TemplateClassName { get; } = "Template";

    /// <summary>
    /// Template类的程序集名,也是命名空间名
    /// </summary>
    public static string AssemblyName { get; } = "RazorService";

    /// <summary>
    /// 编译时引用的元数据
    /// </summary>
    internal static MetadataReference[] Refence { get; set; } ={
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(Assembly.Load("Microsoft.CSharp").Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
        MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
        MetadataReference.CreateFromFile(typeof(TemplateBase).Assembly.Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Linq.Expressions").Location),
        // 当前程序集
        MetadataReference.CreateFromFile(Assembly.GetEntryAssembly().Location),
        };

    internal static void RazorBuilder(RazorProjectEngineBuilder builder)
    {
        // Template类的命名空间
        builder.SetNamespace(AssemblyName);
        // Template类的基类
        builder.SetBaseType(nameof(TemplateBase));
        // 支持@section指令
        SectionDirective.Register(builder);
    }

    /// <summary>
    ///  常用的命名空间.这是添加到razor源文件头部的那些@using语句.
    /// </summary>
    /// <returns></returns>
    internal static string CommonUsingNs { get; } =
    "@using System;\r\n@using System.Collections.Generic;\r\n@using System.Linq;\r\n@using System.Linq.Expressions;\r\n";
}