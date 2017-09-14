using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using DCNC_Tools.Formats;
using DCNC_Tools.IO;

namespace TDFViewer.Utils
{
    public class TDFDictionaryConverter
    {
        public List<string> ColumnDefinitions = new List<string>();
        public Dictionary<int, List<string>> Rows = new Dictionary<int, List<string>>();

        public TDFDictionaryConverter(TDF file)
        {
            using (var reader = new BinaryReaderExt(new MemoryStream(file.ResTable)))
            {
                for (var row = 0; row < file.Header.Row; row++)
                {
                    var columns = new List<string>();
                    for (var col = 0; col < file.Header.Col; col++)
                    {
                        var colText = reader.ReadUnicode();
                        columns.Add(colText.Trim());
                    }
                    Rows.Add(row, columns);
                }
            }
        }

        public TDFDictionaryConverter()
        {
            
        }

        public void GetColumnDefinitions(string fileName)
        {
            FileInfo fi = new FileInfo(fileName);
            var columnDefFile = $"ColumnDefinitions/ColumnDef_{Path.GetFileNameWithoutExtension(fi.Name)}.txt";
            if (File.Exists(columnDefFile))
            {
                ColumnDefinitions = File.ReadLines(columnDefFile).ToList();
                if (Rows[0].Count > 0)
                {
                    if (ColumnDefinitions.Count > Rows[0].Count)
                        ColumnDefinitions.AddRange(new string[ColumnDefinitions.Count - Rows[0].Count]);
                    else
                        throw new Exception("Column Definition Count does not match file column count!");
                }
            }
        }

        public void Import(string fileName)
        {
            var fi = new FileInfo(fileName);
            switch (fi.Extension)
            {
                default:
                    throw new ArgumentException("Unsupported format!");
                case ".csv":
                    ImportCSV(fileName);
                    break;
                case ".xml":
                    //ImportXML(fileName);
                    MessageBox.Show("This feature is not yet available", "Coming Soon!");
                    break;
            }
        }

        public void ImportCSV(string fileName)
        {
            using (TextReader reader = File.OpenText(fileName))
            {
                string line;
                var row = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    Rows.Add(row, line.Split(',').Where(col => col != "").ToList());
                    row++;
                }
            }
        }

        public void Export(string fileName)
        {
            var fi = new FileInfo(fileName);
            switch (fi.Extension)
            {
                default:
                    throw new ArgumentException("Unsupported format!");
                case ".csv":
                    ExportCSV(fileName);
                    break;
                case ".xml":
                    ExportXML(fileName);
                    break;
            }
        }

        public void ExportXML(string fileName)
        {
            using (TextWriter textWriter = File.CreateText(fileName))
            {
                using (XmlWriter writer = XmlWriter.Create(textWriter))
                {
                    writer.WriteStartElement("Rows");
                    foreach (var row in Rows)
                    {
                        writer.WriteStartElement("Row");
                        for (var index = 0; index < row.Value.Count; index++)
                        {
                            var col = row.Value[index];
                            writer.WriteAttributeString(index.ToString(CultureInfo.InvariantCulture), col);
                        }
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }
            }
        }

        public void ExportCSV(string fileName)
        {
            using (TextWriter writer = File.CreateText(fileName))
            {
                var sb = new StringBuilder();
                if (ColumnDefinitions.Count != 0)
                {
                    for (var index = 0; index < ColumnDefinitions.Count; index++)
                    {
                        var columnDefinition = ColumnDefinitions[index];
                        if (Rows[0].Count > index)
                            sb.Append(columnDefinition);
                        else
                            break;

                        if (index < ColumnDefinitions.Count) // Do not append , to the last item
                            sb.Append(",");
                    }
                    sb.AppendLine();
                }

                foreach (var row in Rows)
                {
                    for (var index = 0; index < row.Value.Count; index++)
                    {
                        var col = row.Value[index];
                        sb.Append(col);
                        if (index < row.Value.Count) // Do not append , to the last item
                            sb.Append(",");
                    }
                    sb.AppendLine();
                }
                writer.Write(sb.ToString());
            }
        }
    }
}