using System.Web;

namespace RazorService;

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