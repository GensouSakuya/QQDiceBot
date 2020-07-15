using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using net.gensousakuya.dice;

namespace test
{
    class Program
    {
        static void Main(string[] args)
        {
            DataManager.Init();
            new AdminManager().ExecuteAsync(new List<string> {"rename", "aaa"}, EventSourceType.Friend,
                new UserInfo {QQ = 0}, null, null).Wait();
        }
    }
}
