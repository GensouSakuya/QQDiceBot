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
            new ShaDiaoTuManager().ExecuteAsync(new List<string>
                {"add", "[QQ:pic=a50613af-c820-ef98-4c24-9b940ebe67e0.jpg]"}, EventSourceType.Group, new UserInfo
            {
                QQ = 11111
            }, null, new GroupMember
            {
                GroupId = 1111,
                QQ = 1111
            }).GetAwaiter().GetResult();
        }
    }
}
