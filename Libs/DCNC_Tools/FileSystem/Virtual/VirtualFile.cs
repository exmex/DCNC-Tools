namespace DCNC_Tools.FileSystem.Virtual
{
    public class VirtualFile
    {
        readonly object _innerFileEntry;

        readonly string _fileName;

        public object InnerFileEntry => _innerFileEntry;

        public string FileName => _fileName;

        public string Path => _path;

        readonly string _path;

        public VirtualFile(string fileName, object file)
        {
            _innerFileEntry = file;

            // Since the path comes with microsoft-style separators, this will break
            // any non-microsoft operating system
            // Better replace them with the correct separator for the current OS
            var fullPath = fileName.Replace('\\', System.IO.Path.DirectorySeparatorChar);

            _path = VirtualPath.GetDirectoryName(fullPath);
            _fileName = VirtualPath.GetFileName(fullPath);
        }
    }
}
