using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;
using System.ComponentModel;
using System.Text;
using System.Runtime.CompilerServices;

public class Program
{
    /// <summary>
    /// A lookup dicitionary for C# types to C types
    /// </summary>
    public static Dictionary<string, string> TypeLookup = new Dictionary<string, string>(){
        {"System.Char[]", "char[]"},
        {"System.UInt16", "unsigned short"},
        {"System.Int16", "short"},
        {"System.UInt32", "unsigned int"},
        {"System.Int32", "int"},
        {"System.UInt64", "unsigned long"},
        {"System.Int64", "long"},
    };

    public static Dictionary<string, string> TypeCSharpLookup = new Dictionary<string, string>(){
        {"short", "short"},
        {"unsigned short", "ushort"},
        {"int", "int"},
        {"unsigned int", "uint"},
        {"long", "long"},
        {"unsigned long", "ulong"},
        {"char[]", "string"},
        {"char", "char"},
        {"wchar_t", "char"},
        {"wchar_t[]", "string"},
    };

    /// <summary>
    /// A lookup dictionary for C# types to C byte sizes
    /// </summary>
    public static Dictionary<string, int> SizeLookup = new Dictionary<string, int>(){
        {"System.Char[]", 1},
        {"System.Char", 1},
        {"System.UInt16", 2},
        {"System.Int16", 2},
        {"System.UInt32", 4},
        {"System.Int32", 4},
        {"System.UInt64", 8},
        {"System.Int64", 8},
    };

    public static string cwd = Directory.GetCurrentDirectory();

    /// <summary>
    /// A dictionary lookup table to convert ASCII C variable to Unicode C variable
    /// </summary>
    public static Dictionary<string, string> UnicodeLookup = new Dictionary<string, string>(){
        {"char[]", "wchar_t[]"},
        {"char", "wchar_t"},
    };

    /// <summary>
    /// List of all Assemblies compiled earlier from C# Source
    /// </summary>
    public static List<Assembly> compiledAssemblies = new List<Assembly>();
    public static Dictionary<ushort, Packet> packets = new Dictionary<ushort, Packet>();

    /// <summary>
    /// Compiles and loads all files from a directory
    /// </summary>
    /// <param name="directory">The directory where we search for *.cs files</param>
    public static void CompileAndLoad(string directory)
    {
        var files = Directory.EnumerateFiles("definitions", "*.cs", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var assembly = CompileSourceCodeDom(File.ReadAllText(file));
            if (assembly == null)
            {
                System.Console.WriteLine(new FileInfo(file).Name + " failed to compile. Ignoring");
                continue;
            }

            compiledAssemblies.Add(assembly);
        }
        System.Console.WriteLine($"Loaded {compiledAssemblies.Count} Packet definition assemblies");
    }

    /// <summary>
    /// Generates a Wiki Page for the previously generated C# Class
    /// </summary>
    public static void GenerateWikiPage()
    {
        if (!Directory.Exists(cwd + "\\out\\WikiPages\\"))
            Directory.CreateDirectory(cwd + "\\out\\WikiPages\\");

        if (!File.Exists(cwd + "\\templates\\PacketWikiTemplate.tpl"))
        {
            System.Console.WriteLine("[WIKI] ABORTING! NO PACKET TEMPLATE FOUND!!");
            return;
        }

        foreach (var packet in packets)
        {
            File.WriteAllText(cwd + "\\out\\WikiPages\\" + packet.Value.InternalName + ".md", GeneratePacketWiki(packet.Value));
        }

        // TODO: IMPLEMENT ME!!!
    }

    /// <summary>
    /// Generates a stub C# Class for the specified packet
    /// </summary>
    /// <returns>true if successfull, false otherwise</returns>
    public static bool GenerateCSharpClass()
    {
        if (!Directory.Exists(cwd + "\\out\\Classes\\"))
            Directory.CreateDirectory(cwd + "\\out\\Classes\\");

        if (!File.Exists(cwd + "\\templates\\PacketClassTemplate.tpl") ||
            !File.Exists(cwd + "\\templates\\PacketAckClassTemplate.tpl"))
        {
            System.Console.WriteLine("[C# CLASS] ABORTING! NO PACKET TEMPLATE FOUND!!");
            return false;
        }

        foreach (var packet in packets)
        {
            File.WriteAllText(cwd + "\\out\\Classes\\" + packet.Value.InternalName + ".cs", GeneratePacketClass(packet.Value));
        }

        return true;
    }

    public static string GeneratePacketWiki(Packet packet)
    {
        var template = File.ReadAllText(cwd + "\\templates\\PacketWikiTemplate.tpl");
        var direction = packet.Type == Packet.Direction.ToServer ?
                                "`Client` => `Server`" : "`Server` => `Client`";
        var infSb = new StringBuilder();
        infSb.AppendLine($"| Direction | {direction} |");
        infSb.AppendLine($"| Internal Name | `{packet.InternalName}` |");
        infSb.AppendLine($"| PacketID | `{packet.Id}` (`0x" + packet.Id.ToString("X") + "`) |");
        if(packet.hasUnknownField)
            infSb.AppendLine($"| TotalSize | `{packet.Size}` + `?` |");
        else
            infSb.AppendLine($"| TotalSize | `{packet.Size}` |");

        var sb = new StringBuilder();
        foreach (var field in packet.Fields)
        {
            if (field.Size != null && field.Size.HasValue)
                sb.AppendLine($"| `{field.Name}` | `{field.Type}` | `{field.Size}` | {field.Description} |");
            else
                sb.AppendLine($"| `{field.Name}` | `{field.Type}` | `?` | {field.Description} |");
        }

        var answerSb = new StringBuilder();
        if (packet.Type == Packet.Direction.ToClient)
            answerSb.AppendLine("This is the answer packet to:");
        else
            answerSb.AppendLine("Answer Packet(s):");

        foreach (var answerId in packet.AnswerPacket)
        {
            if(!packets.ContainsKey(answerId)){
                System.Console.WriteLine(packet.Name + " - SKIPPING MISSING ANSWER ID " + answerId + "");
                continue;
            }

            var answerPacket = packets[answerId];
            answerSb.Append("\n* [" + answerPacket.InternalName + " (" + answerId + ", 0x" + answerId.ToString("X") + ")](" + answerPacket.InternalName + ")");
        }
        template = Regex.Replace(template, "{{ \\$answerIds }}", answerSb.ToString());
        template = Regex.Replace(template, "{{ \\$packetInformations }}", infSb.ToString());
        template = Regex.Replace(template, "{{ \\$packetDescription }}", packet.Description);
        template = Regex.Replace(template, "{{ \\$packetStructure }}", sb.ToString());
        template = Regex.Replace(template, "{{ \\$packetHexDumpSize }}", packet.sample.Length.ToString());
        template = Regex.Replace(template, "{{ \\$packetHexDump }}", HexDump(packet.sample));
        template = Regex.Replace(template, "{{ \\$comment }}", "\n> NOTE: This page was autogenerated by WikiPacketGenerator\n" +
                        "<!---\nDelete the line above when page is checked\n--->");

        return template;
    }

    public static string GeneratePacketClass(Packet packet)
    {
        var template = "";
        if (packet.Type == Packet.Direction.ToClient)
            template = File.ReadAllText(cwd + "\\templates\\PacketAckClassTemplate.tpl");
        else
            template = File.ReadAllText(cwd + "\\templates\\PacketClassTemplate.tpl");

        template = Regex.Replace(template, "{{ \\$description }}", "/// <summary>" + "\n" +
            "/// " + packet.Description + "\n" +
            "/// Answer Packets:" +
            "{{ $answerIds }}" + "\n" +
            "/// </summary>");
        template = Regex.Replace(template, "{{ \\$name }}", packet.Name);
        template = Regex.Replace(template, "{{ \\$size }}", packet.Size.ToString());
        var answerSb = new StringBuilder();
        foreach (var answerId in packet.AnswerPacket)
        {
            if(!packets.ContainsKey(answerId)){
                System.Console.WriteLine(packet.Name + " - SKIPPING MISSING ANSWER ID " + answerId + "");
                continue;
            }
            var answerPacket = packets[answerId];
            answerSb.Append("\n/// \t" + answerPacket.Name + " - " + answerId + " (0x" + answerId.ToString("X") + ")");
        }
        template = Regex.Replace(template, "{{ \\$answerIds }}", answerSb.ToString());

        var fieldStringBuilder = new StringBuilder();
        foreach (var item in packet.Fields)
        {
            var type = TypeCSharpLookup[item.Type];
            fieldStringBuilder.AppendLine("\t/// <summary>");
            fieldStringBuilder.AppendLine("\t/// " + item.Description);
            fieldStringBuilder.AppendLine("\t/// Byte Size " + item.Size);
            fieldStringBuilder.AppendLine("\t/// </summary>");
            fieldStringBuilder.AppendLine($"\tpublic {type} {item.Name};\n");
        }
        template = Regex.Replace(template, "{{ \\$fields }}", fieldStringBuilder.ToString());

        return template;
    }

    public static Packet GetPacketFromType(Type type)
    {
        // Skip compiler generated bullshit
        if (Attribute.IsDefined(type, typeof(CompilerGeneratedAttribute)) ||
                type.FullName.IndexOfAny(new char[] { '<', '>', '+' }) != -1)
            return null;

        var typeAttributes = type.GetCustomAttributes(false).ToDictionary(a => a.GetType().Name, a => a);

        if (!Attribute.IsDefined(type, typeof(DescriptionAttribute)))
        {
            System.Console.WriteLine($"SKIPPING TYPE {type.FullName} DUE TO MISSING DESCRIPTION!");
            return null;
        }
        if (!Attribute.IsDefined(type, typeof(PacketAttribute)))
        {
            System.Console.WriteLine($"SKIPPING TYPE {type.FullName} DUE TO MISSING PACKET ATTRIBUTE!");
            return null;
        }
        var typeDescription = ((DescriptionAttribute)typeAttributes["DescriptionAttribute"]).Description;
        var packetDescriptor = ((PacketAttribute)typeAttributes["PacketAttribute"]);

        var packet = new Packet()
        {
            Id = packetDescriptor.packetId,
            Name = type.FullName,
            InternalName = packetDescriptor.internalName,
            Type = packetDescriptor.direction,
        };

        // TODO: Load answerPacket name, etc..
        // If the answerPacketIds has been provided.
        if (packetDescriptor.answerPacketIds != null)
        {
            foreach (var answerPacketId in packetDescriptor.answerPacketIds)
                packet.AnswerPacket.Add(answerPacketId);
        }

        foreach (var field in type.GetFields())
        {
            var fieldAttributes = field.GetCustomAttributes(false).ToDictionary(a => a.GetType().Name, a => a);
            var packetField = new PacketField();

            if (Attribute.IsDefined(field, typeof(DescriptionAttribute)))
                packetField.Description = ((DescriptionAttribute)fieldAttributes["DescriptionAttribute"]).Description;

            packetField.Name = field.Name;
            packetField.Type = TypeLookup[field.FieldType.FullName];
            packetField.Size = SizeLookup[field.FieldType.FullName];

            if (field.FieldType.IsArray)
            {
                if (Attribute.IsDefined(field, typeof(LengthAttribute)))
                {
                    var arrayLength = ((LengthAttribute)fieldAttributes["LengthAttribute"]).Length;
                    if (arrayLength.HasValue)
                        packetField.Size *= ((LengthAttribute)fieldAttributes["LengthAttribute"]).Length;
                    else{
                        packet.hasUnknownField = true;
                        packetField.Size = null;
                    }
                }

                if (field.FieldType == typeof(char[]) && Attribute.IsDefined(field, typeof(UnicodeAttribute)))
                    packetField.Type = UnicodeLookup[packetField.Type];
            }

            packet.Fields.Add(packetField);
            if (packetField.Size.HasValue)
                packet.Size += packetField.Size.Value;
        }

        var fileDir = @"C:\Users\GigaToni\Documents\Visual Studio 2015\Projects\MittronMadness\PacketCaptures\";
        if (packet.Type == Packet.Direction.ToServer)
            fileDir += "incoming\\";
        else
            fileDir += "outgoing\\";

        if (File.Exists(fileDir + packet.Name + ".bin"))
            packet.sample = File.ReadAllBytes(fileDir + packet.Name + ".bin");
        else if (File.Exists(fileDir + packet.Id + ".bin"))
            packet.sample = File.ReadAllBytes(fileDir + packet.Id + ".bin");
        else
        {
            System.Console.WriteLine("WARNING: NO BINARY HEXDUMP AVAILABLE FOR " + packet.Name + " RESORTING TO OLD TYPE METHOD");
            var hexDumpMethod = type.GetMethod("HexDump");
            packet.sample = new byte[0];
            if (hexDumpMethod != null)
                packet.sample = (byte[])hexDumpMethod.Invoke(Activator.CreateInstance(type), new object[] { });
        }

        // Pretty dirty fix for the incoming bin files not having ids..
        if (packet.Type == Packet.Direction.ToServer)
        {
            var incomingBytesOld = new byte[packet.sample.Length + 2];
            var packetIdBytes = BitConverter.GetBytes(packet.Id);
            Array.Copy(packet.sample, 0, incomingBytesOld, 2, packet.sample.Length);
            Array.Copy(packetIdBytes, 0, incomingBytesOld, 0, 2);
            packet.sample = incomingBytesOld;
        }

        return packet;
    }

    public static void Main(string[] args)
    {
        if (!Directory.Exists(cwd + "\\out"))
            Directory.CreateDirectory(cwd + "\\out");

        CompileAndLoad(cwd + "\\definitions");

        System.Console.WriteLine("Loaded defintions.");

        // Preload packets
        foreach (var assembly in compiledAssemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                var packet = GetPacketFromType(type);
                if (packet != null) // Skipped type because of an error.
                    packets.Add(packet.Id, packet);
            }
        }

        if (GenerateCSharpClass())
            GenerateWikiPage();

        /*foreach (var assembly in compiledAssemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                string template = File.ReadAllText("PacketTemplate.tpl");

                var typeAttributes = type.GetCustomAttributes(false).ToDictionary(a => a.GetType().Name, a => a);
                if (!Attribute.IsDefined(type, typeof(DescriptionAttribute)))
                {
                    System.Console.WriteLine($"SKIPPING FIELD {type.FullName} DUE TO MISSING DESCRIPTION!");
                    continue;
                }
                if (!Attribute.IsDefined(type, typeof(PacketAttribute)))
                {
                    System.Console.WriteLine($"SKIPPING FIELD {type.FullName} DUE TO MISSING PACKET ATTRIBUTE!");
                    continue;
                }

                using (var writer = new StreamWriter("out/" + type.FullName + ".md", false))
                {
                    var sb = new StringBuilder();
                    var totalSize = 0;
                    var typeDescription = ((DescriptionAttribute)typeAttributes["DescriptionAttribute"]).Description;
                    var packetDescriptor = ((PacketAttribute)typeAttributes["PacketAttribute"]);

                    var typeDirection = packetDescriptor.direction == Packet.Direction.ClientToServer ?
                                "`Client` => `Server`" : "`Server` => `Client`";
                    var typeInternalName = packetDescriptor.internalName;
                    var typePacketId = packetDescriptor.packetId;
                    var typePacketAnswerId = packetDescriptor.answerPacketId;

                    foreach (var field in type.GetFields())
                    {
                        var fieldAttributes = field.GetCustomAttributes(false).ToDictionary(a => a.GetType().Name, a => a);

                        var fieldName = field.Name;
                        var fieldType = TypeLookup[field.FieldType.FullName];
                        int? fieldSize = SizeLookup[field.FieldType.FullName];
                        var fieldDescription = "";

                        if (!Attribute.IsDefined(field, typeof(DescriptionAttribute)))
                        {
                            System.Console.WriteLine($"SKIPPING FIELD {fieldName} DUE TO MISSING DESCRIPTION!");
                            continue;
                        }
                        fieldDescription = ((DescriptionAttribute)fieldAttributes["DescriptionAttribute"]).Description;

                        if (field.FieldType.IsArray)
                        {
                            if (!Attribute.IsDefined(field, typeof(UnicodeAttribute)))
                            {
                                System.Console.WriteLine($"SKIPPING FIELD {fieldName} DUE TO MISSING LENGTH ATTRIBUTE");
                                continue;
                            }
                            var length = ((LengthAttribute)fieldAttributes["LengthAttribute"]).Length;

                            if (field.FieldType == typeof(char[]) && Attribute.IsDefined(field, typeof(UnicodeAttribute)))
                            {
                                fieldType = UnicodeLookup[fieldType];
                            }

                            if (length == null)
                                fieldSize = null;
                            else
                                fieldSize = fieldSize * (int)length;
                        }

                        if (fieldSize != null && fieldSize.HasValue)
                            sb.AppendLine($"| `{fieldName}` | `{fieldType}` | `{fieldSize}` | {fieldDescription} |");
                        else
                            sb.AppendLine($"| `{fieldName}` | `{fieldType}` | `?` | {fieldDescription} |");

                        if (fieldSize != null && fieldSize.HasValue)
                            totalSize += fieldSize.Value;
                    }

                    var infSb = new StringBuilder();
                    infSb.AppendLine($"| Direction | {typeDirection} |");
                    infSb.AppendLine($"| Internal Name | `{typeInternalName}` |");
                    infSb.AppendLine($"| PacketID | `{typePacketId}` (`0x" + typePacketId.ToString("X") + "`) |");
                    infSb.AppendLine($"| TotalSize | `{totalSize}` |");

                    var hexDumpMethod = type.GetMethod("HexDump");
                    var hexdump = "";
                    var hexdumpBytes = new byte[0];
                    if (hexDumpMethod == null)
                    {
                        hexdump = "|  |  |";
                    }
                    else
                    {
                        hexdumpBytes = (byte[])hexDumpMethod.Invoke(Activator.CreateInstance(type), new object[] { });
                        hexdump = HexDump(hexdumpBytes);
                    }

                    template = Regex.Replace(template, "{{ \\$packetInformations }}", infSb.ToString());
                    template = Regex.Replace(template, "{{ \\$packetDescription }}", typeDescription);
                    template = Regex.Replace(template, "{{ \\$packetStructure }}", sb.ToString());
                    template = Regex.Replace(template, "{{ \\$packetHexDumpSize }}", hexdumpBytes.Length.ToString());
                    template = Regex.Replace(template, "{{ \\$packetHexDump }}", hexdump);
                    template = Regex.Replace(template, "{{ \\$comment }}", "\n> NOTE: This page was autogenerated by WikiPacketGenerator\n" +
                                    "<!---\nDelete the line above when page is checked\n--->");
                }

                File.WriteAllText("out/" + type.FullName + ".md", template);
            }
        }*/
    }

    /// <summary>
    /// Dumps a IEnumerable<byte> to readable Hex
    /// </summary>
    /// <param name="buffer">The buffer to dump</param>
    /// <returns>string containing a hexdump markdown table</returns>
    public static string HexDump(IEnumerable<byte> buffer)
    {
        const int bytesPerLine = 16;
        var hexDump = "";
        //var j = 0;
        foreach (var g in buffer.Select((c, i) => new { Char = c, Chunk = i / bytesPerLine }).GroupBy(c => c.Chunk))
        {
            var s1 = g.Select(c => $"{c.Char:X2} ").Aggregate((s, i) => s + i);
            string s2 = null;
            var first = true;
            foreach (var c in g)
            {
                var s = $"{(c.Char < 32 || c.Char > 122 ? '.' : (char)c.Char)}";
                if (first)
                {
                    first = false;
                    s2 = s;
                    continue;
                }
                s2 = s2 + s;
            }
            //var s3 = $"{j++ * bytesPerLine:d6}: {s1} {s2}";
            var s3 = $"| `{s1}` | `{s2}` | ";
            hexDump = hexDump + s3 + Environment.NewLine;
        }
        return hexDump;
    }

    /// <summary>
    /// Compiles C# Source files into our assembly
    /// </summary>
    /// <param name="sourceCode">The source code of the C# file</param>
    /// <returns>The compiled assembly, or null if errors occurred</returns>
    private static Assembly CompileSourceCodeDom(string sourceCode)
    {
        var cSharpCodeProvider = new CSharpCodeProvider();
        var compilerParams = new CompilerParameters()
        {
            GenerateExecutable = false
        };
        compilerParams.ReferencedAssemblies.AddRange(new string[]{
            Assembly.GetEntryAssembly().Location,
            "System.dll",
            "System.Linq.dll",
        });

        var compileResult = cSharpCodeProvider.CompileAssemblyFromSource(compilerParams, sourceCode);
        if (compileResult.Errors.Count > 0)
        {
            foreach (var err in compileResult.Errors)
                Console.WriteLine(err);

            return null;
        }

        return compileResult.CompiledAssembly;
    }
}