using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Commands.V2
{
    internal class QWenCommander : BaseCommanderV2
    {
        private static readonly Logger _logger = Logger.GetLogger<QWenCommander>();
        public override Task<bool> Check(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            if (originMessage == null)
                return Task.FromResult(false);
            var qwenConfig = DataManager.Instance?.QWenConfig;
            if(qwenConfig == null)
                return Task.FromResult(false);
            if (string.IsNullOrWhiteSpace(qwenConfig.APIKey) || string.IsNullOrWhiteSpace(qwenConfig.AppId))
                return Task.FromResult(false);
            if (source.Type != MessageSourceType.Group && source.Type != MessageSourceType.Discuss)
                return Task.FromResult(false);
            if(!DataManager.Instance.GroupQWenConfig.TryGetValue(member.GroupNumber,out var res) || !res)
                return Task.FromResult(false);
            if(!originMessage.Any(p=>p is TextMessage))
                return Task.FromResult(false);
            var atMessage = originMessage.FirstOrDefault(p => p is AtMessage am && am.QQ == DataManager.QQ);
            if (atMessage == null)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }

        public override async Task NextAsync(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            var limit = DataManager.Instance?.QWenLimig;
            var config = DataManager.Instance?.QWenConfig;
            if (config == null)
                return;

            var sourceText = (originMessage.FirstOrDefault(p => p is TextMessage) as TextMessage)?.Text;
            if (string.IsNullOrWhiteSpace(sourceText))
                return;

            var sourceMessageId = (originMessage?.FirstOrDefault() as SourceMessage)?.Id ?? default;
            var messages = new List<BaseMessage>();
            messages.Add(new QuoteMessage(member.GroupNumber, member.QQ, sourceMessageId));
            messages.Add(new AtMessage(member.QQ));

            if(limit?.Check(config.TokenLimit) == false)
            {
                messages.Add(new TextMessage(config.TokenNotEnoughMessage));
                MessageManager.SendToSource(source, messages);
                return;
            }

            var url = "https://dashscope.aliyuncs.com/api/v1/apps/{0}/completion";
            using (var client = new RestClient())
            {
                var req = new RestRequest(string.Format(url, config.AppId));
                req.Method = Method.Post;
                req.AddHeader("Authorization", $"Bearer {DataManager.Instance?.QWenConfig?.APIKey}");
                req.AddJsonBody(new
                {
                    input = new
                    {
                        prompt = sourceText
                    },
                    parameters = new { },
                    debug = new { }
                }
                );
                var res = await client.ExecuteAsync<ResponseModel>(req);
                if(!res.IsSuccessStatusCode)
                {
                    _logger.Info("qwen request failed, response: {0}", res.Content);
                    return;
                }
                if (string.IsNullOrWhiteSpace(res.Data?.Output?.Text))
                {
                    _logger.Info("qwen text is empty, response: {0}", res.Content);
                    return;
                }

                messages.Add(new TextMessage(res.Data?.Output?.Text));
                var usageModel = res.Data?.Usage?.Models?.FirstOrDefault();
                if (usageModel != null)
                {
                    limit?.AddUsed(usageModel.OutputTokens + usageModel.InputTokens);
                }
            }

            MessageManager.SendToSource(source, messages);
            this.StopChain();
        }

        public class ResponseModel
        {
            [JsonProperty("output")]
            public OutputModel Output { get; set; }
            [JsonProperty("usage")]
            public UsageModel Usage { get; set; }

        }

        public class OutputModel
        {
            [JsonProperty("finish_reason")]
            public string FinishReason { get; set; }

            [JsonProperty("session_id")]
            public string SessionId { get; set; }

            [JsonProperty("text")]
            public string Text { get; set; }
        }

        public class UsageModel
        {
            [JsonProperty("models")]
            public ModelUsageModel[] Models { get; set; }
        }

        public class ModelUsageModel
        {
            [JsonProperty("output_tokens")]
            public long OutputTokens { get; set; }
            [JsonProperty("model_id")]
            public string ModelId { get; set; }
            [JsonProperty("input_tokens")]
            public long InputTokens { get; set; }
        }
    }

    public class QWenConfig
    {
        public string APIKey { get; set; } = "";
        public string AppId { get; set; } = "";
        public long TokenLimit { get; set; } = 500000;
        public string TokenNotEnoughMessage { get; set; } = "Token额度不够啦！";
    }

    public class QWenLimit
    {
        public long Used { get; set; }
        public DateTime Expired { get; set; }

        public bool Check(long totalLimit)
        {
            if(DateTime.Now > Expired)
            {
                Reset();
                return true;
            }
            if(totalLimit <= Used)
            {
                return false;
            }
            return true;
        }

        public void AddUsed(long currentUsed)
        {
            Used += currentUsed;
        }

        private void Reset()
        {
            Used = 0;
            var day = DateTime.Today.AddMonths(1);
            Expired = new DateTime(day.Year, day.Month, 1);
        }
    }
}
