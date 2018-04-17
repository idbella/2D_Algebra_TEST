using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
namespace Algebra
{
    partial class Main
    {
        
        void DrawGrid(Graphics graphics)
        {

            view.SnapedToLines = new List<Grid_Snap>();
             Font drawFont = new Font("Calibri", 10);
            SolidBrush drawBrush = new SolidBrush(Color.Black);
            var coef = view.Zoom;

            var div = coef <= 1 ? (int)(1 / coef) : (int)(coef * 2);

            if (div != 0) div = Better_div(div);
            bool vertical = true;
            start:;
            var min = (int)(vertical ? view.MinValue.X : view.Assist.X);
            var max = (int)(vertical ? view.MaxValue.X : view.Assist.Y);

            for (int i = min - 1; i < max + 1; i++)
            {
                if (coef < 1 && !is_valid_point(i, div)) continue;

                float xy = (float)((vertical ? view.Zero.X : view.Zero.Y) + view.val * i * coef);

                if (coef >= 1)
                    for (float jd = 1; jd <= div - 1; jd++)
                    {
                        float xy0 = xy + ((view.val * (float)coef) / div) * jd;

                        if (view.grid)
                           graphics.DrawLine(new Pen(Color.LightGray, 1), (vertical ? xy0 : 0), (vertical ? 0 : xy0), (vertical ? xy0 : view.size.Width), (vertical ? view.size.Height : xy0));
                        Grid_Snap gr = new Grid_Snap();

                        if (vertical) gr.x = (int)xy0; else gr.y = (int)xy0;

                        double number = i + (1 / (double)div) * jd;

                        if (vertical) gr.x_value = number; else gr.y_value=-number;
                        number = Math.Round(number, 4);
                       
                        view.SnapedToLines.Add(gr);
                        if (view.axes)
                            graphics.DrawString((vertical? number:-number) + "", drawFont, drawBrush, (vertical ? xy0 : (float)Clamp(view.Zero.X, 0, view.size.Width - 15)), (vertical ? (float)Clamp(view.Zero.Y, 0, view.size.Height - 15) : xy0));

                    }

                Grid_Snap gr2 = new Grid_Snap();

                if (vertical)
                {
                    gr2.x = (int)xy;
                    gr2.x_value = i;
                    
                }
                else
                {
                    gr2.y = (int)xy;
                    gr2.y_value = -i;
                }
               view.SnapedToLines.Add(gr2);
               
                if (view.grid)
                    graphics.DrawLine(new Pen(Color.LightGray, 1.2f), vertical ? xy : 0, vertical ? 0 : xy, vertical ? xy : view.size.Width, vertical ? view.size.Height : xy);

                if (view.axes)
                    graphics.DrawString((vertical?i:-i) + "", drawFont, drawBrush, vertical ? xy : (float)Clamp(view.Zero.X, 0, view.size.Width - 15), vertical ? (float)Clamp(view.Zero.Y, 0, view.size.Height - 15) : xy);


            }
            if (view.axes)
                graphics.DrawLine(new Pen(Color.Black, 1), vertical ? view.Zero.X : 0, vertical ? 0 : view.Zero.Y, vertical ? view.Zero.X : view.size.Width, vertical ? view.size.Height : view.Zero.Y);

            if (vertical) { vertical = false; goto start; }

        }

        void DrawSegments(Graphics graphics)
        {
            var segments = project.segments;
            foreach (var seg in segments.FindAll(pt => pt.Properties.Visible == true))
            {
                var prp = seg.Properties;
                var StartPoint = FindPoint(seg.Start);
                var Start = ConvertToReal(StartPoint.Position(this));
                Point End = new Point();
                if (seg.End != null)
                {
                    var EndPoint = FindPoint(seg.End);
                    End = ConvertToReal(EndPoint.Position(this));
                }
                else
                {
                    End = view.MousePosition;
                }

                if(prp.Tmp_Selected||prp.Selected)
                    graphics.DrawLine(new Pen(Color.FromArgb(100,prp.Color), prp.Size+2), Start, End);

                graphics.DrawLine(new Pen(prp.Color,prp.Size), Start, End);
            }
        }

        void DrawCercles(Graphics graphics)
        {
            var cercles = project.cercles;
            foreach (var cer in cercles.FindAll(pt => pt.Properties.Visible == true))
            {
                var prp = cer.Properties;
                var StartPoint = FindPoint(cer.O);
                var Start = ConvertToReal(StartPoint.Position(this));
                Point End = new Point();

                if (cer.R != null)
                {
                    var EndPoint = FindPoint(cer.R);
                    End = ConvertToReal(EndPoint.Position(this));
                }
                else
                {
                    End = view.MousePosition;
                }

                var r = (float)Distance(Start, End);
                Pen pen = new Pen(prp.Color, prp.Size);

                Pen pen2 = new Pen(Color.FromArgb(100, prp.Color), prp.Size*1.5f);
                if (!cer.fix)
                {
                    if (prp.Selected || prp.Tmp_Selected)
                        graphics.DrawEllipse(pen2, (int)Start.X - r, (int)Start.Y - r, r * 2, r * 2);
                    graphics.DrawEllipse(pen, (int)Start.X - r, (int)Start.Y - r, r * 2, r * 2);
                }else
                {
                    r = (float)(cer.value * view.Zoom * view.val);
                    if (prp.Selected || prp.Tmp_Selected)
                        graphics.DrawEllipse(pen2, (int)Start.X - r, (int)Start.Y - r, r * 2, r * 2);
                    graphics.DrawEllipse(pen, (int)Start.X - r, (int)Start.Y - r, r * 2, r * 2);
                }
            }

        }

        void DrawCuves(Graphics graphics)
        {
            var curves = project.curves;
            try
            {
                foreach (var cur in curves.FindAll(pt => pt.Properties.Visible == true))
                {
                    var prp = cur.Properties;
                    var pts = cur.GetPoints(this);

                    if (cur.closed)
                    {
                        if (prp.Selected || prp.Tmp_Selected)
                            graphics.DrawClosedCurve(new Pen(Color.FromArgb(100, prp.Color), prp.Size+2), pts.ToArray());
                        graphics.DrawClosedCurve(new Pen(prp.Color, prp.Size), pts.ToArray());
                    }
                    else
                    {
                        if (!cur.finished)
                            pts.Add(view.MousePosition);
                        if (prp.Selected || prp.Tmp_Selected)
                            graphics.DrawCurve(new Pen(Color.FromArgb(100, prp.Color), prp.Size+2), pts.ToArray());
                        graphics.DrawCurve(new Pen(prp.Color, prp.Size), pts.ToArray());
                    }
                }
            }
            catch  {}
        }

        void DrawBeziers(Graphics graphics)
        {
            try
            {
                var beziers = project.beziers;
                foreach (var biz in beziers.FindAll(pt => pt.Properties.Visible == true&&pt.points.Count>2))
                {
                    var prp = biz.Properties;
                    var pts = biz.GetPoints(this);

                    if (!biz.finished)
                        pts.Add(view.MousePosition);
                    if(prp.Selected||prp.Tmp_Selected)
                        graphics.DrawBezier(new Pen(Color.FromArgb(100,prp.Color), prp.Size+2), pts[0], pts[1], pts[2], pts[3]);
                    graphics.DrawBezier(new Pen(prp.Color, prp.Size), pts[0], pts[1], pts[2], pts[3]);

                }
            }
            catch  { }
        }

        void DrawLines(Graphics graphics)
        {
            try
            {
                var lines = project.lines;

                foreach (var line in lines.FindAll(pt => pt.Properties.Visible == true))
                {
                    var prp = line.Properties;
                     Result st = new Result(),en=st;

                    
                    PointF start = new PointF(), end = start;
                    if (line.parallel||line.perpendicular)
                    {
                       
                            var obj = FindObject(line.Start);
                        var sttt = (string)obj.GetType().GetField("Start").GetValue(obj);
                        var ennn = (string)obj.GetType().GetField("End").GetValue(obj);
                        Result res = Equation(sttt, ennn, 0);
                        
                       
                            var a = res.inf ? res.value.X : res.a;
                        if (!res.inf)
                        {

                            if (line.End != null)
                            {

                                st = Equation(a, line.End, view.MinValue.X, line.parallel);
                                en = Equation(a, line.End, view.MaxValue.X, line.parallel);
                                
                                if (st.inf || en.inf)
                                {
                                    if (line.parallel)
                                    {
                                        start = new PointF(0, ConvertToReal(new PointF(0, st.pos)).Y);
                                        end = new PointF(Canavas.Width, start.Y);
                                    }
                                    else
                                    {
                                        start = new PointF(ConvertToReal(new PointF(st.pos, 0)).X, 0);
                                        end = new PointF(start.X, Canavas.Height);
                                    }
                                }
                                else
                                {
                                    start = ConvertToReal(st.value);
                                    end = ConvertToReal(en.value);
                                }

                            }
                            else
                            {

                                try
                                {
                                    var t = Convert(view.MousePosition);
                                    var stt = Equation(a, t, view.MinValue.X, line.parallel);
                                    var enn = Equation(a, Convert(view.MousePosition), view.MaxValue.X, line.parallel);
                                    
                                    if (float.IsInfinity(stt + enn) || float.IsNaN(enn + stt))
                                    {

                                        start = new PointF(0, ConvertToReal(new PointF(0, t.Y)).Y);
                                        end = new PointF(Canavas.Width, start.Y);

                                    }
                                    else
                                    {
                                        start = ConvertToReal(new PointF(view.MinValue.X, stt));
                                        end = ConvertToReal(new PointF(view.MaxValue.X, enn));

                                    }

                                }
                                catch { }
                            }
                        }
                        else
                        {
                            if (line.End != null)
                            {
                                var psx = FindPoint(line.End).Position(this).X;
                                var psy = FindPoint(line.End).Position(this).Y;

                                if (line.parallel)
                                {
                                    start = new PointF(ConvertToReal(new PointF(psx, 0)).X, 0);
                                    end = new PointF(ConvertToReal(new PointF(psx, 0)).X, Canavas.Height);
                                }
                                else
                                {
                                    start = new PointF(0, ConvertToReal(new PointF(0, psy)).Y);
                                    end = new PointF(Canavas.Width, start.Y);
                                }

                            }
                            else
                            {
                               
                                if (line.parallel)
                                {
                                    start = new PointF(view.MousePosition.X, 0);
                                    end = new PointF(start.X, Canavas.Height);
                                }
                                else
                                {
                                    start = new PointF(0,view.MousePosition.Y);
                                    end = new PointF(Canavas.Width, start.Y);
                                }

                            }
                        }
                       
                    }
                    else//------------------------------------------------------------------------------------------------------------------------
                    {
                        if (line.End == null)
                        {

                            line.preview = Convert(view.MousePosition );
                            st = Equation(line.Start, Convert(view.MousePosition ), view.MinValue.X);
                            en = Equation(line.Start, Convert(view.MousePosition ), view.MaxValue.X);
                        }
                        else
                        {
                            st = Equation(line.Start, line.End, view.MinValue.X);
                            en = Equation(line.Start, line.End, view.MaxValue.X);
                        }

                        if (st.inf || en.inf)
                        {
                            start = new PointF(ConvertToReal(new PointF(st.pos, 0) ).X, 0);
                            end = new PointF(start.X, Canavas.Height);
                        }
                        else
                        {
                            start = ConvertToReal(st.value );
                            end = ConvertToReal(en.value);
                        }
                    }
                    if(prp.Selected||prp.Tmp_Selected)
                        graphics.DrawLine(new Pen(Color.FromArgb(100,prp.Color), prp.Size+2), start, end);
                    graphics.DrawLine(new Pen(prp.Color, prp.Size), start, end);
                    

                }
            }catch(Exception r) { MessageBox.Show(r.StackTrace); }
        }

        void HandCurve(Graphics e)
        {
            
                foreach (var hd in project.handcurves.FindAll(h => h.Properties.Visible))
                {
                try
                {
                    e.DrawLines(new Pen(Color.Blue, 2), hd.list.ToArray());
                }
                catch (Exception ed) { Debug.Print(" error : "+ed.Message); }
            }
            
        }

        void DrawFunctions(Graphics e)
         {
            foreach (var t in project.functions.FindAll(fs => fs.Properties.Visible))
            {
                var prp = t.Properties;
                try
                {
                    
                    var step = 1 / ((float)view.Zoom * view.val);
                    
                    List<Point> pt = new List<Point>();
                    var val = 200 / (view.Zoom * view.val)*2;
                    for (float x = view.MinValue.X; x <= view.MaxValue.X ; x += step)
                    {

                        float y = (float)project.type.GetMethod(prp.Name).Invoke(null, new object[] { x });
                        bool bl = true;///y < view.MaxValue.Y+val && y > view.MinValue.Y-val;
                        if(!float.IsInfinity(y)&&!float.IsNaN(y)&&bl)
                         pt.Add( ConvertToReal(new PointF(x, y)));
                        
                    }
                    
                    if (pt.Count < 1) continue; 
                    if (prp.Selected || prp.Tmp_Selected)
                       e.DrawCurve(new Pen(Color.FromArgb(50, prp.Color), prp.Size + 2), pt.ToArray());
                    e.DrawCurve(new Pen(prp.Color, prp.Size), pt.ToArray());
                }
                catch (Exception tr) { MessageBox.Show(tr.Message+" "+prp.Name); }
               
            }
           
         }


        void DrawPoints(Graphics graphics)
        {
            FontStyle fnt = FontStyle.Bold;
            Font drawFont = new System.Drawing.Font("Calibri", 9, fnt);
            SolidBrush drawBrush = new SolidBrush(Color.Black);

            foreach (var point in points.FindAll(pt => pt.Properties.Visible == true))
            {
                var Point = point.Properties;
                var size = (int)Point.Size;
                int size3 = size * 2;
                int size4 = size + 2;
                var name = Point.Name;

                SolidBrush pn = new SolidBrush(Point.Color);
                SolidBrush pn2 = new SolidBrush(Color.FromArgb(80, 80, 80));
                SolidBrush pn3 = new SolidBrush(Color.FromArgb(100, Point.Color));

                var po = ConvertToReal(point.Position(this));
                var x = po.X;
                var y = po.Y;

                if (Point.Selected || Point.Tmp_Selected)
                {
                    graphics.FillEllipse(pn3, (int)x - size3 / 2, (int)y - size3 / 2, size3, size3);
                    size4 = 0;
                }
                else size3 = 0;
                graphics.FillEllipse(pn2, (int)x - (size + 2) / 2, (int)y - (size + 2) / 2, (size + 2), (size + 2));
                graphics.FillEllipse(pn, (int)x - size / 2, (int)y - size / 2, size, size);
                if (!Point.Name.StartsWith("pp"))
                    graphics.DrawString(name, drawFont, drawBrush, (int)x, (int)y - size - 10);
            }
        }



    }

}

