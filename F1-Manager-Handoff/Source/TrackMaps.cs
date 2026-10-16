using System;
using System.Linq;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Script.Serialization;
using System.Windows.Forms;
namespace F1Manager {
public class CircuitMap {public string Id,Name;public double Length;public double[][] Coordinates;}
public static class TrackMaps {
    static readonly string[] colors={"#ff8700","#ef233c","#3568ee","#27d9bb","#229f87","#ee79ba","#dadde5","#669cff","#2594ff","#e83242","#c8b796","#ed252a"};
    static readonly CircuitMap[] maps=Load();
    static readonly Dictionary<int,PointF[]> projected=new Dictionary<int,PointF[]>();
    static CircuitMap[] Load(){using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("circuitmaps"))using(var reader=new StreamReader(stream))return new JavaScriptSerializer{MaxJsonLength=2000000}.Deserialize<CircuitMap[]>(reader.ReadToEnd());}
    public static CircuitMap Get(int round){return maps[round];}
    public static PointF[] Fit(int round,Rectangle bounds){PointF[] raw;if(!projected.TryGetValue(round,out raw)){var c=maps[round].Coordinates;double latitude=c.Average(v=>v[1])*Math.PI/180,longitude=c[0][0],origin=c[0][1];raw=c.Select(v=>new PointF((float)((v[0]-longitude)*Math.Cos(latitude)),(float)(origin-v[1]))).ToArray();projected.Add(round,raw);}float minX=raw.Min(p=>p.X),minY=raw.Min(p=>p.Y),width=raw.Max(p=>p.X)-minX,height=raw.Max(p=>p.Y)-minY;float scale=Math.Min(bounds.Width/width,bounds.Height/height);float x=bounds.X+(bounds.Width-width*scale)/2,y=bounds.Y+(bounds.Height-height*scale)/2;return raw.Select(p=>new PointF(x+(p.X-minX)*scale,y+(p.Y-minY)*scale)).ToArray();}
    public static PointF Along(PointF[] path,double progress){progress=progress-Math.Floor(progress);double total=0;for(int i=1;i<path.Length;i++)total+=Distance(path[i-1],path[i]);double target=total*progress;for(int i=1;i<path.Length;i++){double length=Distance(path[i-1],path[i]);if(length>=target){double t=length==0?0:target/length;return new PointF((float)(path[i-1].X+(path[i].X-path[i-1].X)*t),(float)(path[i-1].Y+(path[i].Y-path[i-1].Y)*t));}target-=length;}return path[path.Length-1];}
    static double Distance(PointF a,PointF b){return Math.Sqrt((a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y));}
    public static void Draw(Graphics g,int round,Rectangle bounds,LiveSession live,double interpolation,int player=-1){if(bounds.Width<100||bounds.Height<70)return;var saved=g.Save();g.SetClip(bounds,CombineMode.Intersect);g.SmoothingMode=SmoothingMode.AntiAlias;using(var shape=Theme.Round(bounds,14))using(var b=new SolidBrush(Theme.Card))using(var pen=new Pen(Theme.Border)){g.FillPath(b,shape);g.DrawPath(pen,shape);}using(var f=new Font("Segoe UI",11,FontStyle.Bold,GraphicsUnit.Pixel))using(var brush=new SolidBrush(Theme.Muted))using(var format=new StringFormat{Trimming=StringTrimming.EllipsisCharacter,FormatFlags=StringFormatFlags.NoWrap})g.DrawString(Data.Tracks[round].Name+" / "+(maps[round].Length/1000).ToString("0.000")+" км",f,brush,new Rectangle(bounds.X+12,bounds.Y+8,Math.Max(60,bounds.Width-136),18),format);var points=Fit(round,new Rectangle(bounds.X+20,bounds.Y+32,bounds.Width-40,bounds.Height-48));using(var outer=new Pen(Color.FromArgb(65,65,65),7))using(var line=new Pen(Color.FromArgb(214,214,214),2)){outer.LineJoin=line.LineJoin=LineJoin.Round;g.DrawLines(outer,points);g.DrawLines(line,points);}var start=points[0];using(var b=new SolidBrush(Color.White))g.FillRectangle(b,start.X-3,start.Y-3,6,6);using(var b=new SolidBrush(Theme.Red))g.FillRectangle(b,start.X,start.Y-3,3,3);
        if(live!=null){double leader=live.Entries.Where(e=>!e.Dnf).Select(e=>e.RaceTime).DefaultIfEmpty(0).Min();var ours=live.Entries.Where(e=>!e.Dnf).OrderBy(e=>e.Team==player?1:0).ToArray();foreach(var e in ours){double progress=(Math.Max(0,live.RaceTick-1+interpolation)/live.TicksPerLap)-(e.RaceTime-leader)/Data.Tracks[round].BaseLap;var p=Along(points,progress);Color color;try{color=ColorTranslator.FromHtml(colors[Math.Max(0,Math.Min(e.Team,colors.Length-1))]);}catch{color=Theme.Red;}float radius=e.Team==player?4.5f:2.5f;using(var b=new SolidBrush(color))using(var pen=new Pen(e.Team==player?Color.White:Color.Black,1)){g.FillEllipse(b,p.X-radius,p.Y-radius,radius*2,radius*2);g.DrawEllipse(pen,p.X-radius,p.Y-radius,radius*2,radius*2);}}}g.Restore(saved);}
}
public class CircuitMapCanvas:Control {
    readonly int round,player;readonly LiveSession live;
    public CircuitMapCanvas(int circuit,LiveSession session,int team){round=circuit;live=session;player=team;BackColor=Theme.Card;SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.ResizeRedraw|ControlStyles.Opaque,true);}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;using(var background=new SolidBrush(Theme.Card))g.FillRectangle(background,ClientRectangle);using(var white=new SolidBrush(Color.White))using(var muted=new SolidBrush(Theme.Muted))using(var title=new Font("Segoe UI",27,FontStyle.Bold,GraphicsUnit.Pixel))using(var subtitle=new Font("Segoe UI",14,FontStyle.Regular,GraphicsUnit.Pixel)){g.DrawString("СХЕМА ТРАССЫ",title,white,0,0);g.DrawString(Data.Tracks[round].Country+" / "+TrackMaps.Get(round).Name,subtitle,muted,1,44);}TrackMaps.Draw(g,round,new Rectangle(0,84,Width,Height-84),live,1,player);}
}
public class CircuitMapOverlay:GameOverlay {
    internal override bool NeedsBackdrop {get{return false;}}
    bool revealed;readonly CircuitMapCanvas canvas;
    public event Action ClosedMap;
    internal readonly SoftButton BackButton=new SoftButton{Text="Вернуться к гонке",Primary=true};
    protected override Size SheetSize {get{return new Size(960,650);}}
    public CircuitMapOverlay(int round,LiveSession live,int player,bool animated):base(animated){canvas=new CircuitMapCanvas(round,live,player);Sheet.Controls.Add(canvas);Sheet.Controls.Add(BackButton);Sheet.Resize+=(o,e)=>{canvas.Bounds=new Rectangle(28,22,Sheet.Width-56,Sheet.Height-110);BackButton.Bounds=new Rectangle(Sheet.Width-294,Sheet.Height-64,266,42);revealed=false;};BackButton.Click+=(o,e)=>Cancel();}
    protected override void Advance(){if(!Sheet.Visible){revealed=false;return;}if(!revealed){Sheet.Invalidate(true);revealed=true;}}
    public override void Cancel(){if(!Closing&&ClosedMap!=null)ClosedMap();}
}

}
