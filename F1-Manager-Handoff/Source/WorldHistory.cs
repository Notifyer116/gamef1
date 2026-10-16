using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Web.Script.Serialization;
namespace F1Manager {
public static class CountryFlags {
    public static readonly string[] Codes={"au","cn","jp","us","ca","mc","es","at","gb","be","hu","nl","it","es","az","bh","sg","us","mx","br","us","qa","ae"};
    static readonly Dictionary<string,Image> cache=new Dictionary<string,Image>();
    public static Image Get(int round){string code=Codes[round];Image result;if(cache.TryGetValue(code,out result))return result;using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("flag"+code)){if(stream==null)throw new Exception("Не найден флаг "+code);using(var original=Image.FromStream(stream))result=new Bitmap(original);}cache[code]=result;return result;}
    public static void Draw(Graphics g,int round,Rectangle r){if(r.Width<1||r.Height<1)return;var saved=g.Save();using(var path=Theme.Round(r,9)){g.SetClip(path,CombineMode.Intersect);Theme.ImageFit(g,Get(round),r);}g.Restore(saved);}
}
public static class WorldHistory {
    public static readonly Champion[] Champions=Load();
    static Champion[] Load(){using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("champions"))using(var reader=new StreamReader(stream)){var rows=new JavaScriptSerializer().Deserialize<Champion[]>(reader.ReadToEnd());if(rows.Length!=76||rows.Select(c=>c.Year).Distinct().Count()!=76||rows.Min(c=>c.Year)!=1950||rows.Max(c=>c.Year)!=2025)throw new Exception("Неполная история чемпионов.");return rows;}}
    public static IEnumerable<Champion> All(Career s){return s.Hall.Concat(Champions).OrderByDescending(c=>c.Year);}
}
public partial class ManagerForm {
    int Hall(){Heading("WORLD CHAMPIONS / 1950 →","Зал славы");int y=193;Box(X,y,W,135);TextAt("76 сезонов истории + ваша карьера",X+20,y+14,W-40,25,white,true,40);TextAt("Чемпионы мира с 1950 по 2025 год. С 2026 года здесь сохраняются результаты вашей игры.",X+20,y+57,W-40,15,muted,false,40);TextAt("История: F1DB / github.com/f1db/f1db / CC BY 4.0 (creativecommons.org/licenses/by/4.0)",X+20,y+105,W-40,11,muted,false,23);y+=158;int col=(W-185)/2;foreach(var h in WorldHistory.All(State)){Box(X,y,W,119);TextAt(h.Year.ToString(),X+18,y+15,125,37,white,true,56);TextAt(h.Historical?"ИСТОРИЯ F1":"ВАША КАРЬЕРА",X+18,y+77,130,10,muted,true,23);TextAt("ЧЕМПИОН МИРА",X+162,y+12,col-12,11,muted,true,21);TextAt(h.Driver,X+162,y+38,col-12,22,white,true,36);TextAt(h.DriverPoints.ToString("0.##")+" очков",X+162,y+81,col-12,13,muted);int cx=X+170+col;TextAt("КУБОК КОНСТРУКТОРОВ",cx,y+12,col-12,11,muted,true,21);TextAt(h.Team,cx,y+38,col-12,h.Year<1958?15:22,white,true,36);if(h.Year>=1958)TextAt(h.TeamPoints.ToString("0.##")+" очков",cx,y+81,col-12,13,muted);y+=132;}return y+30;}
}
}
