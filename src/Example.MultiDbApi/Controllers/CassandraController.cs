using Cassandra;
using Cassandra.Mapping;
using Example.MultiDbApi.Model;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace Example.MultiDbApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CassandraController : ControllerBase
	{
		private readonly ICluster _cassandraCluster;
		private readonly IConnectionMultiplexer _redisCache;
		private const string CassandraUsersCacheKey = "cassandra-users-cache-key";

		public CassandraController(
			ICluster cassandraCluster,
			IConnectionMultiplexer redisCache)
        {
			_cassandraCluster = cassandraCluster;
			_redisCache = redisCache;
		}

		[HttpPost("cassandra-new-user")]
        public async Task<IActionResult> AddNewCassandraUser()
		{
            Cassandra.ISession session = await _cassandraCluster.ConnectAsync();
			IMapper mapper = new Mapper(session);

			var faker = new Bogus.Faker();			

			var newCassandraUser = new CassandraUser(Guid.NewGuid(), faker.Name.FirstName(), faker.Name.LastName());

			await mapper.InsertAsync(newCassandraUser);

			IDatabase redis = _redisCache.GetDatabase();
			await redis.KeyDeleteAsync(CassandraUsersCacheKey);

			return Ok(newCassandraUser);
		}

		[HttpGet("cassandra-users")]
		public async Task<IActionResult> GetCassandraUsers()
		{
			var retrievedUsers = await GetCassandraReadUsersInternal();

			return Ok(retrievedUsers);
		}
		
		[HttpGet("cassandra-users-cache")]
		public async Task<IActionResult> GetCassandraUsersCached()
		{
			IDatabase redis = _redisCache.GetDatabase();

			var cachedValue = await redis.StringGetAsync(new RedisKey(CassandraUsersCacheKey));

			if (cachedValue.IsNullOrEmpty)
			{
				var usersToCache = await GetCassandraUsersInternal();

				var json = Newtonsoft.Json.JsonConvert.SerializeObject(usersToCache);
				await redis.StringSetAsync(new RedisKey(CassandraUsersCacheKey), new RedisValue(json), TimeSpan.FromSeconds(15), keepTtl: false);

				return Ok(usersToCache);
			}

			var retrievedUsers = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(cachedValue.ToString());

			return Ok(retrievedUsers);
		}

		private async Task<List<string>> GetCassandraUsersInternal()
		{
			Cassandra.ISession session = await _cassandraCluster.ConnectAsync();

			var rowset = session.Execute("SELECT firstname, lastname FROM test.user");

			var retrievedUsers = new List<string>();

			foreach (var row in rowset)
			{
				var firstName = row.GetValue<string>("firstname");
				var lastName = row.GetValue<string>("lastname");

				retrievedUsers.Add($"{firstName} {lastName}");
			}

			return retrievedUsers;
		}

		private async Task<List<string>> GetCassandraReadUsersInternal()
		{
			Cassandra.ISession session = await _cassandraCluster.ConnectAsync();
			IMapper mapper = new Mapper(session);

			var users = await mapper.FetchAsync<CassandraReadUser>();			

			return users.Select(x => x.FullName).ToList();
		}
	}
}
