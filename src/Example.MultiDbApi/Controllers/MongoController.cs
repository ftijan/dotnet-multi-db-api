using Example.MultiDbApi.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace Example.MultiDbApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class MongoController : ControllerBase
	{
		private readonly IMongoCollection<MongoUser> _usersCollection;

		public MongoController(IMongoCollection<MongoUser> usersCollection)
        {
			_usersCollection = usersCollection;
		}

        [HttpPost("mongo-new-user")]
		public async Task<IActionResult> AddNewMongoUser()
		{
			var faker = new Bogus.Faker();

			var newMongoUser = new MongoUser 
			{ 
				FirstName = faker.Name.FirstName(),
				LastName = faker.Name.LastName(),
			};

			await _usersCollection.InsertOneAsync(newMongoUser);

			return Ok(newMongoUser);
		}

		[HttpGet("mongo-users")]
		public async Task<IActionResult> GetMongoUsers()
		{			
			// All users:
			//var users = await _usersCollection
			//	.Find(Builders<MongoUser>.Filter.Empty)
			//	.ToListAsync();

			var builder = Builders<MongoUser>.Projection;
			var projection = builder.Expression(f => new { f.FirstName, f.LastName });

			var users = await _usersCollection.Find(_ => true)
				.Project(projection)
				.ToListAsync();

			var names = users.Select(x => $"{x.FirstName} {x.LastName}");

			return Ok(names);
		}
	}
}
