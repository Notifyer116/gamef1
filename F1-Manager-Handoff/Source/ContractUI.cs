using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace F1Manager {
public class OptionTile:GameChoice {
    readonly string title,subtitle;
    public OptionTile(string text,string detail=""){title=text;subtitle=detail;AccessibleName=text;}
    protected override void DrawChoice(Graphics g){TextLine(g,title,new Rectangle(12,subtitle.Length==0?4:6,Width-24,subtitle.Length==0?Height-8:24),13,Color.White,true);if(subtitle.Length>0)TextLine(g,subtitle,new Rectangle(12,31,Width-24,20),11,Theme.Muted);}
}
public class OfferGauge:Control {
    OfferAssessment offer;double displayed;bool initialized;public OfferAssessment Offer {get{return offer;}set{offer=value;if(!initialized&&value!=null){displayed=value.Score;initialized=true;}Invalidate();}}
    public OfferGauge(){DoubleBuffered=true;BackColor=Theme.Card;FrameClock.Frame+=Animate;}
    void Animate(double dt){if(offer==null)return;double next=FrameClock.Approach(displayed,offer.Score,dt);if(next==displayed)return;displayed=next;Invalidate();}
    protected override void Dispose(bool disposing){if(disposing)FrameClock.Frame-=Animate;base.Dispose(disposing);}
    protected override void OnPaint(PaintEventArgs e){var g=e.Graphics;g.Clear(BackColor);if(Offer==null)return;g.SmoothingMode=System.Drawing.Drawing2D.SmoothingMode.AntiAlias;CanvasInk.Text(g,Offer.Message,new Rectangle(0,0,Width,24),14,Color.White,true);int left=12,right=Width-12,y=43;CanvasInk.Box(g,new Rectangle(left,y,right-left,12),Color.FromArgb(51,51,51),6);int a=left+(int)((right-left)*Negotiation.GoodMin/150),b=left+(int)((right-left)*Negotiation.GoodMax/150);CanvasInk.Box(g,new Rectangle(a,y,b-a,12),Color.FromArgb(100,179,141),5);int x=left+(int)((right-left)*Math.Max(0,Math.Min(150,displayed))/150);using(var brush=new SolidBrush(Color.White))g.FillPolygon(brush,new[]{new Point(x,y-3),new Point(x-6,y-12),new Point(x+6,y-12)});CanvasInk.Text(g,"Ваше предложение: "+Offer.Score.ToString("0.0")+" / выгодный диапазон 100–112",new Rectangle(0,60,Width,23),12,Theme.Muted);}
}
public class ContractOverlay:GameOverlay {
    readonly Career state;readonly Person person;int start,seat,role;
    internal readonly OptionTile[] StartChoices,SeatChoices,RoleChoices;
    internal readonly SliderInput Salary=new SliderInput{Caption="Зарплата в год / млн $",DecimalPlaces=2,Increment=.01m},Years=new SliderInput{Caption="Срок / сезонов"},Severance=new SliderInput{Caption="Отступные / месяцев зарплаты"},Bonus=new SliderInput{Caption="Разовый бонус / млн $",DecimalPlaces=2,Increment=.01m};
    internal readonly OfferGauge Gauge=new OfferGauge();
    internal readonly SoftButton ConfirmButton=new SoftButton{Text="Согласовать и подписать",Primary=true},BackButton=new SoftButton{Text="Назад"};
    internal readonly Label ErrorLabel;readonly Label quote,wishes;
    public event Action Signed,Cancelled;
    protected override Size SheetSize {get{return new Size(1060,700);}}
    public ContractOverlay(Career career,Person candidate,bool animated):base(animated){state=career;person=candidate;seat=candidate.Deal!=null&&candidate.Deal.Team==state.Player?candidate.Deal.Seat:0;role=candidate.Kind==0&&(candidate.Rating>=94||candidate.Titles>=2)?0:1;
        LabelAt(Sheet,"ПЕРЕГОВОРЫ / КОНТРАКТ",32,18,990,40,26,true,Color.White);var portrait=new PictureBox{Image=DriverPhotos.Get(person),SizeMode=PictureBoxSizeMode.Zoom,Bounds=new Rectangle(32,75,92,104),BackColor=Theme.Card};Sheet.Controls.Add(portrait);
        LabelAt(Sheet,person.Name,145,78,850,36,25,true,Color.White);LabelAt(Sheet,Data.Roles[person.Kind]+" / рейтинг "+person.Rating+" / "+person.Age+" лет",147,120,350,25,14);LabelAt(Sheet,Scouting.Own(state,person)?"Требования известны внутри команды":"Отчёт скаута / требования изучены",147,150,350,25,12);
        wishes=LabelAt(Sheet,"",526,117,502,60,13,false,Color.FromArgb(219,219,219));
        LabelAt(Sheet,"НАЧАЛО СОГЛАШЕНИЯ",32,190,460,22,12,true);StartChoices=new[]{new OptionTile("Сейчас",state.Year.ToString()),new OptionTile("Следующий сезон",(state.Year+1).ToString())};for(int i=0;i<2;i++){int k=i;StartChoices[i].Bounds=new Rectangle(32+i*228,214,216,57);StartChoices[i].Click+=(o,e)=>{start=k;UpdateQuote();};Sheet.Controls.Add(StartChoices[i]);}
        LabelAt(Sheet,"МЕСТО В СОСТАВЕ",32,286,460,22,12,true);int count=person.Kind==0||person.Kind==5?2:1;SeatChoices=Enumerable.Range(0,count).Select(i=>new OptionTile(count==2?"Место "+(i+1):Data.Roles[person.Kind])).ToArray();for(int i=0;i<count;i++){int k=i;SeatChoices[i].Bounds=new Rectangle(32+i*228,311,count==2?216:444,46);SeatChoices[i].Click+=(o,e)=>{seat=k;UpdateQuote();};Sheet.Controls.Add(SeatChoices[i]);}
        LabelAt(Sheet,"УСЛОВИЯ / СТАТУС",32,378,460,22,12,true);RoleChoices=(person.Kind==0?new[]{"Первый пилот","Равный статус","Второй пилот"}:new[]{Data.Roles[person.Kind]}).Select(t=>new OptionTile(t)).ToArray();for(int i=0;i<RoleChoices.Length;i++){int k=i;RoleChoices[i].Bounds=new Rectangle(32+i*150,403,RoleChoices.Length==1?444:144,45);RoleChoices[i].Click+=(o,e)=>{role=k;UpdateQuote();};Sheet.Controls.Add(RoleChoices[i]);}if(person.Kind!=0)role=0;
        var initial=Negotiation.Wishes(state,person,state.Year,seat);Salary.SetRange(.1m,Math.Min(200m,Math.Max(150m,(decimal)initial.Salary+50)));Salary.Value=(decimal)initial.Salary;Years.SetRange(1,4);Years.Value=initial.Years;Severance.SetRange(3,24);Severance.Value=initial.Severance;Bonus.SetRange(0,Math.Min(200m,Math.Max(20m,(decimal)initial.Salary*2)));Bonus.Value=(decimal)initial.Bonus;var sliders=new[]{Salary,Years,Severance,Bonus};for(int i=0;i<sliders.Length;i++){sliders[i].Bounds=new Rectangle(526,190+i*63,502,57);Sheet.Controls.Add(sliders[i]);sliders[i].ValueChanged+=(o,e)=>UpdateQuote();}
        quote=LabelAt(Sheet,"",32,466,454,72,13);Gauge.Bounds=new Rectangle(526,455,502,87);Sheet.Controls.Add(Gauge);ErrorLabel=LabelAt(Sheet,"",32,550,996,31,13,false,Color.FromArgb(244,207,149));Sheet.Controls.Add(BackButton);Sheet.Controls.Add(ConfirmButton);Sheet.Resize+=(o,e)=>{BackButton.Bounds=new Rectangle(32,Sheet.Height-64,150,42);ConfirmButton.Bounds=new Rectangle(Sheet.Width-395,Sheet.Height-64,363,42);ErrorLabel.SetBounds(32,Sheet.Height-110,Sheet.Width-64,32);};BackButton.Click+=(o,e)=>Cancel();ConfirmButton.Click+=(o,e)=>{try{Engine.Sign(state,person.Id,state.Year+start,seat,(double)Salary.Value,(int)Years.Value,(int)Severance.Value,RoleChoices.Length==1?Data.Roles[person.Kind]:new[]{"Первый пилот","Равный статус","Второй пилот"}[role],(double)Bonus.Value);if(Signed!=null)Signed();}catch(Exception ex){ErrorLabel.Text=ex.Message;}};UpdateQuote();
    }
    void UpdateQuote(){if(quote==null)return;foreach(var c in StartChoices)c.Selected=false;StartChoices[start].Selected=true;foreach(var c in SeatChoices)c.Selected=false;SeatChoices[seat].Selected=true;foreach(var c in RoleChoices)c.Selected=false;RoleChoices[role].Selected=true;foreach(var c in StartChoices.Concat(SeatChoices).Concat(RoleChoices))c.Invalidate();var w=Negotiation.Wishes(state,person,state.Year+start,seat);bool known=Scouting.Known(state,person);wishes.Text=known?"Пожелания: "+Engine.Money(w.Salary)+" / год, "+w.Years+" сез.\nОтступные "+w.Severance+" мес. / бонус "+Engine.Money(w.Bonus):"Пожелания неизвестны / поручите изучение скауту";var q=Engine.Negotiate(state,person,state.Year+start,seat);var a=Negotiation.Assess(state,person,state.Year+start,seat,(double)Salary.Value,(int)Years.Value,(int)Severance.Value,(double)Bonus.Value);Gauge.Offer=known?a:null;Gauge.Invalidate();quote.Text="Выкуп "+Engine.Money(q.Fee)+" / компенсация "+Engine.Money(q.Compensation)+"\nСейчас к оплате: "+Engine.Money(a.Upfront)+"\nСредние расходы в год: "+Engine.Money(a.AnnualCost);if(ErrorLabel!=null)ErrorLabel.Text="";}
    public override void Cancel(){if(!Closing&&Cancelled!=null)Cancelled();}
}
public partial class ManagerForm {
    internal void ContractDialog(Person person){if(activeOverlay!=null)return;if(!Scouting.Known(State,person)){ScoutCandidate=person.Id;PeopleKind=person.Kind;Go("scouting");Notify("Поручите скауту изучить требования: "+person.Name);return;}var overlay=new ContractOverlay(State,person,Preferences.Animations);overlay.Cancelled+=()=>CloseOverlay(null);overlay.Signed+=()=>{SaveSilent();CloseOverlay(()=>Notify("Контракт подписан: "+person.Name));};ShowOverlay(overlay);}
}
}
