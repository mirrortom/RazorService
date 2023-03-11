using System.Web;

namespace RazorService;

/// <summary>
/// 用于扩展TemplateBase功能,可以使用@Html.Raw()这样的方法
/// </summary>
public class HtmlExt
{
    /// <summary>
    /// 将html字符串编码后输出
    /// </summary>
    /// <param name="htmlTxt"></param>
    /// <returns></returns>
    public string Raw(string htmlTxt)
    {
        return HttpUtility.HtmlEncode(htmlTxt);
    }
}