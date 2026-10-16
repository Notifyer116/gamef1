using System;
using System.Drawing;
using System.Linq;
namespace F1Manager {
public static class Patch251Tests {
    public static void Run(Action<bool,string> check){
        check(RaceDirector.TickSeconds>=.75&&RaceDirector.TickSeconds<=1,"2.5.1 race tick lasts between 0.75 and one second");
        for(int i=0;i<Data.Tracks.Length;i++){var map=TrackMaps.Get(i);var fitted=TrackMaps.Fit(i,new Rectangle(20,30,400,220));check(map.Coordinates.Length>30&&map.Length>3000&&fitted.All(p=>p.X>=19.99&&p.X<=420.01&&p.Y>=29.99&&p.Y<=250.01),"2.5.1 actual circuit centerline fits map without cropping: "+map.Name);}
        check(TrackMaps.Get(5).Id=="mc-1929"&&TrackMaps.Get(13).Id=="es-2026"&&TrackMaps.Get(22).Id=="ae-2009","2.5.1 maps match the game calendar including Monaco, MADRING and Yas Marina");
        check(StartupPresentation.Opacity(0,3,true)==0&&StartupPresentation.Opacity(1,3,true)>.99&&StartupPresentation.Opacity(3,3,true)==0,"2.5.1 splash fades from black and back before the menu");
        check(Enumerable.Range(0,50).Select(i=>StartupPresentation.RandomCaption()).Distinct().Count()>3,"2.5.1 startup captions vary between launches");
        var s=Data.Create(0,"",2511);s.Stage=6;Engine.PrepareWeekend(s);s.Weekend.Weather="Сухо";Engine.Start(s);var live=s.Weekend.Live;live.WeatherAt=double.MaxValue;var e=live.Entries.First(x=>x.Team==s.Player);uint seed=s.Seed;e.EngineMode=0;e.DrivingMode=1;double saveMotor=RaceDirector.Pace(s,e);s.Seed=seed;e.EngineMode=2;double attackMotor=RaceDirector.Pace(s,e);check(saveMotor-attackMotor>2.5,"2.5.1 engine attack versus conserve materially changes fresh-tyre lap pace");s.Seed=seed;e.EngineMode=1;e.DrivingMode=0;double saveDrive=RaceDirector.Pace(s,e);s.Seed=seed;e.DrivingMode=2;double attackDrive=RaceDirector.Pace(s,e);check(saveDrive-attackDrive>1.7,"2.5.1 driving style materially changes fresh-tyre lap pace separately from engine mode");
        var saved=Engine.Deserialize(Engine.Serialize(s));var copy=saved.Weekend.Live.Entries.First(x=>x.Id==e.Id);e.EngineMode=2;e.DrivingMode=2;copy.EngineMode=0;copy.DrivingMode=0;Powertrain.Wear(s,e,15);Powertrain.Wear(saved,copy,15);check(Powertrain.Car(s,e).Condition.Zip(Powertrain.Car(saved,copy).Condition,(a,b)=>b-a).All(x=>x>2),"2.5.1 attack has a visible cost in all three component resources over a stint");check(Powertrain.TyreWearMultiplier(2)/Powertrain.TyreWearMultiplier(0)>3,"2.5.1 driving style has a strong tyre-life tradeoff");
        var straight=Data.Tracks[12];var corners=Data.Tracks[5];check(Math.Abs(Powertrain.EnginePace(straight,2))>Math.Abs(Powertrain.EnginePace(corners,2))&&Math.Abs(Powertrain.DrivingPace(corners,2))>Math.Abs(Powertrain.DrivingPace(straight,2)),"2.5.1 engine mode matters more on straights while driving style matters more in corners");
    }
}
}
