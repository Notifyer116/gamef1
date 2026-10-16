using System;
using System.Collections.Generic;
using System.Linq;
namespace F1Manager {
public static class CalendarFlow {
    public const double DaySeconds=1.2;
    public static int RequiredDay(Career s){if(s.SeasonDone)return s.Day;var track=Data.Tracks[s.Round];string name=Engine.Sessions(s)[s.Stage];return track.Day-(name=="Гонка"?0:name=="Спринт"||name.StartsWith("Q")||name=="FP3"?1:2);}
    public static bool Ready(Career s){return !s.SeasonDone&&s.Day>=RequiredDay(s);}
    public static void AdvanceOne(Career s){if(s.SeasonDone)throw new Exception("Откройте следующий сезон.");if(s.Weekend!=null&&s.Weekend.Live!=null&&!s.Weekend.Live.Finished)throw new Exception("Сначала завершите активную сессию.");if(Ready(s))throw new Exception("Наступил день следующей сессии.");Engine.Advance(s,s.Day+1);}
    public static List<string> Events(Career s,int day){var list=new List<string>();var t=Engine.Team(s,s.Player);foreach(var p in t.Projects){string name=Data.Parts.First(c=>c.Key==p.Key).Name;if(!p.Evaluated&&p.DesignEnd==day)list.Add("Испытания / "+name);if(p.Due==day)list.Add("Поставка / "+name+(p.TargetYear>s.Year?" для "+p.TargetYear:""));}for(int i=0;i<t.Facilities.Length;i++)if(t.Facilities[i].Building&&t.Facilities[i].Due==day)list.Add("Открытие / "+Data.Facilities[i]);foreach(var r in s.Scouting.Reports.Where(r=>r.Completed<0&&r.Due==day))list.Add("Отчёт скаута / "+Engine.Person(s,r.Person).Name);if(day==Research.UnlockDay&&!s.News.Any(n=>n.Year==s.Year&&n.Day==day&&n.Text.StartsWith("Открыта разработка болида")))list.Add("Доступна разработка болида "+(s.Year+1));for(int i=s.Round;i<Data.Tracks.Length;i++){var tr=Data.Tracks[i];if(day==tr.Day-2)list.Add(tr.Country+" / начало уикенда");if(day==tr.Day-1)list.Add(tr.Country+" / "+(tr.Sprint?"спринт и квалификация":"квалификация"));if(day==tr.Day)list.Add(tr.Country+" / Гран-при");}if(day<=s.Day)list.AddRange(s.News.Where(n=>n.Year==s.Year&&n.Day==day&&!n.Text.Contains("Добро пожаловать")&&!s.Teams.Where(team=>team.Id!=s.Player).Any(team=>n.Text.StartsWith(team.Name+": "))).Select(n=>n.Text));return list.Distinct().ToList();}
    public static IEnumerable<KeyValuePair<int,string>> Upcoming(Career s,int limit){int end=s.SeasonDone?s.Day:Math.Max(RequiredDay(s),s.Day+14);for(int day=s.Day;day<=Math.Min(365,end);day++)foreach(var text in Events(s,day).Take(3))yield return new KeyValuePair<int,string>(day,text);}
}
}
