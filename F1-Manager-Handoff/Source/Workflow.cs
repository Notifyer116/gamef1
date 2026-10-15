using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace F1Manager {
// Identity is the person's name, so portraits follow transfers and older save files.
public static class DriverPhotos {
static readonly string[] names={"Lando Norris","Oscar Piastri","Charles Leclerc","Lewis Hamilton","Max Verstappen","Isack Hadjar","George Russell","Kimi Antonelli","Fernando Alonso","Lance Stroll","Pierre Gasly","Franco Colapinto","Esteban Ocon","Oliver Bearman","Liam Lawson","Arvid Lindblad","Carlos Sainz","Alexander Albon","Nico Hulkenberg","Gabriel Bortoleto","Sergio Perez","Valtteri Bottas","Yuki Tsunoda"};
static readonly Dictionary<string,Image> cache=new Dictionary<string,Image>();
public static Image Get(Person person){if(person==null||person.Kind!=0||person.Fictional)return null;Image image;if(cache.TryGetValue(person.Name,out image))return image;int index=Array.IndexOf(names,person.Name);if(index>=0)using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("driver"+index)){if(stream!=null)using(var source=Image.FromStream(stream))image=new Bitmap(source);}cache[person.Name]=image;return image;}
}
// Simulation runs on a private career copy. The UI thread only commits a complete result.
public class SimulationForm:Form {
readonly Career input;readonly string session,track;readonly Timer animation=new Timer();readonly Stopwatch clock=new Stopwatch();Task task;Exception failure;bool completed;public Career Result{get;private set;}
public SimulationForm(Career state,string name){input=state;session=name;track=Data.Tracks[state.Round].Country;Text="F1 Manager • Симуляция";ClientSize=new Size(570,300);FormBorderStyle=FormBorderStyle.FixedDialog;ControlBox=false;StartPosition=FormStartPosition.CenterParent;DoubleBuffered=true;BackColor=Color.FromArgb(12,14,18);animation.Interval=16;animation.Tick+=(o,e)=>{Invalidate();if(task!=null&&task.IsCompleted&&clock.Elapsed.TotalSeconds>=1.1){if(task.IsFaulted)failure=task.Exception.GetBaseException();completed=true;animation.Stop();DialogResult=failure==null?DialogResult.OK:DialogResult.Abort;}};FormClosing+=(o,e)=>{if(!completed)e.Cancel=true;};Shown+=(o,e)=>{clock.Start();animation.Start();task=Task.Factory.StartNew(()=>{Engine.SimulateSession(input);Result=input;});};}
protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;var red=Color.FromArgb(235,35,43);double time=clock.Elapsed.TotalSeconds;using(var b=new LinearGradientBrush(ClientRectangle,Color.FromArgb(37,18,24),BackColor,35f))g.FillRectangle(b,ClientRectangle);using(var b=new SolidBrush(red))g.FillRectangle(b,0,0,Width,5);
using(var p=new Pen(Color.FromArgb(44,48,57),5))g.DrawEllipse(p,42,49,60,60);using(var p=new Pen(red,5)){p.StartCap=LineCap.Round;p.EndCap=LineCap.Round;g.DrawArc(p,42,49,60,60,(float)(time*240)%360,250);}using(var f=new Font("Segoe UI",23,FontStyle.Bold,GraphicsUnit.Pixel))g.DrawString("Симулируем сессию",f,Brushes.White,126,49);using(var f=new Font("Segoe UI",14,FontStyle.Regular,GraphicsUnit.Pixel))using(var b=new SolidBrush(Color.FromArgb(174,180,193)))g.DrawString(session+"  /  "+track,f,b,128,86);
// Three moving light trails suggest speed without presenting fabricated percentages.
var clip=g.Save();g.SetClip(new Rectangle(40,155,490,48));for(int i=0;i<8;i++){float x=(float)((time*310+i*92)%740)-160;using(var p=new Pen(i%2==0?red:Color.FromArgb(120,244,245,248),3))g.DrawLine(p,x,165+(i%3)*14,x+68,165+(i%3)*14);}g.Restore(clip);using(var f=new Font("Segoe UI",13,FontStyle.Regular,GraphicsUnit.Pixel))using(var b=new SolidBrush(Color.FromArgb(160,167,181)))g.DrawString("Рассчитываем круги, стратегию и итоговые позиции",f,b,42,242);}
protected override void OnFormClosed(FormClosedEventArgs e){base.OnFormClosed(e);if(failure!=null)MessageBox.Show(Owner,"Симуляция не завершена: "+failure.Message+"\nКарьера не изменена.","F1 Manager",MessageBoxButtons.OK,MessageBoxIcon.Information);}
protected override void Dispose(bool disposing){if(disposing)animation.Dispose();base.Dispose(disposing);}
}
}
