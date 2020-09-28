using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GensouSakuya.QQBot.Core.Base;
using GensouSakuya.QQBot.Core.Model;
using GensouSakuya.QQBot.Core.PlatformModel;
using net.gensousakuya.dice;

namespace GensouSakuya.QQBot.Core.Commands
{
    [Command("nn")]
    public class NickNameManager : BaseManager
    {
        public override async Task ExecuteAsync(List<string> command, List<BaseMessage> originMessage, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            await Task.Yield();
            if (member == null)
            {
                return;
            }
            var newNickName = command.FirstOrDefault();
            string message = null;
            if (string.IsNullOrWhiteSpace(newNickName))
            {
                DelNickName(member, ref message);
            }
            else
            {
                SetNickName(member, newNickName, ref message);
            }

            MessageManager.SendTextMessage(sourceType, message, member.QQ, member.GroupNumber);

        }

        public static void SetNickName(GroupMember member, string nickName, ref string message)
        {
            var oldName = member.GroupName;
            member.NickName = nickName;
            message = $"已将{oldName}的昵称更改为{nickName}";
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
            }
        }
    }
}