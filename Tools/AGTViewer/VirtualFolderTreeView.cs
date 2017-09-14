using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DCNC_Tools.FileSystem.Virtual;

namespace AGTViewer
{
    public class VirtualFolderTreeView : TreeView
    {
        public VirtualFolderTreeView()
        {
        }

        public EventHandler OnFillFinished;
        public void Fill(VirtualFolder virtualFolder)
        {
            Task.Run(() =>
            {
                var rootnode = new TreeNode("ROOT") {ImageIndex = 0, SelectedImageIndex = 0};
                foreach (var folder in virtualFolder.Folders)
                    AddSubfolders(rootnode, folder);
                foreach (var fileNodes in from file in virtualFolder.Files
                    select new TreeNode(file.FileName)
                    {
                        Tag = file
                    })
                {
                    rootnode.Nodes.Add(fileNodes);
                }

                Invoke((MethodInvoker)(() =>
                {
                    BeginUpdate();
                    Nodes.Clear();
                    Nodes.Add(rootnode);
                    EndUpdate();

                    OnFillFinished?.Invoke(null, null);
                }));
            });
        }

        /// <summary>
        /// Recursive method to add all files & folders from folder
        /// </summary>
        /// <param name="node">The node.</param>
        /// <param name="folder">The folder.</param>
        private static void AddSubfolders(TreeNode node, VirtualFolder folder)
        {
            var newnode = new TreeNode(folder.Name)
            {
                ImageIndex = 1,
                SelectedImageIndex = 1,
                Tag = folder
            };

            foreach (var subfolder in folder.Folders)
            {
                AddSubfolders(newnode, subfolder);
            }

            foreach (var file in folder.Files)
            {
                var fileNodes = new TreeNode(file.FileName)
                {
                    Tag = file
                };
                newnode.Nodes.Add(fileNodes);
            }

            node.Nodes.Add(newnode);
        }
    }
}
