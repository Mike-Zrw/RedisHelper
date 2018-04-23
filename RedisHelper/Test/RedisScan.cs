using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisHelper.Test
{
    public class RedisScan
    {
        public void RedisSetScan()
        {
            IDatabase db = RedisCache.GetDatabase();
            
            for (int i = 1; i <= 20; i++)
            {
                db.SetAdd("文章1", i);
            }
            for (int i = 1; i <= 20; i++)
            {
                db.SetAdd("文章2", i);
            }
            List<RedisValue> result = db.SetScan("文章2","1*").ToList();
            foreach (var item in result)
            {
                Console.WriteLine(item);
            }
            db.KeyDelete("文章1");
            db.KeyDelete("文章2");
            Console.ReadLine();
        }
        public void RedisScanMakeDate()
        {
            IDatabase db = RedisCache.GetDatabase();
            for (int i = 0; i < 50; i++)
            {
                db.StringSet("testkey" + i, i);
            }
        }
        
        public void ExcuteCommand()
        {
            IDatabase db = RedisCache.GetDatabase();
            db.Execute("SET", "name", "张三");
            Console.WriteLine(db.Execute("GET", "name"));
            db.KeyDelete("name");
            Console.ReadLine();
        }
    }
}
