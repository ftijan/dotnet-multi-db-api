using Cassandra;
using Example.MultiDbApi.Model;
using MongoDB.Driver;
using Npgsql;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cassandra
Cluster cluster = Cluster
				.Builder()
				.WithCredentials(builder.Configuration.GetValue<string>("Cassandra:User"), builder.Configuration.GetValue<string>("Cassandra:Password"))
				.WithPort(builder.Configuration.GetValue<int>("Cassandra:Port"))
				.AddContactPoint(builder.Configuration.GetValue<string>("Cassandra:Uri"))
				.Build();
builder.Services.AddSingleton<ICluster>(cluster);

// Redis
var options = ConfigurationOptions.Parse(builder.Configuration.GetValue<string>("Redis:Uri"));
options.User = builder.Configuration.GetValue<string>("Redis:User");
options.Password = builder.Configuration.GetValue<string>("Redis:Password");
ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(options);
builder.Services.AddSingleton<IConnectionMultiplexer>(redis);

// MongoDB - assumes the DB and collection exist
var mongoClient = new MongoClient(builder.Configuration.GetValue<string>("MongoDB:Uri"));
var mongoDb = mongoClient.GetDatabase(builder.Configuration.GetValue<string>("MongoDB:DB"));
var usersCollection = mongoDb.GetCollection<MongoUser>(builder.Configuration.GetValue<string>("MongoDB:Collection"));
builder.Services.AddSingleton(usersCollection);

// PostgresQL
var dataSourceBuilder = new NpgsqlDataSourceBuilder(builder.Configuration.GetConnectionString("postgres"));
await using var dataSource = dataSourceBuilder.Build();
builder.Services.AddSingleton(dataSource);

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
