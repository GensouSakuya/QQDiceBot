using System;
using System.Text.RegularExpressions;

namespace GensouSakuya.QQBot.Core.Helpers
{
    public class WeiboHelper
    {
        //正则部分参考rsshub：https://github.com/DIYgod/RSSHub/blob/master/lib/routes/weibo/utils.ts
        static Regex _faceRegex = new Regex("<span class=[\"']url-icon[\"']><img\\s[^>]*?alt=[\"']?([^>]+?)[\"']?\\s[^>]*?\\/?><\\/span>");
        static Regex _newLineRegex = new Regex("<br\\s/>");
        static Regex _tagRegex = new Regex("<a\\s+href=\".*search.*\"\\s+data-hide=\"\"><span\\s+class=\"surl-text\">(.*?)<\\/span><\\/a>");
        static Regex _videoRegex = new Regex("<a\\s+href=\"(.*video.weibo.com.*)\"\\s+data-hide=\"\"><span\\s+class='url-icon'><img.*_video_.*></span><span\\s+class=\"surl-text\">(.*)</span></a>");
        static Regex _fullTextRegex = new Regex("<a href=\"(.*?)\">全文<\\/a>");
        static Regex _repostRegex = new Regex("<a href='\\/n\\/.*'>(.*?)<\\/a>");
        static Regex _urlRegex = new Regex("<a\\s+href=\"(.*?)\"\\s+.*class=\"surl-text\">网页链接</span></a>");

        public static string FilterHtml(string originText)
        {
            var text = _faceRegex.Replace(originText, "$1");
            text = _newLineRegex.Replace(text, Environment.NewLine);
            text = _tagRegex.Replace(text, "$1");
            text = _videoRegex.Replace(text, "$2($1)");
            text = _fullTextRegex.Replace(text, Environment.NewLine + "[完整内容见原微博:https://m.weibo.cn$1]");
            text = _repostRegex.Replace(text, "$1");
            text = _urlRegex.Replace(text, "$1");
            return text;
        }
    }
}
