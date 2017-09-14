using System;
using System.Collections.Generic;
using System.Linq;

namespace DCNC_Tools.FileSystem.Virtual
{
    public class VirtualFolder
    {
        private readonly Dictionary<string, VirtualFolder> _subfolders = new Dictionary<string, VirtualFolder>();
        private readonly List<VirtualFile> _files = new List<VirtualFile>();

        private readonly string _name;
        public VirtualFolder ParentFolder;
        private long? _folderSize = null;

#if !VC9
        public VirtualFolder(string name, VirtualFolder parent = null)
#else
        public VirtualFolder(string name, VirtualFolder parent)
#endif
        {
            _name = name;
            ParentFolder = parent;
        }

        /// <summary>
        /// Creates a new Folder, if a Folder of the same Name does not exists.
        /// If the Folder already exists, the 
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public VirtualFolder CreateFolder(string folderName)
        {
            // Create Entry if it does not exist
#if !VC9
            if (!_subfolders.ContainsKey(folderName))
                _subfolders.Add(folderName, new VirtualFolder(folderName));
#else
            if (!_subfolders.ContainsKey(folderName))
                _subfolders.Add(folderName, new VirtualFolder(folderName, null));
#endif

            return _subfolders[folderName];
        }


        /// <summary>
        /// Adds a File to this Folder
        /// </summary>
        /// <param name="file"></param>
        public void AddFile(VirtualFile file)
        {
            _files.Add(file);
        }

        public long GetFolderSize()
        {
            /*if (_folderSize == null)
                _folderSize = _files.Sum(file => file.Size) + _subfolders.Sum(virtualFolder => virtualFolder.Value.GetFolderSize());

            return (long)_folderSize;*/
            return 0L;
        }


        public VirtualFile GetFile(string filename)
        {
            foreach (var file in _files)
                if (file.FileName == filename)
                    return file;

            throw new Exception("File " + filename + " not found");
        }

        /// <summary>
        /// Enter a Folder (for Navigation-Purposes only!)
        /// </summary>
        /// <param name="folderName"></param>
        /// <returns></returns>
        public VirtualFolder this[string folderName] => _subfolders[folderName];


        public List<VirtualFile> Files => _files;

        public List<VirtualFolder> Folders => _subfolders.Values.ToList();

        public string Name => _name;
    }
}
