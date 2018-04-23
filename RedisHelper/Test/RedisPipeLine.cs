using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisHelper.Test
{
    public class RedisPipeLine
    {
        public void TestPipeLine()
        {
            IDatabase db = RedisCache.GetDatabase();
            var batch = db.CreateBatch();
            Task t1 = batch.StringSetAsync("name", "bob");
            Task t2 = batch.StringSetAsync("age", 100);
            batch.Execute();
            Task.WaitAll(t1, t2);
            Console.WriteLine("Age:" + db.StringGet("age"));
            Console.WriteLine("Name:" + db.StringGet("name"));
        }
    }
}
