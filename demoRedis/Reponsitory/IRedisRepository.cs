using StackExchange.Redis;

namespace demoRedis.Reponsitory
{
    public interface IRedisRepository
    {
        public Task<List<RedisKey>> GetKeyAsync(int dbNumber);
        public Task<IEnumerable<RedisValue>> GetValueAsync(RedisKey key, int dbNumber);
        public Task<RedisValue> GetValueAsync(RedisKey key, RedisValue field, int dbNumber);
        public Task<bool> SetValueAsync(RedisKey key, RedisValue field, RedisValue value, int dbNumber);
        public Task<bool> SetValueAsync(RedisKey key, RedisValue value, int dbNumber);
        public Task<bool> RemoveAsync(RedisKey key, int dbNumber);
        public Task<bool> RemoveAsync(RedisKey key, RedisValue field, int dbNumber);
        Task<bool> SetValueAsync(RedisKey key, RedisValue value);

    }
}
