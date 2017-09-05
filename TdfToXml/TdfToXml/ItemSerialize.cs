using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using TdfEditor.Utils;

namespace TdfEditor.Items
{
    public class BasicItem
    {
        [XmlAttribute("id")] public string Id;

        [XmlAttribute("category")] public string Category;

        [XmlAttribute("name")] public string Name;

        [XmlAttribute("description")] public string Description;

        [XmlAttribute("function")] public string Function;

        [XmlAttribute("nextstate")] public string NextState;

        [XmlAttribute("buyvalue")] public string BuyValue;

        [XmlAttribute("sellvalue")] public string SellValue;

        [XmlAttribute("expirationtime")] public string ExpirationTime;

        [XmlAttribute("auctionable")] public string Auctionable;

        [XmlAttribute("partsshop")] public string PartsShop;

        [XmlAttribute("sendable")] public string Sendable;
        
        virtual public bool IsStackable()
        {
            return false;
        }

        virtual public uint GetMaxStack()
        {
            return 1;
        }
    }
    
    [Serializable]
    [XmlRoot(ElementName = "UseItems")]
    public class UseItemTable
    {
        public class UseItem : BasicItem
        {
            [XmlAttribute("maxstack")] public string MaxStack;

            [XmlAttribute("stat")] public string StatModifier;

            [XmlAttribute("cooldown")] public string CooldownTime;

            [XmlAttribute("duration")] public string Duration;

            public override bool IsStackable() => Category != "car";

            public override uint GetMaxStack()
            {
                if (MaxStack == "n/a" || MaxStack == "0")
                    return 99;
                return Convert.ToUInt32(MaxStack);
            }
        }
        
        [XmlElement(ElementName = "UseItem")]
        public List<UseItem> UseItemList = new List<UseItem>();
        
        public void Load(string fileName)
        {
            var tdfFile = new TdfFile();
            tdfFile.Load(fileName);

            using (var reader = new BinaryReaderExt(new MemoryStream(tdfFile.ResTable)))
            {
                for (var row = 0; row < tdfFile.Header.Row; row++)
                {
                    //i_n_00026,i_n_00026,partsbox,열려있는 파츠 박스,n/a,n/a,n/a,하이퍼 파츠를 넣을 수 있게 열려있는 파츠 박스.,100,10,n/a,n/a,n/a,0,False,False,False,
                    var item = new UseItem();
                    item.Id = reader.ReadUnicode();//i_n_00019 <- ID
                    item.Category = reader.ReadUnicode(); //accelpointup <-- TYPE
                    item.Name = reader.ReadUnicode(); //가속 향상제 <-- NAME
                    reader.ReadUnicode(); // ig_n_B <-- GROUPID
                    reader.ReadUnicode(); // n/a, i_n_00003 <-- NextID Name
                    reader.ReadUnicode(); // n/a, 10 <-- Roundnum?
                    item.Description = reader.ReadUnicode(); // 가속포인트를 50 초간 30 포인트 올려줌 <-- Description?
                    item.BuyValue = reader.ReadUnicode(); // 100 <-- Cost?
                    item.SellValue = reader.ReadUnicode(); // 10 <-- Sell?
                    item.MaxStack = reader.ReadUnicode(); // 30
                    item.StatModifier = reader.ReadUnicode(); // 50
                    item.CooldownTime = reader.ReadUnicode(); // n/a, 0
                    item.Duration = reader.ReadUnicode(); // 0, /na
                    item.PartsShop = reader.ReadUnicode();
                    item.Sendable= reader.ReadUnicode();
                    item.Auctionable= reader.ReadUnicode();
                    UseItemList.Add(item);
                    
                    /*
                    XiTDFRead::GetString(&v53, &src, 56);
                    UseItemTypeStrToVar(&src, &v50.UseInfo.Type);
                    XiTDFRead::GetString(&v53, v50.Name, 56);
                    XiTDFRead::GetString(&v53, v50.GroupID, 56);
                    XiTDFRead::GetString(&v53, v50.NextID.m_Name, 56);
                    v50.RoundNum = 0;
                    XiTDFRead::GetULong(&v53, &v50.RoundNum);
                    if ( !v50.RoundNum )
                      v50.RoundNum = 99;
                    XiTDFRead::GetULong(&v53, &v50.Cost);
                    XiTDFRead::GetULong(&v53, &v50.Sell);
                    XiTDFRead::GetULong(&v53, &v50.UseInfo.Value);
                    XiTDFRead::GetULong(&v53, &v50.UseInfo.Time);
                    XiTDFRead::GetULong(&v53, &v50.Time);
                    XiTDFRead::GetString(&v53, &src, 56);
                    if ( !wcscmp(&src, L"True") )
                      v50.Shop = 1;
                    XiTDFRead::GetString(&v53, &src, 56);
                    if ( !wcscmp(&src, L"True") )
                      v50.Trade = 1;
                    XiTDFRead::GetString(&v53, &src, 56);
                    if ( !wcscmp(&src, L"True") )
                      v50.Auction = 1;
                    */
                }
            }
        }
        
        public void Save(string fileName)
        {
            var serializer = new XmlSerializer(typeof(UseItemTable));
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            using (var writer = new StreamWriter("UseItems.xml"))
            {
                serializer.Serialize(writer, this, ns);
            }
        }
    }
    
    [Serializable]
    [XmlRoot(ElementName = "Items")]
    public class ItemTable
    {
        public class Item : BasicItem
        {
            [XmlAttribute("setid")] public string SetID;

            [XmlAttribute("setname")] public string SetName;

            [XmlAttribute("grade")] public string Grade;

            [XmlAttribute("requiredLevel")]
            public string RequiredLevel;

            [XmlAttribute("basepoints")] public string BasePoints;

            [XmlAttribute("basepointmodifier")] public string BasePointModifier;

            [XmlAttribute("basepointvariable")] public string BasePointVariable;

            [XmlAttribute("partassist")] public string PartAssist;

            [XmlAttribute("lube")] public string Lube;

            [XmlAttribute("neostats")] public string NeoStats;
        }
        
        

        [XmlElement(ElementName = "Item")]
        public List<Item> ItemList = new List<Item>();
        /*[XmlArray(ElementName = "Item")]
        [XmlArrayItem("BasicItem", typeof(BasicItem))]
        [XmlArrayItem("Item", typeof(Item))]
        [XmlArrayItem("UseItem", typeof(UseItem))]
        public List<BasicItem> ItemList = new List<BasicItem>();*/

        public void Load(string fileName)
        {
            var tdfFile = new TdfFile();
            tdfFile.Load(fileName);

            using (var reader = new BinaryReaderExt(new MemoryStream(tdfFile.ResTable)))
            {
                for (var row = 0; row < tdfFile.Header.Row; row++)
                {
                    var item = new Item();
                    // TODO: If us, this doesn't exist for some reason?!
                    //reader.ReadUnicode(); // Empty
                    item.Category = reader.ReadUnicode(); // Type
                    reader.ReadUnicode(); // Set Type
                    item.Id = reader.ReadUnicode(); // IDname
                    reader.ReadUnicode(); // Group id
                    item.Name = reader.ReadUnicode(); // Name
                    reader.ReadUnicode(); // ???
                    //item.Function
                    item.Grade = reader.ReadUnicode(); // Grade
                    item.RequiredLevel = reader.ReadUnicode(); // Reqlevel
                    reader.ReadUnicode(); //???
                    //item.NextState
                    item.BasePoints = reader.ReadUnicode(); // Value
                    item.BasePointModifier = reader.ReadUnicode(); // Min
                    item.BasePointVariable = reader.ReadUnicode(); // max
                    item.BuyValue = reader.ReadUnicode(); // Cost
                    item.SellValue = reader.ReadUnicode(); // Sell
                    reader.ReadUnicode(); // Next id
                    item.PartsShop = reader.ReadUnicode();
                    item.Sendable= reader.ReadUnicode();
                    item.Auctionable= reader.ReadUnicode();
                    reader.ReadUnicode(); // Set rate
                    item.Description = reader.ReadUnicode(); // Set desc
                    item.PartAssist = reader.ReadUnicode(); // Set assist
                    //
                    item.ExpirationTime = reader.ReadUnicode();
                    ItemList.Add(item);
                }
            }
        }

        public void Save(string fileName)
        {
            var serializer = new XmlSerializer(typeof(ItemTable));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            using (var writer = new StreamWriter("Items.xml"))
            {
                serializer.Serialize(writer, this, ns);
            }
        }
    }
    
    public class ItemDatas
    {
        public List<BasicItem> ItemList = new List<BasicItem>();
        
        public static List<BasicItem> Load(string itemFileName, string useItemFileName)
        {
            var basicItems = new List<BasicItem>();
            
            var serializer = new XmlSerializer(typeof(ItemTable));

            using (var reader = new StreamReader(itemFileName))
            {
                var items = (ItemTable) serializer.Deserialize(reader);
                basicItems.AddRange(items.ItemList);
            }
            
            serializer = new XmlSerializer(typeof(UseItemTable));
            UseItemTable useItems;
            using (var reader = new StreamReader(useItemFileName))
            {
                var items = (UseItemTable) serializer.Deserialize(reader);
                basicItems.AddRange(items.UseItemList);
            }
            return basicItems;
        }
    }
}