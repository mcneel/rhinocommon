using Rhino;
using Rhino.Commands;
using System;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("75ED2D51-3633-4624-947A-02B15706D3F4")]
  public class ViewportResolutionCommand : Command
  {
    public override string EnglishName { get { return "csViewportResolution"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var activeViewport = doc.Views.ActiveView.ActiveViewport;
      RhinoApp.WriteLine(String.Format("Name = {0}: Width = {1}, Height = {2}", 
        activeViewport.Name, activeViewport.Size.Width, activeViewport.Size.Height));
      return Result.Success;
    }
  }
}