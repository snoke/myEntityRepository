/*
 * Created by SharpDevelop.
 * User: Stefan
 * Date: 02.09.2019
 * Time: 23:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace myEntityRepository.Model
{
    //Diese Klasse beschreibt ein vom Repository speicherbares Objekt
    //Eigenschaften können bei abgeleiteten Klassen nach belieben hinzugefügt werden und werden vom Repository erkannt.
    //Unterstützte Typen hierfür sind bis jetzt: Strings, 32Bit-Integer, Boolean und andere Entity-Objekte (ORM)
    public abstract class Entity
    {
        #region properties
        private int? _id;
        #endregion

        #region accessors

        public int? id
        {
            get { return _id; }
            set { _id = value; }
        }
        #endregion

        #region constructors
        public Entity(int? id)
        {
            this.id = id;
        }
        #endregion

        #region workers
        public Entity Clone()
        {
            return (Entity)MemberwiseClone(); //keine deepcopy!!!1!elf
        }

        public List<PropertyInfo> GetProperties()
        {
            Type eType = this.GetType();
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
            return _properties;
        }
        public List<object> GetValues()
        {
            List<object> values = new List<object>();
            foreach(PropertyInfo property in this.GetProperties())
            {
                values.Add(property.GetValue(this, null));
            }
            return values;
        }
        #endregion
    }
}
