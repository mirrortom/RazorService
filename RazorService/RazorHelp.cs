﻿using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Text;

namespace RazorService;

internal static class RazorHelp
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
            Cfg.RazorBuilder);
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
    public static (byte[] assembly, bool isok, string msg) CsSrcCompile(string CsSource)
    {
        SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(CsSource);
        var compilation = CSharpCompilation.Create(
            Path.GetRandomFileName(),
            new[] { syntaxTree },
            references: Cfg.refence,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        MemoryStream memoryStream = new();
        var emitResult = compilation.Emit(memoryStream);
        if (emitResult.Success)
        {
            var bytes = memoryStream.ToArray();
            return (bytes, true, null);
        }
        else
        {
            StringBuilder sb = new();
            foreach (Diagnostic diagnostic in emitResult.Diagnostics)
            {
                sb.AppendLine($"{diagnostic.Location}: {diagnostic.GetMessage()}");
            }
            return (null, false, sb.ToString());
        }
    }

    /// <summary>
    /// 获取Template实例
    /// </summary>
    /// <param name="assemblyBytes"></param>
    /// <returns></returns>
    public static TemplateBase GetInstance(byte[] assemblyBytes)
    {
        var assembly = Assembly.Load(assemblyBytes);
        var type = assembly.GetType($"{Cfg.assemblyName}.{Cfg.TemplateClassName}");
        return Activator.CreateInstance(type) as TemplateBase;
    }
}