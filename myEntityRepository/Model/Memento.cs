using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace myEntityRepository.Model
{
    class Memento
    {
        private int id;
        private bool isAlive;
        private Entity entity;

        public Type EntityType { get => Entity.GetType(); }
        public Entity Entity { get => entity; set => entity = value; }
        public bool IsAlive { get => isAlive; set => isAlive = value; }
        public int Id { get => (int)Entity.id; set => id = value; }

        public Memento(Entity entity,bool isAlive)
        {
            Entity = entity;
            IsAlive = isAlive;
        }
    }
}
