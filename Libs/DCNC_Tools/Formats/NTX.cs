using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using DCNC_Tools.IO;
using DCNC_Tools.Utils.DDSReader;

namespace DCNC_Tools.Formats
{
    /// <summary>
    /// Class to load files in NTX File Format
    /// </summary>
    public class NTX
    {
        /// <summary>
        /// The actual Texture File
        /// </summary>
        public class TextureFile
        {
            /// <summary>
            /// The texture name
            /// Max: 64 Bytes ASCII
            /// </summary>
            public string TextureName;

            /// <summary>
            /// The texture as DDS
            /// </summary>
            public byte[] Texture;

            /// <summary>
            /// Currently Unknown int
            /// </summary>
            public int Unknown;

            /// <summary>
            /// Reads the texture from NTX
            /// </summary>
            /// <param name="reader">The reader.</param>
            public void Read(BinaryReaderExt reader)
            {
                TextureName = reader.ReadAsciiStatic(64);
                var textureSize = reader.ReadInt32();
#if DEBUG
                Unknown = reader.ReadInt32();
#else
                reader.ReadInt32();
#endif
                Texture = reader.ReadBytes(textureSize);
            }

            /// <summary>
            /// Gets the Texture as Image
            /// </summary>
            /// <returns>Texture</returns>
            public Image GetImage()
            {
                return DDS.LoadImage(Texture);
            }

            public void Save(string fileName)
            {
                var extension = Path.GetExtension(fileName);
                if (extension == null) return;
                switch (extension.ToLower())
                {
                    case ".dds":
                        File.WriteAllBytes(fileName, Texture);
                        break;
                    case ".jpg":
                    case ".jpeg":
                        GetImage().Save(fileName, ImageFormat.Jpeg);
                        break;
                    case ".png":
                        GetImage().Save(fileName, ImageFormat.Png);
                        break;
                    case ".bmp":
                        GetImage().Save(fileName, ImageFormat.Bmp);
                        break;
                    case ".exif":
                        GetImage().Save(fileName, ImageFormat.Exif);
                        break;
                    case ".tiff":
                        GetImage().Save(fileName, ImageFormat.Tiff);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(extension);
                }
            }

            /// <summary>
            /// Returns a <see cref="System.String" /> that represents this texture.
            /// </summary>
            /// <returns>
            /// A <see cref="System.String" /> that represents this texture.
            /// </returns>
            public override string ToString()
            {
                return TextureName;
            }
        }

        /// <summary>
        /// Textures in file
        /// </summary>
        public readonly List<TextureFile> Textures = new List<TextureFile>();

        /// <summary>
        /// Loads the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public void Load(string fileName)
        {
            using (FileStream file = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var buff = new byte[3];
                file.Seek(72, SeekOrigin.Begin);
                file.Read(buff, 0, 3);
                if (buff[0] != 'D' || buff[1] != 'D' || buff[2] != 'S')
                    throw new InvalidFileHeaderException();
                file.Seek(0, SeekOrigin.Begin);

                using (var reader = new BinaryReaderExt(file))
                {
                    while (reader.BaseStream.Position < reader.BaseStream.Length)
                    {
                        var texture = new TextureFile();
                        texture.Read(reader);
                        Textures.Add(texture);
                    }
                }
            }
        }
    }
}
