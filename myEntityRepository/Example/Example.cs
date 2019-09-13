using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myEntityRepository;
using myEntityRepository.DataStorage;
namespace myEntityRepositoryExample
{
    public class Example
    {
        public static void Main()
        {
            List<Type> EntityTypes = new List<Type>() { Type.GetType("myEntityRepositoryExample.ExampleModel") };
            EntityRepository Repo = new EntityRepository(EntityTypes, true);
            ExampleModel obj = new ExampleModel("Joe Doe");
            Repo.Save(obj);
            Repo.Flush();
        }
    }
}
