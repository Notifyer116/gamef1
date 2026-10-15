using System;
using System.Collections.Generic;
using System.Linq;
namespace F1Manager {
public static class Patch242Tests {
    static WeekendArchive WetRound(){return new WeekendArchive{Weather="Дождь"};}
    public static void Run(Action<bool,string> check,Action<Action,string> rejects){
        var workforce=Data.Create(0,"",771);Engine.Develop(workforce,"floor",10);
        var team=Engine.Team(workforce,0);var project=team.Projects.Single();project.Failure=0;
        Engine.Advance(workforce,project.DesignEnd);
        check(project.Evaluated&&Engine.Assigned(workforce,team)==10,"engineers remain assigned during manufacturing");
        string before=Engine.Serialize(workforce);
        rejects(()=>Engine.Develop(workforce,"front",10),"manufacturing engineers cannot be assigned to a second component");
        check(before==Engine.Serialize(workforce),"rejected double allocation does not spend cash or create a project");
        workforce=Engine.Deserialize(before);team=Engine.Team(workforce,0);
        check(Engine.Assigned(workforce,team)==10,"manufacturing allocation survives save and reload");
        while(workforce.Round<2){Engine.SimulateSession(workforce);check(Engine.Assigned(workforce,team)<=Engine.Capacity(team),"stage transition preserves the engineering workforce limit");}
        Engine.Advance(workforce,project.Due);
        check(team.Projects.Count==0&&Engine.Assigned(workforce,team)==0,"delivery releases the assigned engineers exactly once");
        var mixed=Data.Create(0,"",772);Engine.Advance(mixed,Research.UnlockDay);Engine.Develop(mixed,"floor",6);Engine.Develop(mixed,"front",4,true);
        foreach(var p in Engine.Team(mixed,0).Projects)p.Failure=0;
        Engine.Advance(mixed,Engine.Team(mixed,0).Projects.Min(p=>p.DesignEnd));
        check(Engine.Assigned(mixed,Engine.Team(mixed,0))==10,"current and next-year manufacturing share the same ten engineers");
        var legacy=Data.Create(0,"",773);var legacyTeam=Engine.Team(legacy,0);
        legacyTeam.Projects.Add(new Project{Key="floor",Engineers=10,TargetYear=2026,Evaluated=true,Start=0,DesignEnd=12,Due=33,Gain=5,Cost=10});
        legacyTeam.Projects.Add(new Project{Key="front",Engineers=10,TargetYear=2026,Start=13,DesignEnd=28,Due=40,Gain=3,Cost=6});
        rejects(()=>Engine.Validate(legacy),"new saves cannot contain more assigned engineers than the team owns");
        uint random=legacy.Seed;var repaired=Engine.Deserialize(Engine.Serialize(legacy));var projects=Engine.Team(repaired,0).Projects;
        check(projects.Count==2&&projects.All(p=>p.Engineers==5),"old overbooked saves retain both projects with valid engineer allocations");
        check(projects[0].Due==33&&projects[1].Due==40&&projects[0].Gain==5&&projects[1].Gain==3&&repaired.Cash==legacy.Cash&&repaired.Seed==random,"allocation repair preserves delivery dates, gains, money and random state");
        int news=repaired.News.Count;repaired=Engine.Deserialize(Engine.Serialize(repaired));
        check(repaired.News.Count==news,"loading an already repaired save does not apply another repair");
        var outlap=Data.Create(0,"",774);outlap.Stage=3;Engine.Start(outlap);var live=outlap.Weekend.Live;
        foreach(var entry in live.Entries)entry.Wait=0;
        Engine.Tick(outlap,.5,true);var driver=live.Entries[0];driver.Remaining=.1;driver.LapDuration=99;
        Engine.Tick(outlap,.5,true);check(driver.Best==99999&&!driver.OutLap,"qualifying out-lap is excluded from the best time");
        driver.Remaining=.1;driver.LapDuration=79;Engine.Tick(outlap,.5,true);
        check(driver.Best==79,"the following flying lap sets the qualifying time");
        foreach(bool sprint in new[]{false,true})foreach(string weather in new[]{"Сухо","Дождь"}){
            double largest=0;bool stable=true;
            for(uint seed=1;seed<=12;seed++){
                var qualifying=Data.Create(0,"",seed);qualifying.Round=sprint?1:0;qualifying.Stage=sprint?1:3;
                Engine.PrepareWeekend(qualifying);qualifying.Weekend.Weather=weather;
                for(int segment=0;segment<3;segment++){
                    Engine.Start(qualifying);qualifying.Weekend.Live.WeatherAt=0;Engine.Tick(qualifying,1,true);
                    stable&=qualifying.Weekend.Live.Weather==weather;
                    Engine.SimulateSession(qualifying);qualifying=Engine.Deserialize(Engine.Serialize(qualifying));
                }
                var results=qualifying.Weekend.Results;
                foreach(var row in results[2].Rows.Take(10)){
                    var times=results.Select(r=>r.Rows.First(x=>x.Id==row.Id).Best).ToArray();
                    largest=Math.Max(largest,times.Max()-times.Min());
                }
                stable&=results.All(r=>r.Weather==weather);
            }
            check(stable,(sprint?"SQ":"Q")+" conditions stay consistent across all three segments and reloads: "+weather);
            check(largest<1.4,(sprint?"SQ":"Q")+" flying times have no artificial multi-second jumps: "+weather);
        }
        var climate=Data.Create(0,"",775);int wetForecasts=0;
        for(int i=0;i<1000;i++){climate.Round=i%23;climate.Weekend=null;Engine.PrepareWeekend(climate);if(climate.Weekend.Weather=="Дождь")wetForecasts++;}
        check(wetForecasts>120&&wetForecasts<400,"weather forecasts follow circuit climate rather than a fifty-fifty toggle");
        check(WeatherModel.ChangeChance(climate,"Сухо")<=.05&&WeatherModel.ChangeChance(climate,"Дождь")>WeatherModel.ChangeChance(climate,"Сухо"),"spontaneous rain is uncommon and wet weather can clear");
        var forcedDry=Data.Create(0,"",776);forcedDry.Archive.Add(WetRound());forcedDry.Archive.Add(WetRound());
        Engine.PrepareWeekend(forcedDry);check(forcedDry.Weekend.Weather=="Сухо","two rainy legacy weekends force the next forecast to be dry");
        forcedDry.Weekend.Weather="Дождь";Engine.PrepareWeekend(forcedDry);check(forcedDry.Weekend.Weather=="Сухо","an unstarted rainy forecast from an old save also respects the new streak limit");
        Engine.Start(forcedDry);forcedDry.Weekend.Live.WeatherAt=0;Engine.Tick(forcedDry,1,true);
        check(forcedDry.Weekend.Live.Weather=="Сухо","a dry recovery weekend cannot turn rainy during a session");
        forcedDry.Archive.Last().Weather="Сухо";forcedDry.Archive.Last().RainAffected=true;
        check(!WeatherModel.AllowsRain(forcedDry),"rain that cleared before the finish still counts toward the streak");
        int wet=0,total=0,longest=0;
        for(uint seed=777;seed<780;seed++){
            var season=Data.Create(0,"",seed);while(!season.SeasonDone)Engine.SimulateSession(season);
            foreach(var round in season.Archive){total++;if(WeatherModel.WasWet(round))wet++;}
            for(int i=0;i<season.Archive.Count;i++){int streak=0;for(int j=i;j>=0&&WeatherModel.WasWet(season.Archive[j]);j--)streak++;longest=Math.Max(longest,streak);}
        }
        check(longest<=WeatherModel.MaxWetWeekends,"three full seasons never exceed two rainy weekends in a row");
        check(wet>total*.08&&wet<total*.45,"rain remains occasional across three full seasons ("+wet+" / "+total+" weekends)");
    }
}
}
