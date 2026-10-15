using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows.Forms;

namespace F1Manager {
public static class Theme {
    public static readonly Color Background=Color.FromArgb(12,14,18), Card=Color.FromArgb(24,27,33), Red=Color.FromArgb(235,35,43), Muted=Color.FromArgb(162,169,183), Border=Color.FromArgb(48,52,63);
    public static GraphicsPath Round(Rectangle r,int radius){var p=new GraphicsPath();int d=Math.Max(2,Math.Min(radius*2,Math.Min(r.Width,r.Height)));p.AddArc(r.X,r.Y,d,d,180,90);p.AddArc(r.Right-d,r.Y,d,d,270,90);p.AddArc(r.Right-d,r.Bottom-d,d,d,0,90);p.AddArc(r.X,r.Bottom-d,d,d,90,90);p.CloseFigure();return p;}
    public static Color Mix(Color a,Color b,double t){t=Math.Max(0,Math.Min(1,t));return Color.FromArgb((int)(a.R+(b.R-a.R)*t),(int)(a.G+(b.G-a.G)*t),(int)(a.B+(b.B-a.B)*t));}
    public static double Ease(double t){t=Math.Max(0,Math.Min(1,t));return 1-Math.Pow(1-t,3);}
    public static void ImageFit(Graphics g,Image image,Rectangle r){if(image==null)return;double scale=Math.Min(r.Width/(double)image.Width,r.Height/(double)image.Height);int w=(int)(image.Width*scale),h=(int)(image.Height*scale);g.DrawImage(image,new Rectangle(r.X+(r.Width-w)/2,r.Y+(r.Height-h)/2,w,h));}
}
public class GameSettings {
    public bool Fullscreen,Animations=true;
    public static GameSettings Load(string path){try{return File.Exists(path)?new JavaScriptSerializer().Deserialize<GameSettings>(File.ReadAllText(path))??new GameSettings():new GameSettings();}catch{return new GameSettings();}}
    public void Save(string path){Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,new JavaScriptSerializer().Serialize(this));}
}
public static class TeamLogos {
    static readonly Dictionary<int,Image> cache=new Dictionary<int,Image>();
    public static Image Get(int id){Image image;if(cache.TryGetValue(id,out image))return image;using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("team"+id)){if(stream!=null)using(var source=Image.FromStream(stream))image=new Bitmap(source);}cache[id]=image;return image;}
}
public partial class ManagerForm {
    internal string View="menu",settingsBack="menu";
    internal GameOverlay ActiveOverlay {get{return activeOverlay;}}
    internal GameSettings Preferences=new GameSettings();
    internal string SettingsPath=Path.Combine(Root,"Saves","settings.json");
    GameOverlay activeOverlay;
    readonly Dictionary<string,double> hoverValues=new Dictionary<string,double>();
    Point pointer=new Point(-1,-1);
    double scrollTarget,motionUntil,transitionStart=-100;
    Rectangle windowedBounds;
    FormWindowState windowedState;
    void InitializeShell(bool test){if(!test){Preferences=GameSettings.Load(SettingsPath);if(Preferences.Fullscreen)SetFullscreen(true,false);} }
    void StartTransition(){transitionStart=Preferences.Animations?watch.Elapsed.TotalSeconds:-100;motionUntil=watch.Elapsed.TotalSeconds+.3;pointer=new Point(-1,-1);hoverValues.Clear();Invalidate();}
    internal void SwitchView(string view){View=view;Scroll=0;scrollTarget=0;StartTransition();}
    internal void Play(){if(State==null)SwitchView("teams");else{SwitchView("game");Go("home");}}
    internal void OpenSettings(){settingsBack=View;SwitchView("settings");}
    void SavePreferences(){try{Preferences.Save(SettingsPath);}catch(Exception ex){Notify("Не удалось сохранить настройки: "+ex.Message);}}
    internal void SetFullscreen(bool enabled,bool save=true){if(enabled&&FormBorderStyle!=FormBorderStyle.None){windowedBounds=Bounds;windowedState=WindowState;WindowState=FormWindowState.Normal;FormBorderStyle=FormBorderStyle.None;Bounds=System.Windows.Forms.Screen.FromControl(this).Bounds;TopMost=true;}else if(!enabled&&FormBorderStyle==FormBorderStyle.None){TopMost=false;WindowState=FormWindowState.Normal;FormBorderStyle=FormBorderStyle.Sizable;if(windowedBounds.Width>0)Bounds=windowedBounds;WindowState=windowedState;}Preferences.Fullscreen=enabled;if(save)SavePreferences();StartTransition();}
    void DrawTransition(){double age=watch.Elapsed.TotalSeconds-transitionStart;if(!Preferences.Animations||age>=.24)return;int alpha=(int)(180*(1-Theme.Ease(age/.24)));using(var b=new SolidBrush(Color.FromArgb(alpha,bg)))g.FillRectangle(b,ClientRectangle);}
    void DrawMenuBackdrop(){Photograph(8,ClientRectangle);using(var b=new SolidBrush(Color.FromArgb(225,7,9,13)))g.FillRectangle(b,ClientRectangle);using(var gradient=new LinearGradientBrush(ClientRectangle,Color.FromArgb(145,71,13,21),Color.FromArgb(0,10,12,18),15f))g.FillRectangle(gradient,ClientRectangle);double phase=Preferences.Animations?watch.Elapsed.TotalSeconds*18:0;for(int i=0;i<7;i++){int x=(int)((phase+i*210)%(ClientSize.Width+700))-350;using(var p=new Pen(Color.FromArgb(18,235,35,43),2))g.DrawLine(p,x,0,x+380,ClientSize.Height);} }
    void DrawMainMenu(){DrawMenuBackdrop();int x=80,y=Math.Max(65,ClientSize.Height/2-245);TextAt("F1",x,y,ClientSize.Width-160,88,red,true,112);TextAt("MANAGER",x,y+106,ClientSize.Width-160,52,white,true,80);TextAt("ВАША КОМАНДА. ВАША ИСТОРИЯ.",x+3,y+195,ClientSize.Width-160,13,muted,true);Btn("Играть",x,y+253,310,Play,true,54);int next=y+321;if(State!=null){TextAt("Продолжить: "+Engine.Team(State,State.Player).Name+" / "+State.Year,x+4,y+311,550,12,muted);next+=29;}Btn("Настройки",x,next,310,OpenSettings,false,48);Btn("Выйти из игры",x,next+62,310,Close,false,48);if(State!=null)Btn("Новая карьера",x+330,next,200,()=>SwitchView("teams"),false,48);TextAt("DESKTOP CAREER 2.3  /  OFFLINE",x,ClientSize.Height-45,600,11,muted,true);TextAt("F11 — полный экран",ClientSize.Width-250,ClientSize.Height-45,220,11,muted);}
    void DrawSettings(){DrawMenuBackdrop();int x=80,w=Math.Min(850,ClientSize.Width-160);TextAt("НАСТРОЙКИ",x,70,w,38,white,true,60);TextAt("ЭКРАН И ДВИЖЕНИЕ",x,137,w,12,red,true);Box(x,193,w,125);TextAt("Полноэкранный режим",x+24,215,w-265,23,white,true,38);TextAt("Без рамки на весь экран. Быстрое переключение: F11.",x+24,262,w-265,13,muted,false,45);Btn(Preferences.Fullscreen?"Включён":"Выключен",x+w-190,230,165,()=>SetFullscreen(!Preferences.Fullscreen),Preferences.Fullscreen,45);Box(x,338,w,125);TextAt("Плавность интерфейса",x+24,360,w-265,23,white,true,38);TextAt("Мягкие переходы, подсветка кнопок и анимация панелей.",x+24,407,w-265,13,muted,false,45);Btn(Preferences.Animations?"Включена":"Уменьшена",x+w-190,375,165,()=>{Preferences.Animations=!Preferences.Animations;SavePreferences();StartTransition();},Preferences.Animations,45);Btn("Сохранить и вернуться",x,495,280,()=>{SavePreferences();SwitchView(settingsBack);},true,48);TextAt("Настройки сохраняются автоматически и не меняют карьеру.",x,565,w,13,muted);}
    void DrawTeamLogo(int id,Rectangle rect){var image=TeamLogos.Get(id);using(var path=Theme.Round(rect,8))using(var b=new SolidBrush(id>=9?Theme.Card:Color.White))g.FillPath(b,path);if(image!=null)Theme.ImageFit(g,image,new Rectangle(rect.X+5,rect.Y+5,rect.Width-10,rect.Height-10));else TextAt("+",rect.X+rect.Width/2-11,rect.Y+8,35,25,red,true,40);}
    void ShowOverlay(GameOverlay panel){if(activeOverlay!=null)throw new InvalidOperationException("Закройте текущее окно.");using(var frame=new Bitmap(ClientSize.Width,ClientSize.Height)){DrawToBitmap(frame,ClientRectangle);panel.SetBackdrop(frame);}activeOverlay=panel;panel.Dock=DockStyle.Fill;Controls.Add(panel);panel.BringToFront();panel.StartDisplay();panel.Focus();}
    void CloseOverlay(Action after){var current=activeOverlay;if(current==null)return;current.BeginClose(()=>{Controls.Remove(current);activeOverlay=null;current.Dispose();Invalidate();if(after!=null)after();});}
}

public class SoftButton:Button {
    readonly Timer timer=new Timer();double amount;bool hovered;public bool Primary;
    public SoftButton(){SetStyle(ControlStyles.UserPaint|ControlStyles.AllPaintingInWmPaint|ControlStyles.OptimizedDoubleBuffer,true);FlatStyle=FlatStyle.Flat;FlatAppearance.BorderSize=0;ForeColor=Color.White;BackColor=Theme.Card;Font=new Font("Segoe UI",13,FontStyle.Bold,GraphicsUnit.Pixel);Cursor=Cursors.Hand;timer.Interval=16;timer.Tick+=(o,e)=>{double target=hovered?1:0;amount+=(target-amount)*.22;if(Math.Abs(target-amount)<.01){amount=target;timer.Stop();}Invalidate();};MouseEnter+=(o,e)=>{hovered=true;timer.Start();};MouseLeave+=(o,e)=>{hovered=false;timer.Start();};}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;var color=Primary?Theme.Red:Color.FromArgb(40,44,54);using(var path=Theme.Round(new Rectangle(0,0,Width-1,Height-1),9))using(var b=new SolidBrush(Theme.Mix(color,Primary?Color.FromArgb(255,64,72):Color.FromArgb(65,70,84),amount)))g.FillPath(b,path);TextRenderer.DrawText(g,Text,Font,ClientRectangle,Enabled?ForeColor:Theme.Muted,TextFormatFlags.HorizontalCenter|TextFormatFlags.VerticalCenter|TextFormatFlags.EndEllipsis);if(Focused)using(var p=new Pen(Color.FromArgb(130,Color.White)))using(var path=Theme.Round(new Rectangle(3,3,Width-7,Height-7),7))g.DrawPath(p,path);}
    protected override void Dispose(bool disposing){if(disposing)timer.Dispose();base.Dispose(disposing);}
}
public class RoundedPanel:Panel {
    public RoundedPanel(){DoubleBuffered=true;BackColor=Theme.Card;Resize+=(o,e)=>{if(Width<2||Height<2)return;var previous=Region;using(var path=Theme.Round(new Rectangle(0,0,Width,Height),16))Region=new Region(path);if(previous!=null)previous.Dispose();};}
    protected override void OnPaint(PaintEventArgs e){e.Graphics.SmoothingMode=SmoothingMode.AntiAlias;using(var path=Theme.Round(new Rectangle(0,0,Width-1,Height-1),16))using(var b=new SolidBrush(BackColor))using(var p=new Pen(Theme.Border)){e.Graphics.FillPath(b,path);e.Graphics.DrawPath(p,path);}base.OnPaint(e);}
}
public abstract class GameOverlay:UserControl {
    protected readonly RoundedPanel Sheet=new RoundedPanel();
    protected readonly Stopwatch Clock=Stopwatch.StartNew();
    readonly Timer timer=new Timer();
    protected readonly bool Animated;
    Bitmap backdrop,sheetFrame;bool started;double closeAt=-1;Action closed;
    protected bool Closing {get{return closeAt>=0;}}
    protected virtual Size SheetSize {get{return new Size(1040,640);}}
    protected GameOverlay(bool animated){Animated=animated;SetStyle(ControlStyles.AllPaintingInWmPaint|ControlStyles.UserPaint|ControlStyles.OptimizedDoubleBuffer,true);BackColor=Theme.Background;TabStop=true;Sheet.Visible=false;Controls.Add(Sheet);timer.Interval=16;timer.Tick+=(o,e)=>{Arrange();Advance();Invalidate();if(closeAt>=0&&(!Animated||Clock.Elapsed.TotalSeconds-closeAt>=.16)){timer.Stop();var callback=closed;closed=null;if(callback!=null)callback();}};Resize+=(o,e)=>Arrange();}
    public void SetBackdrop(Bitmap image){backdrop=new Bitmap(image);}
    public void StartDisplay(){started=true;Clock.Restart();Arrange();timer.Start();}
    void CaptureSheet(){if(sheetFrame!=null)sheetFrame.Dispose();Sheet.Visible=true;sheetFrame=new Bitmap(Sheet.Width,Sheet.Height);Sheet.DrawToBitmap(sheetFrame,new Rectangle(Point.Empty,Sheet.Size));Sheet.Visible=false;}

    void Arrange(){int w=Math.Min(SheetSize.Width,Math.Max(300,Width-48)),h=Math.Min(SheetSize.Height,Math.Max(250,Height-48));double opening=Animated?Theme.Ease(Clock.Elapsed.TotalSeconds/.24):1;double closing=closeAt>=0&&Animated?Theme.Ease((Clock.Elapsed.TotalSeconds-closeAt)/.16):0;Sheet.Bounds=new Rectangle((Width-w)/2,(Height-h)/2+(int)((1-opening+closing)*22),w,h);if(!started)return;bool transitioning=Animated&&(opening<1||Closing);if(transitioning){if(sheetFrame==null||sheetFrame.Width!=w||sheetFrame.Height!=h)CaptureSheet();Sheet.Visible=false;}else {Sheet.Visible=true;if(sheetFrame!=null){sheetFrame.Dispose();sheetFrame=null;}}}
    protected override void OnPaint(PaintEventArgs e){if(backdrop!=null)e.Graphics.DrawImage(backdrop,ClientRectangle);double opacity=Animated?Theme.Ease(Clock.Elapsed.TotalSeconds/.24):1;if(closeAt>=0&&Animated)opacity*=1-Theme.Ease((Clock.Elapsed.TotalSeconds-closeAt)/.16);using(var b=new SolidBrush(Color.FromArgb((int)(190*opacity),3,5,9)))e.Graphics.FillRectangle(b,ClientRectangle);if(sheetFrame!=null){var matrix=new ColorMatrix();matrix.Matrix33=(float)Math.Max(0,Math.Min(1,opacity));using(var attributes=new ImageAttributes()){attributes.SetColorMatrix(matrix);e.Graphics.DrawImage(sheetFrame,Sheet.Bounds,0,0,sheetFrame.Width,sheetFrame.Height,GraphicsUnit.Pixel,attributes);}}}
    protected virtual void Advance(){}
    public virtual void Cancel(){}
    public void BeginClose(Action after){if(Closing)return;closed=after;if(Animated)CaptureSheet();closeAt=Clock.Elapsed.TotalSeconds;Sheet.Enabled=false;}
    protected Label LabelAt(Control host,string text,int x,int y,int w,int h,int size=13,bool bold=false,Color? color=null){var label=new Label{Text=text,Bounds=new Rectangle(x,y,w,h),ForeColor=color??Theme.Muted,BackColor=Color.Transparent,Font=new Font("Segoe UI",size,bold?FontStyle.Bold:FontStyle.Regular,GraphicsUnit.Pixel)};host.Controls.Add(label);return label;}
    protected override void Dispose(bool disposing){if(disposing){timer.Dispose();if(backdrop!=null)backdrop.Dispose();if(sheetFrame!=null)sheetFrame.Dispose();}base.Dispose(disposing);}
}
public class SimulationOverlay:GameOverlay {
    readonly Career state;readonly Task task;readonly string session,track;bool reported;
    public event Action<Career,Exception> Completed;
    protected override Size SheetSize {get{return new Size(610,305);}}
    public SimulationOverlay(Career career,string name,bool animated):base(animated){state=career;session=name;track=Data.Tracks[career.Round].Country;Sheet.Paint+=DrawSimulation;task=Task.Factory.StartNew(()=>Engine.SimulateSession(state));}
    protected override void Advance(){Sheet.Invalidate();if(!reported&&task.IsCompleted&&Clock.Elapsed.TotalSeconds>=1.1){reported=true;if(Completed!=null)Completed(task.IsFaulted?null:state,task.IsFaulted?task.Exception.GetBaseException():null);}}
    void DrawSimulation(object sender,PaintEventArgs e){var g=e.Graphics;g.SmoothingMode=SmoothingMode.AntiAlias;double time=Animated?Clock.Elapsed.TotalSeconds:0;using(var p=new Pen(Theme.Border,5))g.DrawEllipse(p,34,45,60,60);using(var p=new Pen(Theme.Red,5)){p.StartCap=p.EndCap=LineCap.Round;g.DrawArc(p,34,45,60,60,(float)(time*240)%360,250);}using(var font=new Font("Segoe UI",25,FontStyle.Bold,GraphicsUnit.Pixel))g.DrawString("Симулируем сессию",font,Brushes.White,115,43);using(var font=new Font("Segoe UI",14,FontStyle.Regular,GraphicsUnit.Pixel))using(var b=new SolidBrush(Theme.Muted))g.DrawString(session+"  /  "+track,font,b,118,84);var clip=g.Save();g.SetClip(new Rectangle(33,150,Sheet.Width-66,53));for(int i=0;i<8;i++){float x=(float)((time*310+i*92)%780)-160;using(var p=new Pen(i%2==0?Theme.Red:Color.FromArgb(140,Color.White),3))g.DrawLine(p,x,163+(i%3)*14,x+68,163+(i%3)*14);}g.Restore(clip);using(var font=new Font("Segoe UI",13,FontStyle.Regular,GraphicsUnit.Pixel))using(var b=new SolidBrush(Theme.Muted))g.DrawString("Рассчитываем круги, стратегию и итоговые позиции",font,b,34,245);}
}
}
