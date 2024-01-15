using StackExchange.Redis;

namespace demoRedis
{
    public class RedisSetting
    {
        public string Endpoints { get; set; }
        public string ServiceName { get; set; }
        public int DefaultDatabase { get; set; }
        private EndPointCollection GetRedisEndpoints(string endpointsStr)
        {
            endpointsStr = endpointsStr.Replace(" ", "");
            var endpointArr = endpointsStr.Split(',');
            var endpoints = new EndPointCollection();
            foreach (var item in endpointArr)
            {
                var ipPort = item.Split(':');
                var ip = ipPort[0];
                var port = 6379;
                if (ipPort.Length > 1) port = int.Parse(ipPort[1]);
                endpoints.Add(ip, port);
            }
            return endpoints;
        }

        public static RedisSetting MapValue(IConfiguration configuration)
        {
            return configuration.Get<RedisSetting>();
        }

        public ConfigurationOptions GetRedisOptions()
        {
            return new ConfigurationOptions
            {
                //ServiceName = ServiceName,
                DefaultDatabase = DefaultDatabase,
                EndPoints = GetRedisEndpoints(Endpoints)
            };
        }
    }
}
