using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;
using DCNC_Tools.Formats;

namespace DCNC_TrafficMap
{
    public partial class MapRenderer : UserControl
    {
        private long _frameCount = 0;
        private DateTime _lastCheckTime = DateTime.Now;
        private TCS _tcs;
        private float baseX = 600;
        private float baseY = 300;

        private float mouseX;
        private float mouseY;

        private float offsetX;
        private float offsetY;
        private float scale = 10.0f;

        public MapRenderer()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }

        public void LoadTCS(string fileName)
        {
            _tcs = new TCS(fileName);

            Invalidate();
        }

        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg > 0x201)
            {
                //   WM_LBUTTONUP       WM_MOUSEWHEEL
                if (m.Msg != 514 && m.Msg == 522)
                {
                    /*
                    this->m_nDeltaWheel = (signed int)(wparam & 0xFFFF0000) >> 16;
                    v9 = (double) this->m_nDeltaWheel / 120.0 / 10.0;
                    ((void (__thiscall*) (TView *, _DWORD))this->m_tview->vfptr->Scale)(this->m_tview, LODWORD(v9));
                    CGdiPlusHelper::SetDirty((CGdiPlusHelper*) &thisa->vfptr);
                    */
                    var deltaWheel = m.WParam.ToInt32() >> 16;
                    Scale(deltaWheel / 120.0f / 10.0f);
                    Invalidate();
                }
            }
            else
            {
                switch (m.Msg)
                {
                    case 0x201: // WM_LBUTTONDOWN
                        int clickX = (short) m.LParam.ToInt32();
                        //var offX = ClientRectangle.Right - ClientRectangle.Left;
                        var offX = Width / 2;

                        var clickY = m.LParam.ToInt32() >> 16;
                        //var offY = ClientRectangle.Bottom - ClientRectangle.Top;
                        var offY = Height / 2;

                        Scroll(offX - clickX, clickY - offY);
                        break;
                    case 0x101: // WM_KEYUP
                        if (m.WParam.ToInt32() == 32)
                            return;
                        break;
                    case 0x200: // WM_MOUSEMOVE
                        mouseX = (short) m.LParam.ToInt32();
                        mouseY = m.LParam.ToInt32() >> 16;
                        Invalidate();
                        break;
                }
            }
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A)
            {
                Scroll(5, 0);
            }

            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D)
            {
                Scroll(-5, 0);
            }

            if (e.KeyCode == Keys.Up || e.KeyCode == Keys.W)
            {
                Scroll(0, -5);
            }

            if (e.KeyCode == Keys.Down || e.KeyCode == Keys.S)
            {
                Scroll(0, 5);
            }

            base.OnPreviewKeyDown(e);
        }

        protected override void OnResize(EventArgs e)
        {
            baseX = Width / 2.0f;
            baseY = Height / 2.0f;

            Invalidate();

            base.OnResize(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(Color.Black);

            e.Graphics.DrawString("FPS: 0\t\n" +
                                  "Vehicles: 0\n" +
                                  "TCSSend: 0%\n" +
                                  "TCSSend/s: 0.0\n" +
                                  "Session: 0\n" +
                                  $"Pos({XToWorld(mouseX):0000.0}, {YToWorld(mouseY):0000.0})\n" +
                                  "Wheel(0)\n", Font, new SolidBrush(Color.Blue),
                new RectangleF(50.0f, 590.0f, 200.0f, 130.0f));

            DrawGrid(e.Graphics);

            foreach (var node in _tcs.Nodes)
                e.Graphics.DrawEllipse(new Pen(Color.LimeGreen), XToScreen(node.Position.X), YToScreen(node.Position.Y), 10, 10);
        }

        private void DrawGrid(Graphics graphics)
        {
            for (var x = -15000.0f; x <= 15000.0f; x = x + 200.0f)
                graphics.DrawLine(new Pen(Color.Blue), XToScreen(x), YToScreen(-2500.0f), XToScreen(x),
                    YToScreen(2500.0f));
            for (var y = -15000.0f; y <= 15000.0f; y = y + 200.0f)
                graphics.DrawLine(new Pen(Color.Blue), XToScreen(-5000.0f), YToScreen(y), XToScreen(5000.0f),
                    YToScreen(y));
        }

        private new void Scale(float delta)
        {
            scale = delta * scale + scale;

            Invalidate();
        }

        private new void Scroll(int dX, int dY)
        {
            offsetX = dX * scale + offsetX;
            offsetY = dY * scale + offsetY;

            Invalidate();
        }

        protected void Center(int centerX, int centerY)
        {
            baseX = centerX;
            baseY = centerY;
        }

        public Vector2 ToScreen(Vector2 pos)
        {
            return new Vector2(XToScreen(pos.X), YToScreen(pos.Y));
        }

        public float XToScreen(float x)
        {
            return (x + offsetX) / scale + baseX;
        }

        public float YToScreen(float y)
        {
            return baseY - (y + offsetY) / scale;
        }

        public float XToWorld(float x)
        {
            return (x - baseX) * scale - offsetX;
        }

        public float YToWorld(float y)
        {
            return (baseY - y) * scale - offsetY;
        }
    }
}