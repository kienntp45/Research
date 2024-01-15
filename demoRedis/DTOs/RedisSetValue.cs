using StackExchange.Redis;

namespace demoRedis.DTOs
{
    public class RedisSetValue
    {
        public string Key { get; set; }
        public string Field { get; set; }
        public string Value { get; set; }
        public int DbNumber { get; set; }
    }
}
