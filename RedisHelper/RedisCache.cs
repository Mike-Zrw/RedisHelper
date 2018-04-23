using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RedisHelper
{
    internal class RedisCache : ICache
    {
        private static readonly string Coonstr = ConfigHelper.GetConfig<string>("RedisConnectionString");//192.168.1.2:6379,password=123,DefaultDatabase=15
        private static object _locker = new Object();
        private static ConnectionMultiplexer _instance = null;
        private static ConfigurationOptions ConfigurationOption;
        /// <summary>
        /// 使用一个静态属性来返回已连接的实例，如下列中所示。这样，一旦 ConnectionMultiplexer 断开连接，便可以初始化新的连接实例。
        /// </summary>
        internal static ConnectionMultiplexer Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_locker)
                    {
                        if (_instance == null || !_instance.IsConnected)
                        {
                            ConfigurationOption = ConfigurationOptions.Parse(Coonstr);
                            _instance = ConnectionMultiplexer.Connect(Coonstr);

                            //注册如下事件
                            _instance.ConnectionFailed += MuxerConnectionFailed;
                            _instance.ConnectionRestored += MuxerConnectionRestored;
                            _instance.ErrorMessage += MuxerErrorMessage;
                            _instance.ConfigurationChanged += MuxerConfigurationChanged;
                            _instance.HashSlotMoved += MuxerHashSlotMoved;
                            _instance.InternalError += MuxerInternalError;
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 获取当前应用程序指定CacheKey的Cache值
        /// </summary>
        /// <param name="CacheKey"></param>
        /// <returns></returns>
        public T GetCache<T>(string cacheKey)
        {
            cacheKey = MergeKey(cacheKey);
            string result = GetDatabase().StringGet(cacheKey);
            if (string.IsNullOrWhiteSpace(result))
                return default(T);
            if (typeof(T) == typeof(string))
            {
                return (T)((object)result);
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(result);
            }
        }

        public string GetCacheString(string cacheKey)
        {
            cacheKey = MergeKey(cacheKey);
            string result = GetDatabase().StringGet(cacheKey);
            if (string.IsNullOrWhiteSpace(result))
                return default(string);
            return result;
        }
        public async Task<T> GetCacheAsync<T>(string cacheKey)
        {
            cacheKey = MergeKey(cacheKey);
            string result = await GetDatabase().StringGetAsync(cacheKey);
            if (string.IsNullOrWhiteSpace(result))
                return default(T);
            return JsonConvert.DeserializeObject<T>(result);
        }
        /// <summary>
        /// 设置当前应用程序指定CacheKey的Cache值
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="objObject"></param>
        /// <param name="time"></param>
        public void SetCache<T>(string cacheKey, T objObject, TimeSpan time)
        {
            cacheKey = MergeKey(cacheKey);
            if (typeof(T) == typeof(string))
                GetDatabase().StringSet(cacheKey, objObject.ToString(), time);
            else
            {
                string data = JsonConvert.SerializeObject(objObject);
                GetDatabase().StringSet(cacheKey, data, time);
            }
        }
        /// <summary>
        /// 设置当前应用程序指定CacheKey的Cache值
        /// </summary>
        /// <param name="cacheKey"></param>
        /// <param name="objObject"></param>
        /// <param name="time"></param>
        public void SetCache(string cacheKey, object objObject, TimeSpan time)
        {
            cacheKey = MergeKey(cacheKey);
            if (objObject.GetType() == typeof(string))
                GetDatabase().StringSet(cacheKey, objObject.ToString(), time);
            else
            {
                string data = JsonConvert.SerializeObject(objObject);
                GetDatabase().StringSet(cacheKey, data, time);
            }
        }
        public void SetCacheAsync<T>(string cacheKey, T objObject, TimeSpan time)
        {
            cacheKey = MergeKey(cacheKey);
            string data = JsonConvert.SerializeObject(objObject);
            GetDatabase().StringSetAsync(cacheKey, data, time);
        }
        /// <summary>
        /// 从缓存中移除指定CacheKey的缓存值
        /// </summary>
        /// <param name="cacheKey"></param>
        public bool RemoveCache(string cacheKey)
        {
            cacheKey = MergeKey(cacheKey);
            return GetDatabase().KeyDelete(cacheKey);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static IDatabase GetDatabase()
        {
            return Instance.GetDatabase();
        }

        /// <summary>
        /// 这里的 MergeKey 用来拼接 Key 的前缀，具体不同的业务模块使用不同的前缀。
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string MergeKey(string key)
        {
            return SystemInfo.SystemCode + key;
        }

        /// <summary>
        /// 判断在缓存中是否存在该key的缓存数据
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Exists(string key)
        {
            key = MergeKey(key);
            return GetDatabase().KeyExists(key);  //可直接调用
        }
        /// <summary>
        /// 清空所有缓存
        /// </summary>
        /// <returns></returns>
        public bool ClearCache()
        {
            IEnumerable<RedisKey> keys = Instance.GetServer(Instance.GetEndPoints()[0]).Keys(ConfigurationOption.DefaultDatabase ?? 0);
            foreach (string cacheKey in keys)
            {
                if (cacheKey.IndexOf(SystemInfo.SystemCode) == 0)
                {
                    GetDatabase().KeyDelete(cacheKey);
                }
            }
            return true;
        }

        /// <summary>
        /// 实现递增
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        internal long Increment(string key)
        {
            key = MergeKey(key);
            //三种命令模式
            //Sync,同步模式会直接阻塞调用者，但是显然不会阻塞其他线程。
            //Async,异步模式直接走的是Task模型。
            //Fire - and - Forget,就是发送命令，然后完全不关心最终什么时候完成命令操作。
            //即发即弃：通过配置 CommandFlags 来实现即发即弃功能，在该实例中该方法会立即返回，如果是string则返回null 如果是int则返回0.这个操作将会继续在后台运行，一个典型的用法页面计数器的实现：
            return GetDatabase().StringIncrement(key, flags: CommandFlags.FireAndForget);
        }

        /// <summary>
        /// 实现递减
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        internal long Decrement(string key, string value)
        {
            key = MergeKey(key);
            return GetDatabase().HashDecrement(key, value, flags: CommandFlags.FireAndForget);
        }
        /// <summary>
        /// 配置更改时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConfigurationChanged(object sender, EndPointEventArgs e)
        {
            LogHelper.WriteInfoLog("Configuration changed: " + e.EndPoint);
        }
        /// <summary>
        /// 发生错误时
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerErrorMessage(object sender, RedisErrorEventArgs e)
        {
            LogHelper.WriteInfoLog("ErrorMessage: " + e.Message);
        }
        /// <summary>
        /// 重新建立连接之前的错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionRestored(object sender, ConnectionFailedEventArgs e)
        {
            LogHelper.WriteInfoLog("ConnectionRestored: " + e.EndPoint);
        }
        /// <summary>
        /// 连接失败 ， 如果重新连接成功你将不会收到这个通知
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            LogHelper.WriteInfoLog("重新连接：Endpoint failed: " + e.EndPoint + ", " + e.FailureType + (e.Exception == null ? "" : (", " + e.Exception.Message)));
        }
        /// <summary>
        /// 更改集群
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerHashSlotMoved(object sender, HashSlotMovedEventArgs e)
        {
            LogHelper.WriteInfoLog("HashSlotMoved:NewEndPoint" + e.NewEndPoint + ", OldEndPoint" + e.OldEndPoint);
        }
        /// <summary>
        /// redis类库错误
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void MuxerInternalError(object sender, InternalErrorEventArgs e)
        {
            LogHelper.WriteInfoLog("InternalError:Message" + e.Exception.Message);
        }

    }

    internal class SystemInfo
    {
        internal static readonly string SystemCode = ConfigHelper.GetConfig<string>("RedisSystemCode") == null ? Assembly.GetExecutingAssembly().GetName().Name : ConfigHelper.GetConfig<string>("RedisSystemCode");
    }
}
