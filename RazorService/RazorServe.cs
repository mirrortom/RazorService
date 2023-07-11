using System.Security.Cryptography;

namespace RazorService;

public class RazorServe
{
    /// <summary>
    /// 处理razor文件
    /// </summary>
    /// <param name="path"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static string Run(string path, dynamic model = null)
    {
        return Run(path, model, null);
    }

    /// <summary>
    /// 处理razor文本
    /// </summary>
    /// <param name="razorSrc"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public static string RunTxt(string razorSrc, dynamic model = null)
    {
        return RunTxt(razorSrc, model, null);
    }

    private static string Run(string path, dynamic model = null, TemplateBase RefBodyTemplate = null)
    {
        // 从razor文件获取razor源码
        string razorSrc = Helper.FindRazorFile(path);
        return RunTxt(razorSrc, model, RefBodyTemplate);
    }

    /// <summary>
    /// 处理razor文本
    /// </summary>
    /// <param name="razorSrc"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    private static string RunTxt(string razorSrc, dynamic model = null, TemplateBase RefBodyTemplate = null)
    {
        string md5 = Helper.GetHex32Md5(razorSrc);
        // 2.获取cs源码
        string csSrc = RazorCache.GetCsSrc(md5);
        if (csSrc == null)
        {
            // 首次生成时,缓存到文件和内存
            csSrc = RazorCompile.RazorSrcToCsSrc(razorSrc);
            RazorCache.SaveCsSrc(md5, csSrc);
#if DEBUG
            Helper.Log($"新的生成,已缓存cs: {md5}");
#endif
        }
        // 3.获取dll
        byte[] dll = RazorCache.GetDll(md5);
        if (dll == null)
        {
            dll = RazorCompile.CsSrcCompile(csSrc);
            RazorCache.SaveDll(md5, dll);
#if DEBUG
            Helper.Log($"新的编译,已缓存dll: {md5}");
#endif
        }
        // 4.运行
        return Execute(dll, model, RefBodyTemplate);
    }

    /// <summary>
    /// 运行dll程序集,得到结果
    /// </summary>
    /// <param name="dll"></param>
    /// <param name="model"></param>
    /// <param name="tag"></param>
    /// <param name="RefBodyTemplate"></param>
    /// <returns></returns>
    private static string Execute(byte[] dll, dynamic model, TemplateBase RefBodyTemplate = null)
    {
        // 获取实例
        var instance = RazorCompile.GetInstance(dll);
        if (model != null)
        {
            instance.Model = Helper.AnonymousTypeToExpandoObject(model);
        }

        // 运行layout模板时,会进入这里,上次运行的body模板对象会传入,以使用它的数据
        if (RefBodyTemplate != null)
        {
            instance.Body = RefBodyTemplate.Body;
            instance.Sections = RefBodyTemplate.Sections;
            instance.ViewBag = RefBodyTemplate.ViewBag;
            instance.Model = Helper.AnonymousTypeToExpandoObject(RefBodyTemplate.Model);
        }

        // 运行方法后,才会生成属性值,例如sections,body,viewbag这些属性
        // 改用同步的,为了正常捕获异常
        instance.ExecuteAsync().Wait();

        // 先保存模板结果.在执行section时,临时存储会清空
        instance.Body = instance.Result();

        // check layout and section
        // section一般是在body的razor里定义,然后在母版里引用,所以先检查section并且执行,再检查layout
        // 运行模板后,如果定义了section,那么一定执行了DefineSection(),所以要执行section委托,取得section结果.
        foreach (var k in instance.SectionsAction.Keys)
        {
            instance.buffer.Clear();
            instance.SectionsAction[k]();
            // 将section的结果保存在字典里
            instance.Sections.Add(k, instance.Result());
        }

        // check layout
        // 模板如果引用了layout会进入这里
        // 其中的body和section数据,在运行body时都已经获得了
        // 最后一个母版页执行后,是最后结果,所以这是递归终止条件
        if (instance.Layout != string.Empty)
        {
            // layout或者include,是一个新页面的执行过程
            return Run(instance.Layout, model, instance);
        }
        // 结果
        return instance.Body;
    }
}