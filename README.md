# SQLServer

[![Build status](https://ci.appveyor.com/api/projects/status/4k7x5yunrvcaajce/branch/master?svg=true)](https://ci.appveyor.com/project/tachyon1337/sqlserver-m38ks/branch/master)


# Usage

```cs
var dbContext = new SQLServerDbContext(connectionString);
var repository = new SQLServerRepository<Continent, int>(dbContext);

var item = new Continent();
item.Code = "NA";
item.ContinentId = 1;
item.CultureId = 7;
item.Name = "North America";

//add
repository.Add(item);

//get all
var all = repository.Get();

//find
var many = repository.Find(c => c.Id != 1);
```
