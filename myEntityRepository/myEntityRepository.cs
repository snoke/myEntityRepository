/*
 * Created by SharpDevelop.
 * User: Stefan
 * Date: 31.08.2019
 * Time: 18:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using myEntityRepository.DataStorage;
using myEntityRepository.Model;

namespace myEntityRepository
{
    public class EntityRepository
    {
        #region properties
        private DataStorage.DataStorage _dataStorage;
        private List<Type> _types;
        private Dictionary<Type, int> _nextId;
        private Dictionary<Type, List<Entity>> _entities;

        //Der Value eines jeden Mementos enthält den "alive"-Zustand und bestimmt ob diese objekt gespeichert oder gelöscht wird
        private List<Dictionary<Entity, bool>> _mementos;
        private List<Memento> mementos;

        #endregion

        #region accessors
        private List<Memento> _Mementos //Logs von Entityzuständen
        {
            get { return mementos; }
            set { mementos = value; }
        }
        private List<Dictionary<Entity, bool>> Mementos //Logs von Entityzuständen
        {
            get { return _mementos; }
            set { _mementos = value; }
        }
        private List<Type> Types
        {
            get { return _types; }
            set { _types = value; }
        }
        private Dictionary<Type, int> NextId
        {
            get { return _nextId; }
            set { _nextId = value; }
        }
        public Dictionary<Type, List<Entity>> Entities
        {
            get
            {
                return _entities;
            }
            set { _entities = value; }
        }

        private DataStorage.DataStorage dataStorage
        {
            get { return _dataStorage; }
            set { _dataStorage = value; }
        }
        #endregion


        #region constructors
        public EntityRepository(List<Type> types, bool debug)
        {
            dataStorage = new SQLiteStrategy("db.sqlite", types, debug);
            Types = types;
            Entities = new Dictionary<Type, List<Entity>>();
            NextId = new Dictionary<Type, int>();
            _Mementos = new List<Memento>();
            Mementos = new List<Dictionary<Entity, bool>>();
            foreach (Type type in Types)
            {
                CreateSchema(type);
                NextId[type] = dataStorage.GetNextId(type);
                //NextId[entry.Value] = 1;
                Entities[type] = new List<Entity>();
            }
            Pull();
        }
        #endregion

        #region workers

        public bool IsDirty() //Ungespeicherte Änderungen?
        {
            return (_Mementos.Count() > 0);
        }
        private int GetNextId(Type eType) //Simuliere nächste Inkrement ID?
        {
            int id = NextId[eType];
            NextId[eType]++;
            return id;
        }
        public Dictionary<Type, List<Entity>> Pull()//Daten aus Datenspeicherungsschicht aktualisieren
        {
            foreach (Type type in Types)
            {
                Entities[type] = new List<Entity>();
                Load(type);
            }
            return Entities;
            /*
            string listname = nameof(ICollection);
            foreach (KeyValuePair<Type, List<Entity>> entry in Entities)
            {
                foreach (Entity entity in entry.Value)
                {
                    BindingFlags universalBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                    foreach (FieldInfo fieldinfo in entry.Key.GetFields(universalBindingFlags))
                    {
                        Type  fieldType= fieldinfo.FieldType;
                        if (fieldType.GetInterface(listname) != null)
                        {
                            List<Entity> list = new List<Entity>();
                            foreach(int children in (List<int>)fieldinfo.GetValue(entity))
                            {
                                Console.WriteLine();
                                //  list.Add();
                                Console.ReadLine();
                            }
                        }
                    }
                }
            }
            */
        }
        public void Flush() //Speichert den letzten Zustand aller geänderten Objekte in die Datenbank und leert die Mementos
        {
            Dictionary<Type, List<int>> handled = new Dictionary<Type, List<int>>();

            _Mementos.Reverse();
            foreach (Memento memento in _Mementos)
            {
                Entity entity = memento.Entity;
                bool alive = memento.IsAlive;

                    if (handled.Any(x => x.Key == entity.GetType()))
                    {

                    }
                    else
                    {
                        handled[entity.GetType()] = new List<int>();
                    }


                    if (entity.id != null && handled[entity.GetType()].Contains((int)entity.id))
                    {
                        continue;
                    }
                    else if (alive == false)
                    {
                        if (entity.id==null)
                        {
                            continue;
                        } else
                        {
                            handled[entity.GetType()].Add((int)entity.id);
                            dataStorage.RemoveEntity(entity);
                           Entities[entity.GetType()].RemoveAll(x => x.id == entity.id);
                        }
                    }
                    else
                    {
                        entity.id = dataStorage.SaveEntity(entity);
                        handled[entity.GetType()].Add((int)entity.id);
                    }
                }
            _Mementos = new List<Memento>();
        }

        private Entity CreateInstance(Type entityType, List<object> parameters)
        {
            Entity entity = (Entity)Activator.CreateInstance(entityType, parameters.ToArray());
            return SetEntity(entity);
        }
        private void Load(Type entityType)
        {
            List<List<string>> rows = dataStorage.LoadEntities(entityType);
            List<PropertyInfo> properties = entityType.GetProperties().Reverse().ToList();
            //reflection lädt die erweiternden eigenschaften zuerst und die geerbten eigenschaften (id !!!) zuletzt!
            //die sonstige reihenfolge bleibt dabei bestehen
            //TODO:anderes matching für tiefere abstraktion
            List<PropertyInfo> _properties = new List<PropertyInfo>
            {
                properties[0]
            };
            properties.Reverse();
            _properties.AddRange(properties);
            _properties.RemoveAt(_properties.Count() - 1);

            foreach (List<string> row in rows)
            {
                List<object> values = new List<object>();
                for (int i = 0; i < row.Count(); i++)
                {
                    PropertyInfo property = _properties[i];

                    string typeName = property.PropertyType.ToString();
                    if (property.Name == "id")
                    {
                        values.Add((int?)Int32.Parse(row[i]));
                    }
                    else if (typeName == "System.String")
                    {
                        values.Add(row[i]);
                    }
                    else if (typeName == "System.Int32")
                    {
                        values.Add(Int32.Parse(row[i]));
                    }
                    else if (typeName == "System.Boolean")
                    {
                        values.Add(Boolean.Parse(row[i]));
                    }
                    else if (Entities.Any(o => o.Key.FullName == typeName))
                    {
                        string val = row[i];
                        if (val == "") //null objects
                        {
                            values.Add(null);
                        }
                        else
                        {
                            int mapId = Int32.Parse(val);
                            Type type = this.Types.Single(x => x.FullName == typeName);
                            List<Entity> entities = Entities[type];
                            values.Add(entities.Single(x => x.id == mapId));
                        }
                    }
                    else
                    {
                        throw new ArgumentException("(ORM) Mapping failed of type " + typeName);
                    }
                }
                CreateInstance(entityType, values);
            }
        }

        private Entity SetEntity(Entity entity)
        {

            if (entity.id != null && Entities[entity.GetType()].Any(x => x.id == entity.id))
            {
                int index = Entities[entity.GetType()].FindIndex(x => x.id == entity.id);
                Entities[entity.GetType()][index] = entity;
                return Entities[entity.GetType()][index];
            }
            else
            {
                if (entity.id == null)
                {
                    entity.id = GetNextId(entity.GetType());
                }
                else
                {

                }
                Entities[entity.GetType()].Add(entity);
                return Entities[entity.GetType()].Last();
            }
        }
        public Entity Save(Entity entity)
        {
            entity = SetEntity(entity);
            _Mementos.Add(new Memento(entity.Clone(), true));
            return entity;
        }
        public void Remove(Entity entity)
        {
           // dataStorage.RemoveEntity(entity);
            Entities[entity.GetType()].Remove(entity);
            _Mementos.Add(new Memento(entity.Clone(), false));
            entity = null;
        }
        public void CreateSchema(Type entityType)
        {
            dataStorage.CreateSchema(entityType);
        }
        #endregion
    }
}
