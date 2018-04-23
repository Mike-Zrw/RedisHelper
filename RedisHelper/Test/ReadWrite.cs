using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisHelper.Test
{
    public class ReadWrite
    {
        public static void SetTest()
        {
            //Console.WriteLine("Redis写入缓存：Name:张三丰");
            //StackExchangeRedisHelper.Set("Name", "张三丰", new TimeSpan(0, 0, 0, 0, 1000));
            //Console.WriteLine("Redis获取缓存：Name：" + StackExchangeRedisHelper.Get("Name").ToString());
            //Thread.Sleep(1000);
            //Console.WriteLine("一秒后Redis获取缓存：Name：" + StackExchangeRedisHelper.Get("Name") ?? "");
            ICache cache = new RedisCache();
            Console.WriteLine(cache.Exists("stu"));
            List<Student> liststu = new List<Student>() {
                new Student(){ AGe=1,Name="123" },
            };
            cache.SetCache("stu", liststu, new TimeSpan(0, 0, 0, 0, 1000));
            List<Student> liststu2 = cache.GetCache<List<Student>>("stu");
            Console.WriteLine(liststu2[0].Name);
            cache.SetCache<string>("name", "111", new TimeSpan(0, 0, 0, 0, 1000));
            string str = cache.GetCache<string>("name");
            Console.WriteLine(str);
            Console.ReadKey();
        }
    }

    public class Student
    {
        public string Name { get; set; }
        public int AGe { get; set; }
    }
}
