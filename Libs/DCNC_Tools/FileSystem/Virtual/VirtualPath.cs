using System.IO;

namespace DCNC_Tools.FileSystem.Virtual
{
    static class VirtualPath
    {
        public static string GetDirectoryName(string filePath)
        {
            if (filePath == null) return null;
            //filePath = filePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            var ls = filePath.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar/*, Path.VolumeSeparatorChar*/ });

            if (ls < 0)
                return string.Empty;

            var result = string.IsNullOrEmpty(filePath.Substring(0, ls))
                ? filePath.Substring(0, ls)
                : filePath.Substring(0, ls).Replace('/', '\\');

            if (result.EndsWith(@":"))
                return result + @"\";

            return result;
        }

        public static string GetFileName(string filePath)
        {
            if (filePath == null) return null;

            var ls = filePath.LastIndexOfAny(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, Path.VolumeSeparatorChar });
            return ls < 0 ? filePath : filePath.Substring(ls + 1);
        }

        public static string GetExtension(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;

            var splitted = path.Split(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar,
                Path.VolumeSeparatorChar);

            if (splitted.Length > 0)
            {
                var p = splitted[splitted.Length - 1];

                var pos = p.LastIndexOf('.');
                if (pos < 0)
                    return string.Empty;

                return p.Substring(pos);
            }
            return string.Empty;
        }
    }
}
