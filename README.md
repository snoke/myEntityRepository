# myEntityRepository
simple ORM Repository using SQLite Strategy

```c#
using myEntityRepository;
[...]
EntityRepository Repo = new EntityRepository(EntityTypes, true);
ExampleModel obj = new ExampleModel("Joe Doe");

if (!Repo.Entities[obj.GetType()].Any(x=>x.Name==obj.Name) {
  Repo.Save(obj);
  Repo.Flush();
}
```

check [Example](https://github.com/snoke/myEntityRepository/tree/master/myEntityRepository/Example)
or [BundesligaVerwalter](https://github.com/snoke/BundesligaVerwaltung) for details
