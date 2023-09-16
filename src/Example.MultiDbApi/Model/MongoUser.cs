using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System.Text.Json.Serialization;

namespace Example.MultiDbApi.Model
{	
	public class MongoUser
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]		
		public string? Id { get; set; }

		[BsonElement("firstName")]		
		public string FirstName { get; set; }

		[BsonElement("lastName")]
		public string LastName { get; set; }
	}
}
