using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using GensouSakuya.QQBot.Core.Base;
using System.IO;
using HtmlAgilityPack;

namespace GensouSakuya.QQBot.Core.Helpers
{
    internal class BiliLiveHelper
    {
        private static readonly string _spaceHtmlTemplateUrl = "https://space.bilibili.com/{0}";
        private static string ChromeDataPath => Path.Combine(DataManager.DataPath, "chrome");

        public static BiliLiveInfo GetBiliLiveInfo(ulong uid)
        {
            var spaceUrl = string.Format(_spaceHtmlTemplateUrl, uid);
            var model = new BiliLiveInfo();
            var chromeOpt = new ChromeOptions();
            chromeOpt.AddArgument("--headless");
            chromeOpt.AddArgument("--no-sandbox");
            chromeOpt.AddArgument("--disable-dev-shm-usage");
            chromeOpt.AddArgument($"--user-data-dir={ChromeDataPath}");
            string html;
            using (var driver = new ChromeDriver(chromeOpt))
            {
                try
                {
                    driver.Navigate().GoToUrl(spaceUrl);
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                    IWebElement element = wait.Until(p => p.FindElement(By.ClassName("i-live-on")));
                    html = driver.PageSource;
                }
                finally
                {
                    driver.Quit();
                }
            }
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            model.UserName = htmlDoc.DocumentNode.SelectSingleNode("//*[@id=\"h-name\"]").InnerText?.Trim();
            var liveInfoNode = htmlDoc.DocumentNode.SelectSingleNode("//*[@class=\"i-live-on\"]");
            model.Title = liveInfoNode.SelectSingleNode("//*[@class=\"i-live-title\"]").InnerText?.Trim();
            model.Image = liveInfoNode.SelectSingleNode("//*[@class=\"i-live-cover\"]").GetAttributeValue("src", (string)null);
            if (model.Image != null)
            {
                var webpIndex = model.Image.IndexOf("@");
                if (webpIndex > 0)
                {
                    model.Image = model.Image.Substring(0, webpIndex);
                }
                model.Image = $"https:{model.Image}";
            }

            return model;
        }
    }

    internal class BiliLiveInfo
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string UserName { get; set; }
    }
}
