using demoRedis.DTOs;
using demoRedis.Reponsitory;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace demoRedis.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RedisController : ControllerBase
    {
        readonly IRedisRepository _redisRepository;
        readonly ILogger<RedisController> _logger;

        public RedisController(IRedisRepository redisRepository, ILogger<RedisController> logger)
        {
            _redisRepository = redisRepository;
            _logger = logger;
        }

        [HttpGet("GetValueHasField")]
        public async Task<IActionResult> GetValue([FromQuery] GetValueHasFieldDTO getValueHasFieldDTO)
        {
            return Ok(await _redisRepository.GetValueAsync(getValueHasFieldDTO.Key, getValueHasFieldDTO.Field, getValueHasFieldDTO.DbNumber));
        }

        [HttpGet("GetValue")]
        public async Task<IActionResult> GetValue([FromQuery] RedisGetValueDTO redisGetValueDTO)
        {
            try
            {
                var value = await _redisRepository.GetValueAsync(redisGetValueDTO.Key, redisGetValueDTO.DbNumber);
                return Ok(value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [HttpPost("SetValue")]
        public async Task<IActionResult> SetValue([FromBody] RedisSetValue redisSetValue)
        {
            try
            {
                return Ok(await _redisRepository.SetValueAsync(redisSetValue.Key, redisSetValue.Field, redisSetValue.Value, redisSetValue.DbNumber));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [HttpPost("SetValueNoField")]
        public async Task<IActionResult> SetValue([FromBody] RedisSetValueNoFieldDTO redisSetValue)
        {
            try
            {
                return Ok(await _redisRepository.SetValueAsync(redisSetValue.Key, redisSetValue.Value, redisSetValue.DbNumber));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [HttpPost("SetValueNoFieldV2")]
        public async Task<IActionResult> SetValueV2([FromBody] RedisSetValueNoFieldDTO redisSetValue)
        {
            try
            {
                return Ok(await _redisRepository.SetValueAsync(redisSetValue.Key, redisSetValue.Value));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }


        [HttpDelete("DeleteKey")]
        public async Task<IActionResult> DeleteKey([FromBody] DeleteKeyDTOs deleteKey)
        {
            try
            {
                return Ok(await _redisRepository.RemoveAsync(deleteKey.Key, deleteKey.DbNumber));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }

        [HttpDelete("DeleteValue")]
        public async Task<IActionResult> DeleteValue([FromBody] DeleteValueDTO deleteValue)
        {
            try
            {
                return Ok(await _redisRepository.RemoveAsync(deleteValue.Key, deleteValue.Field, deleteValue.DbNumber));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return BadRequest();
            }
        }
    }
}
