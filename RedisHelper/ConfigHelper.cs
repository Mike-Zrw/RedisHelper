using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisHelper
{
    public class ConfigHelper
    {
        private static IDictionary<string, object> _config = new Dictionary<string, object>();
        public static T GetConfig<T>(string key)
        {
            return (T)_config[key];
        }
    }
}
