using System;
using System.Drawing;
using System.Drawing.Drawing2D;
namespace F1Manager {
// Resolution-independent sidewall art; compound markings stay legible at every UI scale.
public static class TyreArt {
    public static void Draw(Graphics g,string code,Rectangle r){if(r.Width<8||r.Height<8)return;var saved=g.Save();g.SetClip(r,CombineMode.Intersect);g.SmoothingMode=SmoothingMode.AntiAlias;float d=Math.Min(r.Width,r.Height)-4,x=r.X+(r.Width-d)/2,y=r.Y+(r.Height-d)/2;using(var rubber=new LinearGradientBrush(new RectangleF(x,y,d,d),Color.FromArgb(73,74,78),Color.FromArgb(13,14,16),35))g.FillEllipse(rubber,x,y,d,d);using(var outline=new Pen(Color.FromArgb(91,93,98),Math.Max(1,d*.025f)))g.DrawEllipse(outline,x,y,d,d);float inset=d*.17f;using(var ring=new Pen(TyrePhotos.ColorFor(code),Math.Max(2,d*.055f)))g.DrawEllipse(ring,x+inset,y+inset,d-2*inset,d-2*inset);using(var rim=new SolidBrush(Color.FromArgb(23,24,27)))g.FillEllipse(rim,x+d*.28f,y+d*.28f,d*.44f,d*.44f);if(code=="I"||code=="W")using(var tread=new Pen(Color.FromArgb(100,110,115),Math.Max(1,d*.018f))){for(int i=0;i<20;i++){double a=i*Math.PI/10;g.DrawLine(tread,(float)(x+d/2+Math.Cos(a)*d*.41),(float)(y+d/2+Math.Sin(a)*d*.41),(float)(x+d/2+Math.Cos(a+.04)*d*.47),(float)(y+d/2+Math.Sin(a+.04)*d*.47));}}using(var font=new Font("Segoe UI",Math.Max(7,d*.29f),FontStyle.Bold,GraphicsUnit.Pixel))using(var brush=new SolidBrush(TyrePhotos.ColorFor(code)))using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})g.DrawString(code,font,brush,new RectangleF(x,y,d,d),format);g.Restore(saved);}
}
}
