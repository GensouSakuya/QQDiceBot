using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Handlers
{
    internal class HandlerResolver
    {
        private Dictionary<string, Type> _commandHandlersMap = new Dictionary<string, Type>();
        private List<Type> _chainHandlers = new List<Type>();
        
        public Task RegisterHandlers(IServiceCollection serviceCollection)
        {
            var baseHandlerType = typeof(IMessageHandler);
            var baseCommandHandlerType = typeof(IMessageCommandHandler);
            var baseChainHandlerType = typeof(IMessageChainHandler);
            var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(p => p.IsClass && !p.IsAbstract && baseHandlerType.IsAssignableFrom(p));
            foreach (var handlerType in handlerTypes)
            {
                serviceCollection.AddSingleton(handlerType);
                if (baseCommandHandlerType.IsAssignableFrom(handlerType))
                {
                    var customCommands = handlerType.GetCustomAttributes<CommandAttribute>().Select(p => p.Command);
                    if (customCommands.Any())
                    {
                        foreach (var com in customCommands)
                        {
                            _commandHandlersMap[com.ToLower()] = handlerType;
                        }
                    }
                    else
                    {
                        var handlerClassName = handlerType.Name;
                        var suffix = "handler";
                        var handlerName = handlerClassName.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) ? handlerClassName.Substring(0, handlerClassName.Length - suffix.Length) : handlerClassName;
                        _commandHandlersMap[handlerClassName.ToLower()] = handlerType;
                    }
                }
                else if (baseChainHandlerType.IsAssignableFrom(handlerType))
                {
                    _chainHandlers.Add(handlerType);
                }
            }

            return Task.CompletedTask;
        }

        public IMessageCommandHandler GetCommandHandler(IServiceProvider serviceProvider,string command)
        { 
            var lowerCommand = command.ToLower();
            if (_commandHandlersMap.ContainsKey(lowerCommand))
                return (IMessageCommandHandler)serviceProvider.GetService(_commandHandlersMap[lowerCommand]);
            return null;
        }

        public IEnumerable<IMessageChainHandler> GetChainHandlers(IServiceProvider serviceProvider)
        {
            foreach (var handlerType in _chainHandlers)
            {
                var handler = (IMessageChainHandler)serviceProvider.GetService(handlerType);
                yield return handler;
            }
        }
    }
}
