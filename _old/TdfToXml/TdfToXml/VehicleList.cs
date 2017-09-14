using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace TdfToXml
{
    [Serializable]
    [XmlRoot(ElementName = "Vehicles")]
    public class VehicleList
    {
        public class VehicleUpgrade
        {
            // Vehicle upgrade
            [XmlAttribute("coupon")] public string Coupon;
            [XmlAttribute("accel")] public string Acceleration;
            [XmlAttribute("speed")] public string Speed;
            [XmlAttribute("crash")] public string Crash;
            [XmlAttribute("boost")] public string Boost;
            [XmlAttribute("price")] public string Price;

            [XmlAttribute("sell")] public string Sell;
            [XmlAttribute("closeSell")] public string CloseSell;
            [XmlAttribute("upgradeMito")] public string UpgradeMito;
            
            /// <summary>
            /// Fuel Efficiency
            /// </summary>
            [XmlAttribute("efficiency")] public string Efficiency;
            
            /// <summary>
            /// Fuel Capacity
            /// </summary>
            [XmlAttribute("capacity")] public string Capacity;

            [XmlAttribute("reqLevel")] public string RequiredLevel;         
        }
        
        public class Vehicle
        {
            [XmlAttribute("name")] public string Name;
            
            // PlayerCar, HUV, Traffic, RacingBattle
            /// <summary>
            /// The type of the vehicle
            /// Possible values:
            /// 0 = Player Car
            /// 1 = HUV
            /// 2 = Traffic
            /// 3 = Racing Battle
            /// </summary>
            [XmlAttribute("type")] public string Type;
            
            // Not sure if we need the string :/.
            [XmlAttribute("typeS")] public string TypeStr;
            
            [XmlAttribute("id")] public string UniqueId;

            /// <summary>
            /// 0 or 1 wether this car can be sold.
            /// </summary>
            [XmlAttribute("sellable")] public string Sellable;

            /// <summary>
            /// Grade typ
            /// </summary>
            [XmlAttribute("grade")] public string Grade;

            /// <summary>
            /// Car Base Acceleration
            /// </summary>
            [XmlAttribute("accel")] public string Acceleration;

            /// <summary>
            /// Car Base Speed
            /// </summary>
            [XmlAttribute("speed")] public string Speed;
            
            /// <summary>
            /// Car Base Crash
            /// </summary>
            [XmlAttribute("crash")] public string Crash;
            
            /// <summary>
            /// Car Base Boost
            /// </summary>
            [XmlAttribute("boost")] public string Boost;

            [XmlAttribute("reqLevel")] public string RequiredLevel;
            [XmlAttribute("level")] public string Level;


            [XmlElement(ElementName = "Upgrade")]
            public List<VehicleUpgrade> Upgrades;
        }
        
        [XmlElement(ElementName = "Vehicle")]
        public List<Vehicle> ItemList = new List<Vehicle>();

        public void Save(string fileName)
        {
            var serializer = new XmlSerializer(typeof(VehicleList));
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");

            using (var writer = new StreamWriter(fileName))
            {
                serializer.Serialize(writer, this, ns);
            }
        }
    }
    
    public class VehicleListLoader
    {
        private static IEnumerable<string[]> LoadCsv(string fileName, int skipLines = 0)
        {
            var lines = new List<string[]>();
            using (TextReader reader = File.OpenText(fileName))
            {
                // 4 Lines header.
                string line;
                int i = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    i++;
                    if (i <= skipLines)
                        continue;
                    
                    if (line.StartsWith(";"))
                        continue;
                    
                    var data = line.Split(',');

                    lines.Add(data);
                }
            }
            return lines;
        }
        
        public static void Load(string vehicleListFileName, string vehicleUpgradeFileName)
        {
            if (!File.Exists(vehicleListFileName))
                throw new FileNotFoundException();
            if (!File.Exists(vehicleUpgradeFileName))
                throw new FileNotFoundException();

            var vehicleLines = LoadCsv(vehicleListFileName, 4);
            var vehicleUpgradeLines = LoadCsv(vehicleUpgradeFileName, 3);
            
            var vehicleUpgrades = new Dictionary<string, List<string[]>>();
            foreach (var line in vehicleUpgradeLines)
            {
                if (!vehicleUpgrades.ContainsKey(line[1]))
                {
                    vehicleUpgrades.Add(line[1], new List<string[]>());
                }

                vehicleUpgrades[line[1]].Add(line);
            }

            var vehList = new VehicleList
            {
                ItemList = new List<VehicleList.Vehicle>()
            };

            //9 should be Unique ID.
            //var sortedLines = vehicleLines.OrderBy(strings => strings[9]);
            foreach (var line in vehicleLines)
            {
                var veh = new VehicleList.Vehicle();
                veh.Type = line[2]; // Vehicle
                veh.TypeStr = line[3]; // Desc
                veh.Name = line[5];
                veh.UniqueId = line[9]; // Should be sorted but whatever.
                veh.Sellable = line[10];
                veh.Grade = line[15];
                veh.Acceleration = line[16];
                veh.Speed = line[17];
                veh.Crash = line[18];
                veh.Boost = line[19];
                veh.RequiredLevel = line[20];
                veh.Level = line[21];
                veh.Upgrades = new List<VehicleList.VehicleUpgrade>();
                if (!vehicleUpgrades.ContainsKey(veh.UniqueId))
                    Console.WriteLine($"Skipped upgrades for {veh.UniqueId}");
                else
                {
                    foreach (var upgrade in vehicleUpgrades[veh.UniqueId])
                    {
                        var upgr = new VehicleList.VehicleUpgrade();

                        upgr.Coupon = upgrade[5];
                        upgr.Acceleration = upgrade[6];
                        upgr.Speed = upgrade[7];
                        upgr.Crash = upgrade[8];
                        upgr.Boost = upgrade[9];
                        upgr.Price = upgrade[10];
                        upgr.Sell = upgrade[11];
                        upgr.CloseSell = upgrade[12];
                        upgr.UpgradeMito = upgrade[13];
                        upgr.Efficiency = upgrade[14];
                        upgr.Capacity = upgrade[15];
                        upgr.RequiredLevel = upgrade[16];

                        veh.Upgrades.Add(upgr);
                    }
                }
                vehList.ItemList.Add(veh);
            }
            vehList.Save("Vehicles.xml");
        }
    }
}