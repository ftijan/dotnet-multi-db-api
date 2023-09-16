using Cassandra.Mapping.Attributes;

namespace Example.MultiDbApi.Model;

// Assumes below keyspace and table have been already created:
[Table("user", Keyspace = "test")]
public class CassandraUser
{
	public CassandraUser(Guid id, string firstName, string lastName)
	{
		Id = id;
		FirstName = firstName;
		LastName = lastName;
	}

	[Column("id")]
    public Guid Id { get; set; }		

	[Column("firstname", Type = typeof(string))]
	public string FirstName { get; set; }

	[Column("lastname", Type = typeof(string))]
	public string LastName { get; set; }
}