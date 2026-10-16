using System;
using System.Collections.Generic;
using System.Linq;
namespace F1Manager {
public class ScoutReport {public int Person,Year,Started,Due,Completed=-1;}
public class ScoutDepartment {public int Level=1;public List<ScoutReport> Reports=new List<ScoutReport>();}
public class ContractWishes {public double Salary,Bonus;public int Years,Severance;}
public class OfferAssessment {public double Score,AnnualCost,Upfront;public bool Accepted,Efficient;public string Message;}
public static class Scouting {
    public static int Capacity(Career s){return 1+(s.Scouting.Level-1)/2;}
    public static bool Own(Career s,Person p){return p.Deal!=null&&p.Deal.Team==s.Player;}
    public static bool Known(Career s,Person p){return Own(s,p)||s.Scouting.Reports.Any(r=>r.Person==p.Id&&r.Completed>=0&&r.Year==s.Year&&s.Day-r.Completed<=90);}
    public static ScoutReport Active(Career s,Person p){return s.Scouting.Reports.FirstOrDefault(r=>r.Person==p.Id&&r.Completed<0);}
    public static int Duration(Career s,Person p){return Math.Max(2,9-s.Scouting.Level+(p.Deal==null?-1:0));}
    public static double Price(Career s){return Math.Round(.12+.04*s.Scouting.Level,2);}
    public static void Start(Career s,int id){Engine.Lock(s);var p=Engine.Person(s,id);if(p.Retired)throw new Exception("Этот специалист завершил карьеру.");if(Known(s,p))throw new Exception("Требования уже известны.");if(Active(s,p)!=null)throw new Exception("Скаут уже работает с этим кандидатом.");if(s.Scouting.Reports.Count(r=>r.Completed<0)>=Capacity(s))throw new Exception("Все скауты заняты. Дождитесь отчёта или улучшите отдел.");double fee=Price(s);if(s.Cash<fee)throw new Exception("Недостаточно средств на работу скаута.");Engine.Pay(s,"Скаутинг / "+p.Name,-fee);s.Scouting.Reports.RemoveAll(r=>r.Person==id&&r.Completed>=0);s.Scouting.Reports.Add(new ScoutReport{Person=id,Year=s.Year,Started=s.Day,Due=s.Day+Duration(s,p)});Engine.Note(s,"Скаут изучает требования: "+p.Name+". Отчёт в день "+(s.Day+Duration(s,p))+".");}
    public static void Upgrade(Career s){Engine.Lock(s);if(s.Scouting.Level>=5)throw new Exception("Отдел скаутов достиг максимального уровня.");double cost=2+s.Scouting.Level*2;if(s.Cash<cost)throw new Exception("Недостаточно средств на отдел скаутов.");Engine.Pay(s,"Развитие отдела скаутов",-cost);s.Scouting.Level++;Engine.Note(s,"Отдел скаутов: уровень "+s.Scouting.Level+". Отчёты готовятся быстрее.");}
    public static void Advance(Career s){foreach(var r in s.Scouting.Reports.Where(r=>r.Completed<0&&r.Due<=s.Day)){r.Completed=s.Day;r.Year=s.Year;Engine.Note(s,"Отчёт скаута готов: "+Engine.Person(s,r.Person).Name+". Требования доступны в переговорах.");}}
    public static void NewSeason(Career s){foreach(var r in s.Scouting.Reports.Where(r=>r.Completed<0)){r.Started-=s.Day;r.Due=Math.Max(1,r.Due-s.Day);r.Year=s.Year;}}
    public static void Validate(Career s){if(s.Scouting==null||s.Scouting.Level<1||s.Scouting.Level>5||s.Scouting.Reports==null||s.Scouting.Reports.Count>10000||s.Scouting.Reports.Any(r=>r==null||!s.People.Any(p=>p.Id==r.Person)||r.Year<2026||r.Year>s.Year||r.Completed< -1||r.Due<r.Started||r.Completed>=0&&(r.Completed<r.Due||r.Year==s.Year&&r.Completed>s.Day))||s.Scouting.Reports.Where(r=>r.Completed<0).GroupBy(r=>r.Person).Any(g=>g.Count()>1)||s.Scouting.Reports.Count(r=>r.Completed<0)>Capacity(s))throw new Exception("Повреждены данные отдела скаутов.");}
}
public static class Negotiation {
    public const double GoodMin=100,GoodMax=112;
    public static ContractWishes Wishes(Career s,Person p,int start,int seat){var q=Engine.Negotiate(s,p,start,seat);int years=p.RetireYear>0?Math.Max(1,Math.Min(4,p.RetireYear-start+1)):p.Kind==0&&p.Age>=35?1:2+p.Id%2;return new ContractWishes{Salary=q.Salary,Years=years,Severance=q.MinSeverance,Bonus=Math.Round(q.Salary*(start>s.Year?.05:.1),2)};}
    public static OfferAssessment Assess(Career s,Person p,int start,int seat,double salary,int years,int severance,double bonus){var q=Engine.Negotiate(s,p,start,seat);var w=Wishes(s,p,start,seat);double fit=Math.Max(.5,1-Math.Abs(years-w.Years)*.12);double value=salary+bonus/Math.Max(1,years)+w.Salary*.12*(severance/(double)w.Severance-1)+w.Salary*.08*(fit-1);double score=100*value/Math.Max(.01,w.Salary+w.Bonus/w.Years);bool accepted=salary>=w.Salary*.55&&severance>=3&&score+.001>=GoodMin;return new OfferAssessment{Score=score,Accepted=accepted,Efficient=accepted&&score<=GoodMax,AnnualCost=salary+bonus/Math.Max(1,years),Upfront=q.Fee+q.Compensation+bonus,Message=!accepted?"Предложение ниже ожиданий":score<=GoodMax?"Условия подходят обеим сторонам":"Кандидат согласен / можно снизить расходы"};}
    public static void ValidateOffer(double salary,int years,int severance,double bonus){if(double.IsNaN(salary)||double.IsInfinity(salary)||salary<0||salary>200||years<1||years>4||severance<3||severance>24||double.IsNaN(bonus)||double.IsInfinity(bonus)||bonus<0||bonus>200)throw new Exception("Проверьте условия предложения.");}
}
}
