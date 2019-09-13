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

        public string Name { get => name; set => name = value; }

        public ExampleModel(string name) : base(null)
        {
            Name = name;
        }
        public ExampleModel(int? id, string name):base(id)
        {
            Name = name;
        }

    }
}
