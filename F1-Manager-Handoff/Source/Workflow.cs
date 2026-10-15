using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace F1Manager {
// Identity is the person's name, so portraits follow transfers and older save files.
public static class DriverPhotos {
static readonly string[] names={"Lando Norris","Oscar Piastri","Charles Leclerc","Lewis Hamilton","Max Verstappen","Isack Hadjar","George Russell","Kimi Antonelli","Fernando Alonso","Lance Stroll","Pierre Gasly","Franco Colapinto","Esteban Ocon","Oliver Bearman","Liam Lawson","Arvid Lindblad","Carlos Sainz","Alexander Albon","Nico Hulkenberg","Gabriel Bortoleto","Sergio Perez","Valtteri Bottas","Yuki Tsunoda"};
static readonly Dictionary<string,Image> cache=new Dictionary<string,Image>();
public static Image Get(Person person){if(person==null||person.Kind!=0||person.Fictional)return null;Image image;if(cache.TryGetValue(person.Name,out image))return image;int index=Array.IndexOf(names,person.Name);if(index>=0)using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("driver"+index)){if(stream!=null)using(var source=Image.FromStream(stream))image=new Bitmap(source);}cache[person.Name]=image;return image;}
}
}
