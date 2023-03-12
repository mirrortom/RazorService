using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Text;

namespace RazorService;

internal static class RazorCompile
{
    /// <summary>
    /// razor源码生成C#源码
    /// </summary>
    /// <returns></returns>
    public static string RazorSrcToCsSrc(string razorSrc)
    {
        RazorProjectEngine engine = RazorProjectEngine.Create(
            RazorConfiguration.Default,
            RazorProjectFileSystem.Create(@"."),
            RazorCfg.RazorBuilder);
        //
        RazorSourceDocument document = RazorSourceDocument.Create(
            razorSrc,
            Path.GetRandomFileName());
        RazorCodeDocument codeDocument = engine.Process(
            document,
            null,
            new List<RazorSourceDocument>(),
            new List<TagHelperDescriptor>());
        RazorCSharpDocument razorCSharpDocument = codeDocument.GetCSharpDocument();
        return razorCSharpDocument.GeneratedCode;
    }

    /// <summary>
    /// 用Roslyn编译razorcs源码,得到程序集
    /// </summary>
    /// <param name="CsSource"></param>
    /// <returns></returns>
    public static byte[] CsSrcCompile(string CsSource)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(CsSource);
        var compilation = CSharpCompilation.Create(
            Path.GetRandomFileName(),
            new[] { syntaxTree },
            references: RazorCfg.Refence,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        MemoryStream memoryStream = new();
        var emitResult = compilation.Emit(memoryStream);
        if (emitResult.Success)
        {
            return memoryStream.ToArray();
        }
        // 编译失败,程序终止
        StringBuilder sb = new();
        foreach (Diagnostic diagnostic in emitResult.Diagnostics)
        {
            sb.AppendLine($"Compile Error : {diagnostic.Location.GetLineSpan()} {diagnostic.GetMessage()}");
        }
        sb.AppendLine("- Source Code Of CS -");
        sb.AppendLine($"{CsSource}");
        throw new RazorServeException(sb.ToString());
    }

    /// <summary>
    /// 获取Template实例
    /// </summary>
    /// <param name="assemblyBytes"></param>
    /// <returns></returns>
    public static TemplateBase GetInstance(byte[] assemblyBytes)
    {
        var assembly = Assembly.Load(assemblyBytes);
        var type = assembly.GetType($"{RazorCfg.AssemblyName}.{RazorCfg.TemplateClassName}");
        return Activator.CreateInstance(type) as TemplateBase;
    }
}