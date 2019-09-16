/*
 * Created by SharpDevelop.
 * User: Stefan
 * Date: 03.09.2019
 * Time: 19:16
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using myEntityRepository.Model;

namespace myEntityRepository.DataStorage
{
    public class XmlStrategyPrototype:DataStorage

    {
        #region properties
        #endregion

        #region accessors
        #endregion

        #region constructors
        #endregion

        #region workers
        public override int GetNextId(Type entityType)
        {
            return 1;
        }
        public override void CreateSchema(Type entityType)
        {
        }


        private List<string> XElementToList(XElement element)
        {
            List<string> list = new List<string>();
            foreach (XElement parameter in element.Descendants())
            {
                list.Add(parameter.Value);
            }
            return list;
        }


        public override List<List<string>> LoadEntities(Type entityType)
        {
            XDocument document;
            try
            {
                document = XDocument.Load(entityType.Name + ".xml");

            } catch(FileNotFoundException)
            {
                this.CreateSchema(entityType);
                return new List<List<string>>();
            }
            List<List<string>> entities = new List<List<string>>();
            foreach (XElement element in document.Root.Elements(entityType.Name))
            {
                entities.Add(XElementToList(element));
            }
            return entities;
        }

        public override void RemoveEntity(Entity entity)
        {
        }

        private void SaveList(string name,List<PropertyInfo> keys, List<List<string>> entities)
        {
            XElement root = new XElement("root");
            foreach (List<string> entity in entities)
            {
                XElement e = new XElement(name);

                List<string> values = entity;
                for (int i = 0; i < keys.Count(); i++)
                {
                    e.Add(new XElement(keys[i].Name, values[i].ToString()));
                }
                root.Add(e);
            }
            XDocument xdoc = new XDocument();
            xdoc.Add(root);
            xdoc.Save(name + ".xml");
        }
        public override int SaveEntity(Entity entity)
        {
            XElement root = new XElement("root");
            string name = entity.GetType().Name;
            List<List<string>> entities = LoadEntities(entity.GetType());
            foreach(object v in entity.GetValues())
            {
                List<string> list = new List<string>();
                if (null == v)
                {
                    list.Add("null");

                }
                else if (v is Entity)
                {
                    list.Add(Convert.ToString(((Entity)v).id));


                }
                else
                {
                    list.Add(v.ToString());
                }


                entities.Add(list);
            }
            SaveList(entity.GetType().Name,entity.GetProperties(),entities);
            return entities.Count();
        }
        #endregion
    }
}
