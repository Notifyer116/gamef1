using System;
using System.Collections.Generic;
using System.Linq;
namespace F1Manager {
public class Contract { public int Team=-1,Seat,Start=2026,Until=2027,Severance=12; public double Salary;public string Role="Равный статус"; }
public class Person { public int Id,Kind,Age,Rating,Wins,Titles,Points,Podiums,RetireYear;public string Name,Source,RealRole; public bool Retired,Fictional;public Contract Deal;public List<int> Finishes=new List<int>();public double Mood=65; }
public class Booking {public int Person;public Contract Deal;public double Paid;}
public class Facility {public int Level=1,Due=0;public bool Building;}
public class Component {public string Key,Name,Department,Description;public int Stat,Facility,Days;public double Price,Gain;public Component(string key,string name,string dept,string desc,int stat,int fac,int days,double price,double gain){Key=key;Name=name;Department=dept;Description=desc;Stat=stat;Facility=fac;Days=days;Price=price;Gain=gain;}}
public class Installed {public string Key;public int Version=1;public double Bonus;}
public class Project {public string Key;public int Start,DesignEnd,Due,Engineers,TargetYear;public double Cost,Gain,Failure;public bool Evaluated,Failed;}
public class Team {public int Id,Points,Wins;public double ResearchSpent;public int NextResearchDay;public string Name,Color,Engine;public bool Works;public int EngineUntil=2027,EngineUpgradedYear;public string NextEngine;public int NextEngineYear,NextEngineUntil;public double EngineRating=80,EngineReliability=85;public double[] Base=new double[6];public Facility[] Facilities=Enumerable.Range(0,6).Select(x=>new Facility()).ToArray();public List<Installed> Parts=new List<Installed>(),NextParts=new List<Installed>();public List<Project> Projects=new List<Project>();public Dictionary<string,double> Chemistry=new Dictionary<string,double>();public Dictionary<string,int> Pairs=new Dictionary<string,int>(); }
public class News {public int Year,Day;public string Text;}
public class Champion {public int Year,DriverId,TeamId;public string Driver,Team;public int DriverPoints,TeamPoints;}
public class Ledger {public int Year;public string Text;public double Value;}
public class Row {public int Id,Team,Position,Laps,Points;public double Time,Best;public bool Dnf;public string Tyre,Detail;}
public class SessionResult {public string Name,Weather;public List<Row> Rows=new List<Row>();}
public class WeekendArchive {public bool RainAffected;public int Year,Round;public string Weather;public List<SessionResult> Sessions=new List<SessionResult>();}
public class Entry {public int Id,Team,Lap,StintLaps,Stops,ScheduledStops,Plan,NextTyre,RunLaps;public double Remaining,LapDuration,Total,Best=99999,Last,TyreWear,Wait,FinishTime,Damage;public bool Dnf,Finished,Garage,PitRequested,WindowNotified,OutLap;public string Tyre="M",Status="В гараже",Reason="";}
public class LiveSession {public string Name,Weather="Сухо",Event="";public int TargetLaps;public double Elapsed,Duration,WeatherAt,SafetyUntil;public bool WeatherChanged,Started,Finished,Paused=true,AwaitingEvent;public List<Entry> Entries=new List<Entry>();public List<string> Log=new List<string>();}
public class Weekend {public bool RainAffected;public string Weather="Сухо",QualWeather,SprintQualWeather;public List<int> Qual=new List<int>(),SQ=new List<int>();public List<SessionResult> Results=new List<SessionResult>();public LiveSession Live;}
public class Plan {public int Mode=1;public List<int> Stops=new List<int>();public List<string> Tyres=new List<string>();}
public class Career {public int Version=5,Year=2026,Round,Stage,Day,NextId=1000,Player;public double[] LastCarMetrics;public List<EngineeringReport> EngineeringReports=new List<EngineeringReport>();public List<UpgradeReview> UpgradeTrials=new List<UpgradeReview>();public uint Seed=1234;public bool Preseason=true,SeasonDone;public double Cash=120,Spent;public string Name="Карьера";public List<Person> People=new List<Person>();public List<Team> Teams=new List<Team>();public List<Booking> Bookings=new List<Booking>();public List<News> News=new List<News>();public List<Champion> Hall=new List<Champion>();public List<Ledger> Ledger=new List<Ledger>();public List<WeekendArchive> Archive=new List<WeekendArchive>();public Dictionary<string,Plan> Plans=new Dictionary<string,Plan>();public Weekend Weekend;}
public class Track {public string Country,Name,Slug;public int Laps,Day,Wear;public bool Sprint;public double BaseLap,Rain;public Track(string c,string n,string slug,int laps,int day,int wear,bool sprint,double lap,double rain){Country=c;Name=n;Slug=slug;Laps=laps;Day=day;Wear=wear;Sprint=sprint;BaseLap=lap;Rain=rain;}}
public class Quote {public double Salary,Fee,Bonus,Compensation;public int Mood,MinSeverance;public string Reason;}
}
