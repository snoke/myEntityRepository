/*
 * Created by SharpDevelop.
 * User: Stefan
 * Date: 02.09.2019
 * Time: 23:47
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

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
        #endregion
    }
}
