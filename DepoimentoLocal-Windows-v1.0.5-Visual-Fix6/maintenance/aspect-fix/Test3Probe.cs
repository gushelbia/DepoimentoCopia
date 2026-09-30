using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
public static class Test3Probe {
 delegate bool EnumProc(IntPtr h, IntPtr data);
 [DllImport("user32.dll")] static extern bool EnumWindows(EnumProc callback,IntPtr data);
 [DllImport("user32.dll")] static extern bool EnumChildWindows(IntPtr parent,EnumProc callback,IntPtr data);
 [DllImport("user32.dll")] static extern uint GetWindowThreadProcessId(IntPtr h,out uint id);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetWindowText(IntPtr h,StringBuilder text,int count);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern int GetClassName(IntPtr h,StringBuilder text,int count);
 [DllImport("user32.dll")] static extern bool IsWindowVisible(IntPtr h);
 [DllImport("user32.dll")] static extern IntPtr GetParent(IntPtr h);
 [DllImport("user32.dll")] static extern int GetDlgCtrlID(IntPtr h);
 [DllImport("user32.dll")] static extern bool PostMessage(IntPtr h,uint msg,IntPtr w,IntPtr l);
 [DllImport("user32.dll",CharSet=CharSet.Unicode)] static extern IntPtr SendMessageTimeout(IntPtr h,uint msg,IntPtr w,StringBuilder text,uint flags,uint timeout,out IntPtr result);
 static string Text(IntPtr h) {var s=new StringBuilder(4096);IntPtr result;SendMessageTimeout(h,0xD,new IntPtr(s.Capacity),s,2,1000,out result);return s.ToString();}
 [DllImport("user32.dll", EntryPoint="SendMessageW", CharSet=CharSet.Unicode)] static extern IntPtr SetText(IntPtr h,uint msg,IntPtr w,string text);
 static string ReadLog(string path) {
  using(var stream=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite))
  using(var reader=new StreamReader(stream,Encoding.UTF8)) return reader.ReadToEnd();
 }
 public static void Run(string root) {
  string reportDir=Path.Combine(root,"maintenance","aspect-fix");
  var report=new List<string>();
  var start=new ProcessStartInfo(Path.Combine(root,"DepoimentoLocal.exe"));
  start.UseShellExecute=false;start.CreateNoWindow=true;start.WorkingDirectory=Path.GetTempPath();
  start.EnvironmentVariables["DOTNET_ROOT"]=Path.Combine(reportDir,"runtime-not-installed");
  start.EnvironmentVariables["DOTNET_ROOT_X64"]=start.EnvironmentVariables["DOTNET_ROOT"];
  start.EnvironmentVariables["DOTNET_MULTILEVEL_LOOKUP"]="0";
  start.EnvironmentVariables["COREHOST_TRACE"]="1";
  start.EnvironmentVariables["COREHOST_TRACEFILE"]=Path.Combine(reportDir,"host-trace.txt");
  string logPath=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"DepoimentoLocal","logs","depoimento-local.log");
  int logStart=File.Exists(logPath)?ReadLog(logPath).Length:0;
  Process process=Process.Start(start);
  IntPtr window=IntPtr.Zero;
  try {
   var timer=Stopwatch.StartNew();
   while(timer.Elapsed.TotalSeconds<35 && window==IntPtr.Zero){
    if(process.HasExited)throw new Exception("Executable exited: "+process.ExitCode);
    EnumWindows(delegate(IntPtr h,IntPtr d){uint id;GetWindowThreadProcessId(h,out id);if(id==process.Id && Text(h)=="Depoimento Local — Windows")window=h;return true;},IntPtr.Zero);
    Thread.Sleep(100);
   }
   if(window==IntPtr.Zero)throw new Exception("Main interface not found");
   if(!IsWindowVisible(window))throw new Exception("Main interface not visible");
   Console.WriteLine("checkpoint " + timer.Elapsed); report.Add("PASS abertura direta do EXE a partir de outro diretório; interface visível");
   string expected=Path.Combine(root,"modelo","qwen2.5-3b-instruct-q4_k_m.gguf");
   bool modelFound=false;IntPtr load=IntPtr.Zero;
   EnumChildWindows(window,delegate(IntPtr h,IntPtr d){string text=Text(h);if(!String.IsNullOrWhiteSpace(text) && text.EndsWith("qwen2.5-3b-instruct-q4_k_m.gguf",StringComparison.OrdinalIgnoreCase)) { try { modelFound=Path.GetFullPath(Path.Combine(root,"engine",text))==expected; } catch { } }if(text=="Carregar modelo")load=h;return true;},IntPtr.Zero);
   if(!modelFound)throw new Exception("Portable model not located");
   Console.WriteLine("checkpoint " + timer.Elapsed); report.Add("PASS GGUF localizado automaticamente em modelo/");
   bool localRuntime=false;
   foreach(ProcessModule module in process.Modules){if(module.ModuleName.Equals("coreclr.dll",StringComparison.OrdinalIgnoreCase)){if(!module.FileName.StartsWith(Path.Combine(root,"engine"),StringComparison.OrdinalIgnoreCase))throw new Exception("External runtime used");localRuntime=true;}}
   if(!localRuntime)throw new Exception("Bundled runtime not loaded");
   Console.WriteLine("checkpoint " + timer.Elapsed); report.Add("PASS runtime self-contained carregado de engine/coreclr.dll com DOTNET_ROOT inválido");
   if(load==IntPtr.Zero)throw new Exception("Model load button missing");
   PostMessage(GetParent(load),0x111,new IntPtr(GetDlgCtrlID(load) & 0xffff),load);
   bool loaded=false;timer.Restart();
   while(timer.Elapsed.TotalSeconds<90&&!loaded){
    EnumChildWindows(window,delegate(IntPtr h,IntPtr d){string text=Text(h);if(text.IndexOf("Modelo carregado",StringComparison.OrdinalIgnoreCase)>=0)loaded=true;return true;},IntPtr.Zero);
    Thread.Sleep(200);
   }
   if(!loaded)throw new Exception("Model load did not complete");
   Console.WriteLine("checkpoint " + timer.Elapsed); report.Add("PASS carregamento offline do modelo pela interface do novo executável");
   var editors=new List<IntPtr>();IntPtr reformulate=IntPtr.Zero;
   EnumChildWindows(window,delegate(IntPtr h,IntPtr d){
    var cls=new StringBuilder(256);GetClassName(h,cls,256);
    if(cls.ToString().IndexOf("richedit",StringComparison.OrdinalIgnoreCase)>=0)editors.Add(h);
    if(Text(h)=="Reformular texto")reformulate=h;
    return true;
   },IntPtr.Zero);
   if(editors.Count!=3||reformulate==IntPtr.Zero)throw new Exception("Three editors / Reformular button not found");
   string source=File.ReadAllText(Path.Combine(reportDir,"teste3-original.txt"),Encoding.UTF8);
   SetText(editors[0],0xC,IntPtr.Zero,source);
   if(Text(editors[0]).Replace("\r\n","\n")!=source.Replace("\r\n","\n"))throw new Exception("Input changed");
   report.Add("PASS texto original integral inserido, sem alterações; chars="+source.Length);
   PostMessage(GetParent(reformulate),0x111,new IntPtr(GetDlgCtrlID(reformulate)&0xffff),reformulate);
   timer.Restart();string runLog="";bool complete=false;
   while(timer.Elapsed.TotalSeconds<240&&!complete){
    string all=ReadLog(logPath);if(all.Length<logStart)throw new Exception("Log rotated");runLog=all.Substring(logStart);
    complete=runLog.Contains("[GENERATION_OK]")||runLog.Contains("[GENERATION_FAILED]");
    Thread.Sleep(150);
   }
   Thread.Sleep(500);
   string output=Text(editors[1]);
   File.WriteAllText(Path.Combine(reportDir,"teste3-output.txt"),output,Encoding.UTF8);
   File.WriteAllText(Path.Combine(reportDir,"teste3-log.txt"),runLog,Encoding.UTF8);
   report.Add("Generation accepted: "+runLog.Contains("[GENERATION_OK]"));
   report.Add("Automatic retry: "+runLog.Contains("[BLOCK_RETRY_START]"));
   report.Add("Context overflow: "+runLog.Contains("ContextOverflowException"));
   File.WriteAllLines(Path.Combine(reportDir,"teste3-results.txt"),report.ToArray(),Encoding.UTF8);
   if(!complete||!runLog.Contains("[GENERATION_OK]"))throw new Exception("Generation not accepted; see teste3-log.txt");
   PostMessage(window,0x10,IntPtr.Zero,IntPtr.Zero);
   if(!process.WaitForExit(10000))throw new Exception("Application did not close");
   Console.WriteLine("checkpoint " + timer.Elapsed); report.Add("PASS encerramento normal");
   File.WriteAllLines(Path.Combine(reportDir,"launch-results.txt"),report.ToArray(),Encoding.UTF8);
   foreach(string line in report)Console.WriteLine(line);
  } finally {if(!process.HasExited){process.CloseMainWindow();if(!process.WaitForExit(3000))process.Kill();}process.Dispose();}
 }
}






