using StackExchange.Redis;

namespace demoRedis.Reponsitory
{
    public class RedisRepository: IRedisRepository
    {
        readonly IConnectionMultiplexer _redis;
        readonly IDatabase _database;
        readonly ILogger<RedisRepository> _logger;
       
        public RedisRepository(ILogger<RedisRepository> logger, IConnectionMultiplexer redis)
        {
            _logger = logger;
            _redis = redis;
            _database = _redis.GetDatabase();
        }

        public async Task<List<RedisKey>> GetKeyAsync(int dbNumber)
        {
            try
            {
                var server = GetServer(dbNumber);
                return server.Keys().ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return default;
            }
        }

        private IServer GetServer(int dbNumber)
        {
            try
            {
                var endpoint = _redis.GetEndPoints();
                var databaseNumber = dbNumber < 0 ? 0 : dbNumber;
                return _redis.GetServer(endpoint[databaseNumber]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return default;
            }
        }

        public async Task<IEnumerable<RedisValue>> GetValueAsync(RedisKey key, int dbNumber)
        {
            try
            {
                var databaseNumber = dbNumber < 0 ? 0 : dbNumber;
                var data = await _redis.GetDatabase(databaseNumber).HashValuesAsync(key, CommandFlags.PreferReplica);                   
                if (data == null)
                {
                    return default;
                }
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return default;
            }
        }

        public async Task<RedisValue> GetValueAsync(RedisKey key, RedisValue field, int dbNumber)
        {
            try
            {
                var databaseNumber = dbNumber < 0 ? 0 : dbNumber;
                var data = await _redis.GetDatabase(databaseNumber).HashGetAsync(key,field, CommandFlags.PreferReplica);
                if (data.IsNullOrEmpty)
                {
                    return default;
                }
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return default;
            }
        }

        public async Task<bool> SetValueAsync(RedisKey key, RedisValue field, RedisValue value, int dbNumber)
        {
            try
            {
                var databaseNumber = dbNumber < 0 ? 0 : dbNumber;
                await _redis.GetDatabase(databaseNumber).HashSetAsync(key, field, value);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
        }

        public async Task<bool> SetValueAsync(RedisKey key, RedisValue value, int dbNumber)
        {
            try
            {
                var databaseNumber = dbNumber < 0 ? 0 : dbNumber;
                await _redis.GetDatabase(databaseNumber).StringSetAsync(key, value);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
        }

        public async Task<bool> SetValueAsync(RedisKey key, RedisValue value)
        {
            try
            {
                var rs = await _database.StringSetAsync(key, value);
                return rs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
        }

        public async Task<bool> RemoveAsync(RedisKey key, int dbNumber)
        {
            try
            {
                var databaseNumber = dbNumber < 0 ? 0 : dbNumber;
                return await _redis.GetDatabase(databaseNumber).KeyDeleteAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
        }

        public async Task<bool> RemoveAsync(RedisKey key, RedisValue field, int dbNumber)
        {
            try
            {
                var databaseNumber = dbNumber < 0 ? 0 : dbNumber;
                return await _redis.GetDatabase(databaseNumber).HashDeleteAsync(key, field);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return false;
            }
        }
    }
}
