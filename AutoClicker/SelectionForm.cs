using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AutoClicker
{
    public partial class SelectionForm : Form
    {
        public bool LeftButtonDown = false;
        public bool RectangleDrawn = false;
        public bool ReadyToDrag = false;

        public Point ClickPoint = new Point();
        public Point CurrentTopLeft = new Point();
        public Point CurrentBottomRight = new Point();
        public Point DragClickRelative = new Point();

        public int RectangleHeight = new int();
        public int RectangleWidth = new int();

        Pen RedPen = new Pen(Color.Red, 2);
        SolidBrush TransparentBrush = new SolidBrush(Color.White);
        Pen EraserPen = new Pen(Color.FromArgb(051, 153, 255), 2);
        SolidBrush EraserBrush = new SolidBrush(Color.FromArgb(051, 153, 255));
        Pen BorderPen = new Pen(Color.Green, 1);

        private MainForm instanceRef = null;
        public MainForm InstanceRef
        {
            get
            {
                return instanceRef;
            }
            set
            {
                instanceRef = value;
            }
        }

        bool initPaint;
        Graphics formGraphics;
        Bitmap bmpScrren;
        Bitmap bmpCanvas;
        Graphics gc;

        // 要让窗体在载入的时候就显示图像必须重写OnPaint方法
        protected override void OnPaint(PaintEventArgs e) {
            //Graphics g = e.Graphics;
            //g.Clear(this.BackColor);

            if (!initPaint) {
                initPaint = true;

                bmpCanvas = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                gc = Graphics.FromImage(bmpCanvas);  //创建画笔

                this.Opacity = 0.00;

                Graphics g = e.Graphics;
                InitCanvas(g);

                bmpScrren = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
                Graphics g2 = Graphics.FromImage(bmpScrren);  //创建画笔
                g2.CopyFromScreen(new Point(0, 0), new Point(0, 0), bmpScrren.Size);//截屏
                g2.Dispose();//关闭画笔

                RefreshCanvas();

                this.Opacity = 1.00;
            }
        }

        public SelectionForm(MainForm instanceRef)
        {
            InitializeComponent();

            //激活窗体的双缓冲技术,可以注释掉看看是什么效果
            //this.SetStyle(ControlStyles.OptimizedDoubleBuffer, true);   // 双缓冲
            //this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);    // 禁止擦除背景.
            this.SetStyle(ControlStyles.ResizeRedraw, true);
            this.SetStyle(ControlStyles.UserPaint, true);

            InstanceRef = instanceRef;
            this.MouseDown += HandleMouseClick;
            this.MouseMove += HandleMouseMove;
            this.MouseUp += HandleMouseUp;

            this.Left = 0;
            this.Top = 0;
            var width = 0;
            var height = 0;
            var screens = Screen.AllScreens;
            foreach (var screen in screens)
            {
                if (screen.Bounds.Height > height)
                {
                    height = screen.Bounds.Height;
                }
                width += screen.Bounds.Width;
            }

            this.Width = width;
            this.Height = height;

            formGraphics = this.CreateGraphics();
        }

        private void HandleMouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ClickPoint = new Point(MousePosition.X, MousePosition.Y);
                LeftButtonDown = true;
            }
        }

        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            instanceRef.SendRectangle(CurrentTopLeft.X, CurrentTopLeft.Y, CurrentBottomRight.X - CurrentTopLeft.X, CurrentBottomRight.Y - CurrentTopLeft.Y);
            this.Close();
        }

        private void HandleMouseMove(object sender, MouseEventArgs e)
        {
            if (LeftButtonDown)
            {
                DrawSelection();
            }
        }

        private void DrawSelection()
        {
            RefreshCanvas();

            //Erase the previous rectangle
            gc.DrawRectangle(EraserPen, CurrentTopLeft.X, CurrentTopLeft.Y, CurrentBottomRight.X - CurrentTopLeft.X, CurrentBottomRight.Y - CurrentTopLeft.Y);

            //Calculate X Coordinates
            if (Cursor.Position.X < ClickPoint.X)
            {
                CurrentTopLeft.X = Cursor.Position.X;
                CurrentBottomRight.X = ClickPoint.X;
            }
            else
            {
                CurrentTopLeft.X = ClickPoint.X;
                CurrentBottomRight.X = Cursor.Position.X;
            }

            //Calculate Y Coordinates
            if (Cursor.Position.Y < ClickPoint.Y)
            {
                CurrentTopLeft.Y = Cursor.Position.Y;
                CurrentBottomRight.Y = ClickPoint.Y;
            }
            else
            {
                CurrentTopLeft.Y = ClickPoint.Y;
                CurrentBottomRight.Y = Cursor.Position.Y;
            }

            //Draw a new rectangle
            gc.DrawRectangle(RedPen, CurrentTopLeft.X, CurrentTopLeft.Y, CurrentBottomRight.X - CurrentTopLeft.X, CurrentBottomRight.Y - CurrentTopLeft.Y);

            DrawToForm();
        }

        private void InitCanvas(Graphics g) {
            System.Drawing.Size screenSize = new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            g.CopyFromScreen(new Point(0, 0), new Point(0, 0), screenSize);//截屏

            g.DrawRectangle(BorderPen, 0, 0, screenSize.Width, screenSize.Height);

            //g.Dispose();
        }

        private void RefreshCanvas() {
            RefreshCanvas(gc);
        }
        private void RefreshCanvas(Graphics g) {
            // 获取这块内存画布的Graphics引用：
            //Graphics gc = Graphics.FromImage(this.bmpCanvas);
            // 在这块内存画布上绘图：
            g.DrawImage(this.bmpScrren, Point.Empty);
            g.DrawRectangle(BorderPen, 0, 0, this.bmpScrren.Width - BorderPen.Width, this.bmpScrren.Height -BorderPen.Width);
        }

        private void DrawToForm() {
            formGraphics.DrawImage(bmpCanvas, 0, 0);
        }

    
        private void CopyFromScreen_old() {
            this.Opacity = 0.00;

            Bitmap bmp2 = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Graphics g2 = Graphics.FromImage(bmp2);  //创建画笔
            g2.CopyFromScreen(new Point(0, 0), new Point(0, 0), bmp2.Size);//截屏
            g2.Dispose();//关闭画笔
            //bmp2.Dispose();//关闭对象
            this.BackgroundImage = bmp2;

            this.Opacity = 1.00;
        }
        private void RepaintScreen(Graphics g) {
            //Bitmap bmp2 = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            //System.Drawing.Size screenSize = new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            //g.CopyFromScreen(new Point(0, 0), new Point(0, 0), screenSize);//截屏
            g.DrawImage(bmpScrren, new Point(0, 0));
        }
    }
}
