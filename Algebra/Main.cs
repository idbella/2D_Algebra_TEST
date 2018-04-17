
using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace Algebra
{
    public partial class Main : Form
    {

        bool drag, drag_point;
        public bool handmode;
        public Project project = new Project();
        List<oPoint> points = new List<oPoint>();
        View view;
        xSlider sliders;
        CodeDomProvider provider;
        public Main()
        {
            
            provider = CodeDomProvider.CreateProvider("C#");
            InitializeComponent();
            Resize += OnSize;
            container.SplitterMoved += OnSize;
            Canavas.Paint += Canavas_Paint;
            Canavas.MouseDown += Canavas_MouseDown;
            Canavas.MouseUp += Canavas_MouseUp;
            Canavas.MouseWheel += Canavas_MouseWheel;
            Canavas.MouseMove += Canavas_MouseMove;
            Canavas.MouseEnter += Canavas_MouseEnter;
            input.KeyDown += Input_KeyDown;
            KeyDown += Main_KeyDown;

            Point_btn.Tag = new oPoint();
            segment_btn.Tag = new Segment();
            cercle_btn.Tag = new Cercle();
            cercle_btn2.Tag = new Cercle() { fix = true };
            curve_btn.Tag = new Curve();
            bezier_btn.Tag = new Bezier();
            line_btn.Tag = new Line();
            handcurve.Tag = new HandCurve();
            line_btn2.Tag = new Line() { parallel = true };
            line_btn3.Tag = new Line() { perpendicular = true };
            Select_btn.Tag = null;
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == 13)
            {
                var str = input.Text;

                Function fs = new Function();
                fs.function = parse(str);
                fs.Properties.Name = project.GenerateName("n");
                project.functions.Add(fs);
                if (Execute())
                {
                    Canavas.Invalidate();
                    RefreshList();
                }
                else
                {
                    project.NameRemouved(fs.Properties.Name);
                    project.functions.Remove(fs);
                }
            }
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {

        }

        void Canavas_MouseEnter(object sender, EventArgs e)
        {
            Canavas.Focus();
        }

        void Canavas_MouseMove(object sender, MouseEventArgs e)
        {
             bool invalidate = false;

            if (drag_point)
            {
                invalidate = DragPoint(e.Location);
            }
            else if (handmode)
            {

                Task(e.Location);

            }
            else
            {


                if (drag)
                {
                    Cursor.Current = Cursors.NoMove2D;
                    view.ConfigView(e.Location);
                    invalidate = true;

                }
                else
                {
                    bool nothing_here = true;
                    try
                    {
                        invalidate = ScanArea(e.Location, true, ref nothing_here, invalidate);
                    }
                    catch { }

                    if (project.Task != null)
                    {
                        if (project.refresh)
                        {
                            view.MousePosition = e.Location;
                            invalidate = true;
                        }
                        else
                        {
                            Cursor.Current = Cursors.Cross;
                        }
                    }
                }
            }
            if (invalidate) Canavas.Invalidate();



        }

        void Canavas_MouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {
                project.task(null, project.Task);
                Canavas.Invalidate();
            }
            else
            {

                bool invalidate = false;
                try
                {
                    invalidate = ScanArea(e.Location, false, ref drag_point, invalidate);
                }
                catch { }
                if (project.Task != null)
                {
                    drag_point = Task(e.Location);
                    invalidate = true;
                    try
                    {
                        RefreshList();
                    }
                    catch(Exception e) { }
                }
                else if (!drag_point)
                {
                    view.dx = e.X - view.x;
                    view.dy = e.Y - view.y;
                    drag = true;
                }
                if (invalidate) Canavas.Invalidate();
            }

        }

        void Canavas_MouseWheel(object sender, MouseEventArgs e)
        {

            bool up = e.Delta > 0;

            view.Zoom *= up ? 1.05 : 0.95;

            view.ConfigView();


            Canavas.Invalidate();
        }

        void Canavas_MouseUp(object sender, MouseEventArgs e)
        {
            drag = false;
            if (handmode)
            {
                handmode = false;
                project.handcurves[project.handcurves.Count - 1].finished = true;
            }

            drag_point = false;

        }

        void Canavas_Paint(object sender, PaintEventArgs e)
        {

            try
            {

                view.SnapedToLines = new List<Grid_Snap>();
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                DrawGrid(e.Graphics);
                DrawSegments(e.Graphics);
                DrawCercles(e.Graphics);
                DrawCuves(e.Graphics);
                DrawLines(e.Graphics);
                DrawPoints(e.Graphics);
                DrawBeziers(e.Graphics);
                HandCurve(e.Graphics);

                ; DrawFunctions(e.Graphics);
            }
            catch (Exception r) { MessageBox.Show(r.StackTrace); }
        }

        void OnLoad(object o, EventArgs e)
        {
            view = project.view;
            project.main = this;
            points = project.points;
            OnSize(o, e);
            project.initialize();
            selectbutton(Select_btn);

            sliders = new xSlider(SliderContainer);
            sliders.Click += Sliders_Click;

        }

        private void Sliders_Click(object sender, EventArgs e)
        {
            MessageBox.Show(sender.ToString());
        }

        void OnSize(object o, EventArgs e)
        {
            var spliter = container.SplitterDistance;
            container.Width = ClientSize.Width;
            container.Height = ClientSize.Height - btm_panel.Height - toolbar.Height;
            toolbar.Width = ClientSize.Width;
            Canavas.Size = new Size(container.Panel2.ClientSize.Width, container.Panel2.ClientSize.Height - properties_panel.Height);
            Canavas.Location = new Point(0, properties_panel.Height);
            properties_panel.Width = Canavas.Width;
            btm_panel.Width = ClientSize.Width;
            input.Width = btm_panel.Width - input.Location.X - 50;
            info.Location = new Point(btm_panel.Width - info.Width - 10, 5);
            btm_panel.Location = new Point(0, container.ClientRectangle.Bottom + toolbar.Height);
            container.SplitterDistance = spliter;
            project.view.size = Canavas.Size;
            project.view.WindowSize = ClientSize;
            project.view.ConfigView();
            textBox2.Location = Canavas.Location;
            textBox2.Size = Canavas.Size;
            Canavas.Invalidate();

        }

        void info_Click(object sender, EventArgs e)
        {

        }

        void ToolButton(object o, EventArgs e)
        {
            if (project.task((o as Button).Tag))
            {
                Canavas.Invalidate();
                RefreshList();
            }
            selectbutton(o as Button);

        }

        void selectbutton(Button o)
        {
            try
            {
                var bg = (Image)Properties.Resources.bg.Clone();
                var g = Graphics.FromImage(bg);
                Rectangle rect = new Rectangle(new Point(1, 1), new Size(39, 39));
                Brush f = Brushes.DarkBlue;

                Image im;
                foreach (var b in toolbar.Controls)
                {
                    var btn = (b as Button);
                    try
                    {
                        var img = (Image)bg.Clone();
                        im =  (Image) new dynamic(new images()).Get(btn.Text).Value;
                        Graphics.FromImage(img).DrawImage(im, 6, 6);
                        btn.BackgroundImage = img;
                    }
                    catch { }
                }
                im = (Image)new dynamic(new images()).Get(o.Text).Value;
                g.FillRectangle(f, rect);
                g.DrawImage(im, 6, 6);
                o.BackgroundImage = bg;
            }
            catch { }

        }

        public void CheckBox(object o, EventArgs e)
        {
            List(o);
            Canavas.Invalidate();
        }

        public void ObjectTextEnter(object o, EventArgs e)
        {
            List(o, container.Panel1, true);
            Canavas.Invalidate();
        }

        public void ObjectTextLeave(object o, EventArgs e)
        {
            List(o, container.Panel1, true, true);
            Canavas.Invalidate();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            
        }

        object FindObject(string name)
        {
            foreach (var item in project.items)
            {
                object[] arr = (object[])item.GetType().GetMethod("ToArray").Invoke(item, null);
                if (arr != null)
                {
                    foreach (var items in arr)
                    {
                        try
                        {
                            var prp = items.GetType().GetField("Properties").GetValue(items);
                            var nm = (string)prp.GetType().GetField("Name").GetValue(prp);
                            if (name == nm)
                                return items;
                        }
                        catch { }
                    }
                }
            }
            return null;
        }

        private void label2_Click(object sender, EventArgs e)
        {
            textBox2.Visible = true;
            Canavas.Enabled = false;
            textBox2.Text = source;
        }
        string source;
        private void label4_Click(object sender, EventArgs e)
        {
            textBox2.Visible = true;
            Canavas.Enabled = false;
            textBox2.Text = project.script;
        }

        private void label5_Click(object sender, EventArgs e)
        {
            textBox2.Visible = false;
            Canavas.Enabled = true;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            project.script = textBox2.Text;
        }

        public void print(object data)
        {

            MessageBox.Show(data == null ? "null" : data.ToString());
        }
        public void printf(object data)
        {
            Debug.Print(data == null ? "null" : data.ToString());
        }



    }

    public class Tool
    {
        public void to(Array o)
        {
            var u = o.GetType();
            //List<u> j = new List<u>();
        }
    }
    
    public class dynamic 
    {

        object o { get; }

        bool type;
        Type tp;
        public dynamic(object o)
        {
            this.o = o; 
            Value = o;
        }
        public dynamic(Type o)
        {
            this.tp = o;
            Value = o;
        }
        public object Value { get; }
        
        public dynamic Get(string value,object[] args=null,bool invoke=true)
        {
            try
            {
                foreach (var t in o.GetType().GetMembers())
                {
                    if (value == t.Name)
                    {
                        switch (t.MemberType.ToString())
                        {
                            case "Method": goto meth;
                            case "Property": goto prop;
                            case "Field": goto field;
                            default: goto exit;
                        }
                        
                    }
                }
              
                prop:;
                var ob1 = o.GetType().GetProperty(value);
                    return new dynamic(ob1.GetValue(o, args));


                field:;
                var ob2 = o.GetType().GetField(value);
                    return new dynamic(ob2.GetValue(o));

                meth:;
                var ob3 = o.GetType().GetMethod(value);
                    return new dynamic(ob3.Invoke(invoke ? o : null, args));

            }
            catch{ }
            exit:;
            return null;
        }

        public bool  Set(string value, object data)
        {
            try
            {
                var ob1 = o.GetType().GetProperty(value);
                var ob2 = o.GetType().GetField(value);
                if (ob1 != null)
                {
                    ob1.SetValue(o, data, new object[0]);
                    return true;
                }

                else if (ob2 != null)
                {
                    ob2.SetValue(o, data);
                    return true;
                }

            }
            catch (Exception r){ MessageBox.Show(r.Message + r.StackTrace); }

            return false;
        }

    }
    
}

    







