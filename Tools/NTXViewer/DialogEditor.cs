using System.Collections.Generic;
using System.Windows.Forms;

namespace NTXViewer
{
    public partial class DialogEditor : UserControl
    {
        public DialogEditor()
        {
            InitializeComponent();
        }

        public void AddDialogs(IEnumerable<DialogListLoader.DialogList.UIDialog> dialogs)
        {
            foreach (var uiDialog in dialogs)
            {
                var dialogTreeNode = new TreeNode(uiDialog.C);
                foreach (var uiControls in uiDialog.Controls)
                {
                    dialogTreeNode.Nodes.Add(uiControls.Info.Name);
                }
                treeView1.Nodes.Add(dialogTreeNode);
            }
        }
    }

    public class UIControls
    {
        public int Width;
        public int Height;
        public string Name;
        public int X;
        public int Y;
    }

    public class UIDialog
    {
        public int Id;
        public List<UIControls> Controls = new List<UIControls>();
    }
}
