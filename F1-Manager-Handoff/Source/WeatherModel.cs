using System;
using System.Linq;
namespace F1Manager {
public static class WeatherModel {
    public const int MaxWetWeekends=2;
    public static bool WasWet(WeekendArchive round){return round.RainAffected||round.Weather=="Дождь"||round.Sessions.Any(s=>s.Weather=="Дождь");}
    public static int WetStreak(Career state){int count=0;for(int i=state.Archive.Count-1;i>=0&&WasWet(state.Archive[i]);i--)count++;return count;}
    public static bool AllowsRain(Career state){return WetStreak(state)<MaxWetWeekends;}
    public static double ChangeChance(Career state,string weather){double climate=Data.Tracks[state.Round].Rain;return weather=="Дождь"?.12+(1-climate)*.18:AllowsRain(state)?climate*.10:0;}
}
}
