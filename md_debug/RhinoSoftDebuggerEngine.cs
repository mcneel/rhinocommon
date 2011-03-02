using System;

namespace MonoDevelop.Debugger.Soft.Rhino
{
	public class RhinoSoftDebuggerEngine : MonoDevelop.Debugger.IDebuggerEngine
	{
		public string Id
		{
			get{ return "Mono.Debugger.Soft.Rhino"; }
		}
		
		public string Name
		{
			get { return "Mono Soft Debugger for Rhino"; }
		}
		
		public bool CanDebugCommand (Core.Execution.ExecutionCommand cmd)
		{
			return true;
		}	
		public Mono.Debugging.Client.DebuggerStartInfo CreateDebuggerStartInfo (Core.Execution.ExecutionCommand cmd)
		{
			return new RhinoDebuggerStartInfo("Rhino");
		}

		public Mono.Debugging.Client.DebuggerSession CreateSession ()
		{
			return new RhinoSoftDebuggerSession();
		}

		public Mono.Debugging.Client.ProcessInfo[] GetAttachableProcesses ()
		{
			return new Mono.Debugging.Client.ProcessInfo[0];
		}
	}
	
	class RhinoDebuggerStartInfo : Mono.Debugging.Soft.SoftDebuggerStartInfo
	{
    public RhinoDebuggerStartInfo(string appName)
			: base(new Mono.Debugging.Soft.SoftDebuggerListenArgs(appName, System.Net.IPAddress.Loopback, 0))
		{
		}
	}
}

