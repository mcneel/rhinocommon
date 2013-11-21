using Rhino;
using Rhino.Commands;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("CB7F7039-A986-44DC-BD51-C3EBEC6A212A")]
  public class ex_evnormal : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csDetermineNormDirOfBrepFace"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      // select a surface
      var gs = new Rhino.Input.Custom.GetObject();
      gs.SetCommandPrompt("select surface");
      gs.GeometryFilter = Rhino.DocObjects.ObjectType.Surface;
      gs.DisablePreSelect();
      gs.SubObjectSelect = false;
      gs.Get();
      if (gs.CommandResult() != Result.Success)
        return gs.CommandResult();

      // get the selected face
      var face = gs.Object(0).Face();
      if (face == null)
        return Result.Failure;

      // pick a point on the surface.  Constain
      // picking to the face.
      var gp = new Rhino.Input.Custom.GetPoint();
      gp.SetCommandPrompt("select point on surface");
      gp.Constrain(face, false);
      gp.Get();
      if (gp.CommandResult() != Result.Success)
        return gp.CommandResult();

      // get the parameters of the point on the
      // surface that is clesest to gp.Point()
      double u, v;
      if (face.ClosestPoint(gp.Point(), out u, out v))
      {
        var dir = face.NormalAt(u, v);
        if (face.OrientationIsReversed)
          dir.Reverse();
        RhinoApp.WriteLine(
          string.Format(
            "Surface normal at uv({0:f},{1:f}) = ({2:f},{3:f},{4:f})", 
            u, v, dir.X, dir.Y, dir.Z));
      }
      return Rhino.Commands.Result.Success;
    }
  }
}