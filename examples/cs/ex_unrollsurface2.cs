using System;

partial class Examples
{
  public static Rhino.Commands.Result UnrollSurface2(Rhino.RhinoDoc doc)
  {
    Rhino.Commands.Result rc = Rhino.Commands.Result.Success;
    Rhino.DocObjects.ObjectType filter = Rhino.DocObjects.ObjectType.Brep | Rhino.DocObjects.ObjectType.Surface;
    Rhino.DocObjects.ObjRef objref;
    rc = Rhino.Input.RhinoGet.GetOneObject("Select surface or brep to unroll", false, filter, out objref);
    if (rc != Rhino.Commands.Result.Success)
      return rc;
    Rhino.Geometry.Unroller unroll=null;
    Rhino.Geometry.Brep brep = objref.Brep();
    if (brep != null)
      unroll = new Rhino.Geometry.Unroller(brep);
    else
    {
      Rhino.Geometry.Surface srf = objref.Surface();
      if (srf != null)
        unroll = new Rhino.Geometry.Unroller(srf);
    }
    if (unroll == null)
      return Rhino.Commands.Result.Cancel;

    Rhino.Geometry.Mesh mesh = brep.Faces[0].GetMesh();
    if (mesh == null)
      return Rhino.Commands.Result.Cancel;

    unroll.AddFollowingGeometry(mesh.Vertices.ToPoint3dArray());

    unroll.ExplodeOutput = false;
    Rhino.Geometry.Curve[] curves;
    Rhino.Geometry.Point3d[] points;
    Rhino.Geometry.TextDot[] dots;
    Rhino.Geometry.Brep[] breps = unroll.PerformUnroll(out curves, out points, out dots);

    // change the mesh vertices to the flattened form and add it to the document
    if( points.Length == mesh.Vertices.Count )
    {
      for( int i=0; i<points.Length; i++ )
        mesh.Vertices.SetVertex(i, points[i]);
      mesh.Normals.ComputeNormals();
    }
    doc.Objects.AddMesh(mesh, objref.Object().Attributes);
    doc.Views.Redraw();
    return Rhino.Commands.Result.Success;
  }
}
