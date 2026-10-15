using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
namespace F1Manager {
// One paced UI clock. Coalesce missed frames instead of flooding the message queue.
public static class FrameClock {
    public const double Interval=1.0/60;
    public static event Action<double> Frame;
    static readonly Stopwatch watch=Stopwatch.StartNew();static System.Threading.Timer timer;static Control host;static int pending;static double deadline,last;static bool precision;
    [DllImport("winmm.dll")]static extern uint timeBeginPeriod(uint period);
    [DllImport("winmm.dll")]static extern uint timeEndPeriod(uint period);
    public static void Start(Control control){Stop();host=control;last=watch.Elapsed.TotalSeconds;deadline=last;if(Environment.OSVersion.Platform==PlatformID.Win32NT)precision=timeBeginPeriod(1)==0;timer=new System.Threading.Timer(Pulse,null,0,2);}
    static void Pulse(object unused){var owner=host;if(watch.Elapsed.TotalSeconds<deadline||owner==null||owner.IsDisposed||!owner.IsHandleCreated||Interlocked.CompareExchange(ref pending,1,0)!=0)return;try{owner.BeginInvoke(new Action(()=>{try{if(owner!=host||owner.IsDisposed)return;double now=watch.Elapsed.TotalSeconds;if(now<deadline)return;double delta=Math.Min(.1,Math.Max(0,now-last));last=now;deadline+=Interval;if(deadline<now)deadline=now+Interval;var handler=Frame;if(handler!=null)handler(delta);}finally{Interlocked.Exchange(ref pending,0);}}));}catch(InvalidOperationException){Interlocked.Exchange(ref pending,0);}}
    public static double Approach(double value,double target,double dt){double next=value+(target-value)*(1-Math.Exp(-dt*17));return Math.Abs(next-target)<.002?target:next;}
    public static void Stop(){host=null;if(timer!=null){timer.Dispose();timer=null;}Interlocked.Exchange(ref pending,0);if(precision){timeEndPeriod(1);precision=false;}}
}
}
