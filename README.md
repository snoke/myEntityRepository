# myEntityRepository
lightweight ORM Repository supporting 1:1 aggregations and different data storage strategies (mysql,sqlite,xml)

```c#
using myEntityRepository;
[...]

//our first object to be stored in database
ExampleModel obj = new ExampleModel("Joe Doe");

//the repository works by Types to load class informations
Type objectType = obj.GetType();

//define entity types to be controlled by repository
List<Type> EntityTypes = new List<Type>() { objectType }; 

//define repositories data storage strategy
DataStorage dataStorage = new SQLiteStrategy("db.sqlite", EntityTypes, debug); 

EntityRepository Repo = new EntityRepository(EntityTypes, dataStorage, debug);

//easy access to entities
if (!Repo.Entities[objectType].Any(x=>x.Name==obj.Name) {
  //store new object in repository
  Repo.Save(obj); 
  Repo.Flush(); //flush repository into data storage (write database entries)
} else {
  Message("Do you want to override your previous version?");
  [...]
}
```

check [Example](https://github.com/snoke/myEntityRepository/tree/master/myEntityRepository/Example)
or [BundesligaVerwalter](https://github.com/snoke/BundesligaVerwaltung) for details
