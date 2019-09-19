using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using myEntityRepository.Model;

namespace myEntityRepositoryExample
{
    public class ExampleSubModel: ExampleModel
    {
        private string abstractName;
        private string abstractName2;
        public string AbstractName { get { return abstractName; } set { abstractName = value; } }
        public string AbstractName2 { get { return abstractName2; } set { abstractName2 = value; } }
        public ExampleSubModel(int? id,string name, string name2, string abstractName, string abstractName2 ) :base(id,name, name2)
        {
            this.AbstractName = abstractName;
            this.AbstractName2 = abstractName2;
        }
    }
}
