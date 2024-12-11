using GensouSakuya.QQBot.Core.PlatformModel;
using System;

namespace GensouSakuya.QQBot.Core.Model
{
    public class SubscribeModel
    {
        public MessageSourceType Source { get; set; }
        public string SourceId { get; set; }

        public override bool Equals(object obj)
        {
            return obj is SubscribeModel model &&
                   Source == model.Source &&
                   SourceId == model.SourceId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Source, SourceId);
        }

        public override string ToString()
        {
            return $"{Source}:{SourceId}";
        }
    }
}
