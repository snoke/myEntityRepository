using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using myEntityRepository.Model;

namespace myEntityRepositoryExample
{
    public class ExampleModel:Entity
    {
        private string name;
        private string name2;

        public string Name { get { return name; } set { name = value; } }
        public string Name2 { get { return name2; } set { name2 = value; } }

        public ExampleModel(int? id, string name, string name2):base(id)
        {
            Name = name;
            Name2 = name2;
        }

    }
}
