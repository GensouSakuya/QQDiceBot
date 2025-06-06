using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using System;
using GensouSakuya.QQBot.Core.Base;
using System.IO;
using HtmlAgilityPack;
using OpenQA.Selenium.DevTools;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using OpenQA.Selenium.Chromium;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace GensouSakuya.QQBot.Core.Helpers
{
    internal class BiliLiveHelper
    {
        private static readonly Logger _logger = Logger.GetLogger<BiliLiveHelper>();
        private static readonly string _spaceHtmlTemplateUrl = "https://space.bilibili.com/{0}";
        private static readonly string _spaceDynamicTemplateUrl = "https://space.bilibili.com/{0}/dynamic";
        private static string ChromeDataPath => Path.Combine(DataManager.DataPath, "chrome");
        private static int _chromePort = 9222;

        public static BiliLiveInfo GetBiliLiveInfo(ulong uid, bool retry = true)
        {
            try
            {
                var spaceUrl = string.Format(_spaceHtmlTemplateUrl, uid);
                var model = new BiliLiveInfo();
                string html;
                using (var driver = GetChromeDriver())
                {
                    try
                    {
                        driver.Navigate().GoToUrl(spaceUrl);
                        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(30));
                        IWebElement element;
                        try
                        {
                            //wait new style
                            element = wait.Until(p => p.FindElement(By.ClassName("living-section")));
                            html = driver.PageSource;
                            return GetBiliLiveInfoV2(html);
                        }
                        catch (WebDriverTimeoutException)
                        {
                            //新版元素没找到，尝试找旧版
                            element = wait.Until(p => p.FindElement(By.ClassName("i-live-on")));
                            html = driver.PageSource;
                            return GetBiliLiveInfoV1(html);
                        }
                    }
                    finally
                    {
                        driver.Quit();
                    }
                }
                
            }
            catch(Exception e)
            {
                _logger.Error(e, "GetBiliLiveInfo failed");
                if(retry)
                {
                    return GetBiliLiveInfo(uid, false);
                }
                else
                {
                    throw;
                }
            }
        }

        private static BiliLiveInfo GetBiliLiveInfoV2(string html)
        {
            var model = new BiliLiveInfo();
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);
            model.UserName = htmlDoc.DocumentNode.SelectSingleNode("//*[@class=\"nickname\"]").InnerText?.Trim();
            var liveInfoNode = htmlDoc.DocumentNode.SelectSingleNode("//*[@class=\"living-section\"]");
            model.Title = liveInfoNode.SelectSingleNode("//*[@class=\"living-section-title\"]").InnerText?.Trim();
            model.Image = liveInfoNode.SelectSingleNode("//*[@alt=\"living cover\"]").GetAttributeValue("src", (string)null);
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
        private static BiliLiveInfo GetBiliLiveInfoV1(string html)
        {
            var model = new BiliLiveInfo();
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

        public static async Task<List<BiliSpaceDynamic>> GetBiliSpaceDynm(string uid, bool retry = true)
        {
            string html = "";
            try
            {
                List<BiliSpaceDynamic> dynamics = null;
                var complementTask = new TaskCompletionSource<string>();
                var spaceUrl = string.Format(_spaceDynamicTemplateUrl, uid);
                var model = new BiliLiveInfo();
                using (var driver = GetChromeDriver())
                {
                    try
                    {
                        string requestId = null;
                        DevToolsSession session = driver.GetDevToolsSession();
                        await session.Domains.Network.EnableNetwork();
                        session.DevToolsEventReceived += Session_DevToolsEventReceived;
                        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                        void Session_DevToolsEventReceived(object? sender, DevToolsEventReceivedEventArgs e)
                        {
                            if (!e.EventData.TryGetProperty("type", out var type))
                                return;

                            if (type.GetString() != "Fetch")
                                return;

                            if (!e.EventData.TryGetProperty("response", out var property) || !property.TryGetProperty("url", out var urlProperty))
                                return;
                            var url = urlProperty.GetString();
                            //Console.WriteLine(url);
                            if (url != null && url.StartsWith("https://api.bilibili.com/x/polymer/web-dynamic/v1/feed/space"))
                            {
                                ////Console.WriteLine(e.EventData);
                                requestId = e.EventData.GetProperty("requestId").GetString();
                                complementTask.SetResult("test");
                                session.DevToolsEventReceived -= Session_DevToolsEventReceived;
                            }
                        }
                        driver.Navigate().GoToUrl(spaceUrl);
                        var dynamicjson = await complementTask.Task.WaitAsync(cancellationTokenSource.Token);

                        if (requestId == null)
                        {
                            return dynamics;
                        }
                        await Task.Delay(5000);
                        var response = driver.ExecuteCdpCommand("Network.getResponseBody", new Dictionary<string, object>() { { "requestId", requestId } }) as Dictionary<string, object>;
                        if (response.TryGetValue("body", out object? bodyObj) && bodyObj != null)
                        {
                            dynamics = new List<BiliSpaceDynamic>();
                            string body = bodyObj.ToString();
                            var json = JObject.Parse(body);
                            var items = json["data"]?["items"]?.ToArray();
                            foreach(var item in items)
                            {
                                var dync = GetDynamicFromJson(item);
                                if (dync != null)
                                    dynamics.Add(dync);
                            }
                        }
                    }
                    finally
                    {
                        html = driver.PageSource;
                        driver.Quit();
                    }
                }

                return dynamics;
            }
            catch (Exception e)
            {
                _logger.Error(e, "GetBiliSpaceDynm failed");
                if (retry)
                {
                    return await GetBiliSpaceDynm(uid, false);
                }
                else
                {
                    if(e is TaskCanceledException)
                        _logger.Error(e, "GetBiliSpaceDynm failed again, html:{0}", Environment.NewLine + html);
                    throw;
                }
            }
        }

        private static BiliSpaceDynamic GetDynamicFromJson(JToken item)
        {
            var dyn = new BiliSpaceDynamic();
            dyn.Id = item["id_str"].Value<string>();
            var type = item["type"].Value<string>();
            var modules = item["modules"];
            dyn.IsTop = modules["module_tag"]?["text"]?.Value<string>() == "置顶";
            dyn.AuthorName = modules["module_author"]?["name"]?.Value<string>();
            var publishTimeSpan = modules["module_author"]?["pub_ts"]?.Value<long>();
            dyn.PublishTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds((double)publishTimeSpan);
            //图文动态 or 纯文本动态
            if (type == "DYNAMIC_TYPE_DRAW" || type == "DYNAMIC_TYPE_WORD")
            {
                var major = modules["module_dynamic"]["major"];
                var dynContent = major["opus"];
                dyn.Images = dynContent["pics"]?.ToArray().Select(p => p["url"].Value<string>()).ToList();
                dyn.Content = dynContent["summary"]["text"].Value<string>();
            }
            //视频动态
            else if (type == "DYNAMIC_TYPE_AV")
            {
                var major = modules["module_dynamic"]["major"];
                dyn.Images = new List<string> { major["archive"]["cover"].Value<string>() };
                dyn.Content = "投稿了视频：" + major["archive"]["title"].Value<string>();
            }
            //转发动态
            else if (type == "DYNAMIC_TYPE_FORWARD")
            {
                var desc = modules["module_dynamic"]["desc"];
                dyn.Content = desc["text"].Value<string>();
                dyn.IsRepost = true;
                dyn.RepostOrigin = GetDynamicFromJson(item["orig"]);
            }
            else
            {
                return null;
            }
            return dyn;
        }

        private static ChromeDriver GetChromeDriver()
        {
            var port = _chromePort++;
            if(_chromePort >= 9230)
            {
                _chromePort = 9222;
            }
            var chromeOpt = new ChromeOptions();
            chromeOpt.AddArgument("--headless");
            chromeOpt.AddArgument("--no-sandbox");
            chromeOpt.AddArgument("--disable-gpu");
            chromeOpt.AddArgument("--disable-dev-shm-usage");
            chromeOpt.AddArgument($"--remote-debugging-port={port}");
            chromeOpt.AddArgument($"--user-data-dir={ChromeDataPath}");
            chromeOpt.SetLoggingPreference("performance", OpenQA.Selenium.LogLevel.Info); //启用performance日志，等级为Info即可
            chromeOpt.PerformanceLoggingPreferences = new ChromiumPerformanceLoggingPreferences()
            {
                IsCollectingNetworkEvents = true //采集网络请求事件
            };
            return new ChromeDriver(chromeOpt);
        }
    }

    internal class BiliLiveInfo
    {
        public string Url { get; set; }
        public string Title { get; set; }
        public string Image { get; set; }
        public string UserName { get; set; }
    }

    internal class BiliSpaceDynamic
    {
        public string Id { get; set; }
        public string Content { get; set; }
        public bool IsTop { get; set; }
        public List<string> Images { get; set; }

        public bool IsRepost { get; set; }

        public string AuthorName { get; set; }
        public DateTime PublishTime { get; set; }
        public BiliSpaceDynamic RepostOrigin { get; set; }
    }
}
