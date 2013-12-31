using Rhino;
using Rhino.Commands;
using Rhino.Display;
using System.Linq;
using Rhino.Input.Custom;
using Rhino.DocObjects;

namespace examples_cs
{
  public class SingleColorBackfacesCommand : Command
  {
    public override string EnglishName { get { return "csSingleColorBackfaces"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var display_mode_descs =
        from dm in DisplayModeDescription.GetDisplayModes()
        where dm.EnglishName == "Shaded"
        select dm;

      foreach (var dmd in display_mode_descs)
      {
        //RhinoApp.WriteLine(dmd.DisplayAttributes.);
      }
      return Result.Success;
    }
  }
}