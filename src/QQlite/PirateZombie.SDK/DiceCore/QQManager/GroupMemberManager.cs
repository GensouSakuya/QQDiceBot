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
        public static List<GroupMember> GroupMembers = new List<GroupMember>();

        public static GroupMember Get(long qq,long groupNo)
        {
            var member = GroupMembers.Find(p => p.QQ == qq && p.GroupId == groupNo);
            if (member != null)
                return member;

            string result = QLAPI.Api_GetGroupMemberList(groupNo.ToString(), false, QLMain.ac);
            //QLAPI.Api_SendLog("Debug", "Api_GetGroupMemberList:" + result, 0, QLMain.ac);
            if (string.IsNullOrEmpty(result))
            {
                return null;
            }
            var info = JsonConvert.DeserializeObject<GroupMemberJsonModel>(result);
            if (!info.members.ContainsKey(qq.ToString()))
                return null;

            info.members.Select(p => new
            {
                QQ = p.Key,
                member = p.Value
            }).ToList().ForEach(p =>
            {
                var basemember = new PirateZombie.SDK.BaseModel.GroupMember();

                basemember.GroupId = groupNo;
                basemember.QQId = long.Parse(p.QQ);
                //member.Nick = binary.ReadString_Ex(_defaultEncoding);

                var memberInfo = info.members[p.QQ];

                var card = Regex.Unescape((memberInfo.cd ?? memberInfo.nk).Replace("&nbsp;", " "));
                basemember.Card = card;
                //member.Sex = (Sex)binary.ReadInt32_Ex();
                //member.Age = binary.ReadInt32_Ex();
                //member.Area = binary.ReadString_Ex(_defaultEncoding);
                //member.JoiningTime = binary.ReadInt32_Ex().ToDateTime();
                //member.LastDateTime = binary.ReadInt32_Ex().ToDateTime();
                //member.Level = binary.ReadString_Ex(_defaultEncoding);
                basemember.PermitType = info.owner == basemember.QQId
                    ? PermitType.Holder
                    : (info.adm!=null && info.adm.Any() && info.adm.Contains(basemember.QQId) ? PermitType.Manage : PermitType.None);
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
                member = new GroupMember(basemember);
                GroupMembers.Add(member);
            });
            member = GroupMembers.Find(p => p.QQ == qq && p.GroupId == groupNo);
            return member;
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
