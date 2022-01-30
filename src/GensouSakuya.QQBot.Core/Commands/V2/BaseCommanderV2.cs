using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Commands.V2
{
    internal abstract class BaseCommanderV2
    {
        public abstract Task<bool> Check(List<BaseMessage> originMessage, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member);

        public bool IsHandled { get; set; }

        protected void StopChain()
        {
            this.IsHandled = true;
        }

        public abstract Task NextAsync(List<BaseMessage> originMessage, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member);
    }

    internal class CommanderEngine
    {
        private static readonly Logger _logger = Logger.GetLogger<CommanderEngine>();

        public CommanderEngine(List<BaseCommanderV2> commanders)
        {
            Commanders = commanders;
        }

        public List<BaseCommanderV2> Commanders { get; }

        public async Task<bool> ExecuteAsync(List<BaseMessage> originMessage, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            var handled = false;
            try
            {
                if (Commanders == null)
                    return handled;

                foreach(var commander in Commanders)
                {
                    if (!await commander.Check(originMessage, sourceType, qq, group, member))
                        continue;

                    await commander.NextAsync(originMessage, sourceType, qq, group, member);

                    if (commander.IsHandled)
                    {
                        handled = true;
                        break;
                    }
                }
                return handled;
            }
            catch(Exception e)
            {
                _logger.Error(e, "execute message error");
                return handled;
            }
        }
    }
}
