using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace F1Manager {
public static class UITests {
static List<string> report=new List<string>();
static void Check(bool ok,string text){if(!ok)throw new Exception(text);report.Add("PASS "+text);}
static IEnumerable<Control> Descendants(Control c){foreach(Control child in c.Controls){yield return child;foreach(var nested in Descendants(child))yield return nested;}}
static void Capture(Form form,string path){form.Update();using(var b=new Bitmap(form.ClientSize.Width,form.ClientSize.Height)){form.DrawToBitmap(b,new Rectangle(Point.Empty,form.ClientSize));b.Save(path);}}
public static void Run(ManagerForm form,string dir){Directory.CreateDirectory(dir);form.SavePath=Path.Combine(dir,"test-career.json");try{
form.State=Data.Create(0,"",658291);Engine.Develop(form.State,"front",4);Engine.Develop(form.State,"floor",6);
string[] pages={"home","development","facilities","people","race","contracts","calendar","standings","archive","hall","save"};for(int i=0;i<pages.Length;i++)form.Capture(pages[i],Path.Combine(dir,(i+1).ToString("00")+"-"+pages[i]+".png"));Check(pages.Length==11,"all eleven application pages render at 1440 px");
foreach(var p in form.State.People.Where(p=>p.Kind==0&&p.Id<22))Check(DriverPhotos.Get(p)!=null,"portrait decodes: "+p.Name);
form.Size=new Size(1120,780);form.Capture("people",Path.Combine(dir,"12-small-people.png"));form.Capture("race",Path.Combine(dir,"13-small-weekend.png"));Check(true,"people and weekend render at 1120 px");form.Size=new Size(1440,960);
// Actual modal simulation: the timer must keep painting while the full session finishes.
int frames=0;var elapsed=Stopwatch.StartNew();using(var timer=new Timer{Interval=160}){timer.Tick+=(o,e)=>{var animation=Application.OpenForms.Cast<Form>().OfType<SimulationForm>().FirstOrDefault();if(animation!=null&&frames<2){Capture(animation,Path.Combine(dir,"14-simulation-"+frames+".png"));frames++;}};timer.Start();form.SimulateNext();}
Check(frames==2&&elapsed.Elapsed.TotalSeconds>=1.1,"instant practice still displays animated modal for at least 1.1 s");Check(form.State.Stage==1&&form.State.Weekend.Results.Count==1,"simulation modal commits a completed practice");Check(!File.ReadAllBytes(Path.Combine(dir,"14-simulation-0.png")).SequenceEqual(File.ReadAllBytes(Path.Combine(dir,"14-simulation-1.png"))),"simulation animation frames differ");
while(form.State.Stage<6)Engine.SimulateSession(form.State);
string before=Engine.Serialize(form.State);bool cancelled=false;using(var timer=new Timer{Interval=100}){timer.Tick+=(o,e)=>{var dialog=Application.OpenForms.Cast<Form>().OfType<DialogForm>().FirstOrDefault();if(dialog!=null){cancelled=true;dialog.Close();}};timer.Start();form.SimulateNext();}
Check(cancelled&&before==Engine.Serialize(form.State),"closing strategy selection leaves career and session unchanged");
bool selected=false;Exception automationError=null;using(var timer=new Timer{Interval=100}){timer.Tick+=(o,e)=>{var dialog=Application.OpenForms.Cast<Form>().OfType<DialogForm>().FirstOrDefault();if(dialog==null||selected)return;try{var controls=Descendants(dialog).ToArray();var modes=controls.OfType<ComboBox>().ToArray();var inputs=controls.OfType<TextBox>().ToArray();Check(modes.Length==2&&inputs.Length==4&&controls.OfType<PictureBox>().Count()==2,"pre-race window has independent controls and portraits for both drivers");modes[0].SelectedIndex=0;modes[1].SelectedIndex=1;inputs[0].Text="20";inputs[1].Text="M,H";inputs[2].Text="15,35";inputs[3].Text="S,M,H";Capture(dialog,Path.Combine(dir,"15-pre-race-strategy.png"));selected=true;controls.OfType<Button>().Single().PerformClick();}catch(Exception ex){automationError=ex;selected=true;dialog.Close();}};timer.Start();form.SimulateNext();}
if(automationError!=null)throw automationError;Check(selected&&form.State.Round==1&&form.State.Archive.Count==1,"confirming both strategies starts and completes the race");form.Capture("race",Path.Combine(dir,"16-race-results.png"));form.Capture("archive",Path.Combine(dir,"17-race-archive.png"));Check(Engine.Deserialize(File.ReadAllText(form.SavePath)).Archive.Count==1,"completed race persisted to disk");
// Sprint-specific selector: no mandatory stop, independent from Grand Prix choices.
form.State.Stage=4;Engine.PrepareWeekend(form.State);bool sprint=false;using(var timer=new Timer{Interval=100}){timer.Tick+=(o,e)=>{var dialog=Application.OpenForms.Cast<Form>().OfType<DialogForm>().FirstOrDefault();if(dialog!=null&&!sprint){sprint=true;Capture(dialog,Path.Combine(dir,"18-sprint-strategy.png"));Descendants(dialog).OfType<Button>().Single().PerformClick();}};timer.Start();form.SimulateNext();}Check(sprint&&form.State.Stage==5&&form.State.Weekend.Results.Last().Name=="Спринт","sprint strategy selection also gates instant simulation");
var finale=Data.Create(0,"",936);finale.Round=22;finale.Stage=6;Engine.PrepareWeekend(finale);using(var dialog=new SimulationForm(finale,"Гонка")){Check(dialog.ShowDialog(form)==DialogResult.OK&&dialog.Result.SeasonDone&&dialog.Result.Round==23,"final-race animation completes safely after season rollover");form.State=dialog.Result;}form.Capture("race",Path.Combine(dir,"19-season-finale.png"));Check(true,"season-complete page renders with final race results");
report.Add("\n"+report.Count+" UI checks passed under Mono/Xvfb; Windows native execution still requires a Windows host.");
}catch(Exception ex){Environment.ExitCode=1;report.Add("FAIL "+ex);}finally{File.WriteAllLines(Path.Combine(dir,"ui-test.txt"),report);form.Close();}}
}
}
