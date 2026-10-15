using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
namespace F1Manager {
public class UpgradeReview {
    public string Key,Status;public int Year,Day,Version;public double ExpectedGain,ActualGain;public bool BenchFailed,Reported;
    public double[] Delta=new double[10];
}
public class EngineeringReport {
    public int Year,Round,OverallRank;public string Track,Layout;
    public double[] Values=new double[10],Change=new double[10];public int[] Ranks=new int[10];
    public double TrackScore;public List<UpgradeReview> Upgrades=new List<UpgradeReview>();
}
public class CircuitProfile {
    public string Layout;public double[] Importance;public int EngineHeat,TyreHeat;
    public CircuitProfile(string layout,int engine,int tyres,params double[] weights){Layout=layout;EngineHeat=engine;TyreHeat=tyres;double total=weights.Sum();Importance=weights.Select(w=>w/total).ToArray();}
}
public static class CarModel {
    public static readonly string[] Names={"Прижимная сила","Медленные повороты","Средние повороты","Быстрые повороты","Скорость на прямых","Сохранение шин","Охлаждение двигателя","Охлаждение шин","Надёжность","Мощность двигателя"};
    public static readonly string[] Groups={"Аэродинамика","Шасси и кузов","Подвеска и тормоза","Охлаждение и системы"};
    // Fixed game profiles: corner mix, straights and thermal load differ at every venue.
    // The weights are balancing parameters, not official circuit telemetry.
    public static readonly CircuitProfile[] Circuits={
        new CircuitProfile("Альберт-Парк / быстрые связки и разгоны",74,72,11,8,14,19,16,10,6,6,4,6),
        new CircuitProfile("Шанхай / затяжные дуги и длинная прямая",73,82,12,11,14,12,17,14,5,6,3,6),
        new CircuitProfile("Сузука / S-связки, быстрые повороты",70,86,16,5,18,22,10,12,4,6,3,4),
        new CircuitProfile("Майами / медленный сектор и быстрые разгоны",90,86,10,16,9,10,17,10,10,8,4,6),
        new CircuitProfile("Монреаль / торможения, шиканы и прямые",75,65,7,16,11,6,23,8,7,7,6,9),
        new CircuitProfile("Монако / шпилька, медленные повороты",82,58,20,30,11,3,4,10,8,6,6,2),
        new CircuitProfile("Барселона / длинные нагруженные повороты",82,91,16,8,18,18,9,14,5,6,3,3),
        new CircuitProfile("Шпильберг / короткий круг и набор скорости",73,76,8,12,12,11,22,10,6,6,5,8),
        new CircuitProfile("Сильверстоун / скоростные смены направления",70,89,17,4,16,25,11,12,4,5,3,3),
        new CircuitProfile("Спа / длинные прямые и скоростной сектор",65,73,11,6,12,21,22,9,4,4,4,7),
        new CircuitProfile("Хунгароринг / связки без длинных прямых",88,86,20,19,22,9,5,10,5,5,3,2),
        new CircuitProfile("Зандворт / профилированные быстрые дуги",73,84,20,9,18,21,5,13,4,5,3,2),
        new CircuitProfile("Монца / минимум сопротивления и шиканы",79,67,4,10,7,7,31,8,8,6,6,13),
        new CircuitProfile("Мадрид / городские торможения и длинная дуга",88,88,15,16,17,12,11,11,7,6,3,2),
        new CircuitProfile("Баку / узкий сектор и сверхдлинная прямая",79,65,6,22,8,4,29,6,8,5,5,7),
        new CircuitProfile("Сахир / тяга на выходе и абразивный асфальт",90,94,9,18,12,7,16,16,8,7,3,4),
        new CircuitProfile("Сингапур / медленные связки и жара",96,92,18,27,16,4,5,11,8,7,3,1),
        new CircuitProfile("Остин / быстрый первый и медленный третий сектор",85,86,13,14,16,18,10,12,6,5,3,3),
        new CircuitProfile("Мехико / высота, прямые и дефицит охлаждения",98,79,13,13,10,9,18,8,13,7,3,6),
        new CircuitProfile("Интерлагос / перепады высоты и длинный разгон",79,82,12,11,18,14,17,10,5,5,4,4),
        new CircuitProfile("Лас-Вегас / холодные прямые и торможения",48,48,4,15,8,5,32,8,5,8,5,10),
        new CircuitProfile("Лусаил / быстрые дуги и высокая нагрузка шин",92,98,17,3,15,25,8,15,6,7,2,2),
        new CircuitProfile("Яс-Марина / два разгона и медленные повороты",86,84,10,18,13,9,19,10,7,6,3,5)
    };
    static readonly Dictionary<string,double[]> effects=new Dictionary<string,double[]>{
        {"front",new[]{1,.65,.5,.35,-.12,.1,0,0,.05,0}},
        {"rear",new[]{1,.15,.5,.9,-.25,0,0,0,0,0}},
        {"floor",new[]{.9,.55,.75,1,.2,.15,0,0,0,0}},
        {"diffuser",new[]{.8,.2,.6,.9,.25,.1,0,0,0,0}},
        {"mono",new[]{0,.9,.6,.5,.3,.3,0,0,.2,0}},
        {"nose",new[]{.5,.55,.35,.2,.3,0,.05,0,0,0}},
        {"sidepods",new[]{.2,.15,.2,.2,.4,0,.8,.2,.1,0}},
        {"suspF",new[]{0,.8,.6,.4,0,1,0,.5,.15,0}},
        {"suspR",new[]{0,.65,.65,.4,.15,1,0,.4,.2,0}},
        {"brakes",new[]{0,.6,.3,.1,0,.5,0,1,.3,0}},
        {"radiators",new[]{0,0,0,0,-.12,.1,1,.35,.6,0}},
        {"ducts",new[]{.15,0,.1,.1,-.05,.25,.65,.8,.3,0}},
        {"gearbox",new[]{0,.3,.2,.1,.1,0,.1,0,1,0}},
        {"electronics",new[]{0,.25,.15,.1,.1,.45,.4,.35,.9,0}},
        {"hydraulics",new[]{0,.35,.3,.1,0,.2,.1,0,1,0}}
    };
    sealed class Cached {
        public Cached(){}
        public double[] Input,Values,Bonuses;public string[] Keys;public double Power,Reliability;
        public bool Matches(Team t){return Input!=null&&Input.SequenceEqual(t.Base)&&Power==t.EngineRating&&Reliability==t.EngineReliability&&Keys.Length==t.Parts.Count&&t.Parts.Select(p=>p.Key).SequenceEqual(Keys)&&t.Parts.Select(p=>p.Bonus).SequenceEqual(Bonuses);}
    }
    static readonly ConditionalWeakTable<Team,Cached> cache=new ConditionalWeakTable<Team,Cached>();
    static double[] Raw(Team t){var c=cache.GetOrCreateValue(t);if(!c.Matches(t)){c.Input=(double[])t.Base.Clone();c.Power=t.EngineRating;c.Reliability=t.EngineReliability;c.Keys=t.Parts.Select(p=>p.Key).ToArray();c.Bonuses=t.Parts.Select(p=>p.Bonus).ToArray();double a=t.Base[0],h=t.Base[2],s=t.Base[3],cool=t.Base[4];var v=new[]{a,.5*h+.35*s+.15*a,.45*a+.3*h+.25*s,.65*a+.2*h+.15*s,.55*t.EngineRating+.3*a+.15*h,.6*s+.3*h+.1*cool,cool,.6*cool+.4*s,.5*(t.Base[5]+t.EngineReliability),t.EngineRating};foreach(var p in t.Parts){var e=effects[p.Key];for(int i=0;i<v.Length;i++)v[i]+=p.Bonus*e[i];}c.Values=v.Select(x=>Engine.Clamp(x,35,99)).ToArray();}return c.Values;}
    public static double[] Values(Team t){return (double[])Raw(t).Clone();}
    public static double Metric(Team t,int metric){return Raw(t)[metric];}
    public static double[] Effects(string key,double gain){return effects[key].Select(v=>v*gain).ToArray();}
    public static int Group(string key){return new[]{"front","rear","floor","diffuser"}.Contains(key)?0:new[]{"mono","nose","sidepods"}.Contains(key)?1:new[]{"suspF","suspR","brakes"}.Contains(key)?2:3;}
    public static CircuitProfile Profile(Track track){int index=Array.IndexOf(Data.Tracks,track);return Circuits[index<0?0:index];}
    public static double Score(Team t,Track track){var v=Raw(t);var weights=Profile(track).Importance;double total=0;for(int i=0;i<v.Length;i++)total+=v[i]*weights[i];return total;}
    public static int Rank(Career s,int metric){var own=Engine.Team(s,s.Player);double mine=Metric(own,metric);return 1+s.Teams.Count(t=>Metric(t,metric)>mine+.00001);}
    public static int TrackRank(Career s,Track track){double mine=Score(Engine.Team(s,s.Player),track);return 1+s.Teams.Count(t=>Score(t,track)>mine+.00001);}
    public static string ImpactText(string key,double gain,int limit=4){var v=Effects(key,gain);var order=Enumerable.Range(0,v.Length).Where(i=>v[i]!=0).OrderByDescending(i=>Math.Abs(v[i])).Take(limit).ToArray();return string.Join("  •  ",order.Select(i=>Names[i]+" "+v[i].ToString("+0.00;-0.00;0")));}
}
public static class Engineering {
    public static void BenchFailure(Career s,Team t,Project project){if(t.Id!=s.Player)return;s.UpgradeTrials.Add(new UpgradeReview{Key=project.Key,Year=s.Year,Day=s.Day,ExpectedGain=project.Gain,BenchFailed=true,Status="Не прошла стенд • не установлена"});}
    public static void Install(Career s,Team t,Project project,Installed part){var before=CarModel.Values(t);double quality=(Engine.Quality(s,t.Id,3)+Engine.Quality(s,t.Id,4))*.5;double miss=Math.Max(.015,.10-(quality-65)*.0015-(t.Facilities[1].Level-1)*.012);double factor=Engine.Rand(s)<miss?0:.5+Engine.Rand(s)*.65+Math.Max(0,quality-75)*.002+(t.Facilities[1].Level-1)*.015;double actual=Math.Round(project.Gain*factor,3);part.Bonus+=actual;if(t.Id==s.Player){var after=CarModel.Values(t);s.UpgradeTrials.Add(new UpgradeReview{Key=project.Key,Version=part.Version,Year=s.Year,Day=s.Day,ExpectedGain=project.Gain,ActualGain=actual,Delta=after.Select((v,i)=>v-before[i]).ToArray(),Status="Ожидает проверки на трассе"});}}
    public static string Verdict(UpgradeReview trial){if(trial.BenchFailed)return "Не прошла стенд • не установлена";if(trial.ActualGain<=.01)return "Не сработала • нет прироста";if(trial.Delta.All(v=>v<=.001))return "Прирост ограничен пределом болида";double ratio=trial.ActualGain/Math.Max(.001,trial.ExpectedGain);return ratio<.75?"Ниже расчёта":ratio>1.1?"Выше расчёта":"Прирост подтверждён";}
    public static void CompleteRound(Career s){var team=Engine.Team(s,s.Player);var track=Data.Tracks[s.Round];var current=CarModel.Values(team);var baseline=s.LastCarMetrics??current;var report=new EngineeringReport{Year=s.Year,Round=s.Round,Track=track.Country,Layout=CarModel.Profile(track).Layout,Values=current,Change=current.Select((v,i)=>v-baseline[i]).ToArray(),Ranks=Enumerable.Range(0,10).Select(i=>CarModel.Rank(s,i)).ToArray(),OverallRank=CarModel.TrackRank(s,track),TrackScore=CarModel.Score(team,track)};foreach(var trial in s.UpgradeTrials.Where(t=>!t.Reported)){trial.Status=Verdict(trial);trial.Reported=true;report.Upgrades.Add(new UpgradeReview{Key=trial.Key,Year=trial.Year,Day=trial.Day,Version=trial.Version,ExpectedGain=trial.ExpectedGain,ActualGain=trial.ActualGain,BenchFailed=trial.BenchFailed,Reported=true,Delta=(double[])trial.Delta.Clone(),Status=trial.Status});}s.EngineeringReports.Add(report);s.LastCarMetrics=(double[])current.Clone();Engine.Note(s,"Инженерная сводка: "+track.Country+" — болид P"+report.OverallRank+" из "+s.Teams.Count+". "+report.Upgrades.Count+" новинок проверено.");}
    public static void Validate(Career s){if(s.EngineeringReports==null||s.UpgradeTrials==null||s.LastCarMetrics==null||s.LastCarMetrics.Length!=10||s.LastCarMetrics.Any(v=>double.IsNaN(v)||double.IsInfinity(v)))throw new Exception("Повреждены данные анализа болида.");foreach(var r in s.EngineeringReports){if(r==null||r.Round<0||r.Round>=Data.Tracks.Length||r.Year<2026||r.Year>2200||r.Values==null||r.Change==null||r.Ranks==null||r.Values.Length!=10||r.Change.Length!=10||r.Ranks.Length!=10||r.Ranks.Any(n=>n<1||n>s.Teams.Count)||r.Upgrades==null||r.OverallRank<1||r.OverallRank>s.Teams.Count||r.Values.Any(v=>double.IsNaN(v)||double.IsInfinity(v)||v<35||v>99)||r.Change.Any(v=>double.IsNaN(v)||double.IsInfinity(v)))throw new Exception("Повреждён инженерный отчёт.");}if(s.UpgradeTrials.Any(u=>u==null||!Data.Parts.Any(p=>p.Key==u.Key)||u.Delta==null||u.Delta.Length!=10||double.IsNaN(u.ActualGain)||double.IsInfinity(u.ActualGain)))throw new Exception("Повреждены данные проверки новинок.");}
}
}
