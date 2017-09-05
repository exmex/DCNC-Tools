using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TdfToXml.Items;

namespace TdfToXml
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //ItemDatas.Load("Items_US.xml", "UseItems_US.xml");
            //ItemDatas.Load("Items_KR.xml", "UseItems_KR.xml");
            
            /*
            //Test missing items
            var missingItems = new List<Items.ItemTable.Item>();
            var serializer = new XmlSerializer(typeof(Items.ItemTable));

            using (var reader = new StreamReader("Items_KR.xml"))
            {
                var items = (Items.ItemTable) serializer.Deserialize(reader);
                missingItems.AddRange(items.ItemList);
            }
            
            using (var reader = new StreamReader("Items_US.xml"))
            {
                var items = (Items.ItemTable) serializer.Deserialize(reader);
                foreach (var item in items.ItemList)
                {
                    var index = missingItems.FindIndex(itm => itm.Id == item.Id);
                    if(index != -1)
                        missingItems.RemoveAt(index);
                }
            }
            
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            
            var itemClass = new Items.ItemTable();
            itemClass.ItemList = missingItems;

            using (var writer = new StreamWriter("MissingItems.xml"))
            {
                serializer.Serialize(writer, itemClass, ns);
            }
            */
            
            // Combine multiple items
            var missingItems = new List<Items.ItemTable.Item>();
            var serializer = new XmlSerializer(typeof(Items.ItemTable));

            using (var reader = new StreamReader("Items_US.xml"))
            {
                var items = (Items.ItemTable) serializer.Deserialize(reader);
                missingItems.AddRange(items.ItemList);
            }
            
            using (var reader = new StreamReader("Items_KR.xml"))
            {
                var items = (Items.ItemTable) serializer.Deserialize(reader);
                int i = 0;
                foreach (var item in items.ItemList)
                {
                    if(missingItems.FindIndex(itm => itm.Id == item.Id) == -1)
                        missingItems.Insert(i, item);
                    i++;
                }
            }
            
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            
            var itemClass = new Items.ItemTable();
            itemClass.ItemList = missingItems;

            using (var writer = new StreamWriter("MissingItems.xml"))
            {
                serializer.Serialize(writer, itemClass, ns);
            }
            
            /*
            // Use ITem combine
            var missingItems = new List<Items.UseItemTable.UseItem>();
            var serializer = new XmlSerializer(typeof(Items.UseItemTable));

            using (var reader = new StreamReader("UseItems_US.xml"))
            {
                var items = (Items.UseItemTable) serializer.Deserialize(reader);
                missingItems.AddRange(items.UseItemList);
            }
            
            using (var reader = new StreamReader("UseItems_KR.xml"))
            {
                var items = (Items.UseItemTable) serializer.Deserialize(reader);
                int i = 0;
                foreach (var item in items.UseItemList)
                {
                    if(missingItems.FindIndex(itm => itm.Id == item.Id) == -1)
                        missingItems.Insert(i, item);
                    i++;
                }
            }
            
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            
            var itemClass = new Items.UseItemTable();
            itemClass.UseItemList = missingItems;

            using (var writer = new StreamWriter("MissingUseItems.xml"))
            {
                serializer.Serialize(writer, itemClass, ns);
            }*/
            
            return;
            var fileName = args.Length == 0 ? Console.ReadLine() : args[0];

            if (!File.Exists(fileName))
            {
                Console.WriteLine("File does not exist!");
                Console.ReadKey();
                return;
            }

            if (fileName != null)
            {
                if (fileName.Contains("QuestClient.tdf"))
                {
                    var questTable = new QuestTable();
                    questTable.Load(fileName);
                    questTable.Save("Quests.xml");
                }else if (fileName.Contains("UseItemClient.tdf")){   
                    var useItemTable = new UseItemTable();
                    useItemTable.Load(fileName);
                    useItemTable.Save("UseItems.xml");
                }else if (fileName.Contains("ItemClient.tdf")){
                    var itemTable = new ItemTable();
                    itemTable.Load(fileName);
                    itemTable.Save("Items.xml");
                }
            }
        }
    }
}