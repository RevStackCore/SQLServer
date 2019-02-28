# RevStackCore.SQLServer

[![Build status](https://ci.appveyor.com/api/projects/status/4k7x5yunrvcaajce/branch/master?svg=true)](https://ci.appveyor.com/project/tachyon1337/sqlserver-m38ks/branch/master)


A SQL Server implementation of the RevStackCore repository pattern with POCO First Support

# Nuget Installation

``` bash
Install-Package RevStackCore.SQLServer

```

# Repositories

```cs
public interface IRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
{
    IEnumerable<TEntity> Get();
    TEntity GetById(TKey id);
    IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);
    TEntity Add(TEntity entity);
    TEntity Update(TEntity entity);
    void Delete(TEntity entity);
}
 public interface ISQLRepository<TEntity,TKey> : IRepository<TEntity,TKey> where TEntity:class, IEntity<TKey>
 {
     IEnumerable<TResult> ExecuteProcedure<TResult>(string sp_procedure, object param) where TResult : class;
     IEnumerable<TResult> ExecuteProcedure<TResult>(string sp_procedure) where TResult : class;
     TResult ExecuteProcedureSingle<TResult>(string sp_procedure, object param) where TResult : class;
     TResult ExecuteProcedureSingle<TResult>(string sp_procedure) where TResult : class;
     TValue ExecuteScalar<TValue>(string s_function, object param) where TValue : struct;
     TValue ExecuteScalar<TValue>(string s_function) where TValue : struct;
     TResult ExecuteFunction<TResult>(string s_function, object param) where TResult : class;
     TResult ExecuteFunction<TResult>(string s_function) where TResult : class;
     IEnumerable<TResult> ExecuteFunctionWithResults<TResult>(string s_function) where TResult : class;
     IEnumerable<TResult> ExecuteFunctionWithResults<TResult>(string s_function, object param) where TResult : class;
     DynamicParameters Execute(string sp_procedure, DynamicParameters param);
     void Execute(string sp_procedure, object param);
     void Execute(string sp_procedure);
     IDbConnection Db { get; }
 }
 public class SQLServerRepository<TEntity, TKey> : ISQLRepository<TEntity, TKey> where TEntity : class, IEntity<TKey>
```

# Implementations
SQLServerRepository<TEntity,Tkey> implements IRepository<TEntity,TKey> for basic Crud operations and Find
SQLServerRepository<TEntity,Tkey> implements ISQLRepository<TEntity,TKey> for Crud + SQL query operations

## Notes on Implementations
RevStackCore.SQLServer is an extension of the RevStackCore Repository pattern wrapper around Stack Exchange Dapper. In addition, RevStackCore.SQLServer offers POCO First capability via a RegisterTable extension method.


# POCO First
POCO First, or Code First, means we create and define our SQL Server tables from C# classes. Consistent with the repository pattern, all POCO items must implement an Id prop(TKey Id). By default, Id will be the Table primary key. 


For example:
```cs

public class Person : IEntity<string>
{
    [PrimaryKey(explicitKey:true)]
    public string Id { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    [Index]
    public string Email { get; set; }

    public Person()
    {
        Id = Guid.NewGuid().ToString();
    }
}
```

## Data Annotations
If POCO First capability is not needed, there is no requirement to annotate your POCO class properties, other than marking Id as an [ExplicitKey] if the value is being explicitly set(i.e, Id is not autogenerated). However, POCO First functionality can be fine tuned by using data annotations.

```cs
using RevStackCore.DataAnnotations

//value of Id prop is supplied
[ExplicityKey]
public TKey Id {get; set;}

//primary key, value is supplied
[PrimaryKey(explicitKey:true)]
public TValue Property {get; set;}

//NOTE: if Property=Id, [ExplicityKey] & PrimaryKey(explicitKey:true)] are equivalent

//Create an Index on Property
[Index]
public TValue Property {get; set;}

//Create a Clustered, Unique Index on Property
[Index(Clustered =true,Unique =true)]
public TValue Property {get; set;}

//Autoincremented property, database generated
[AutoIncrement]
public int Property {get; set;}

//Autoincremented primary key, database generated
[PrimaryKey]
[AutoIncrement]
public int Property {get; set;}

//ignore and set writeable to false for a CLR Type
[Ignore]
[Write(false)]
public Address Address { get; set; }

//use another table name instead of the name of the class
[Table ("Person")]
public class Persons
{
    public int Id { get; set; }
    public string Name { get; set; }
}

//override default CLR to Sql Type mapping; e.g, by default, string maps to varchar(100)
[SqlType(SqlDbType.VarChar, 50)]
public string Name {get; set;}

[SqlType(SqlDbType.Text)]
public string Bio {get; set;}
  
```


## Register Table
Register your tables in the Startup.cs or Program.cs
```cs
private static void RegisterTables(SQLServerDbContext db)
{
    db.RegisterTable<Person,string>();
      .RegisterTable<Tag,int>();
    
}
```
RegisterTable is not a database migrations implementation tool. It only operates on the specified table by executing a metadata schema comparison between an existing DB Table and the corresponding POCO type. At a minimum, the underlying Database must exist. If the table does not exist in the database, RegisterTable will CREATE the table,as well as the annotated primary keys and indexes.  If the Db Table exists, RegisterTable will only run a ALTER Table schema change on POCO properties that DO NOT have a corresponding column match in the Db Table. By design, RegisterTable will neither modify nor delete columns in the existing Db table schema. Nor will it remove existing indexes or keys. In short, if you add a property to your POCO, RegisterTable will add it to the Db Table schema. But if you modify or a delete a POCO property, RegisterTable will not correspondingly change the Db Table schema. You will have to manually update the schema in the database. Additionally, RegisterTable will not change the primary key constraints for an existing table schema. Lastly, Foreign keys are not supported at all and will have to be set manually.

## Enforce Class Name to Table Name Mapping
Unfortunately, table pluralization on inserts/updates is a default behavior of Dapper.Contrib that has not of yet been removed from the RevStackCore implementation. Override this with one line at startup.

```cs
using RevStackCore.SQL.Client

 SQLExtensions.SetTableNameMapper();
 ```


# CRUD

```cs
//add
_repository.Add(item);

//update
_repository.Update(item);

//delete
_repository.Delete(item)

//Get All
_repository.Get()

//Get Item By Id, if TKey Id==hash key
_repository.GetById(id)

```

# Queries and Complex Mappings
IRepository.Find(Expression<Func<TEntity, bool>> predicate) should be able to handle any query scenario on a single table. However, SQL Server is not a NoSql database and most applications that rely on a rdbms backend usually make use of joining tables to fetch data. There are two stratgies to handle complex mappings. One is to write your queries in stored procedures/function and map the result to a presentation model. The second is write the queries as an inline string that will be mapped to a presentation model. ISQLRepository extends IRepository with a number of additional methods that should be able to handle #1 and also exposes the IDbConnection object that will allow you to write inline queries just as you would with Dapper.


## Linq Support
Not all linq extension methods are currently supported. Apply a ToList() in such an event.

```cs
 var result = _repository.Find(x => x.City == "Atlanta");
 //int count=result.Count() --> not implemented exception
 int count=result.ToList().Count();
 ```


# Usage

## SQLServerDbContext
Inject an instance of SQLServerDbContext with the sql server connection string passed in the constructor.
```cs
var dbContext = new SQLServerDbContext(connectionString);
```

## Dependency Injection

```cs
using RevStackCore.Pattern; //IRepository interface
using RevStackCore.Pattern.SQL; //ISQLRepository interface
using RevStackCore.SQL.Client;
using RevStackCore.SQLServer; 

class Program
{
    static void main(string[] args)
    {
        var serviceCollection = new ServiceCollection();
        ConfigureServices(serviceCollection);
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var db = serviceProvider.GetService<SQLServerDbContext>();
        var dataService = serviceProvider.GetService<IDataService>();
        db.RegisterTable<Person, string>()
          .RegisterTable<Tag, int>()
          .RegisterTable<Contact, int>();

        dataService.MyMethod();
    }
    private static void ConfigureServices(IServiceCollection services)
    {
        string connectionString = "";
        services
            .AddSingleton(x => new SQLServerDbContext(connectionString))
            .AddSingleton<IRepository<Person, string>, SQLServerRepository<Person, string>>()
            .AddSingleton<IRepository<Tag, int>, SQLServerRepository<Tag, int>>()
            .AddSingleton<ISQLRepository<Contact, int>, SQLServerRepository<Contact, int>>()
            .AddSingleton<IDataService, DataService>();

        services.AddLogging();
        //enforce class name to table name for databse reads/writes
        SQLExtensions.SetTableNameMapper();
    }
}

```

# AspNetCore Identity framework
SQLServerRepository can be plugged into the RevStackCore generic implementation of the AspNetCore Identity framework
https://github.com/RevStackCore/Identity

# Asynchronous Services
```cs

IDbConnection Db { get; }
Task<IEnumerable<TEntity>> GetAsync();
Task<TEntity> GetByIdAsync(TKey id);
Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
Task<TEntity> AddAsync(TEntity entity);
Task<TEntity> UpdateAsync(TEntity entity);
Task DeleteAsync(TEntity entity);
Task<IEnumerable<TResult>> ExecuteProcedureAsync<TResult>(string sp_procedure, object param) where TResult : class;
Task<IEnumerable<TResult>> ExecuteProcedureAsync<TResult>(string sp_procedure) where TResult : class;
Task<TResult> ExecuteProcedureSingleAsync<TResult>(string sp_procedure, object param) where TResult : class;
Task<TResult> ExecuteProcedureSingleAsync<TResult>(string sp_procedure) where TResult : class;
Task<TValue> ExecuteScalarAsync<TValue>(string s_function, object param) where TValue : struct;
Task<TValue> ExecuteScalarAsync<TValue>(string s_function) where TValue : struct;
Task<TResult> ExecuteFunctionAsync<TResult>(string s_function, object param) where TResult : class;
Task<TResult> ExecuteFunctionAsync<TResult>(string s_function) where TResult : class;
Task<DynamicParameters> ExecuteAsync(string sp_procedure, DynamicParameters param);
Task<IEnumerable<TResult>> ExecuteFunctionWithResultsAsync<TResult>(string s_function) where TResult : class;
Task<IEnumerable<TResult>> ExecuteFunctionWithResultsAsync<TResult>(string s_function, object param) where TResult : class;
Task ExecuteAsync(string sp_procedure, object param);
Task ExecuteAsync(string sp_procedure);

```

# Implementations
SQLService<TEntity,Tkey> implements IService<TEntity,TKey> for basic Async Crud operations and FindAsync
SQLService<TEntity,Tkey> implements ISQLService<TEntity,TKey> for Async Crud + SQL query operations







