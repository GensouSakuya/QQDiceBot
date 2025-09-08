using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Helpers;
using GensouSakuya.QQBot.Agent;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using GensouSakuya.QQBot.Core.Handlers;
using System.Threading.Tasks;
using BotSharp.Core.Infrastructures;
using BotSharp.Core.Plugins;

namespace GensouSakuya.QQBot.Core
{
    public static class QQBotExtensions
    {
        public static IApplicationBuilder UseQQBot(this IApplicationBuilder builder)
        {
            builder.UseAgent();
            return builder;
        }

        public static async Task QQInit(this WebApplication app)
        {
            //app.UseBotSharp();
            //var handlerResolver = app.Services.GetRequiredService<HandlerResolver>();
            //_serviceCollection
            //    .AddAi()
            //    .AddAiAgent(_configuration)
            //    .AddSingleton<DataManager>();
            //CommandCenter.ReloadManagers();
        }

        public static IServiceCollection AddQQBot(this IServiceCollection services, IConfiguration configuration, BaseConfig baseConfig)
        {
            services.AddLogging(p =>
            {
                p.AddConfiguration(configuration).AddSerilog(new LoggerConfiguration()
                    .ReadFrom.Configuration(configuration)
                    .CreateLogger());
            });
            services.AddSingleton(p => baseConfig);
            services.AddSingleton<Core>();
            services.AddAi()
                .AddAiAgent(configuration)
                .AddSingleton<DataManager>()
                .AddSingleton<HandlerResolver>();
            var handlerResolver = new HandlerResolver();
            handlerResolver.RegisterHandlers(services);
            services.AddSingleton(handlerResolver);
            return services;
        }
    }
}
