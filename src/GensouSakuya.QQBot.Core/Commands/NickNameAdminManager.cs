using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using GensouSakuya.QQBot.Core.QQManager;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("ann")]
    public class NickNameAdminManager : BaseManager
    {
        public override async Task ExecuteAsync(MessageSource source, List<string> command, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
        {
            await Task.Yield();
            if (member == null)
            {
                return;
            }
            if (!member.IsGroupAdmin())
            {
                return;
            }
            string message = null;
            if (!long.TryParse(command.FirstOrDefault(), out var targetQQ))
            {
                return;
            }

            var targetMember = await GroupMemberManager.Get(targetQQ, member.GroupId);
            if (targetMember == null)
            {
                return;
            }

            var originText = (originMessage.ElementAt(0) is TextMessage tm) ? tm.Text : null;
            if (originText == null)
                return;

            var index = originText.IndexOf(targetQQ.ToString(), StringComparison.Ordinal)+ targetQQ.ToString().Length;
            var newNickName = originText.Length < index + 1 ? null : originText.Substring(index + 1);
            if (string.IsNullOrWhiteSpace(newNickName))
            {
                DelNickName(targetMember, ref message);
            }
            else
            {
                SetNickName(targetMember, newNickName, ref message);
            }

            MessageManager.SendTextMessage(source.Type, message, member.QQ, member.GroupNumber);

        }

        public static void SetNickName(GroupMember member, string nickName, ref string message)
        {
            var oldName = member.GroupName;
            member.NickName = nickName;
            message = $"已将{oldName}的昵称更改为{nickName}";
            DataManager.NoticeConfigUpdatedAction();
        }

        public static void DelNickName(GroupMember member,ref string message)
        {
            if (string.IsNullOrWhiteSpace(member.NickName))
            {
                message = $"{member.GroupName}没有设置昵称，无法删除";
            }
            else
            {
                var oldNickName = member.NickName;
                member.NickName = null;
                message = $"已将{oldNickName}的昵称删除";
                DataManager.NoticeConfigUpdatedAction();
            }
        }
    }
}
