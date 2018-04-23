using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisHelper.Test
{
    public class RedisTran
    {
        public void TestTran()
        {
            IDatabase db = RedisCache.GetDatabase();
            string name = db.StringGet("name");
            string age = db.StringGet("age");
            Console.WriteLine("NAME:" + name);
            Console.WriteLine("Age:" + age);
            var tran = db.CreateTransaction();
            tran.AddCondition(Condition.StringEqual("name", name));
            Console.WriteLine("tran begin");
            tran.StringSetAsync("name", "leap");
            tran.StringSetAsync("age", 12);
            Thread.Sleep(40000);
            bool result = tran.Execute();
            Console.WriteLine("执行结果：" + result);
            Console.WriteLine("Age:" + db.StringGet("age"));
            Console.WriteLine("Name:" + db.StringGet("name"));
        }
    }
}
