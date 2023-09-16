using Cassandra;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

Cluster cluster = Cluster
				.Builder()
				.WithCredentials("cassandra", "cassandra")
				.WithPort(9042)
				.AddContactPoint("localhost")
				.Build();
builder.Services.AddSingleton<ICluster>(cluster);


var options = ConfigurationOptions.Parse("localhost:6379");
options.User = "user";
options.Password = "pass";
ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
