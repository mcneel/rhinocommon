using System;
using System.Diagnostics;

namespace MonoDevelop.Debugger.Soft.Rhino
{
	public class RhinoSoftDebuggerSession : MonoDevelop.Debugger.Soft.RemoteSoftDebuggerSession
	{
		Process m_rhino_app;
		const string DEFAULT_PROFILE="monodevelop-rhino-debug";
		
		protected override void OnRun (Mono.Debugging.Client.DebuggerStartInfo startInfo)
		{
			var dsi = startInfo as RhinoDebuggerStartInfo;
			StartRhinoProcess(dsi);
			StartListening(dsi);
		}
		
		void StartRhinoProcess(RhinoDebuggerStartInfo dsi)
		{
			if( m_rhino_app!=null )
				throw new InvalidOperationException("Rhino already started");
			
			string userhome = System.Environment.GetFolderPath(Environment.SpecialFolder.Personal);
			string dev_path = System.IO.Path.Combine(userhome, "dev/rhino/src4/rhino4/build/Debug/Rhino.app/Contents/MacOS/Rhino");
			//string user_path = "/Applications/Rhino.app/Contents/MacOS/Rhino";
			var psi = new ProcessStartInfo(dev_path);
			psi.UseShellExecute = false;
			psi.RedirectStandardOutput = true;
			psi.RedirectStandardError = true;
			
			string envvar = string.Format("transport=dt_socket,address={0}:{1}", dsi.Address, dsi.DebugPort);
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

