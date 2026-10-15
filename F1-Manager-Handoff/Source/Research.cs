using System;
using System.Linq;
using System.Collections.Generic;
namespace F1Manager {
// Every team uses the same component catalogue, engineering constraints and annual cap.
public static class Research {
    public const double Carryover=.65;
    public static int UnlockDay {get{return Data.Tracks[10].Day;}}
    public static bool NextYearOpen(Career s){return s.Day>=UnlockDay&&!s.SeasonDone&&s.Year<2200;}
    public static double Spent(Career s,Team t){return t.Id==s.Player?s.Spent:t.ResearchSpent;}
    public static void PlanDay(Career s){if(s.SeasonDone)return;foreach(var team in s.Teams.Where(t=>t.Id!=s.Player)){
        if(s.Day<team.NextResearchDay)continue;team.NextResearchDay=s.Day+5+(int)(Engine.Rand(s)*5);
        int free=Engine.Capacity(team)-Engine.Assigned(s,team);if(free<2||team.Projects.Count>=Engine.Slots(team)||team.ResearchSpent>=95)continue;
        double progress=Engine.Clamp((s.Day-UnlockDay)/135.0,0,1);int standing=1+s.Teams.Count(t=>t.Points>team.Points);
        bool future=NextYearOpen(s)&&Engine.Rand(s)<.25+progress*.5+(standing>s.Teams.Count/2?.12:0);
        int engineers=Math.Min(free,4+(int)(Engine.Rand(s)*3));var metrics=CarModel.Values(team);
        var weights=future?Enumerable.Repeat(.1,10).ToArray():Enumerable.Range(Math.Min(s.Round,22),Math.Min(3,23-Math.Min(s.Round,22))).Select(i=>CarModel.Profile(Data.Tracks[i]).Importance).Aggregate(new double[10],(a,w)=>a.Select((v,i)=>v+w[i]).ToArray());
        var options=Data.Parts.Where(p=>!team.Projects.Any(q=>q.Key==p.Key&&(q.TargetYear==0?s.Year:q.TargetYear)==s.Year+(future?1:0))).Select(p=>new{Part=p,Project=Engine.EstimateFor(s,team,p,engineers,future)}).Where(p=>team.ResearchSpent+p.Project.Cost<=95).Select(p=>new{p.Part,p.Project,Value=CarModel.Effects(p.Part.Key,p.Project.Gain).Select((v,i)=>v*weights[i]*(1+Math.Max(0,85-metrics[i])*.035)).Sum()/p.Project.Cost*(.8+Engine.Rand(s)*.5)}).OrderByDescending(p=>p.Value).FirstOrDefault();
        if(options==null)continue;team.ResearchSpent=Math.Round(team.ResearchSpent+options.Project.Cost,2);team.Projects.Add(options.Project);
    }}
    public static void Adopt(Career s,Team team){foreach(var next in team.NextParts){var before=CarModel.Values(team);var old=team.Parts.FirstOrDefault(p=>p.Key==next.Key);if(old==null){old=new Installed{Key=next.Key,Version=0};team.Parts.Add(old);}old.Bonus+=next.Bonus;old.Version+=next.Version;
        if(team.Id==s.Player){var trials=s.UpgradeTrials.Where(u=>u.Year==s.Year&&u.Key==next.Key&&!u.BenchFailed&&!u.Reported).ToArray();var after=CarModel.Values(team);double total=trials.Sum(t=>t.ActualGain);foreach(var trial in trials){trial.Delta=after.Select((v,i)=>(v-before[i])*(total>0?trial.ActualGain/total:0)).ToArray();trial.Status="Ожидает проверки на трассе";}}}
        team.NextParts.Clear();
    }
    public static void ReconcileWorkforce(Career s){foreach(var team in s.Teams){int capacity=Engine.Capacity(team),count=team.Projects.Count;long total=team.Projects.Sum(p=>(long)p.Engineers);if(total<=capacity)continue;if(count>capacity)throw new Exception("Число проектов превышает штат инженеров.");int available=capacity-count;var allocations=team.Projects.Select(p=>1+(int)((p.Engineers-1)/(double)(total-count)*available)).ToArray();int remaining=capacity-allocations.Sum();for(int i=0;i<remaining;i++)allocations[i%count]++;for(int i=0;i<count;i++)team.Projects[i].Engineers=allocations[i];if(team.Id==s.Player)Engine.Note(s,"Патч 2.4.2: инженеры перераспределены между проектами в пределах штата ("+capacity+"). Сроки, стоимость и результаты разработки сохранены.");}}
    public static void ValidateWorkforce(Career s){if(s.Teams.Any(t=>t.Projects.Sum(p=>(long)p.Engineers)>Engine.Capacity(t)))throw new Exception("В проектах назначено больше инженеров, чем есть в штате.");}
    static bool Bad(Installed p){return p==null||!Data.Parts.Any(c=>c.Key==p.Key)||double.IsNaN(p.Bonus)||double.IsInfinity(p.Bonus)||p.Bonus<0||p.Version<0;}
    public static void Validate(Career s){foreach(var team in s.Teams){if(team.NextParts==null||team.NextParts.Any(Bad)||team.Parts.Any(Bad)||team.NextParts.GroupBy(p=>p.Key).Any(g=>g.Count()>1)||double.IsNaN(team.ResearchSpent)||double.IsInfinity(team.ResearchSpent)||team.ResearchSpent<0||team.ResearchSpent>95.001||team.Projects.Any(p=>p.TargetYear!=0&&p.TargetYear!=s.Year&&p.TargetYear!=s.Year+1))throw new Exception("Повреждены данные разработки болида.");}}
}
}
