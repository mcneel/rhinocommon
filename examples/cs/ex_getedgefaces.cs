using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Input;

namespace examples_cs
{
  public class GetEdgeFacesCommand : Command
  {
    public override string EnglishName { get { return "csGetEdgeFaces"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      ObjRef obj_ref;
      var rc = RhinoGet.GetOneObject(
        "Select edge curve", false, ObjectType.EdgeFilter, out obj_ref);
      if (rc != Result.Success)
        return rc;
      var edge = obj_ref.Edge();

      var face_idxs = edge.AdjacentFaces();
      var edge_owning_brep = edge.Brep;

      foreach (var idx in face_idxs)
      {
        var face = edge_owning_brep.Faces[idx];
        var face_copy = face.DuplicateFace(true);
        var id = doc.Objects.AddBrep(face_copy);
        doc.Objects.Find(id).Select(true);
      }
      doc.Views.Redraw();
      return Result.Success;
    }
  }
}