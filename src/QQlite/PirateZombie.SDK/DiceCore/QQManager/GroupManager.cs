using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.gensousakuya.dice
{
    public class GroupManager
    {
        private static List<Group> _groups
        {
            get { return DataManager.Instance.Groups; }
        }

        public static Group Get(long groupNo)
        {
            var group = _groups.Find(p => p.GroupNumber == groupNo);
            if (group == null)
            {
                ///
            }
            return _groups.Find(p => p.GroupNumber == groupNo);
        }

        public static Group Add(Group group)
        {
            if (_groups.Any(p => p.GroupNumber == group.GroupNumber))
                return _groups.Find(p => p.GroupNumber == group.GroupNumber);
            else
            {
                _groups.Add(group);
                return group;
            }
        }
    }
}
