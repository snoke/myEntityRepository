using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using myEntityRepository;
using myEntityRepository.DataAccessObject;
namespace myEntityRepositoryExample
{
    class Example
    {
        static void Main(string[] args)
        {
            bool debug = true;

            Type EsmType = Type.GetType("myEntityRepositoryExample.ExampleSubModel");

            List < Type > entityTypes = new List<Type>() { EsmType };
       
            SQLiteStrategy strategy = new SQLiteStrategy("db.sqlite",entityTypes,debug);

            EntityRepository Repo = new EntityRepository(
                entityTypes,
                strategy,
                debug
            );

            Repo.Pull();
            ExampleModel obj = new ExampleSubModel(null, "name1", "name2","abstract1", "abstract2");
            Repo.Save(obj);

            Repo.Flush();
            Console.WriteLine("ExampleSubModel objects in database:");
            foreach (ExampleSubModel esm in Repo.Entities[EsmType])
            {
                Console.WriteLine(esm.id + ":" + esm.Name);
            }
            Console.ReadLine();
        }
    }
}
