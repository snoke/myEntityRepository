/*
 * Created by SharpDevelop.
 * User: Stefan
 * Date: 03.09.2019
 * Time: 19:15
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Reflection;
using myEntityRepository.Model;

namespace myEntityRepository.DataStorage
{
    public class SQLiteStrategy : DataStorage
    {
        #region properties

        private bool _debug;
        private List<Type> _entityTypes;
        private string _filename;
        private SQLiteConnection _dbConnection;
        #endregion

        #region accessors
        private List<Type> entityTypes
        {
            get
            {
                return _entityTypes;
            }
            set
            {
                _entityTypes = value;
            }
        }
        private bool debug
        {
            get
            {
                return _debug;
            }
            set
            {
                _debug = value;
            }
        }
        private string filename
        {
            get
            {
                return _filename;
            }
            set
            {
                _filename = value;
            }
        }
        private SQLiteConnection dbConnection
        {
            get
            {
                //lazy loading
                if (_dbConnection == null)
                {
                    _dbConnection = new SQLiteConnection("Data Source =" + filename + "; Version = 3;");
                    _dbConnection.Open();
                }
                else { }
                return _dbConnection;
            }
        }
        #endregion

        #region constructors
        public SQLiteStrategy(string filename, List<Type> entityTypes, bool debug)
        {
            this.filename = filename;
            this.entityTypes = entityTypes;
            this.debug = debug;
        }
        #endregion

        #region workers
        private List<List<string>> query(string sql)
        {

            if (debug)
            {
                Console.WriteLine(sql);
            }
            else
            {

            }
            SQLiteCommand Command = new SQLiteCommand(sql, dbConnection);
            SQLiteDataReader reader = Command.ExecuteReader();
            List<List<string>> rows = new List<List<string>>();

            while (reader.Read())
            {
                List<string> row = new List<string>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader[i].ToString());
                }
                rows.Add(row);
            }

            reader.Close();
            reader.Dispose();
            Command.Dispose();
            return rows;
        }

        //Todo: umschreiben und SQLiteParameter nutzen um sqlinjection zu verhindern
        public override int SaveEntity(Entity e)
        {
            Type eType = e.GetType();
            string sql = "REPLACE INTO " + eType.Name + "(";
            List<PropertyInfo> properties = eType.GetProperties().Reverse().ToList();
            foreach (PropertyInfo property in properties)
            {
                sql += property.Name.ToLower() + ",";
            }
            sql = sql.Substring(0, sql.Length - 1) + ") VALUES(";
            foreach (PropertyInfo property in properties)
            {
                object v = property.GetValue(e, null);

                if (null == v)
                {
                    sql += "null" + ",";

                }
                else if (v is Entity)
                {
                    sql += "'" + ((Entity)v).id + "',";


                }
                else
                {
                    sql += "'" + v.ToString() + "',";
                }
            }

            sql = sql.Substring(0, sql.Length - 1) + ");";
            query(sql);
            if (e.id == null)
            {
                return Int32.Parse(query("SELECT id FROM " + eType.Name + " WHERE id = (SELECT MAX(id) FROM " + eType.Name + ");").Single().First());
            }
            else
            {
                return (int)e.id;
            }
        }

        public void SaveEntities(Entity[] entities)
        {
            foreach (Entity entity in entities)
            {
                SaveEntity(entity);
            }
        }

        public override int GetNextId(Type entityType)
        {
            List<List<string>> Result = query("SELECT id FROM " + entityType.Name + " WHERE id = (SELECT MAX(id) FROM " + entityType.Name + ");");
            if (Result.Any())
            {
                return Int32.Parse(Result.Single().First()) + 1;

            }
            else
            {
                return 1;
            }
        }
        public override List<List<string>> LoadEntities(Type entityType)
        {
            return query("SELECT * FROM " + entityType.Name + ";");
        }
        public override void RemoveEntity(Entity entity)
        {
            query("DELETE FROM " + entity.GetType().Name + " WHERE id=" + entity.id + ";");
        }
        public override void CreateSchema(Type eType)
        {
            string sql = "CREATE TABLE IF NOT EXISTS " + eType.Name + "(";

            List<PropertyInfo> properties = eType.GetProperties().Reverse().ToList();

            //reflection lädt die erweiternden eigenschaften zuerst und die geerbten eigenschaften (id !!!) zuletzt!
            //die sonstige reihenfolge bleibt dabei bestehen
            //Todo:anderes matching für tiefere abstraktion
            List<PropertyInfo> _properties = new List<PropertyInfo>
            {
                properties[0]
            };
            properties.Reverse();
            _properties.AddRange(properties);
            _properties.RemoveAt(_properties.Count() - 1);

            foreach (PropertyInfo property in _properties)
            {
                if (property.Name.ToLower() == "id")
                {
                    sql += property.Name.ToLower() + " INTEGER PRIMARY KEY AUTOINCREMENT,";
                }
                else
                {
                    string pType = property.PropertyType.ToString();
                    if (pType == "System.String")
                    {
                        sql += property.Name.ToLower() + " TEXT,";

                    }
                    else if (pType == "System.Boolean")
                    {
                        sql += property.Name.ToLower() + " BOOLEAN,";
                    }
                    else if (pType == "System.Int32")
                    {
                        sql += property.Name.ToLower() + " INTEGER,";
                    }
                    else if (entityTypes.Any(o => o.FullName == pType))
                    {
                        sql += property.Name.ToLower() + " INTEGER,";
                    }
                    else
                    {
                        throw new ArgumentException("Unsupported Property " + property.Name + " of type " + pType);
                    }

                }
            }
            sql = sql.Substring(0, sql.Length - 1) + ")";
            query(sql);
        }
        #endregion
    }
}
