using BotSharp.Abstraction.Messaging.JsonConverters;
using BotSharp.Core;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GensouSakuya.QQBot.Agent
{
    public static class QQBotAgentExtensions
    {
        public static IServiceCollection AddAiAgent(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBotSharpCore(configuration, options =>
            {
                options.JsonSerializerOptions.Converters.Add(new RichContentJsonConverter());
                options.JsonSerializerOptions.Converters.Add(new TemplateMessageJsonConverter());
            });
            return services.AddScoped<IQQBotAgent, QQBotAgent>();
        }

        public static IApplicationBuilder UseAgent(this IApplicationBuilder builder)
        {
            builder.UseBotSharp();
            return builder;
        }
    }
}
