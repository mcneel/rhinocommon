using Rhino;
using Rhino.Commands;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using System.Drawing;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("DE24BC2C-B2E7-4CA1-B2BE-BC0ED742E645")]
  public class MeshDrawingCommand : Command
  {
    public override string EnglishName { get { return "csDrawMesh"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var gs = new GetObject();
      gs.SetCommandPrompt("select sphere");
      gs.GeometryFilter = ObjectType.Surface;
      gs.DisablePreSelect();
      gs.SubObjectSelect = false;
      gs.Get();
      if (gs.CommandResult() != Result.Success)
        return gs.CommandResult();

      Sphere sphere;
      gs.Object(0).Surface().TryGetSphere(out sphere);
      if (sphere.IsValid)
      {
        var mesh = Mesh.CreateFromSphere(sphere, 10, 10);
        if (mesh == null)
          return Result.Failure;

        var conduit = new DrawBlueMeshConduit(mesh) {Enabled = true};
        doc.Views.Redraw();

        string inStr = "";
        Rhino.Input.RhinoGet.GetString("press <Enter> to continue", true, ref inStr);

        conduit.Enabled = false;
        doc.Views.Redraw();
        return Result.Success;
      }
      else
        return Result.Failure;
    }
  }

  class DrawBlueMeshConduit : DisplayConduit
  {
    Mesh _mesh = null;
    Color _color;
    DisplayMaterial _material = null;
    BoundingBox _bbox;

    public DrawBlueMeshConduit(Mesh mesh)
    {
      // set up as much data as possible so we do the minimum amount of work possible inside
      // the actual display code
      _mesh = mesh;
      _color = System.Drawing.Color.Blue;
      _material = new DisplayMaterial();
      _material.Diffuse = _color;
      if (_mesh != null && _mesh.IsValid)
        _bbox = _mesh.GetBoundingBox(true);
    }

    // this is called every frame inside the drawing code so try to do as little as possible
    // in order to not degrade display speed. Don't create new objects if you don't have to as this
    // will incur an overhead on the heap and garbage collection.
    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
    {
      base.CalculateBoundingBox(e);
      // Since we are dynamically drawing geometry, we needed to override
      // CalculateBoundingBox. Otherwise, there is a good chance that our
      // dynamically drawing geometry would get clipped.
 
      // Union the mesh's bbox with the scene's bounding box
      e.BoundingBox.Union(_bbox);
    }

    protected override void PreDrawObjects(DrawEventArgs e)
    {
      base.PreDrawObjects(e);
      var vp = e.Display.Viewport;
      if (vp.DisplayMode.EnglishName.ToLower() == "wireframe")
        e.Display.DrawMeshWires(_mesh, _color);
      else
        e.Display.DrawMeshShaded(_mesh, _material);
    }
  }
}