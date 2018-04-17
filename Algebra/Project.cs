using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;



    namespace Algebra
{
    public class Project
    {
        public string func_str = @"
   public static float <name>(float x)
    {
      return (float)(<function>);
    }
        //---SAID IDBELLA---\\
";

        
        


public string assign_seg = @"
  public static Segment  <name> = new Segment(  <val> ,  ""<name>"" , ref segments);
";

        //----------------------------------------------
        public string script  = @" 
 using System;
 using System.Collections.Generic;

   public static class Calculator
    {

public static bool AddPoint(float x,float yy,string name)
{
Point q = new Point(x,yy,name,ref points);
return true;
}

public static List<Point> points = new List<Point>();
public static List<Segment> segments = new List<Segment>();

          


//--------------Function Area-------------

            <Functions>

//----------------------------------------


   public  static float xof(string pt)
    { 
       foreach(Point p in points)
        {
            if(p.name==pt)
               return p.x; 
        }
      return 0;
    }

      //------------------\\

   public  static float y(string pt)
    { 
       foreach(Point p in points)
        {
            if(p.name==pt)
               return p.y; 
        }
      return 0;
    }

      //------------------\\

  public static void Repos(string pt,float[] value)
  { 
     foreach(Point p in points)
      {
       if(p.name==pt)
         {
            p.x = value[0];
            p.y = value[1];
            break;
         }
      }
  }


}
     public class Point
    {
       public Point(float x,float y,string name,ref List<Point> ls)
        {
            this.x = x;
            this.y = y;
            this.name = name;
            ls.Add(this);
        }
            public Point() { }
            public float x, y;
            public string name;
    }
        public class Segment
        {
        public Segment(float val , string na,ref List<Segment> ls)
        {
            value = val;
            name = na;
            ls.Add(this);
        }
          public Segment() { }
            public float value;
            public string name;
        }



";
        
        public Type type;

        public View view = new View();

        public Main main;

        public List<oPoint> points = new List<oPoint>();

        public List<Function> functions = new List<Function>();

        public List<Segment> segments = new List<Segment>();

        public List<Cercle> cercles = new List<Cercle>();

        public List<Curve> curves = new List<Curve>();

        public List<HandCurve> handcurves = new List<HandCurve>();

        public List<Bezier> beziers = new List<Bezier>();

        public List<Line> lines = new List<Line>();

        public List<object> items = new List<object>() ;

        public bool refresh;

        public object Task=null;
       
        public string AddPoint(PointF e,bool real=false,string x=null,string y=null,string name=null)
        {
            var pts = points.FindAll(pt => pt.Properties.Visible == true);
           
            if(!real)
            foreach (var pt in pts)
            {
                    
                var pt_pos = main.ConvertToReal(pt.Position(main));
                if (main.Distance(pt_pos, e) < 5)
                {
                    name = pt.Properties.Name;
                }
            }
            if (!real&&name != null)
            {
                return name;
            }
            else
            {
                oPoint point = new oPoint();
                point.Properties.Name = name!=null?name: GenerateName("pt");
                var pnt = new Point((int)e.X, (int)e.Y);
                var ps = main.Convert(pnt);
                ps = main.SnapToGrid(ps, pnt);
                point.Pos = real?e:ps;
                point.X = x;
                point.Y = y;
                var rela = functions.Find(f => f.Properties.Selected);
                if (rela != null)
                    point.RelativeTo = rela.Properties.Name;
                point.Properties.Size = 8;
                points.Add(point);
               
                return point.Properties.Name;
            }
           
        }

        public void AddSegment(Point e,string[] points=null,string name=null)
        {
            int index = segments.Count-1;
            var mode2 = points == null;
            if (mode2 && index >= 0 && segments[index].End == null)
            {
                segments[index].End = AddPoint(e);
                refresh = false;
                segments[index].Properties.CreationMode = false;
                
            }
            else
            { 
                Segment seg = new Segment();
                seg.Properties.Name = name==null ?GenerateName("pt"):name;
                seg.Start = mode2?AddPoint(e):points[0];
                seg.End = mode2 ? null : points[1];
                seg.Properties.CreationMode = mode2 ? true : false;
                segments.Add(seg);
                if( mode2) refresh = true;
            }
        }

        public void AddCurve(Point e)
        {
            var index = curves.Count - 1;
            if(index>=0&&!curves[index].finished)
            {
                var cu = curves[index];
                var name = AddPoint(e);
                if(name!=cu.points[cu.points.Count-1])
                if (curves[index].points[0] == name)
                {
                    curves[index].closed = true;
                    curves[index].finished = true;
                    refresh = false;
                }else
                {
                    curves[index].points.Add(name);
                }
            }
            else
            {
                Curve curve = new Curve();
                curve.Properties.Name = GenerateName();
                curve.points.Add(AddPoint(e));
                curves.Add(curve);
                refresh = true;
            }
        }

        public void AddBezier(Point e)
        {
            var index = beziers.Count - 1;
            if (index >= 0 && beziers[index].Properties.CreationMode)
            {
                 var cu = beziers[index];
                var name = AddPoint(e);
               
                    beziers[index].points.Add(name);
                if (beziers[index].points.Count >= 4)
                {

                    cu.Properties.CreationMode=false;
                    refresh = false;
                }
            }
            else
            {
                Bezier bezier = new Bezier();
                bezier.Properties.Name = GenerateName();
                bezier.points.Add(AddPoint(e));
                bezier.Properties.CreationMode = true;
                beziers.Add(bezier);
                refresh = true;
            }
        }

        public void AddLine(Point e,string[] points=null,bool parallel=false,bool perpendicular=false)
        {
            object d = lines.Find(ln => ln.Properties.Selected);
            if(d==null)
            d = segments.Find(ln => ln.Properties.Selected);

            var optionB =points == null;
            var index = lines.Count - 1;
            if(optionB &&index>=0&&lines[index].End==null)
            {
                lines[index].End = AddPoint(e);
                refresh = false;
                lines[index].Properties.CreationMode = false;
            }
            
            else
            {
                if ((d==null) &&( parallel||perpendicular))
                    goto exit;
                Line line = new Line();
                line.Properties.Name = GenerateName();
                string nm = "";
                try
                {
                    var prp = d.GetType().GetField("Properties").GetValue(d);
                    nm = (string)prp.GetType().GetField("Name").GetValue(prp);
                }
                catch { }
                line.Start = !(parallel||perpendicular)? (optionB ? AddPoint(e) : points[0]):nm;

                line.End = optionB ? null : points[1];
                line.preview = view.MousePosition;
                refresh = optionB?true:refresh;
                line.Properties.CreationMode = true;
                line.parallel = parallel;
                line.perpendicular = perpendicular;
                lines.Add(line);
            }
            exit:;

        }

        public void AddCercle(Point e,bool fix)
        {
           
            if (!fix)
            {
               
                int index = cercles.Count - 1;
                if (index >= 0 && !cercles[index].fix && cercles[index].R == null)
                {
                    var R = AddPoint(e);
                    if (R == cercles[index].O)
                    {
                        cercles.RemoveAt(index);
                    }
                    else
                    {
                        cercles[index].R = AddPoint(e);
                        cercles[index].Properties.CreationMode = false;
                        refresh = false;
                    }
                }
                else
                {
                   
                    Cercle cer = new Cercle();
                    cer.Properties.Name = GenerateName("pt");
                    cer.O = AddPoint(e);
                    cer.Properties.CreationMode = true;
                    cercles.Add(cer);
                    refresh = true;
                }
            }
            else
            {
                inputbox i = new inputbox();
                i.project = this;
                i.main = main;
                i.name = AddPoint(e);
                main.Enabled = false;
                i.Show();
            }

        }
        
        public void AddHandCurve(Point e)
        {
            var index = handcurves.Count - 1;
            if (index >= 0 && !handcurves[index].finished)
            {
                handcurves[index].list.Add(main.Convert(e));
               

            } else
            { 

                HandCurve hnd = new HandCurve();
                hnd.Properties.Name = GenerateName();
                hnd.list.Add(main.Convert(e));
               
                handcurves.Add(hnd);
            }
        }

        public bool task(object o,object resume=null)
        {
            List<object> ls = new List<object>() { points, segments };
            bool result = false;

            if (o == null || o.GetType() != typeof(Segment))
            {
                var sg = segments.Find(seg => seg.End == null);
                if (sg != null)
                {
                    segments.Remove(sg);
                    result = true;
                }
            }
            if (o == null || o.GetType() != typeof(Curve))
            {
                var cu = curves.Find(cur => cur.finished==false);
                if (cu!= null)
                {
                    cu.finished = true;
                    result = true;
                }
            }
            if (o == null || o.GetType() != typeof(Line))
            {
                var l = lines.Find(ln => ln.End== null);
                if (l != null)
                {
                    lines.Remove(l);
                    result = true;
                }
            }
            if (o == null || o.GetType() != typeof(Cercle))
            {
                var c = cercles.Find(ln => (!ln.fix&&ln.R == null));
                if (c != null)
                {
                    cercles.Remove(c);
                    result = true;
                }
            }
            if (o == null || o.GetType() != typeof(Bezier))
            {
                var c = beziers.Find(ln => ln.Properties.CreationMode == true);
                if (c != null)
                {
                    beziers.Remove(c);
                    result = true;
                }
            }

            Task = resume==null? o :resume;
            return result;
        }

        public void initialize()
        {
            
            items = new List<object>() {functions,points,curves,cercles,beziers,lines,segments,handcurves };
        }
        
        //
        public List<string> point_names = new List<string>() { "a", "b", "c", "d", "e", "i", "k", "l", "m", "n", "o", "u", "v", "w", "z" };

        public List<string> functions_names = new List<string>() { "f", "g", "h", "p", "q", "r", "s", "t" };

        public int point_index = 0, function_index = 0, point_row = 0, function_row = 0;

        public List<string> available_points = new List<string>();

        public List<string> available_functions = new List<string>();

        public string GenerateName(string type = "pt")
        {

            if (available_points.Count != 0 || available_functions.Count != 0)
            {
                if (type == "pt")
                {
                    var name = available_points[0];
                    available_points.RemoveAt(0);
                    return name;
                }
                else
                {
                    var name = available_functions[0];
                    available_functions.RemoveAt(0);
                    return name;
                }
            }
            else
            {
                if (type == "pt")
                {
                    if (point_index >= point_names.Count)
                    {
                        point_index = 0;
                        point_row++;
                    }
                    var extra = "";
                    if (point_row != 0) extra += point_row;
                    return point_names[point_index++].ToUpper() + extra;
                }
                else
                {
                    if (function_index >= functions_names.Count)
                    {
                        function_index = 0;
                        function_row++;
                    }
                    var extra = "";
                    if (function_row != 0) extra += function_row;
                    return functions_names[function_index++] + extra;
                }
            }

        }

        public void NameRemouved(string name)
        {
            if (point_names.IndexOf(name) >= 0 && available_points.IndexOf(name) == -1)
                available_points.Add(name);
            else if (functions_names.IndexOf(name) >= 0 && available_functions.IndexOf(name) == -1)
                available_functions.Add(name);
        }
        //

    }

   public class View
    {

        public double Zoom = 1;
        public Point  Zero;
        public int x, y, dx, dy,val=100;
        public PointF MinValue,
                      MaxValue;
        public PointF Assist;
        public Size size, WindowSize;
        public bool grid=true, axes=true;
        public Point MousePosition;

        public List<Grid_Snap> SnapedToLines = new List<Grid_Snap>();

        public void ConfigView(Point e = new Point())
        {
            if (!e.IsEmpty)
            {
                x = e.X - dx;
                y = e.Y - dy;
            }
            Zero = new Point(size.Width / 2 + x, y + size.Height / 2);
            MinValue = new PointF((float)(-Zero.X / (Zoom * val)), (float)((-size.Height + Zero.Y) / (Zoom * val)));
            MaxValue = new PointF((float)((size.Width - Zero.X) / (Zoom * val)), (float)(Zero.Y / (Zoom * val)));
            Assist = new PointF((float)(-Zero.Y / (Zoom * val)), (float)((size.Height - Zero.Y) / (Zoom * val)));

        }
    }















}





