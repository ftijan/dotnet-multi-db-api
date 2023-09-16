using Example.MultiDbApi.Model;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Example.MultiDbApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PostgresController : ControllerBase
	{
		private readonly NpgsqlDataSource _npgsqlDataSource;

		public PostgresController(NpgsqlDataSource npgsqlDataSource)
        {
			_npgsqlDataSource = npgsqlDataSource;
		}

        [HttpPost("postgres-new-user")]
		public async Task<IActionResult> AddNewPostgresUser()
		{
			var faker = new Bogus.Faker();
			var newUser = new PostgresUser
			{
				FirstName = faker.Name.FirstName(),
				LastName = faker.Name.LastName(),
			};

			await using var connection = _npgsqlDataSource.CreateConnection();
			await connection.OpenAsync();
			
			try
			{
				await using var cmd = new NpgsqlCommand("INSERT INTO users (first_name, last_name) VALUES ($1, $2) RETURNING id;", connection);
				cmd.Parameters.Add(new() { Value = newUser.FirstName });
				cmd.Parameters.Add(new() { Value = newUser.LastName });
				
				var id = await cmd.ExecuteScalarAsync();

				// TODO: error handling
				var userGuid = new Guid(id.ToString());

				newUser.Id = userGuid;
			}
			finally
			{
				await connection.CloseAsync();
			}

			return Ok(newUser);
		}

		[HttpGet("postgres-users")]
		public async Task<IActionResult> GetPostgresUsers()
		{	
			var users = new List<string>();
			await using var connection = _npgsqlDataSource.CreateConnection();
			await connection.OpenAsync();

			try
			{
				await using var cmd = new NpgsqlCommand("SELECT first_name, last_name FROM users;", connection);

				await using var reader = await cmd.ExecuteReaderAsync();

				while (await reader.ReadAsync())
				{
					var firstName = reader.GetString(0);
					var lastName = reader.GetString(1);

					users.Add($"{firstName} {lastName}");
				}
			}
			finally
			{
				await connection.CloseAsync();				
			}

			return Ok(users);
		}
	}
}
