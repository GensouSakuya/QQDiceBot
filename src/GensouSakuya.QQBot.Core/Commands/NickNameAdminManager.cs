﻿using System;
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
        public override async Task ExecuteAsync(List<string> command, List<BaseMessage> originMessage, MessageSourceType sourceType, UserInfo qq, Group group, GroupMember member)
        {
            await Task.Yield();
            if (member == null)
            {
                return;
            }
            var permit = member.PermitType;
            if (permit == PermitType.None)
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

            var originText = (originMessage.ElementAt(1) is TextMessage tm) ? tm.Text : null;
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

            MessageManager.SendTextMessage(sourceType, message, member.QQ, member.GroupNumber);

        }

        public static void SetNickName(GroupMember member, string nickName, ref string message)
        {
            var oldName = member.GroupName;
            member.NickName = nickName;
            message = $"已将{oldName}的昵称更改为{nickName}";
            DataManager.Instance.NoticeConfigUpdated();
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
                DataManager.Instance.NoticeConfigUpdated();
            }
        }
    }
}