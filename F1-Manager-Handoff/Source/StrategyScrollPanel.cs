using System;
using System.Drawing;
using System.Windows.Forms;
namespace F1Manager {
// The two driver columns scroll independently, without native window scrollbars.
public class StrategyScrollPanel:RoundedPanel {
    public readonly Panel Content=new Panel{BackColor=Color.FromArgb(28,28,28)};
    double position,target;bool dragging;
    public const int DocumentHeight=573;
    internal double ScrollPosition {get{return position;}}
    int Limit {get{return Math.Max(0,DocumentHeight-ClientSize.Height);}}
    public StrategyScrollPanel(){BackColor=Content.BackColor;Controls.Add(Content);Register(Content);MouseWheel+=Wheel;MouseDown+=(o,e)=>{if(e.Button==MouseButtons.Left&&e.X>=Width-20&&Limit>0){dragging=true;Capture=true;Drag(e.Y);}};MouseMove+=(o,e)=>{if(dragging)Drag(e.Y);};MouseUp+=(o,e)=>{dragging=false;Capture=false;};MouseCaptureChanged+=(o,e)=>{if(!Capture)dragging=false;};Resize+=(o,e)=>{target=Math.Min(target,Limit);position=Math.Min(position,Limit);ArrangeContent();};FrameClock.Frame+=Animate;}
    void Register(Control control){control.ControlAdded+=(o,e)=>Register(e.Control);if(!(control is SliderInput))control.MouseWheel+=Wheel;foreach(Control child in control.Controls)Register(child);}
    void Wheel(object sender,MouseEventArgs e){QueueScroll(e.Delta);var handled=e as HandledMouseEventArgs;if(handled!=null)handled.Handled=true;}
    internal void QueueScroll(int delta){target=Engine.Clamp(target-delta*.55,0,Limit);}
    void Drag(int y){target=Engine.Clamp((y-10)/(double)Math.Max(1,Height-20)*Limit,0,Limit);}
    public void ResetScroll(){position=target=0;ArrangeContent();}
    void ArrangeContent(){Content.SetBounds(1,1-(int)Math.Round(position),Math.Max(1,Width-18),DocumentHeight);Invalidate();}
    void Animate(double dt){double next=FrameClock.Approach(position,target,dt);if(next==position)return;position=next;ArrangeContent();}
    protected override void OnPaint(PaintEventArgs e){base.OnPaint(e);if(Limit<=0)return;int h=Height-20,thumb=Math.Max(28,(int)(h*Height/(double)DocumentHeight)),y=10+(int)((h-thumb)*position/Limit);CanvasInk.Box(e.Graphics,new Rectangle(Width-11,10,5,h),Color.FromArgb(42,42,42),3);CanvasInk.Box(e.Graphics,new Rectangle(Width-11,y,5,thumb),Color.FromArgb(163,163,163),3);}
    protected override void Dispose(bool disposing){if(disposing)FrameClock.Frame-=Animate;base.Dispose(disposing);}
}
}
