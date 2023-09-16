# A Multi-database API Example App
This API demos very simple access patterns of using the following data sources:
- Cassandra
- MongoDB
- PostgresQL
- Redis

Uses the following NuGet packages:
- `CassandraCSharpDriver`
- `MongoDB.Driver`
- `Npgsql`
- `StackExchange.Redis`

Also uses `Bogus` to generate some sample data.

See the associated `docker-compose.yml` for a simple (but unsecure) way or running all of the DBs in a single docker container stack.

## Creating Test DBs from DB CLIs:
Cassandra:
```
CREATE KEYSPACE IF NOT EXISTS test WITH replication = {'class': 'SimpleStrategy', 'replication_factor': '1'};
CREATE TABLE IF NOT EXISTS test.user (id UUID PRIMARY KEY, firstname TEXT, lastname TEXT);
```

MongoDB:
```
use Ledger
db.createCollection('Users')
```

PostgresQL:
```
CREATE DATABASE ledger;
\c ledger
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE TABLE IF NOT EXISTS USERS (id UUID DEFAULT uuid_generate_v4 (), first_name VARCHAR NOT NULL, last_name VARCHAR NOT NULL, PRIMARY KEY (id));
```