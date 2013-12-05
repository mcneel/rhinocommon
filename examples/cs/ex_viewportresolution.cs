using Rhino;
using System;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("75ED2D51-3633-4624-947A-02B15706D3F4")]
  public class ex_viewportresolution : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csViewportResolution"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var avp = doc.Views.ActiveView.ActiveViewport;
      RhinoApp.WriteLine(String.Format("Name = {0}: Width = {1}, Height = {2}", avp.Name, avp.Size.Width, avp.Size.Height));
      return Rhino.Commands.Result.Success;
    }
  }
}