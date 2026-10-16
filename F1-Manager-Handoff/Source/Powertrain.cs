using System;
using System.Linq;
using System.Collections.Generic;
namespace F1Manager {
// Inventories belong to the car seat: changing a driver never renews its components.
public static class Powertrain {
    public static readonly string[] Names={"ДВС","ERS","Коробка передач"};
    public static readonly int[] Limits={4,2,4};
    public static readonly double[] Prices={8,6,5};
    public static readonly string[] Modes={"Сохранять","Средний","Атака"};
    public static void Reset(Team team){team.PowerCars=new List<PowerCar>{new PowerCar{Seat=0},new PowerCar{Seat=1}};}
    public static PowerCar Car(Team team,int seat){if(team.PowerCars==null||team.PowerCars.Count==0)Reset(team);return team.PowerCars.First(c=>c.Seat==seat);}
    public static PowerCar Car(Career state,Entry entry){return Car(Engine.Team(state,entry.Team),Engine.Person(state,entry.Id).Deal.Seat);}
    public static bool Active(Career s){return s.Weekend!=null&&s.Weekend.Live!=null&&!s.Weekend.Live.Finished;}
    public static double ReplacementPrice(PowerCar car,int kind){return car.Used[kind]>=Limits[kind]?Prices[kind]:0;}
    public static void Replace(Career s,int seat,int kind){if(seat<0||seat>1||kind<0||kind>2)throw new Exception("Неизвестный агрегат или машина.");if(Active(s))throw new Exception("Заменить агрегат можно после завершения сессии.");var car=Car(Engine.Team(s,s.Player),seat);double cost=ReplacementPrice(car,kind);if(s.Cash<cost)throw new Exception("Недостаточно средств для новой детали.");if(cost>0){Engine.Pay(s,Names[kind]+" сверх лимита / машина "+(seat+1),-cost);car.GridPenalty=true;}car.Used[kind]++;car.Condition[kind]=100;Engine.Note(s,"Машина "+(seat+1)+": установлен новый агрегат "+Names[kind]+" №"+car.Used[kind]+(cost>0?". Старт с конца решётки следующего Гран-при; "+Engine.Money(cost):" в пределах сезонного лимита.")+".");}
    public static void PrepareAI(Career s){foreach(var t in s.Teams.Where(t=>t.Id!=s.Player))for(int seat=0;seat<2;seat++){var c=Car(t,seat);for(int k=0;k<3;k++)if(c.Condition[k]<24){if(c.Used[k]>=Limits[k])c.GridPenalty=true;c.Used[k]++;c.Condition[k]=100;}}}
    public static double EngineWearMultiplier(int mode){return mode==2?2.0:mode==0?.50:1;}
    public static double TyreWearMultiplier(int mode){return mode==2?1.85:mode==0?.55:1;}
    public static double EnginePace(Track track,int mode){double straights=CarModel.Profile(track).Importance[4]+CarModel.Profile(track).Importance[9];double weight=.65+straights*2;return (mode==2?-1.25:mode==0?1.60:0)*weight;}
    public static double DrivingPace(Track track,int mode){var w=CarModel.Profile(track).Importance;double weight=.55+w[1]+w[2]+w[3];return (mode==2?-.95:mode==0?1.20:0)*weight;}
    public static string EngineDescription(Track track,int mode){return EnginePace(track,mode).ToString("+0.00;-0.00;0.00")+" с/круг · ДВС/ERS ×"+EngineWearMultiplier(mode).ToString("0.00");}
    public static string DrivingDescription(Track track,int mode){return DrivingPace(track,mode).ToString("+0.00;-0.00;0.00")+" с/круг · шины ×"+TyreWearMultiplier(mode).ToString("0.00");}
    public static void Wear(Career s,Entry e,double laps){var c=Car(s,e);var t=Engine.Team(s,e.Team);var track=Data.Tracks[s.Round];double heat=1+Math.Max(0,CarModel.Profile(track).EngineHeat-CarModel.Metric(t,6))*.008;double motor=EngineWearMultiplier(e.EngineMode);double driving=e.DrivingMode==2?1.65:e.DrivingMode==0?.65:1;double reliability=1+(90-CarModel.Metric(t,8))*.005;double[] rate={.20*motor*heat,.105*(EngineWearMultiplier(e.EngineMode)* (e.EngineMode==2?1.1:1))*heat,.18*driving*(e.EngineMode==2?1.3:e.EngineMode==0?.85:1)};for(int k=0;k<3;k++)c.Condition[k]=Engine.Clamp(c.Condition[k]-rate[k]*laps*reliability,0,100);}
    public static double PaceLoss(Career s,Entry e){var c=Car(s,e);return Math.Max(0,35-c.Condition[0])*.065+Math.Max(0,30-c.Condition[1])*.055+Math.Max(0,25-c.Condition[2])*.045;}
    public static void SetModes(Career s,int id,int engine,int driving){if(engine<0||engine>2||driving<0||driving>2)throw new Exception("Неизвестный режим.");var live=s.Weekend==null?null:s.Weekend.Live;var e=live==null?null:live.Entries.FirstOrDefault(x=>x.Id==id);if(live==null||!live.TickRace||live.Finished||e==null||e.Team!=s.Player||e.Dnf||e.Finished)throw new Exception("Режим доступен для вашего участника текущей гонки.");e.EngineMode=engine;e.DrivingMode=driving;}
    public static void Validate(Career s){foreach(var t in s.Teams){if(t.PowerCars==null||t.PowerCars.Count!=2||t.PowerCars.Select(c=>c.Seat).OrderBy(x=>x).SequenceEqual(new[]{0,1})==false||t.PowerCars.Any(c=>c.Condition==null||c.Used==null||c.Condition.Length!=3||c.Used.Length!=3||c.Condition.Any(x=>double.IsNaN(x)||double.IsInfinity(x)||x<0||x>100)||c.Used.Any(x=>x<1||x>1000)))throw new Exception("Повреждены агрегаты силовой установки.");}}
}
}
