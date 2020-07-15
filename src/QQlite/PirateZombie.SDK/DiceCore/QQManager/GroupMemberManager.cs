using PirateZombie.SDK;
using PirateZombie.SDK.BaseModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using PirateZombie.SDK.DiceCore.QQManager;

namespace net.gensousakuya.dice
{
    public class GroupMemberManager
    {
        public static GroupMember Get(long qq,long groupNo)
        {
            string result = QLAPI.Api_GetGroupMemberList(groupNo.ToString(), false, QLMain.ac);
            //QLAPI.Api_SendLog("Debug", "Api_GetGroupMemberList:" + result, 0, QLMain.ac);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            var info = JsonConvert.DeserializeObject<GroupMemberJsonModel>(result);
            var member = new PirateZombie.SDK.BaseModel.GroupMember();

            member.GroupId = groupNo;
            member.QQId = qq;
            //member.Nick = binary.ReadString_Ex(_defaultEncoding);

            if (!info.members.ContainsKey(qq.ToString()))
                return null;

            var memberInfo = info.members[qq.ToString()];

            var card = Regex.Unescape((memberInfo.cd ?? memberInfo.nk).Replace("&nbsp;", " "));
            member.Card = card;
            //member.Sex = (Sex)binary.ReadInt32_Ex();
            //member.Age = binary.ReadInt32_Ex();
            //member.Area = binary.ReadString_Ex(_defaultEncoding);
            //member.JoiningTime = binary.ReadInt32_Ex().ToDateTime();
            //member.LastDateTime = binary.ReadInt32_Ex().ToDateTime();
            //member.Level = binary.ReadString_Ex(_defaultEncoding);
            member.PermitType = info.owner == qq
                ? PermitType.Holder
                : (info.adm.Contains(qq) ? PermitType.Manage : PermitType.None);
            //member.BadRecord = binary.ReadInt32_Ex() == 1;
            //member.SpecialTitle = binary.ReadString_Ex(_defaultEncoding);
            //member.SpecialTitleDurationTime = binary.ReadInt32_Ex().ToDateTime();
            //member.CanModifiedCard = binary.ReadInt32_Ex() == 1;
            //#endregion
            //var gm = _groupMembers.Find(p => p.QQ == qq && p.GroupNumber == groupNo);
            //if (gm == null)
            //{
            //    gm = new GroupMember(member);
            //    _groupMembers.Add(gm);
            //}
            //else
            //{
            //    gm.Copy(member);
            //}

            return new GroupMember(member);
        }
    }
    public class GroupMemberJsonModel
    {
        public class infoModel
        {
            public long lst { get; set; }
            public long jt { get; set; }
            /// <summary>
            /// qq名
            /// </summary>
            public string nk { get; set; }
            public int? fr { get; set; }
            /// <summary>
            /// 群名片
            /// </summary>
            public string cd { get; set; }
        }

        public Dictionary<string, infoModel> members { get; set; }
        public long owner { get; set; }
        public List<long> adm { get; set; }
    }
}
