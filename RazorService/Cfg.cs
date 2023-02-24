using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.CodeAnalysis;
using System.Reflection;

namespace RazorService;

internal static class Cfg
{
    /// <summary>
    /// razor缓存文件目录 使用绝对或者相对程序运行目录
    /// </summary>
    internal const string cacheDir = "RazorTemplateCache";

    /// <summary>
    /// razor文件扩展名
    /// </summary>
    internal const string extName = "cshtml";

    /// <summary>
    /// razor源文件搜索目录 使用绝对或者相对程序运行目录
    /// </summary>
    internal static string[] searchDirs = { "razorFiles" };

    /// <summary>
    /// razor源文件生成的CS类名,来自方法:Microsoft.AspNetCore.Razor.Language.RazorProjectEngine.AddDefaultFeatures()
    /// </summary>
    internal const string TemplateClassName = "Template";

    /// <summary>
    /// Template类的程序集名,也是命名空间名
    /// </summary>
    internal const string assemblyName = "RazorService";

    /// <summary>
    /// 编译时引用的元数据
    /// </summary>
    internal static MetadataReference[] refence = {
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(Assembly.Load("Microsoft.CSharp").Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
        MetadataReference.CreateFromFile(Assembly.Load("netstandard").Location),
        MetadataReference.CreateFromFile(typeof(TemplateBase).Assembly.Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Linq").Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Linq.Expressions").Location),
        };

    internal static void RazorBuilder(RazorProjectEngineBuilder builder)
    {
        // Template类的命名空间
        builder.SetNamespace(assemblyName);
        // Template类的基类
        builder.SetBaseType(nameof(TemplateBase));
        // 支持@section指令
        SectionDirective.Register(builder);
    }
}