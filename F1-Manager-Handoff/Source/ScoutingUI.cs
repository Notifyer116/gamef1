using System;
using System.Drawing;
using System.Linq;
namespace F1Manager {
public partial class ManagerForm {
    int ScoutCandidate=-1;
    int ScoutingPage(){Heading("TALENT & SCOUTING","Отдел скаутов");var s=State;int y=193,active=s.Scouting.Reports.Count(r=>r.Completed<0);Box(X,y,W,114);TextAt("Уровень "+s.Scouting.Level+" / скауты "+active+" из "+Scouting.Capacity(s)+" заняты",X+18,y+13,W-325,23,white,true,35);TextAt("Скаут узнаёт пожелания по зарплате, сроку, бонусу и отступным.\\nТребования своей команды известны сразу; отчёт рынка действует 90 дней.".Replace("\\n","\n"),X+19,y+54,W-337,14,muted,false,50);if(s.Scouting.Level<5)Btn("Улучшить / "+Engine.Money(2+s.Scouting.Level*2),X+W-306,y+34,288,()=>Scouting.Upgrade(s),true,42);y+=136;
        foreach(var report in s.Scouting.Reports.Where(r=>r.Completed<0).OrderBy(r=>r.Due)){Box(X,y,W,61);TextAt(Engine.Person(s,report.Person).Name,X+16,y+9,W-260,18,white,true,28);TextAt("Отчёт в день "+report.Due+" / осталось "+Math.Max(0,report.Due-s.Day)+" дн.",X+16,y+36,W-260,12,muted);Btn("Календарь",X+W-212,y+12,194,()=>Go("calendar"),false,36);y+=73;}
        int bw=(W-25)/6;for(int i=0;i<6;i++){int k=i;Btn(Data.Roles[i],X+i*(bw+5),y,bw,()=>{PeopleKind=k;Scroll=0;scrollTarget=0;},PeopleKind==i,42);}y+=65;
        foreach(var p in s.People.Where(p=>p.Kind==PeopleKind&&!p.Retired&&!Scouting.Own(s,p)).OrderByDescending(p=>p.Id==ScoutCandidate).ThenByDescending(p=>Scouting.Known(s,p)).ThenByDescending(p=>p.Rating)){bool known=Scouting.Known(s,p);var pending=Scouting.Active(s,p);Box(X,y,W,115);int inset=p.Kind==0?93:18;if(p.Kind==0)DriverPortrait(p,new Rectangle(X+12,y+10,65,90));TextAt(p.Name,X+inset,y+12,W-inset-238,21,white,true,31);string team=p.Deal==null?"Свободный агент":Engine.Team(s,p.Deal.Team).Name;TextAt(team+" / рейтинг "+p.Rating+" / "+p.Age+" лет",X+inset,y+47,W-inset-230,13,muted);string detail="Пожелания пока неизвестны";if(known){var w=Negotiation.Wishes(s,p,s.Year,0);detail=Engine.Money(w.Salary)+" / год · "+w.Years+" сез. · бонус "+Engine.Money(w.Bonus);}else if(pending!=null)detail="Скаут работает / отчёт в день "+pending.Due;TextAt(detail,X+inset,y+77,W-inset-230,13,known?white:muted);if(known)Btn("Переговоры",X+W-213,y+34,195,()=>ContractDialog(p),true,43);else if(pending==null)Btn("Изучить / "+Engine.Money(Scouting.Price(s)),X+W-213,y+34,195,()=>Scouting.Start(s,p.Id),true,43);else TextAt("В работе",X+W-205,y+39,180,16,muted,true,30);y+=128;}return y+20;
    }
}
}
