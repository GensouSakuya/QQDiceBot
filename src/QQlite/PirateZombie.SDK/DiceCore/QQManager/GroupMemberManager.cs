using PirateZombie.SDK;
using PirateZombie.SDK.BaseModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using PirateZombie.SDK.DiceCore.QQManager;

namespace net.gensousakuya.dice
{
    public class GroupMemberManager
    {
        private static Encoding _defaultEncoding => Config.DefaultEncoding;
        private static List<GroupMember> _groupMembers => DataManager.Instance.GroupMember;

        public static GroupMember Get(long qq,long groupNo)
        {
            GroupMember member;
            string result = QLAPI.Api_GetGroupMemberList(groupNo.ToString(), false, QLMain.ac);
            if (string.IsNullOrEmpty(result))
            {
                member = null;
                return member;
            }
            #region --其它_转换_文本到群成员信息--
            BinaryReader binary = new BinaryReader(new MemoryStream(Convert.FromBase64String(result)));
            member = new GroupMember();
            member.GroupId = binary.ReadInt64_Ex();
            member.QQId = binary.ReadInt64_Ex();
            member.Nick = binary.ReadString_Ex(_defaultEncoding);
            member.Card = binary.ReadString_Ex(_defaultEncoding);
            member.Sex = (Sex)binary.ReadInt32_Ex();
            member.Age = binary.ReadInt32_Ex();
            member.Area = binary.ReadString_Ex(_defaultEncoding);
            member.JoiningTime = binary.ReadInt32_Ex().ToDateTime();
            member.LastDateTime = binary.ReadInt32_Ex().ToDateTime();
            member.Level = binary.ReadString_Ex(_defaultEncoding);
            member.PermitType = (PermitType)binary.ReadInt32_Ex();
            member.BadRecord = binary.ReadInt32_Ex() == 1;
            member.SpecialTitle = binary.ReadString_Ex(_defaultEncoding);
            member.SpecialTitleDurationTime = binary.ReadInt32_Ex().ToDateTime();
            member.CanModifiedCard = binary.ReadInt32_Ex() == 1;
            #endregion
            var gm = _groupMembers.Find(p => p.QQ == qq && p.GroupNumber == groupNo);
            if (gm == null)
            {
                gm = new GroupMember(member);
                _groupMembers.Add(gm);
            }
            else
            {
                gm.Copy(member);
            }

            return gm;
        }
    }
}
