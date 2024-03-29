﻿using System;
using System.Dynamic;
using System.Text;

namespace RazorService;


public abstract class TemplateBase
{

    public readonly StringBuilder buffer = new();
    public string Layout { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public dynamic ViewBag { get; set; } = new ExpandoObject();
    public dynamic Model { get; set; } = null;
    public Dictionary<string, Action> SectionsAction { get; set; } = new();
    public Dictionary<string, string> Sections { get; set; } = new();
    private string attributeSuffix = null;
    /// <summary>
    /// @html帮助方法 Raw()等
    /// </summary>
    public readonly HtmlExt Html = new();

    public abstract Task ExecuteAsync();

    public void WriteLiteral(string literal)
    {
        buffer.Append(literal);
    }

    public void Write(object obj)
    {
        buffer.Append(obj);
    }

    public void BeginWriteAttribute(string name, string prefix, int prefixOffset, string suffix, int suffixOffset, int attributeValuesCount)
    {
        this.attributeSuffix = suffix;
        buffer.Append(prefix);
    }
    public void WriteAttributeValue(string prefix, int prefixOffset, object? value, int valueOffset, int valueLength, bool isLiteral)
    {
        buffer.Append(prefix);
        buffer.Append(value);
    }
    public void EndWriteAttribute()
    {
        buffer.Append(this.attributeSuffix);
        this.attributeSuffix = null;
    }
    public string RenderBody()
    {
        return Body;
    }

    /// <summary>
    /// 当section定义时,传入的参数是个Task,运行的时机一定是在RenderSection方法运行前.
    /// 简单的说,使用结果前要准备好结果
    /// </summary>
    /// <param name="key"></param>
    /// <param name="action"></param>
    public void DefineSection(string key, Action action)
    {
        SectionsAction.Add(key, action);
    }

    public string RenderSection(string key, bool isRequire = false)
    {
        if (Sections.ContainsKey(key))
        {
            return Sections[key];
        }
        if (isRequire)
            throw new Exception("section key is not exist!");
        return string.Empty;
    }

    /// <summary>
    /// 这个Include()本质是插入一个新的模板的运行结果.
    /// 参数是新模板地址,编译运行这个模板得到结果,加入buffer.
    /// 可以传入main模板的model数据,但其它章节或者再套layout则不再支持
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string Include(string path)
    {
        return RazorServe.Run(path,Model);
    }

    public string Result()
    {
        return buffer.ToString();
    }
}