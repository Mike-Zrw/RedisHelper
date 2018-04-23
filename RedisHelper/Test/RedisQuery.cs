using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisHelper.Test
{
    public class RedisQuery
    {
        public void LatestUserTop10()
        {
            IDatabase db = RedisCache.GetDatabase();
            //模拟有一百名用户
            for (int i = 1; i <= 100; i++)
            {
                db.ListLeftPush("user", "用户" + i);
                //每一名用户插入都取最后的十个用户
                db.ListTrim("user", 0, 9);
            }
            RedisValue[] userStores = db.ListRange("user");
            foreach (var item in userStores)
            {
                Console.Write((string)item + ",");
            }
            db.KeyDelete("user");
            Console.ReadLine();
        }

        public void RedisSetTest()
        {
            IDatabase db = RedisCache.GetDatabase();

            for (int i = 1; i <= 20; i++)
            {
                db.SetAdd("文章1", i);
            }
            for (int i = 15; i <= 35; i++)
            {
                db.SetAdd("文章2", i);
            }
            RedisValue[] inter = db.SetCombine(SetOperation.Intersect, "文章1", "文章2");
            RedisValue[] union = db.SetCombine(SetOperation.Union, "文章1", "文章2");
            RedisValue[] dif1 = db.SetCombine(SetOperation.Difference, "文章1", "文章2");
            RedisValue[] dif2 = db.SetCombine(SetOperation.Difference, "文章2", "文章1");
            int x = 0;
            Console.WriteLine("两篇文章都评论过的用户");
            foreach (var item in inter.OrderBy(m => m).ToList())
            {
                Console.Write((string)item + "  ");
            }
            Console.WriteLine("\n评论过两篇文章中任意一篇文章的用户");
            foreach (var item in union.OrderBy(m => m).ToList())
            {
                Console.Write((string)item + "  ");
            }
            Console.WriteLine("\n只评论过其第一篇文章的用户");
            foreach (var item in dif1.OrderBy(m => m).ToList())
            {
                Console.Write((string)item + "  ");
            }
            Console.WriteLine("\n只评论过其第二篇文章的用户");
            foreach (var item in dif2.OrderBy(m => m).ToList())
            {
                Console.Write((string)item + "  ");
            }
            db.KeyDelete("文章1");
            db.KeyDelete("文章2");
            Console.ReadLine();
        }
        public void RedisHashTest()
        {
            IDatabase db = RedisCache.GetDatabase();
            db.HashSet("student1", "name", "张三");
            db.HashSet("student1", "age", 12);
            db.HashSet("student1", "class", "五年级");
            Console.WriteLine(db.HashGet("student1", "name"));
            RedisValue[] result = db.HashGet("student1", new RedisValue[] { "name", "age", "class" });
            Console.WriteLine(string.Join(",", result));
            db.KeyDelete("student1");
            Console.ReadLine();
        }
        public void RedisHashVsStringSet()
        {
            IDatabase db = RedisCache.GetDatabase();
            Stopwatch sw = new Stopwatch();
            sw.Start();
            //for (int i = 0; i < 100000; i++)
            //{
            //    db.HashSet("studenths" + i, "name", "张三" + i);
            //    db.HashSet("studenths" + i, "age", 12 + i);
            //    db.HashSet("studenths" + i, "class", "五年级" + i);
            //}
            //Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            //sw.Restart();
            for (int i = 0; i < 100000; i++)
            {
                db.StringSet("studentstr_name" + i, "张三" + i);
                db.StringSet("studentstr_age" + i, 12 + i);
                db.StringSet("studentstr_class" + i, "五年级" + i);
            }
            Console.WriteLine(sw.Elapsed.TotalMilliseconds);
            //for (int i = 0; i < 100000; i++)
            //{
            //    db.KeyDelete("studenths" + i);
            //    db.KeyDelete("studentstr_name" + i);
            //    db.KeyDelete("studentstr_age" + i);
            //    db.KeyDelete("studentstr_class" + i);
            //}
            Console.ReadLine();
        }
        public void HotestUserTop10()
        {
            IDatabase db = RedisCache.GetDatabase();
            //模拟有一百名评论者，开始每个用户被“赞”的次数为1
            List<SortedSetEntry> entrys = new List<SortedSetEntry>();
            for (int i = 1; i <= 100; i++)
            {
                db.SortedSetAdd("文章1", "评论者" + i, 1);
            }
            //评论者2又被赞了两次
            db.SortedSetIncrement("文章1", "评论者2", 2); //对应的值的score+2
            //评论者101被赞了4次
            db.SortedSetIncrement("文章1", "评论者101", 4);  //若不存在该值，则插入一个新的
            RedisValue[] userStores = db.SortedSetRangeByRank("文章1", 0, 10, Order.Descending);
            for (int i = 0; i < userStores.Length; i++)
            {
                Console.WriteLine(userStores[i] + ":" + db.SortedSetScore("文章1", userStores[i]));
            }
            db.KeyDelete("文章1");
            Console.ReadLine();
        }
    }
}
