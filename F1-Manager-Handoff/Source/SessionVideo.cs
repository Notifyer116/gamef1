using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
namespace F1Manager {
public static class SessionVideo {
    public const int Fps=60,Count=210;public const double Duration=3.5;
    static Task<byte[][]> preparation;
    public static void Prepare(){if(preparation==null)preparation=Task.Factory.StartNew(Read);}
    public static byte[][] Frames {get{Prepare();return preparation.Result;}}
    static byte[][] Read(){using(var stream=Assembly.GetExecutingAssembly().GetManifestResourceStream("sessiontransition"))using(var reader=new BinaryReader(stream)){
        if(reader.ReadInt32()!=0x52543146||reader.ReadInt32()!=1||reader.ReadInt32()!=Fps||reader.ReadInt32()!=Count||reader.ReadInt32()!=1280||reader.ReadInt32()!=720)throw new InvalidDataException("Повреждена анимация перехода.");
        var frames=new byte[Count][];for(int i=0;i<Count;i++){int size=reader.ReadInt32();if(size<1||size>1024*1024)throw new InvalidDataException("Повреждён кадр перехода.");frames[i]=reader.ReadBytes(size);if(frames[i].Length!=size)throw new EndOfStreamException();}return frames;}}
}
// Decode a small look-ahead window off the UI thread; memory does not grow with clip length.
public sealed class TransitionPlayer:IDisposable {
    readonly byte[][] compressed;readonly object gate=new object();readonly Dictionary<int,Bitmap> decoded=new Dictionary<int,Bitmap>();readonly AutoResetEvent signal=new AutoResetEvent(false);
    int desired,lastDrawn;volatile bool stopped;internal int DrawnFrame {get;private set;}internal int DecodedCount {get{lock(gate)return decoded.Count;}}
    public TransitionPlayer(){compressed=SessionVideo.Frames;decoded[0]=Decode(0);Task.Factory.StartNew(Produce);}
    Bitmap Decode(int index){using(var stream=new MemoryStream(compressed[index],false))using(var image=Image.FromStream(stream))return new Bitmap(image);}
    void Produce(){try{while(!stopped){int next=-1;lock(gate){if(stopped)break;for(int i=desired;i<Math.Min(SessionVideo.Count,desired+6);i++)if(!decoded.ContainsKey(i)){next=i;break;}}
        if(next<0){signal.WaitOne(50);continue;}var frame=Decode(next);lock(gate){if(stopped){frame.Dispose();break;}if(!decoded.ContainsKey(next))decoded.Add(next,frame);else frame.Dispose();foreach(int old in decoded.Keys.Where(k=>(k<desired-1||k>desired+6)&&k!=lastDrawn).ToArray()){decoded[old].Dispose();decoded.Remove(old);}}}
    }finally{signal.Dispose();}}
    public void Request(double seconds,bool animated){lock(gate){if(stopped)return;desired=animated?Math.Min(SessionVideo.Count-1,Math.Max(0,(int)(seconds*SessionVideo.Fps))):90;signal.Set();}}
    public void Draw(Graphics graphics,Rectangle bounds,double opacity){lock(gate){if(stopped||decoded.Count==0)return;int index=decoded.Keys.Where(i=>i<=desired).DefaultIfEmpty(decoded.Keys.Min()).Max();lastDrawn=index;DrawnFrame=index;var frame=decoded[index];double scale=Math.Min(bounds.Width/1280.0,bounds.Height/720.0);int w=(int)(1280*scale),h=(int)(720*scale);var dest=new Rectangle(bounds.X+(bounds.Width-w)/2,bounds.Y+(bounds.Height-h)/2,w,h);
        if(opacity>=.999)graphics.DrawImage(frame,dest);else {var matrix=new ColorMatrix{Matrix33=(float)Math.Max(0,opacity)};using(var attributes=new ImageAttributes()){attributes.SetColorMatrix(matrix);graphics.DrawImage(frame,dest,0,0,frame.Width,frame.Height,GraphicsUnit.Pixel,attributes);}}}}
    public void Dispose(){lock(gate){if(stopped)return;stopped=true;signal.Set();foreach(var frame in decoded.Values)frame.Dispose();decoded.Clear();}}
}
}
