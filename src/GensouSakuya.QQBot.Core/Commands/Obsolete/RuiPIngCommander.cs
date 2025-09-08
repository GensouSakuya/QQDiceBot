//using GensouSakuya.QQBot.Core.Base;
//using GensouSakuya.QQBot.Core.Model;
//using GensouSakuya.QQBot.Core.PlatformModel;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;

//namespace GensouSakuya.QQBot.Core.Commands.V2
//{
//    internal class RuipingCommander : BaseCommanderV2
//    {
//        public override async Task<bool> Check(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
//        {
//            await Task.Yield();
//            if (originMessage == null)
//                return false;
//            if (RuipingSentences == null || !RuipingSentences.Any())
//                return false;
//            if (source.Type != MessageSourceType.Group && source.Type != MessageSourceType.Discuss)
//                return false;
//            if ((originMessage?.FirstOrDefault() as SourceMessage)?.Id == null)
//                return false;
//            var atMessage = originMessage.FirstOrDefault(p => p is AtMessage am && am.QQ == DataManager.QQ);
//            if (atMessage == null)
//                return false;
//            if (!originMessage.Any(p => p is TextMessage tm && tm.Text.Contains("锐评")))
//                return false;

//            return true;
//        }

//        public override async Task NextAsync(MessageSource source, List<BaseMessage> originMessage, UserInfo qq, Group group, GroupMember member, GuildUserInfo guildUser, GuildMember guildmember)
//        {
//            await Task.Yield();

//            var sourceMessageId = (originMessage?.FirstOrDefault() as SourceMessage)?.Id ?? default;
//            var messages = new List<BaseMessage>();
//            messages.Add(new ReplyMessage(member.GroupNumber, member.QQ, sourceMessageId));
//            var sentence = GetRandomRuipingSentence();
//            messages.Add(new TextMessage(sentence));
//            MessageManager.SendToSource(source, messages);
//        }

//        private static Random _random = new Random();

//        private static List<string> _ruipingSentences = new List<string>();
//        public static List<string> RuipingSentences
//        {
//            get => _ruipingSentences;
//            set
//            {
//                if (value == null)
//                {
//                    _ruipingSentences = new List<string>();
//                }
//                else
//                {
//                    _ruipingSentences = value;
//                }
//            }
//        }

//        public string GetRandomRuipingSentence()
//        {
//            return RuipingSentences[_random.Next(0, RuipingSentences.Count)];
//        }
//    }
//}
