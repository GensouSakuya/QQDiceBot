using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GensouSakuya.QQBot.Core.Commands.V2
{
    internal abstract class BaseCommanderV2
    {
        public abstract Task<bool> Check(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember);

        public bool IsHandled { get; private set; }

        protected void StopChain()
        {
            this.IsHandled = true;
        }

        public abstract Task NextAsync(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember);
    }

    internal class CommanderEngine
    {
        private static readonly Logger _logger = Logger.GetLogger<CommanderEngine>();

        public CommanderEngine(List<BaseCommanderV2> commanders)
        {
            Commanders = commanders;
        }

        public List<BaseCommanderV2> Commanders { get; }

        public async Task<bool> ExecuteAsync(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            var handled = false;
            try
            {
                if (Commanders == null)
                    return handled;

                foreach (var commander in Commanders)
                {
                    if (!await commander.Check(source, originMessage, qq, group, member, guildUser, guildmember))
                        continue;

                    await commander.NextAsync(source, originMessage, qq, group, member, guildUser, guildmember);

                    if (commander.IsHandled)
                    {
                        handled = true;
                        break;
                    }
                }
                return handled;
            }
            catch (Exception e)
            {
                _logger.Error(e, "execute message error");
                return handled;
            }
        }
    }
}
