using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;

namespace OpenLaMulana.Entities
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    public class World : IGameEntity
    {
        public int DrawOrder { get; set; }
        public const int tileWidth = 8;
        public const int tileHeight = 8;
        public int currField = 3;

        private List<Field> _fields;
        private List<Texture2D> _fieldTextures = null;
        private int currRoomX = 0;
        private int currRoomY = 0;

        public World()
        {
            ParseXML("");

            _fields = new List<Field>();
            _fields.Add(new Field("map00", "mapg00", "eveg00"));
            _fields.Add(new Field("map01", "mapg01", "eveg01"));
            _fields.Add(new Field("map02", "mapg02", "eveg02"));
            _fields.Add(new Field("map03", "mapg03", "eveg03"));
            _fields.Add(new Field("map04", "mapg04", "eveg04"));
        }

        private void ParseXML(string v)
        {
            //string text = File.ReadAllText(@"Content/data/script_ENG.dat", Encoding.UTF8);


            // StrinBuilderだと、なぜかUnicode(UTF-16)になってしまう。 
            // MSDN(http://msdn.microsoft.com/ja-jp/library/system.xml.xmlwritersettings.encoding.aspx)によると、
            // XmlWriterSettingsのEncodingよりも、基になるライターのエンコーディングが優先されるようだ。StringWriterも同様らしい。 
            var str = new StringBuilder();
            using (var writer = XmlWriter.Create(str, new XmlWriterSettings() { Encoding = Encoding.GetEncoding("Shift-JIS") }))
            {
                writer.WriteStartElement("Root");
                writer.WriteElementString("Name", "うお座の花");
                writer.WriteFullEndElement();
            }
            Console.WriteLine(str.ToString()); // <?xml version="1.0" encoding="utf-16"?><Root><Name>うお座の花</Name></Root>


            // xmlヘッダーを書き換える方法もあるが、MemoryStreamを使うことでも解消できる。 
            using (var stream = new MemoryStream())
            {
                using (var writer = XmlWriter.Create(stream, new XmlWriterSettings() { Encoding = Encoding.GetEncoding("Shift-JIS") }))
                {
                    writer.WriteStartElement("Root");
                    writer.WriteElementString("Name", "うお座の花");
                    writer.WriteFullEndElement();
                }

                Console.WriteLine(Encoding.GetEncoding("Shift-JIS").GetString(stream.ToArray())); // <?xml version="1.0" encoding="shift_jis"?><Root><Name>うお座の花</Name></Root>
            }



            /*
            using (FileStream fs = File.Open("Content/data/script_ENG.dat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (BufferedStream bs = new BufferedStream(fs))
                {
                    using (StreamReader sr = new StreamReader(bs, Encoding.GetEncoding("Shift-JIS")))
                    {
                        string line;
                        int j = 0;
                        while ((line = sr.ReadLine()) != null)
                        {
                            if (j >= 5)
                                break;

                            char[] decoded = new char[line.Length];

                            for (int i = 0; i < decoded.Length; i++)
                                decoded[i] = (char)(line[i] + 0x1F);

                            String decodedStr = new String(decoded);

                            Console.WriteLine(decodedStr, Encoding.GetEncoding("Shift-JIS"));

                            j++;
                        }
                    }
                }
            }
            */

            /*
            string str =
@"<?xml version=""1.0""?>  
<!-- comment at the root level -->  
<Root>  
    <Child>Content</Child>  
</Root>";
            XDocument doc1 = XDocument.Parse(str, LoadOptions.PreserveWhitespace);
            Console.WriteLine("nodes when preserving whitespace: {0}", doc1.DescendantNodes().Count());
            XDocument doc2 = XDocument.Parse(str, LoadOptions.None);
            Console.WriteLine("nodes when not preserving whitespace: {0}", doc2.DescendantNodes().Count());
            */


            /*
            XmlDocument xmlDoc = new XmlDocument(); // Create an XML document object
            xmlDoc.Load("Content/data/script.txt"); // Load the XML document from the specified file

            // Get elements
            XmlNodeList worldNode = xmlDoc.GetElementsByTagName("WORLD");
            XmlNodeList fieldNode = xmlDoc.GetElementsByTagName("FIELD");

            // Display the results
            //Console.WriteLine("Address: " + worldNode[0].InnerText);
            Console.WriteLine("Field: " + fieldNode[0].InnerText);
            */
        }

        public void SetAreaTexturesList(List<Texture2D> inTexList)
        {
            _fieldTextures = inTexList;
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var _thisField = _fields[currField];
            var _thisTex = _fieldTextures[currField];
            var currFieldRoomData = _thisField.GetMapData();
            var _thisRoom = currFieldRoomData[currRoomX, currRoomY];

            //mapData[roomX, roomY].Tiles[_rtx, _rty] = tileID;


            

            // Loop through every single Room[_x][_y] tile to draw every single tile in a given room
            for (int _y = 0; _y < Field.RoomHeight; _y++)
            {
                for (int _x = 0; _x < Field.RoomWidth; _x++)
                {
                    int _thisTile = _thisRoom.Tiles[_x, _y];

                    //spriteBatch.Draw(_fieldTextures[currField], new Vector2(0, 0), new Rectangle(16, 16, tileWidth, tileHeight), Color.White);
                    var _posx = (_x * 8);
                    var _posy = (_y * 8);
                    var _texX = (_thisTile % 40) * 8;
                    var _texY = (_thisTile / 40) * 8;

                    spriteBatch.Draw(_fieldTextures[currField], new Vector2(_posx, OpenLaMulanaGame.HUD_HEIGHT + _posy), new Rectangle(_texX, _texY, tileWidth, tileHeight), Color.White);
                }
            }
        }

        public void FieldTransitionRight()
        {
            currRoomX++;
            if (currRoomX > Field.FieldWidth - 1)
                currRoomX = 0;
        }
        public void FieldTransitionLeft()
        {
            currRoomX--;
            if (currRoomX < 0)
                currRoomX = Field.FieldWidth - 1;
        }
        public void FieldTransitionUp()
        {
            currRoomY--;
            if (currRoomY < 0)
                currRoomY = Field.FieldHeight - 1;
        }
        public void FieldTransitionDown()
        {
            currRoomY++;
            if (currRoomY > Field.FieldHeight - 1)
                currRoomY = 0;
        }
    }
}