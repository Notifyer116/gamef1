using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
namespace F1Manager {
// Mouse-first numeric control; dragging chooses a value and +/- provide precise steps.
public class SliderInput:Control {
    decimal minimum,maximum=10,value,step=1;bool dragging;
    public string Caption="",Suffix="";public int DecimalPlaces;
    public event EventHandler ValueChanged;
    public decimal Minimum {get{return minimum;}set{SetRange(value,Math.Max(value,maximum));}}
    public decimal Maximum {get{return maximum;}set{SetRange(Math.Min(minimum,value),value);}}
    public decimal Increment {get{return step;}set{step=Math.Max(.001m,value);}}
    public decimal Value {get{return value;}set{decimal next=Math.Max(minimum,Math.Min(maximum,value));if(this.value==next)return;this.value=next;Invalidate();if(ValueChanged!=null)ValueChanged(this,EventArgs.Empty);}}
    public SliderInput(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer|ControlStyles.Selectable,true);Height=58;MinimumSize=new Size(95,44);BackColor=Color.FromArgb(30,30,30);ForeColor=Color.White;Cursor=Cursors.Hand;TabStop=true;AccessibleRole=AccessibleRole.Slider;MouseDown+=(o,e)=>{if(e.Button!=MouseButtons.Left||!Enabled)return;Focus();if(e.X<22){Value-=Increment;return;}if(e.X>Width-22){Value+=Increment;return;}dragging=true;Capture=true;MoveValue(e.X);};MouseMove+=(o,e)=>{if(dragging)MoveValue(e.X);};MouseUp+=(o,e)=>{dragging=false;Capture=false;};MouseCaptureChanged+=(o,e)=>{if(!Capture)dragging=false;};MouseWheel+=(o,e)=>{if(Enabled)Value+=e.Delta>0?Increment:-Increment;};KeyDown+=(o,e)=>{if(e.KeyCode==Keys.Right||e.KeyCode==Keys.Up){Value+=Increment;e.Handled=true;}if(e.KeyCode==Keys.Left||e.KeyCode==Keys.Down){Value-=Increment;e.Handled=true;}};}
    public void SetRange(decimal min,decimal max){if(max<min)throw new ArgumentException("Некорректные границы ползунка.");minimum=min;maximum=max;Value=value;Invalidate();}
    void MoveValue(int x){double fraction=Math.Max(0,Math.Min(1,(x-29)/(double)Math.Max(1,Width-58)));decimal raw=minimum+(maximum-minimum)*(decimal)fraction;Value=minimum+Math.Round((raw-minimum)/Increment)*Increment;}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;using(var path=Theme.Round(new Rectangle(0,0,Width-1,Height-1),8))using(var b=new SolidBrush(BackColor))g.FillPath(b,path);var textColor=Enabled?Color.White:Theme.Muted;string number=Value.ToString(DecimalPlaces==0?"0":"0."+new string('0',DecimalPlaces))+Suffix;using(var font=new Font("Segoe UI",11,FontStyle.Regular,GraphicsUnit.Pixel))TextRenderer.DrawText(g,Caption,font,new Rectangle(10,3,Math.Max(15,Width-68),20),Theme.Muted,TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);using(var font=new Font("Segoe UI",13,FontStyle.Bold,GraphicsUnit.Pixel))TextRenderer.DrawText(g,number,font,new Rectangle(Math.Max(5,Width-92),2,82,22),textColor,TextFormatFlags.Right|TextFormatFlags.VerticalCenter);int y=Height-17,left=29,right=Width-29;double fraction=maximum==minimum?0:(double)((Value-minimum)/(maximum-minimum));int knob=left+(int)((right-left)*fraction);using(var p=new Pen(Theme.Border,4)){p.StartCap=p.EndCap=LineCap.Round;g.DrawLine(p,left,y,right,y);}using(var p=new Pen(Enabled?Theme.Red:Theme.Border,4)){p.StartCap=p.EndCap=LineCap.Round;g.DrawLine(p,left,y,knob,y);}using(var b=new SolidBrush(textColor))g.FillEllipse(b,knob-6,y-6,12,12);using(var font=new Font("Segoe UI",17,FontStyle.Bold,GraphicsUnit.Pixel)){TextRenderer.DrawText(g,"−",font,new Rectangle(0,y-12,23,24),textColor,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);TextRenderer.DrawText(g,"+",font,new Rectangle(Width-23,y-12,23,24),textColor,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter);}if(Focused)using(var path=Theme.Round(new Rectangle(1,1,Width-3,Height-3),8))using(var p=new Pen(Theme.Red))g.DrawPath(p,path);}
}
}
