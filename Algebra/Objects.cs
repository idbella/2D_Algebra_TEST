using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace Algebra
{
    public class ObjectProperties
    {
        public string Name;
        public bool  Visible = true;
        public bool  Selected,Tmp_Selected;
        public float Size=2f;
        public Color Color=Color.Black;
        public bool CreationMode;
        
    }
    public class images
    {
        public Bitmap point = Properties.Resources.point;
        public Bitmap cercle = Properties.Resources.cercle;
        public Bitmap curve = Properties.Resources.curve;
        public Bitmap segment = Properties.Resources.segment;
        public Bitmap perpendicular = Properties.Resources.perpendicular;
        public Bitmap parallel = Properties.Resources.parallel;
        public Bitmap line = Properties.Resources.line;
        public Bitmap select = Properties.Resources.select;
    }

    public class oPoint
    {

        public PointF Pos=new PointF();
        public ObjectProperties Properties = new ObjectProperties() { Color = Color.Blue};
        public string X, Y;
        public string RelativeTo;
        public PointF Position(Main main)
        {
            
            PointF ret = Pos;
            if(X!=null)
            {
                     ret.X = main.FindPoint(X).Position(main).X;
            }

            if (Y != null)
            {
                ret.Y = new Main().FindPoint(Y).Position(main).Y;
            }
            return ret;
        }
        public string Display(Main main)
        {
            var p = Position(main);
            return Properties.Name + " = (" + Math.Round(p.X,4) + "," + Math.Round(p.Y,4) + ")";
        }
    }

    public class Segment
    {
        public string Start,End;
        public ObjectProperties Properties = new ObjectProperties();
        public string Display(Main main)
        {
            return Properties.Name + " = "+Math.Round(Lenght(main),4);
        }
        public double Lenght(Main main)
        {
            var a = main.FindPoint(Start).Position(main);
            var b = main.FindPoint(End).Position(main);
            return main.Distance(a, b);
        }
    }

    public class Cercle
    {
        public ObjectProperties get() { return Properties; }
        public string O;
        public string R;
        public bool fix;
        public double value;
        public double r(Main main)
        {
            if (fix) return value;
            var o = main.FindPoint(O).Position(main);
            var rr = main.FindPoint(R).Position(main);
            return main.Distance(o, rr);
        }
        public ObjectProperties Properties = new ObjectProperties();

        public string Display(Main main)
        {
            var o = main.FindPoint(O).Position(main);
            
            return Properties.Name + " : (x-" + o.X + ")²+(y-" + o.Y + ")² = "+Math.Pow(r(main),2);
        }
    }
    
    public class HandCurve
    {
        public bool finished=false;
        public ObjectProperties Properties = new ObjectProperties();
        public List<PointF> list = new List<PointF>();
    }

    public class Curve
    {

        public List<string> points = new List<string>();
        public bool closed;
        public bool finished;
        public ObjectProperties Properties = new ObjectProperties();
        public List<Point> GetPoints(Main main)
        {
            List<Point> pn = new List<Point>();
            foreach (var y in points)
            {
                pn.Add(main.ConvertToReal(main.FindPoint(y).Position(main)));
            }
            return pn;
        }
        public string Display(Main main)
        {
            return "curve "+Properties.Name;
        }
        
    }

    public class Bezier
    {

        public List<string> points = new List<string>();
        public bool finished;
        public ObjectProperties Properties = new ObjectProperties();
        public List<Point> GetPoints(Main main)
        {
            List<Point> pn = new List<Point>();
            
            foreach (var y in points)
            {
                pn.Add(main.ConvertToReal(main.FindPoint(y).Position(main)));
            }
            return pn;
        }
        public string Display(Main main)
        {
            return "bezier " + Properties.Name;
        }
    }

    public class Line
    {

        public string Start, End;
        public PointF preview=new PointF();
        public ObjectProperties Properties = new ObjectProperties();
        public bool parallel;
        public bool perpendicular;
        public string display="fsd";
        public string Display(Main main)
        {
            if (!(parallel||perpendicular))
            {
                var res = main.Equation(Start, End, 0);
                return "line " + Properties.Name + " : " + (res.inf ? "x" : "y") + "=" + (res.inf ? res.pos + "" : res.function);
            }
            
            return "dis = "+display;
        }
        
    }

    public class Result
    {

       public  PointF value;
       public  bool inf;
       public  float pos;
       public string function;
       public float a;
    }

    public class Grid_Snap
    {
        public int x, y;
        public double x_value, y_value;
    }

    public class Function
    {

        public ObjectProperties Properties = new ObjectProperties();
        public string function;
        public string Display(Main main)
        {
            return Properties.Name+"(x) = "+main.parse(function," ");
        }
    }

    public class  xSlider : Main
    {

        Bitmap bg = Properties.Resources.sliderBg;
       
        Bitmap sb = Properties.Resources.sliderBtn;
        
        public  xSlider(PictureBox pic)
        {
            pic.MouseDown += MouseDown;
            pic.MouseUp += MouseUp;
            pic.MouseMove+= MouseMove;
            pic.Paint += SliderPaint;

            bg = new Bitmap(bg, 100, bg.Height);
            sb = new Bitmap(sb, sb.Size);

            pic.Invalidate();
        }
        
        public int Value { get; set; }

        void SliderPaint(object o, PaintEventArgs e)
        {
            e.Graphics.DrawImage(bg, 0, 0);
            e.Graphics.DrawImage(bg, 10, 0);
            e.Graphics.DrawImage(sb, (int)Clamp(Value - 4, -4, 94), 0);
        }

        bool Down;

        new void MouseMove(object sender, MouseEventArgs e)
        {
            if (Down)
            {
                Value = (int)Clamp(e.Location.X, 0, 100);
                (sender as PictureBox).Invalidate();
            }
        }

        new void MouseUp(object o, MouseEventArgs e){  Down = false;  }

        new void MouseDown(object o, MouseEventArgs e)
        {
            Down = true;
            Value = e.X;
            (o as PictureBox).Invalidate();
        }


    }


}




