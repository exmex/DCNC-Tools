using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace TdfToXml
{
    public class TdfFile
    {
        // TODO: More column names :)
        private static readonly string[] QuestColumns =
        {
            // TODO: Column count doesn't match sometimes
            "Quest ID",
            "Quest ID",
            "Prev Quest ID",
            "Event",
            "Need Level",
            "Need Level Percent",
            "Give Post",
            "Title",
            "End Post",
            "Place",
            "Place",
            "Place",
            "Place",
            "Place",
            "Crash Time",
            "Crash Time",
            "Crash Time",
            "Crash Time",
            "Crash Time",
            "Time Limit",
            "Time Limit",
            "Time Limit",
            "Time Limit",
            "Time Limit",
            "Min Speed",
            "Min Speed",
            "Min Speed",
            "Min Speed",
            "Min Speed",
            "Min Speed",
            "Max Speed",
            "Max Speed",
            "Max Speed",
            "Max Speed",
            "Max Speed",
            "Max Speed",
            "Min Ladius",
            "Min Ladius",
            "Min Ladius",
            "Min Ladius",
            "Min Ladius",
            "Max Ladius",
            "Max Ladius",
            "Max Ladius",
            "Max Ladius",
            "Max Ladius",
            "Quest Path 1",
            "Quest Path 2",
            "Car 1",
            "Car 2",
            "Clear Quest ID",
            "Count",
            "Reward Exp",
            "Reward Money",
            "Item 1",
            "Item 2",
            "Item 3"
        };

        private static readonly string[] ItemClientColumns =
        {
            "Empty",
            "Type",
            "Set Type",
            "ID Name",
            "Group ID",
            "Name",
            "???",
            "Grade",
            "Required Level",
            "???",
            "Value",
            "Min",
            "Max",
            "Cost",
            "Sell",
            "Next ID",
            "Shop",
            "Trade",
            "Auction",
            "Set Rate",
            "Set Desc",
            "Set Assist",
            "Time"
        };

        private static readonly string[] ItemServerColumns =
        {
            "Type",
            "Set Type",
            "ID Name",
            "Group ID",
            "Name",
            "Grade",
            "Required Level",
            "Value",
            "Min",
            "Max",
            "Cost",
            "Sell",
            "Assist Num",
            "Assist Field",
            "Belong",
            "Next ID",
            "Shop",
            "Trade",
            "Auction",
            "Set Rate",
            "Set Desc",
            "Set Assist",
            "Time",
            "Jewel Type"
        };

        public readonly SBitmap Bitmap;
        public int[] DataTable;
        public readonly SHeader Header;
        public byte[] ResTable;
        public readonly SVersion Version;

        public TdfFile()
        {
            Bitmap = new SBitmap();
            Version = new SVersion();
            Header = new SHeader
            {
                Date = new SDate()
            };
            Header.Date.Day = (char) DateTime.Now.Day;
            Header.Date.Month = (char) DateTime.Now.Month;
            Header.Date.Year = (ushort) DateTime.Now.Year;

            Bitmap.BfType = 19778;
            Bitmap.BfSize = 21054;
            Bitmap.BfOffBits = 54;
            Version.Major = 1;
            Version.Minor = 4;
        }

        public void Load(string fileName)
        {
            using (var reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
            {
                Bitmap.BfType = reader.ReadInt16();
                Bitmap.BfSize = reader.ReadUInt32();
                Bitmap.BfReserved1 = reader.ReadInt16();
                Bitmap.BfReserved2 = reader.ReadInt16();
                Bitmap.BfOffBits = reader.ReadUInt32();
                if (Bitmap.BfType == 19778)
                    reader.ReadBytes((int) Bitmap.BfSize - 14);

                Version.Major = reader.ReadUInt16();
                Version.Minor = reader.ReadUInt16();
                if (Version.Major != 1 && Version.Minor != 4)
                {
                    /*MessageBox.Show(
                        @"The version of this file is not supported. Please create an issue @https://github.com/exmex/DCNC-Tools/issues/new");*/
                    return;
                }

                Header.Date.Year = reader.ReadUInt16();
                Header.Date.Month = reader.ReadChar();
                Header.Date.Day = reader.ReadChar();

                Header.Flag = reader.ReadUInt32();
                Header.Offset = reader.ReadUInt32();
                Header.Col = reader.ReadUInt32();
                Header.Row = reader.ReadUInt32();

                DataTable = new int[Header.Col * Header.Row];
                for (var i = 0; i < Header.Col * Header.Row; i++)
                    DataTable[i] = reader.ReadInt32();

                ResTable = new byte[Header.Offset - (Header.Col * 4 * Header.Row + 24)];
                for (long i = 0; i < Header.Offset - (Header.Col * 4 * Header.Row + 24); i++)
                    ResTable[i] = reader.ReadByte();

#if DEBUG
                Debug.WriteLine("Loaded TDF Version: {0:D}.{1:D} ({2:D}/{3:D}/{4:D})", (int) Version.Major,
                    Version.Minor, (short) Header.Date.Month, (short) Header.Date.Day, Header.Date.Year);
                Debug.WriteLine("File Offset: {2} contains {0} Rows and {1} Columns", Header.Row, Header.Col, Header.Offset);

                Debug.WriteLine("DataTable size: {0:D}, ResTable size: {1:D}", DataTable.Length, ResTable.Length);
#endif
            }
        }

        public void Save(string fileName)
        {
            using (var writer = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                writer.Write(Bitmap.BfType);
                writer.Write(Bitmap.BfSize);
                writer.Write(Bitmap.BfReserved1);
                writer.Write(Bitmap.BfReserved2);
                writer.Write(Bitmap.BfOffBits);
                if (Bitmap.BfType == 19778)
                    writer.Write(new byte[(int) Bitmap.BfSize - 14]);
                writer.Write(Version.Major);
                writer.Write(Version.Minor);
                writer.Write(Header.Date.Year);
                writer.Write(Header.Date.Month);
                writer.Write(Header.Date.Day);
                writer.Write(Header.Flag);
                writer.Write(Header.Offset);
                writer.Write(Header.Col);
                writer.Write(Header.Row);
                
                for (var i = 0; i < Header.Col * Header.Row; i++)
                    writer.Write(DataTable[i]);

                for (long i = 0; i < Header.Offset - (Header.Col * 4 * Header.Row + 24); i++)
                    writer.Write(ResTable[i]);
            }
        }

        public List<object> GetData()
        {
            return null;
        }

        public static string GetColumnName(int column, string fileName)
        {
            switch (fileName)
            {
                default:
                //case "QuestClient.tdf":
                    break;
                case "ItemClient.tdf":
                    if (ItemClientColumns.Length > column)
                        return ItemClientColumns[column];
                    break;
                case "ItemServer.tdf":
                    if (ItemServerColumns.Length > column)
                        return ItemServerColumns[column];
                    break;
                case "QuestServer.tdf":
                    if (QuestColumns.Length > column)
                        return QuestColumns[column];
                    break;
            }
            return column.ToString();
        }

        public class SBitmap
        {
            public uint BfOffBits;
            public short BfReserved1;
            public short BfReserved2;
            public uint BfSize;
            public short BfType;
        }

        public class SVersion
        {
            public ushort Major;
            public ushort Minor;
        }

        public class SDate
        {
            public char Day;
            public char Month;
            public ushort Year;
        }

        public class SHeader
        {
            public uint Col;
            public SDate Date;
            public uint Flag;
            public uint Offset;
            public uint Row;
        }
    }
}