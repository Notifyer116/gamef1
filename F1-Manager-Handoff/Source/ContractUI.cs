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
public class ContractOverlay:GameOverlay {
    readonly Career state;readonly Person person;int start,seat,role;
    internal readonly OptionTile[] StartChoices,SeatChoices,RoleChoices;
    internal readonly SliderInput Salary=new SliderInput{Caption="Зарплата в год / млн $",DecimalPlaces=2,Increment=.01m},Years=new SliderInput{Caption="Срок / сезонов"},Severance=new SliderInput{Caption="Отступные / месяцев зарплаты"};
    internal readonly SoftButton ConfirmButton=new SoftButton{Text="Согласовать и подписать",Primary=true},BackButton=new SoftButton{Text="Назад"};
    internal readonly Label ErrorLabel;readonly Label quote;
    public event Action Signed,Cancelled;
    protected override Size SheetSize {get{return new Size(1000,650);}}
    public ContractOverlay(Career career,Person candidate,bool animated):base(animated){state=career;person=candidate;seat=candidate.Deal!=null&&candidate.Deal.Team==state.Player?candidate.Deal.Seat:0;role=candidate.Kind==0&&(candidate.Rating>=94||candidate.Titles>=2)?0:1;
        LabelAt(Sheet,"ПЕРЕГОВОРЫ / КОНТРАКТ",32,20,936,40,26,true,Color.White);var portrait=new PictureBox{Image=DriverPhotos.Get(person),SizeMode=PictureBoxSizeMode.Zoom,Bounds=new Rectangle(32,80,104,129),BackColor=Theme.Card};Sheet.Controls.Add(portrait);
        LabelAt(Sheet,person.Name,160,86,800,38,26,true,Color.White);LabelAt(Sheet,Data.Roles[person.Kind]+" / рейтинг "+person.Rating+" / "+person.Age+" лет",163,132,790,26,14);LabelAt(Sheet,person.Deal==null?"Свободный агент":Engine.Team(state,person.Deal.Team).Name+" / договор до "+person.Deal.Until,163,165,790,27,13);
        LabelAt(Sheet,"НАЧАЛО СОГЛАШЕНИЯ",32,228,460,22,11,true);StartChoices=new[]{new OptionTile("Сейчас",state.Year.ToString()),new OptionTile("Следующий сезон",(state.Year+1).ToString())};for(int i=0;i<2;i++){int k=i;StartChoices[i].Bounds=new Rectangle(32+i*228,253,216,57);StartChoices[i].Click+=(o,e)=>{start=k;UpdateQuote();};Sheet.Controls.Add(StartChoices[i]);}
        LabelAt(Sheet,"МЕСТО В СОСТАВЕ",32,329,460,22,11,true);int count=person.Kind==0||person.Kind==5?2:1;SeatChoices=Enumerable.Range(0,count).Select(i=>new OptionTile(count==2?"Место "+(i+1):Data.Roles[person.Kind])).ToArray();for(int i=0;i<count;i++){int k=i;SeatChoices[i].Bounds=new Rectangle(32+i*228,355,count==2?216:444,46);SeatChoices[i].Click+=(o,e)=>{seat=k;UpdateQuote();};Sheet.Controls.Add(SeatChoices[i]);}
        LabelAt(Sheet,"УСЛОВИЯ / СТАТУС",32,425,460,22,11,true);RoleChoices=(person.Kind==0?new[]{"Первый пилот","Равный статус","Второй пилот"}:new[]{Data.Roles[person.Kind]}).Select(t=>new OptionTile(t)).ToArray();for(int i=0;i<RoleChoices.Length;i++){int k=i;RoleChoices[i].Bounds=new Rectangle(32+i*150,452,RoleChoices.Length==1?444:144,52);RoleChoices[i].Click+=(o,e)=>{role=k;UpdateQuote();};Sheet.Controls.Add(RoleChoices[i]);}if(person.Kind!=0)role=0;
        var initial=Engine.Negotiate(state,person,state.Year,seat);Salary.SetRange(.1m,Math.Max(150m,(decimal)initial.Salary+50));Salary.Value=(decimal)initial.Salary;Years.SetRange(1,4);Years.Value=person.RetireYear>0?1:2;Severance.SetRange(3,24);Severance.Value=initial.MinSeverance;var sliders=new[]{Salary,Years,Severance};for(int i=0;i<sliders.Length;i++){sliders[i].Bounds=new Rectangle(520,228+i*76,448,62);Sheet.Controls.Add(sliders[i]);sliders[i].ValueChanged+=(o,e)=>UpdateQuote();}
        quote=LabelAt(Sheet,"",520,465,448,84,12);ErrorLabel=LabelAt(Sheet,"",32,550,936,30,12,false,Theme.Red);BackButton.Bounds=new Rectangle(32,587,150,42);ConfirmButton.Bounds=new Rectangle(615,587,353,42);Sheet.Controls.Add(BackButton);Sheet.Controls.Add(ConfirmButton);BackButton.Click+=(o,e)=>Cancel();ConfirmButton.Click+=(o,e)=>{try{Engine.Sign(state,person.Id,state.Year+start,seat,(double)Salary.Value,(int)Years.Value,(int)Severance.Value,RoleChoices.Length==1?Data.Roles[person.Kind]:new[]{"Первый пилот","Равный статус","Второй пилот"}[role]);if(Signed!=null)Signed();}catch(Exception ex){ErrorLabel.Text=ex.Message;}};UpdateQuote();
    }
    void UpdateQuote(){if(quote==null)return;foreach(var c in StartChoices)c.Selected=false;StartChoices[start].Selected=true;foreach(var c in SeatChoices)c.Selected=false;SeatChoices[seat].Selected=true;foreach(var c in RoleChoices)c.Selected=false;RoleChoices[role].Selected=true;foreach(var c in StartChoices.Concat(SeatChoices).Concat(RoleChoices))c.Invalidate();var q=Engine.Negotiate(state,person,state.Year+start,seat);double bonus=(double)Salary.Value*(start==0?.1:.05);quote.Text="Запрос от "+Engine.Money(q.Salary)+" / год • настрой "+q.Mood+"%\nВыкуп "+Engine.Money(q.Fee)+" • компенсация "+Engine.Money(q.Compensation)+"\nБонус "+Engine.Money(bonus)+" • к оплате "+Engine.Money(q.Fee+q.Compensation+bonus)+(person.RetireYear>0?"\nПоследний сезон: "+person.RetireYear:"");if(ErrorLabel!=null)ErrorLabel.Text="";}
    public override void Cancel(){if(!Closing&&Cancelled!=null)Cancelled();}
}
public partial class ManagerForm {
    internal void ContractDialog(Person person){if(activeOverlay!=null)return;var overlay=new ContractOverlay(State,person,Preferences.Animations);overlay.Cancelled+=()=>CloseOverlay(null);overlay.Signed+=()=>{SaveSilent();CloseOverlay(()=>Notify("Контракт подписан: "+person.Name));};ShowOverlay(overlay);}
}
}
