using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Handlers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Helpers
{
    internal class AiServiceFactory
    {
        private readonly DataManager _dataManager;
        private readonly IServiceProvider _serviceProvider;
        public AiServiceFactory(DataManager dataManager, IServiceProvider serviceProvider) 
        {
            _dataManager = dataManager;
            _serviceProvider = serviceProvider;
        }

        public IAiService Acquire()
        {
            var config = _dataManager?.Config?.AiConfig;
            if (config == null || config.Type == Handlers.AiType.None)
            {
                return null; 
            }

            switch (config.Type)
            {
                case Handlers.AiType.QWen:
                    return _serviceProvider.GetService<QWenAiService>();
                case Handlers.AiType.Deepseek:
                    return _serviceProvider.GetService<DeepseekAiService>();
                default:
                    return null;
            }
        }
    }

    internal static class AiServiceFactoryExtension
    {
        public static IServiceCollection AddAi(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<AiServiceFactory>();
            serviceCollection.AddScoped<QWenAiService>();
            serviceCollection.AddScoped<DeepseekAiService>();
            return serviceCollection;
        }
    }

    internal interface IAiService
    {
        Task<string> Chat(string userMessage);
    }

    internal class QWenAiService : IAiService
    {
        private const string ChatUrlTemplate = "https://dashscope.aliyuncs.com/api/v1/apps/{0}/completion";
        private readonly AiConfig _config;
        private readonly ILogger _logger;
        public QWenAiService(DataManager dataManager, ILoggerFactory loggerFactory)
        {
            _config = dataManager?.Config?.AiConfig;
            _logger = loggerFactory?.CreateLogger<QWenAiService>();
        }

        public async Task<string> Chat(string userMessage)
        {
            var appId = _config?.Spec?.Value<string>("AppId");
            if (string.IsNullOrWhiteSpace(appId))
                return null;

            using (var client = new RestClient())
            {
                var req = new RestRequest(string.Format(ChatUrlTemplate, appId));
                req.Method = Method.Post;
                req.AddHeader("Authorization", $"Bearer {_config.APIKey}");
                req.AddJsonBody(new
                    {
                        input = new
                        {
                            prompt = userMessage
                        },
                        parameters = new { },
                        debug = new { }
                    }
                );
                var res = await client.ExecuteAsync<ResponseModel>(req);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogInformation("qwen request failed, response: {0}", res.Content);
                    return null;
                }
                if (string.IsNullOrWhiteSpace(res.Data?.Output?.Text))
                {
                    _logger.LogInformation("qwen text is empty, response: {0}", res.Content); 
                    return null;
                }

                var reply = res.Data?.Output?.Text;
                //var usageModel = res.Data?.Usage?.Models?.FirstOrDefault();
                //if (usageModel != null)
                //{
                //    limit?.AddUsed(usageModel.OutputTokens + usageModel.InputTokens);
                //}
                return reply;
            }
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

    internal class DeepseekAiService : IAiService
    {
        private const string ChatUrl = "https://api.deepseek.com/chat/completions";
        private readonly AiConfig _config;
        private readonly ILogger _logger;
        public DeepseekAiService(DataManager dataManager, ILoggerFactory loggerFactory)
        {
            _config = dataManager?.Config?.AiConfig;
            _logger = loggerFactory?.CreateLogger<DeepseekAiService>();
        }

        public async Task<string> Chat(string userMessage)
        {
            string systemPrompt = null;
            string userPrompt = null;
            var systemPromptFilePath = _config.SystemPromptFilePath;
            if (systemPromptFilePath != null && File.Exists(systemPromptFilePath))
            {
                systemPrompt = File.ReadAllText(systemPromptFilePath);
            }

            var userPromptFilePath = _config.UserPrompFilePath;
            if (userPromptFilePath != null && File.Exists(userPromptFilePath))
            {
                var userPromptTemp = File.ReadAllText(userPromptFilePath);
                if(userPromptTemp.Contains("{Placeholder}", StringComparison.OrdinalIgnoreCase))
                {
                    userPrompt = userPromptTemp.Replace("{Placeholder}", userMessage);
                }
                else
                {
                    userPrompt = userPromptTemp + userMessage;
                }
            }
            else
            {
                userPrompt = userMessage;
            }

            using (var client = new RestClient())
            {
                var req = new RestRequest(ChatUrl);
                req.Method = Method.Post;
                req.AddHeader("Authorization", $"Bearer {_config.APIKey}");
                req.AddJsonBody(new
                {
                    model = "deepseek-chat",
                    messages = string.IsNullOrWhiteSpace(systemPrompt) ?
                        new[]
                        {
                            new
                            {
                                role="user",
                                content=userPrompt
                            }
                        } :
                        new[] {
                            new
                            {
                                role="system",
                                content = systemPrompt
                            },
                            new
                            {
                                role="user",
                                content=userPrompt
                            }
                        },
                    stream = false
                });
                var res = await client.ExecuteAsync<ResponseModel>(req);
                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogInformation("deepseek request failed, response: {0}", res.Content);
                    return null;
                }
                var reply = res.Data?.Choices?.FirstOrDefault()?.Message?.Content;
                if (string.IsNullOrWhiteSpace(reply))
                {
                    _logger.LogInformation("deepseek text is empty, response: {0}", res.Content);
                    return null;
                }

                return reply;
            }
        }

        public class ResponseModel
        {
            public string Id { get; set; }
            public string Object { get; set; }
            public long Created { get; set; }
            public string Model { get; set; }
            public List<ChoiceModel> Choices { get; set; }
            public string SystemFingerprint { get; set; }
            //public Usage Usage { get; set; }
        }

        public class ChoiceModel
        {
            public int Index { get; set; }
            public MessageModel Message { get; set; }
            public string FinishReason { get; set; }
        }

        public class MessageModel
        {
            public string Role { get; set; }
            public string Content { get; set; }
        }
    }
}
