using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
namespace F1Manager {
public static class StartupPresentation {
    static readonly string[] captions={"Ремонтируем болид…","Замеряем давление в шинах…","Прогреваем двигатель…","Проверяем радиосвязь…","Настраиваем переднее антикрыло…","Готовим команду к старту…","Сверяем прогноз погоды…","Затягиваем колёсные гайки…","Проверяем заряд ERS…","Выезжаем из боксов…"};
    public static string RandomCaption(){return captions[new Random(Guid.NewGuid().GetHashCode()).Next(captions.Length)];}
    public static double Opacity(double time,double duration,bool animated){return animated?Theme.Ease(time/.65)*(1-Theme.Ease((time-(duration-.5))/.5)):1;}
    public static void Draw(Graphics g,Rectangle bounds,double time,double duration,string caption,bool animated){g.Clear(Color.Black);g.SmoothingMode=SmoothingMode.AntiAlias;double opacity=Opacity(time,duration,animated);int alpha=(int)(255*opacity);int cx=bounds.Width/2,cy=bounds.Height/2-28;using(var format=new StringFormat{Alignment=StringAlignment.Center,LineAlignment=StringAlignment.Center})using(var large=new Font("Segoe UI",76,FontStyle.Bold,GraphicsUnit.Pixel))using(var small=new Font("Segoe UI",42,FontStyle.Bold,GraphicsUnit.Pixel))using(var detail=new Font("Segoe UI",14,FontStyle.Regular,GraphicsUnit.Pixel))using(var red=new SolidBrush(Color.FromArgb(alpha,Theme.Red)))using(var white=new SolidBrush(Color.FromArgb(alpha,Color.White)))using(var muted=new SolidBrush(Color.FromArgb(alpha,Theme.Muted))){g.DrawString("F1",large,red,new Rectangle(cx-300,cy-100,600,100),format);g.DrawString("MANAGER",small,white,new Rectangle(cx-350,cy+1,700,70),format);g.DrawString(caption,detail,muted,new Rectangle(cx-430,bounds.Height-113,860,32),format);}int y=bounds.Height-163;using(var ring=new Pen(Color.FromArgb(alpha,44,44,44),3))using(var arc=new Pen(Color.FromArgb(alpha,Theme.Red),3)){arc.StartCap=arc.EndCap=LineCap.Round;g.DrawEllipse(ring,cx-15,y,30,30);g.DrawArc(arc,cx-15,y,30,30,animated?(float)(time*220%360):-90,110);}}
}
public static class WindowTheme {
    [DllImport("dwmapi.dll")]static extern int DwmSetWindowAttribute(IntPtr window,int attribute,ref int value,int size);
    public static void Apply(IntPtr handle){if(Environment.OSVersion.Platform!=PlatformID.Win32NT)return;try{int dark=1;if(DwmSetWindowAttribute(handle,20,ref dark,4)!=0)DwmSetWindowAttribute(handle,19,ref dark,4);int border=0x0c0c0c,caption=0x121212;DwmSetWindowAttribute(handle,34,ref border,4);DwmSetWindowAttribute(handle,35,ref caption,4);}catch(DllNotFoundException){}catch(EntryPointNotFoundException){}}
}
public partial class ManagerForm {
    protected override void OnHandleCreated(EventArgs e){base.OnHandleCreated(e);WindowTheme.Apply(Handle);}
}
}
