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
using System.Linq;
using System.Reflection;
using myEntityRepository.Model;

namespace myEntityRepository.DataAccessObject
{
    public abstract class DataAccessObject
    {
        #region properties
        #endregion

        #region accessors
        #endregion

        #region constructors
        #endregion

        #region workers
        public abstract List<List<string>> LoadEntities(Type entityType);
        public abstract int SaveEntity(Entity entity);
        public abstract int GetNextId(Type entityType);
        public abstract void RemoveEntity(Entity entity);
        public abstract void CreateSchema(Type entityType);
        #endregion
    }
}
