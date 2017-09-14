using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace NTXViewer
{


    /*DialogListLoader dll;

    var encoding = Encoding.GetEncoding("euc-kr");
    var serializer = new XmlSerializer(typeof(DialogListLoader));

    using (var reader = new StreamReader("game.nui", encoding))
    {
        dll = (DialogListLoader)serializer.Deserialize(reader);
    }

    dialogEditor1.AddDialogs(dll.Dialogs.Dialogs);*/

    [Serializable]
    [XmlRoot(ElementName = "document")]
    public class DialogListLoader
    {
        public class DialogList
        {

            public class UIControlInfo
            {
                [XmlAttribute("TYPE")] public string Type;
                [XmlAttribute("NM")] public string Name;
                [XmlAttribute("EC")] public string Ec;
                [XmlAttribute("SZ1")] public string Size;
                [XmlAttribute("I1_1")] public string Image;
                [XmlAttribute("UV1_1")] public string Uv;
                [XmlAttribute("POS1")] public string Position;
            }

            public class UIControl
            {
                [XmlAttribute("B")] public string B;

                [XmlElement("INFO")] public UIControlInfo Info;
            }

            public class UIDialog
            {
                [XmlAttribute("C")] public string C;
                [XmlAttribute("I")] public string I;
                [XmlAttribute("F")] public string F;
                [XmlAttribute("H")] public string H;

                [XmlElement("CONTROL")]
                public List<UIControl> Controls;
            }

            [XmlElement("DIALOG")]
            public List<UIDialog> Dialogs = new List<UIDialog>();
        }

        [XmlElement(ElementName = "DIALOGLIST")]
        public DialogList Dialogs;

        public class UIFileInfo
        {
            [XmlAttribute("Type")] public string Type;
            [XmlAttribute("Nation")] public string Nation;
            [XmlAttribute("Time")] public string Time;
        }

        [XmlElement("FILEINFO")] public UIFileInfo FileInfo;
    }
}
