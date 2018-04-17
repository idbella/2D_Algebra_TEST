using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace Algebra
{


    partial class Main
    {

        public int Better_div(int i)
        {
            
            
            double result = Math.Log(i) / Math.Log(2);

            if (Math.Round(result) == result) return i;

            return (int)(Math.Pow(2, (int)result));
        }

        public bool is_valid_point(int pt, int n)
        {
            float val = ((1 / (float)n) * pt);
            if ((int)val == val) return true;
            return false;
        }

        public double Clamp(double val, double min, double max)
        {
            if (val < min) return min;
            if (val > max) return max;
            return val;
        }

        public PointF Convert(Point Pos)
        {
            return new PointF((float)((Pos.X - view.Zero.X) / (view.Zoom * view.val)), (float)((view.Zero.Y - Pos.Y) / (view.Zoom * view.val)));
        }

        public Point ConvertToReal(PointF pos)
        {
            return new Point((int)(pos.X * view.Zoom * view.val + view.Zero.X), (int)(-pos.Y * view.Zoom * view.val + view.Zero.Y));
        }

        public oPoint FindPoint(string name)
        {
            return points.Find(pnt => pnt.Properties.Name == name);
        }

        public double Distance(PointF from, PointF to)
        {
            return Math.Abs(Math.Sqrt(Math.Pow(to.X - from.X, 2) + Math.Pow(to.Y - from.Y, 2)));
        }

        public int SelectPoint(Point pos, bool tmp)
        {
            int result = 0;
            var pts = project.points.FindAll(pt => pt.Properties.Visible == true);
            var selpts = project.points.FindAll(pt => tmp ? pt.Properties.Tmp_Selected == true : pt.Properties.Selected == true);
            foreach (var pt in selpts)
            {
                SelecObject(pt.Properties.Name, tmp, false);
                result = 1;
            }
            foreach (var pt in pts)
            {
                var pt_pos = ConvertToReal(pt.Position(this));
                if (Distance(pt_pos, pos) < pt.Properties.Size)
                {
                    SelecObject(pt.Properties.Name, tmp);
                    result = 2;
                }
            }
            return result;
        }

        public int SelectLine(Point pos, bool tmp = false, bool seg = false, int result = 0)
        {
            object pts = project.segments.FindAll(pt => (pt.Properties.Visible == true && !pt.Properties.CreationMode));
            
            if (!seg)
            {
                pts = project.lines.FindAll(pt => (pt.Properties.Visible == true && !pt.Properties.CreationMode));
                var selpts = project.lines.FindAll(pt => (tmp ? pt.Properties.Tmp_Selected == true : pt.Properties.Selected == true && !pt.Properties.CreationMode));
                foreach (var pt in selpts)
                {
                    SelecObject(pt.Properties.Name, tmp, false);
                    result = 1;
                }
            }
            foreach (var pt in (object[])new dynamic(pts).Get("ToArray").Value)
            {

                dynamic dyna = new dynamic(pt);
                var tp = dyna.Get("Properties").Value;
                dynamic dyna2 = new dynamic(tp);

                bool para = !seg && (bool) dyna.Get("parallel").Value;
                bool perp = !seg && (bool) dyna.Get("perpendicular").Value;
                
                var name = dyna2.Get("Name").Value.ToString();
                var size = (float)dyna2.Get("Size").Value;

                string  start = dyna.Get("Start").Value.ToString();
                string  End   = dyna.Get("End").Value.ToString();

                if (!seg && (para || perp))
                {
                    var lin = project.lines.Find(ln => ln.Properties.Name == start);
                    start = lin.Start;
                }
                
                var eq = Equation(start,End, Convert(pos).X);
                var pt_pos = ConvertToReal(eq.value);
                var a = FindPoint(start).Position(this);
                var b = FindPoint(End).Position(this);
                var c = Convert(pos);
                var cond = (seg ? Inside(a, b, c) : true);
                if (!eq.inf && cond)
                {
                    if (Distance(pt_pos, pos) < size)
                    {
                        SelecObject(name, tmp);
                        result = 2;
                    }
                }
                else if (cond)
                {
                    var val = ConvertToReal(new PointF(eq.pos, 0)).X;
                    var dis = Distance(new PointF(val, 0), new PointF(pos.X, 0));

                    if (dis < 5)
                    {
                        SelecObject(name, tmp);
                        result = 2;
                    }
                }

            }
            return result;
        }

        public int SelectSegment(Point pos, bool tmp = false)
        {
            int result = 0;
            var selpts = project.segments.FindAll(pt => (tmp ? pt.Properties.Tmp_Selected : pt.Properties.Selected && !pt.Properties.CreationMode));
            foreach (var pt in selpts)
            {
                SelecObject(pt.Properties.Name, tmp, false);
                result = 1;
            }
            result = SelectLine(pos, tmp, true, result);
            return result;
        }

        public int SelectCercle(Point pos, bool tmp)
        {
            int result = 0;

            var pts = project.cercles.FindAll(pt => (pt.Properties.Visible == true && !pt.Properties.CreationMode));
            var selpts = project.cercles.FindAll(pt => (tmp ? pt.Properties.Tmp_Selected == true : pt.Properties.Selected == true && !pt.Properties.CreationMode));
            foreach (var pt in selpts)
            {
                SelecObject(pt.Properties.Name, tmp, false);
                result = 1;
            }

            foreach (var pt in pts)
            {
                var o = ConvertToReal(FindPoint(pt.O).Position(this));
                double r2;
                if (!pt.fix)
                {
                    var r = ConvertToReal(FindPoint(pt.R).Position(this));

                    r2 = Distance(r, o);
                }
                else
                    r2 = view.Zoom * pt.value * view.val;


                var r1 = Distance(pos, o);



                if (Math.Abs(r1 - r2) < pt.Properties.Size / 2)
                {
                    SelecObject(pt.Properties.Name, tmp);
                    result = 2;
                }


            }
            return result;
        }

        public void SelecObject(string name, bool tmp, bool select = true)
        {
            Panel list = container.Panel1;
            var d = FindObject(name);

            var tp = d.GetType().GetField("Properties").GetValue(d);
            var nm= (string)tp.GetType().GetField("Name").GetValue(tp);
            var clr = (Color)tp.GetType().GetField("Color").GetValue(tp);
            var sel = (bool)tp.GetType().GetField("Selected").GetValue(tp);


            if (tmp) tp.GetType().GetField("Tmp_Selected").SetValue(tp, select);


            else
            {
                tp.GetType().GetField("Selected").SetValue(tp, select);
                Clr.BackColor = clr;
            }

            foreach (var b in list.Controls)
            {
                if (b.GetType() == typeof(TextBox) && (b as TextBox).Name == nm)
                {
                     sel = (bool)tp.GetType().GetField("Selected").GetValue(tp);

                    if (!sel)
                        (b as TextBox).BackColor = select ? Color.LightBlue : container.Panel1.BackColor;
                }
            }
        }

        public bool DragPoint(Point e)
        {
            var pnt = project.points.Find(pt => pt.Properties.Selected == true);
            if (pnt != null)
            {
                var name = pnt.Properties.Name;
                var ps = Convert(e);
                ps = SnapToGrid(ps, e);
                if (pnt.RelativeTo != null)
                    ps = new PointF(ps.X,(float)project.type.GetMethod(pnt.RelativeTo).Invoke(null, new object[] { ps.X }));
                pnt.Pos = ps;

                if (project.type != null)
                {
                    try
                    {
                        project.type.GetMethod("Repos").Invoke(null, new object[] { name, new float[] { ps.X, ps.Y } });

                    }
                    catch (Exception r) { Debug.Print(r.StackTrace); }

                    
                }
               var u =  container.Panel1.Controls.Find(name,false);
                if (u != null && u[0].GetType() == typeof(TextBox))
                    (u[0] as TextBox).Text = pnt.Display(this);


                foreach (var sg in project.segments.FindAll(sg => sg.Start == name || sg.End == name))
                {
                     u = container.Panel1.Controls.Find(sg.Properties.Name, false);
                    if (u != null && u[0].GetType() == typeof(TextBox))
                        (u[0] as TextBox).Text = sg.Display(this);
                }
                foreach (var sg in project.lines.FindAll(sg => sg.Start == name || sg.End == name))
                {
                    u = container.Panel1.Controls.Find(sg.Properties.Name, false);
                    if (u != null && u[0].GetType() == typeof(TextBox))
                        (u[0] as TextBox).Text = sg.Display(this);
                }
                return true;
                
            }

            return true;
        }

        public void movePoint(string name)
        {
            var step = 0.001f;
            
            var ps = FindPoint(name);
            var pos = ps.Position(this);

            var pos2 = new PointF(pos.X + step, pos.Y);
            if (ps.RelativeTo != null)
            {
                 pos2 = new PointF(pos2.X,(float)project.type.GetMethod(ps.RelativeTo).Invoke(null,new object[] { pos2.X}));
            }
            ps.Pos = pos2;

        }

        public bool Task(Point Location)
        {
            if (project.Task.GetType() == typeof(oPoint))
            {
                var name = project.AddPoint(Location);

                SelecObject(name, false);
                return true;
            }
            else if (project.Task.GetType() == typeof(Segment))
            {
                project.AddSegment(Location);
            }
            else if (project.Task.GetType() == typeof(Cercle))
            {
                project.AddCercle(Location, (project.Task as Cercle).fix);
            }
            else if (project.Task.GetType() == typeof(Curve))
            {
                project.AddCurve(Location);
            }
            else if (project.Task.GetType() == typeof(Bezier))
            {
                project.AddBezier(Location);
            }
            else if (project.Task.GetType() == typeof(Line))
            {
                var lne = (project.Task as Line);
                project.AddLine(Location, null, lne.parallel, lne.perpendicular);
            }
            else if (project.Task.GetType() == typeof(HandCurve))
            {
                project.AddHandCurve(Location);
                handmode = true;
            }
            return false;
        }

        bool ParsePoint(string query, string nm = null)
        {

            PointF loc = new PointF(1, 1);
            string x = null, y = null;
            foreach (var pt in points)
            {
                var name = pt.Properties.Name;
                if (query.Contains("x(" + name + ")"))
                {
                    x = name;
                    query = query.Replace("x(" + name + ")", "0");

                }
                else if (query.Contains("y(" + name + ")"))
                {

                    y = name;
                    query = query.Replace("y(" + name + ")", "0");
                }
            }

            query = query.Replace(")", "");
            var xy = Regex.Split(query, ",");

            try
            {
                if (x == null) loc.X = float.Parse(xy[0]);
                if (y == null) loc.Y = float.Parse(xy[1]);
                project.AddPoint(loc, true, x, y, nm);
                return true;
            }
            catch { }
            return false;
        }

        public bool Parse(string query)
        {
            query = query.Replace(" ", "");
            var nm = Regex.Split(query, Regex.Escape("("))[0];
            query = query.Replace(nm + "(", "");

            if (nm.ToLower() == "point")
            {
                ParsePoint(query);
            }

            else
            {
                var i = 0;
                if (nm.ToLower() == "segment")
                {
                    nm = null;
                    i = 1;
                }
                else if (nm.ToLower() == "line")
                {
                    i = 2;
                }
                if ((query.Contains(",") && query.Contains(")")))
                {



                    List<string> Spoints = new List<string>();
                    foreach (var pt in points)
                    {
                        var name = pt.Properties.Name;
                        if ((query.Contains(name) || query.Contains(name + ")")) && (!query.Contains("x(" + name) && !query.Contains("y(" + name)))
                        {
                            i = i == 2 ? 2 : 1;
                            Spoints.Add(name);
                        }

                    }

                    if (points.Find(pt => pt.Properties.Name == nm) == null && project.segments.Find(sg => sg.Properties.Name == nm) == null)
                    {
                        if (i == 0)
                        {
                            return ParsePoint(query, nm);

                        }
                        else if (points.Count == 2)
                        {
                            if (i == 1)
                            {
                                project.AddSegment(new Point(), Spoints.ToArray(), nm);
                                return true;
                            }
                            else if (i == 2)
                            {
                                project.AddLine(new Point(), Spoints.ToArray());
                                return true;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show(@"Name "" " + nm + @" "" Already Exist !");
                    }
                }

            }
            return false;
        }

        public PointF SnapToGrid(PointF pos, Point Location)
        {
            foreach (var t in view.SnapedToLines)
            {
                if (!(t.x == 0 && t.x_value == 0))
                    if (Math.Abs(Location.X - t.x) <= 5)
                    {
                        pos.X = (float)t.x_value;
                    }

                if (!(t.y == 0 && t.y_value == 0))
                    if (Math.Abs(Location.Y - t.y) <= 5)
                    {
                        pos.Y = (float)t.y_value;
                    }
            }
            return pos;
        }

        Image CheckBox(bool sel = true)
        {
            Pen pen = sel ? new Pen(Color.FromArgb(150, Color.Blue), 6) : new Pen(Color.FromArgb(200, Color.LightBlue), 6);
            Bitmap b = new Bitmap(12, 12);
            var g = Graphics.FromImage(b);
            g.DrawEllipse(pen, 3, 3, 5, 5);
            return b;
        }

        public void RefreshList()
        {
            Panel list = container.Panel1;
            var controls = new List<Control>();
            list.Controls.Clear();
            int sep = -15;
            foreach (object dd in project.items)
            {
                Point loc;

                if ((int)dd.GetType().GetProperty("Count").GetValue(dd, null) != 0)
                {
                    var str = ((object[])(dd.GetType().GetMethod("ToArray").Invoke(dd, null)))[0].GetType().Name;

                    controls.Add
                    (
                        new Label()
                        {
                            Text = str.Replace("oPoint", "Point") + "s",
                            Location = new Point(10, sep += 25),
                            BackColor = list.BackColor,
                            AutoSize = false,
                            Height = 15
                        }
                    );
                }
                foreach (object d in (object[])dd.GetType().GetMethod("ToArray").Invoke(dd,null))
                {
                    var prp = (ObjectProperties)d.GetType().GetField("Properties").GetValue(d);
                    
                    
                    if (prp.CreationMode)
                        continue;
                    loc = new Point(20, (sep += 20));
                    PictureBox pic = new PictureBox()
                    {
                        Location = loc,
                        Size = new Size(12, 12),
                        Tag = prp.Name,
                        Image = CheckBox(prp.Visible),
                        BackColor = list.BackColor
                    };
                    pic.MouseDown += CheckBox;


                    TextBox box = new TextBox()
                    {
                        BorderStyle = BorderStyle.None,
                        Location = new Point(loc.X + 20, loc.Y),
                        Width = list.Width - 30,
                        Text = (d.GetType().GetMethod("Display").Invoke(d,new object[] { this}) as string).Replace("--", "+").Replace("+-", "-").Replace("1x", "x").Replace("x+0", "x").Replace("0x", "").Replace("-+", "-").Replace("(x-0)²", "x²").Replace("(y-0)²", "y²"),
                        Name = prp.Name,
                        Tag = prp.Name,
                        BackColor = (prp.Selected || prp.Tmp_Selected) ? Color.LightBlue : Color.White
                    };
                    box.MouseEnter += ObjectTextEnter;
                    box.MouseLeave += ObjectTextLeave;
                    controls.Add(pic);
                    controls.Add(box);
                }
            }
            list.Controls.AddRange(controls.ToArray());
            list.AutoScroll = true;
            list.VerticalScroll.Enabled = true;
        }

        public void List(object o, Panel list = null, bool highlight = false, bool leave = false)
        {
            
            var tag = (string)o.GetType().GetProperty("Tag").GetValue(o,null);

            if (list != null)

                foreach (var ob in list.Controls)
                {
                    if (ob.GetType() == typeof(TextBox))
                        try
                        {
                            var d = FindObject((ob as TextBox).Name);
                            var tp = d.GetType().GetField("Properties").GetValue(d);
                            var sel = (bool)tp.GetType().GetField("Selected").GetValue(tp);

                            if (!sel)
                                SelecObject((ob as TextBox).Name, true, false);
                        }
                        catch { }
                }

            if (!leave)
            {
                var obj = FindObject(tag);
                if (highlight)
                {
                    SelecObject(tag, true);
                }
                else
                {
                    var tp = obj.GetType().GetField("Properties").GetValue(obj);
                    var visible = (bool)tp.GetType().GetField("Visible").GetValue(tp);


                    tp.GetType().GetField("Visible").SetValue(tp, !visible);
                    (o as PictureBox).Image = CheckBox(!visible);
                }
            }

        }

        //------------------------------------------------------------------------

        public Result Equation(float aa, string end, float x, bool para = true)
        {
            Result res = new Result();
            PointF bpt = FindPoint(end).Position(this);
            float a = (-1 / aa);
            float aaa = para ? aa : a;
            float b = bpt.Y - aaa * bpt.X;
            res.a = a;
            res.function = aaa + "x+" + b;

            res.value = new PointF(x, aaa * x + b);
            if (float.IsInfinity(a) || float.IsNaN(a))
            {
                res.inf = true;
                res.pos = para ? bpt.Y : bpt.X;
            }
            return res;
        }

        public float Equation(float aa, PointF bpt, float x, bool para = true)
        {

            float a = (-1 / aa);
            float aaa = para ? aa : a;
            float b = bpt.Y - aaa * bpt.X;

            return aaa * x + b;

        }

        //---------\\

        public Result Equation(string start, PointF bpt, float x)
        {
            Result rs = new Result();
            PointF apt = FindPoint(start).Position(this);

            var a = (apt.Y - bpt.Y) / (apt.X - bpt.X);
            var b = apt.Y - a * apt.X;
            if (double.IsInfinity(a + b) || double.IsNaN(a + b))
            {
                rs.inf = true;
                rs.pos = apt.X;
            }
            rs.value = new PointF(x, a * x + b);
            var fun = (a + "x+" + b);
            rs.function = fun;
            rs.a = a;
            return rs;
        }

        public Result Equation(string start, string end, float x)

        {
            PointF bpt = FindPoint(end).Position(this);
            return Equation(start, bpt, x);
        }

        //--------------------------------------------------------------------

        public bool Inside(PointF A, PointF B, PointF C)
        {

            bool result1 = false, result2 = false;
            float x0, y0, x1, y1;

            x0 = Math.Min(A.X, B.X);
            x1 = Math.Max(A.X, B.X);
            y0 = Math.Min(A.Y, B.Y);
            y1 = Math.Max(A.Y, B.Y);
            if (C.X >= x0 && C.X <= x1)
            {
                result1 = true;

            }
            if (C.Y <= y1 && C.Y >= y0)
            {
                result2 = true;

            }

            return (result1 && result2);
        }
       
        public bool ScanArea(Point Location, bool tmp, ref bool drag_point, bool invalidate)
        {
            var res = SelectPoint(Location, tmp);
            if (res != 0)
            {
                drag_point = res == 2 ? true : false;
                invalidate = true;
            }
            if (res != 2)
            {
                invalidate = SelectLine(Location, tmp) != 0 ? true : invalidate;
                invalidate = SelectSegment(Location, tmp) != 0 ? true : invalidate;
                invalidate = SelectCercle(Location, tmp) != 0 ? true : invalidate;
                invalidate = SelectFunction(Location, tmp) ? true : invalidate;
            }
            return invalidate;
        }

        public bool Execute()
        {
            var script = handle();
            textBox2.Text = script;
            source = script;

           
            var y = new CompilerParameters();
            y.GenerateInMemory = true;
            y.ReferencedAssemblies.Add("System.dll");

            var result = provider.CompileAssemblyFromSource(y, script);
            
            if (result.Errors.Count == 0)
            {
                project.type = result.CompiledAssembly.GetType("Calculator");

                
                foreach (var p in project.points)
                {
                    
                  project.type.GetMethod("AddPoint").Invoke(null, new object[] { p.Position(this).X, p.Position(this).Y, p.Properties.Name });
                  
                }
                return true;
            }
             textBox2.Visible = true;
            
            Canavas.Enabled = false;
                print(result.Errors[0].ErrorText);
            return false;


           

            
            
        }

        string handle()
        {

            var script = "";

            foreach (var f in project.functions)
                script += project.func_str
                    .Replace("<name>",f.Properties.Name)
                    .Replace("<function>",f.function);


            foreach (var point in project.points)
            {
                var nm = point.Properties.Name;

               
                script = script.Replace("x*(" + nm + ")", @"xof(""" + nm + @""")");
                script = script.Replace("y(" + nm + ")", @"y(""" + nm + @""")");

                
            }
            
           /* foreach (var point in project.segments)
              {
                var nm = point.Properties.Name;

                

             }
             */






            for (int f = 0; f < functions_names.GetLength(0); f++)
                script = script.Replace(functions_names[f, 0] + "(", functions_names[f, 1] + "(");

            for (int f = 0; f < newnames.GetLength(0); f++)
                script = script.Replace(newnames[f, 0] + "(", newnames[f, 1] + "(");


            script = project.script.Replace("<Functions>", script);
               


            return script;
        }

        public string parse(string str, string value = "*")
        {
            
            //str = str.ToLower();
            //str = str.Replace(" ", "");
            str = str.Replace("*)", ")");
            str = str.Replace("(*", "(");
            str = str.Replace("+*", "*");
            str = str.Replace("-*", "*");
            str = str.Replace("++", "+");
            str = str.Replace("--", "+");
            str = str.Replace("+-", "-");
            str = str.Replace("-+", "-");
            for (int i = 0; i < functions_names.GetLength(0); i++)
            {

                int added_size = 0;

                foreach (Match found in Regex.Matches(str, functions_names[i, 0]))
                {
                    if (found.Index != 0)
                    {
                        var skip = false;
                        string[] symbols = new string[] { ",", ".", " ", "a", "e", "=", "*", "+", "-", "(", "/" };
                        foreach (string symb in symbols)
                        {
                            var str2 = Regex.Escape(symb + functions_names[i, 0]);

                            foreach (Match found2 in Regex.Matches(str, str2))
                            {
                                if (found2.Value == "") continue;

                                if (found2.Index == found.Index - 1)
                                {
                                    skip = true;
                                    break;
                                }
                            }
                        }
                        if (!skip)
                            str = str.Insert(found.Index + added_size++, value);
                    }
                }

                for (int id = 0; id < 10; id++)
                {
                    str = str.Replace(id + "(", id + value + "(");
                    str = str.Replace("x" + id, "x" + value + id);
                    str = str.Replace(")" + id, ")" + value + id);
                }
                str = str.Replace("*", value);
                str = str.Replace("x(", "x" + value + "(");
                str = str.Replace("(" + value, "(");
                str = str.Replace(value + value, value);
                str = str.Replace(")(", ")" + value + "(");
                str = str.Replace("xx", "x" + value + "x");
                str = str.Replace("(", "(");
                str = str.Replace(")", ")");
            }
            
            return str;

        }
       
        bool SelectFunction(Point e, bool tmp)
        {
            bool ret = false;
            foreach (var t in project.functions.FindAll(fs => fs.Properties.Visible))
            {


                foreach (var td in project.functions.FindAll(ts => tmp ? ts.Properties.Tmp_Selected : ts.Properties.Selected))
                {
                    SelecObject(td.Properties.Name, tmp, false);

                    ret = true;
                }




                var ee = Convert(e);
                float y = (float)project.type.GetMethod(t.Properties.Name).Invoke(null, new object[] { ee.X });
                var aa = ConvertToReal(new PointF(0, y));

                var dis = Math.Abs(e.Y - aa.Y);

                if (dis < 5)
                {
                    SelecObject(t.Properties.Name, tmp);
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        
        string[,] functions_names = new string[,]
{
                 {"x"    , "x"/* improtant */ },
                 {"acos" , "Math.ACOS" },{ "asin", "Math.ASIN" },
                 {"atan" , "Math.ATAN" },{ "exp" , "Math.Exp"  },
                 {"ln"   , "Math.Log"  },{ "log" , "Math.Log"  },
                 {"pow"  , "Math.Pow"  },{ "sqrt", "Math.Sqrt" },
                 {"sinh" , "Math.Sinh" },{ "cosh", "Math.Cosh" },
                 {"tanh" , "Math.Tanh" },{ "tan" , "Math.Tan"  },
                 {"sin"  , "Math.Sin"  },{ "cos" , "Math.Cos"  }
},
            
        newnames = new string[,]
{
                 {"ACOS", "Acos"  },{ "ASIN" ,"Asin"},
                 {"ATAN", "Atan"  }
};

        

        
    }

    
        
        public class Calculator
        {

            static List<Segmentf> segments = new List<Segmentf>();

        Segmentf f = new Segmentf();
       
        }

        
        public class Segmentf
        {
        public Segmentf(float val , string na)
        {
            value = val;
            name = na;
        }
        public Segmentf() { }
        public float value;
            public string name;
        }

    /*
    public class List<T> : System.Collections.Generic.IEnumerable<T>

    {
        System.Collections.Generic.List<T> ls = new System.Collections.Generic.List<T>();

        public List()
        {
           
        }
       
        public List<T> FindAll(Predicate<T> pre)
        {
            return null;
        }
        public T Find(Predicate<T> pre)
        {
            T a = (T)new object();
            return a;
        }
        public void Add(T s)
        {
            ls.Add(s);
            Count++;
        }
        public T[] ToArray()
        {
            T[] arr = new T[Count];
            int i = 0;
            foreach(var l in ls)
            {
                arr[i] = ls[i++];
            }
            return arr;
        }
        
        public int Count;

        

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
*/




}