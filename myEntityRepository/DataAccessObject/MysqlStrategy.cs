using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using myEntityRepository.Model;
using MySql.Data;
using MySql.Data.MySqlClient;

namespace myEntityRepository.DataAccessObject
{
    class MysqlStrategy : DataAccessObject
    {
        #region properties
        private MySqlConnection connection;
        private List<Type> entityTypes;
        private bool debug;
        private  string server;
        private  string database;
        private  string username;
        private  string password;
        #endregion

        #region accessors
        public MySqlConnection Connection
        {
            get
            {
                if (connection == null)
                {
                    connection = new MySqlConnection("Server="+this.Server+ "; database=" + this.Database + "; UID=" + this.Username + "; password=" + this.Password);
                    connection.Open();
                }
                return connection;

            }
            set { }
        }
        public List<Type> EntityTypes { get { return entityTypes; } set { entityTypes = value; } }
        public bool Debug { get { return debug; } set { debug = value; } }
        public string Server { get { return server; } set { server = value; } }
        public string Database { get { return database; } set { database = value; } }
        public string Username { get { return username; } set { username = value; } }
        public string Password { get { return password; } set { password = value; } }
        #endregion

        #region constructors
        public MysqlStrategy(string server,string database,string username,string password,List<Type> entityTypes, bool debug)
        {
            this.Server = server;
            this.Database = database;
            this.Username = username;
            this.Password = password;
            this.EntityTypes = entityTypes;
            this.Debug = debug;
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
            MySqlCommand Command = new MySqlCommand(sql, Connection);
            MySqlDataReader reader = Command.ExecuteReader();
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
            List<PropertyInfo> properties = eType.GetProperties().ToList().GroupBy(p => p.DeclaringType)
                .Reverse()
                .SelectMany(g => g)
                .ToList();
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
                return GetNextId(e.GetType());
               //return Int32.Parse(query("SELECT id FROM " + eType.Name + " WHERE id = (SELECT MAX(id) FROM " + eType.Name + ");").Single().First());
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
            List<List<string>> Result = query("SELECT MAX(id) FROM `" + entityType.Name + "`;");
            if (Result.Any())
            {
                int i = 0;
                Int32.TryParse(Result.Single().First(),out i);
                return i + 1;

            }
            else
            {
                return 1;
            }
        }
        public override List<List<string>> LoadEntities(Type entityType)
        {
            return query("SELECT * FROM `" + entityType.Name + "`;");
        }
        public override void RemoveEntity(Entity entity)
        {
            query("DELETE FROM `" + entity.GetType().Name + "` WHERE id=" + entity.id + ";");
        }
        public override void CreateSchema(Type eType)
        {
            string sql = "CREATE TABLE IF NOT EXISTS `" + eType.Name + "`(";


            //reflection lädt die erweiternden eigenschaften zuerst und die geerbten eigenschaften (id !!!) zuletzt!
            //die sonstige reihenfolge bleibt dabei bestehen
            //Todo:anderes matching für tiefere abstraktion

            List<PropertyInfo> _properties = eType.GetProperties().ToList().GroupBy(p => p.DeclaringType)
                .Reverse()
                .SelectMany(g => g)
                .ToList();

            foreach (PropertyInfo property in _properties)
            {
                if (property.Name.ToLower() == "id")
                {

                    sql += property.Name.ToLower() + " INT PRIMARY KEY AUTO_INCREMENT,";
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
            sql = sql.Substring(0, sql.Length - 1) + ") ENGINE = InnoDB";
            query(sql);
        }
        #endregion
    }
}
