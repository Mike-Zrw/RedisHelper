using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisHelper.Test
{
    public class RedisSubPub
    {
        public void SubScribe(string cnl)
        {
            Console.WriteLine("主线程：" + Thread.CurrentThread.ManagedThreadId);
            var sub = RedisCache.Instance.GetSubscriber();
            sub.Subscribe(cnl, SubHandel);
            Console.WriteLine("订阅了一个频道："+ cnl);
        }
        public void SubHandel(RedisChannel cnl, RedisValue val)
        {
            Console.WriteLine();
            Console.WriteLine("频道：" + cnl + "\t收到消息:" + val); ;
            Console.WriteLine("线程：" + Thread.CurrentThread.ManagedThreadId + ",是否线程池：" + Thread.CurrentThread.IsThreadPoolThread);
            if (val == "close")
                RedisCache.Instance.GetSubscriber().Unsubscribe(cnl);
            if (val == "closeall")
                RedisCache.Instance.GetSubscriber().UnsubscribeAll();
        }
    }
}
