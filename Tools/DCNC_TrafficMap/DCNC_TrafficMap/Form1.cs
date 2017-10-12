using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DCNC_TrafficMap
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            mapRenderer1.LoadTCS("Oros.tcs");
        }
    }
}
