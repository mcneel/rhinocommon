// To build the add-in for MonoDevelop 2.6+
// go to the build directory with terminal and run the following
// /Applications/MonoDevelop.app/Contents/MacOS/mdtool setup pack MonoDevelop.RhinoDebug.dll
//
// This will generate a .mpack file that you can distribute to users

using System;
using System.Diagnostics;

namespace MonoDevelop.Debugger.Soft.Rhino
{
	public class RhinoSoftDebuggerSession : Mono.Debugging.Soft.SoftDebuggerSession
	{
		Process m_rhino_app;
		const string DEFAULT_PROFILE="monodevelop-rhino-debug";
		
		protected override void OnRun (Mono.Debugging.Client.DebuggerStartInfo startInfo)
		{
			var dsi = startInfo as RhinoDebuggerStartInfo;
      int assignedDebugPort;
      StartListening(dsi, out assignedDebugPort);
			StartRhinoProcess(dsi, assignedDebugPort);
		}
		
		void StartRhinoProcess(RhinoDebuggerStartInfo dsi, int assignedDebugPort)
		{
			if( m_rhino_app!=null )
				throw new InvalidOperationException("Rhino already started");
			
			//string userhome = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			//string app_path = System.IO.Path.Combine(userhome, "dev/rhino/src4/rhino4/build/Debug/Rhino.app/Contents/MacOS/Rhino");
			string app_path = "arch";
			var psi = new ProcessStartInfo(app_path);
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardError = true;
      psi.Arguments = "-i386 /Applications/Rhinoceros.app/Contents/MacOS/Rhino";
			
      var args = (Mono.Debugging.Soft.SoftDebuggerRemoteArgs) dsi.StartArgs;
			string envvar = string.Format("transport=dt_socket,address={0}:{1}", args.Address, assignedDebugPort);
			psi.EnvironmentVariables.Add("RHINO_SOFT_DEBUG", envvar);
						
			m_rhino_app = Process.Start(psi);
			ConnectOutput(m_rhino_app.StandardOutput, false);
			ConnectOutput(m_rhino_app.StandardError, true);
			m_rhino_app.EnableRaisingEvents = true;
			m_rhino_app.Exited += delegate { EndSession(); };
		}
		
		protected override void EndSession ()
		{
			EndRhinoProcess();
			base.EndSession ();
		}
		protected override void OnExit ()
		{
			EndRhinoProcess();
			base.OnExit();
		}
		void EndRhinoProcess()
		{
			if( m_rhino_app!=null && !m_rhino_app.HasExited )
			{
				m_rhino_app.Kill();
			}
			m_rhino_app = null;
		}
	}
}

