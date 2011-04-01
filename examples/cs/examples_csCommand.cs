using System;
using Rhino;
using Rhino.Geometry;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("96fdf4ac-5457-439a-95ed-4085977065df")]
  public class examples_csCommand : Rhino.Commands.Command
  {
    static examples_csCommand m_thecommand;
    public examples_csCommand()
    {
      // Rhino only creates one instance of each command class defined in a plug-in, so it is
      // safe to hold on to a static reference.
      m_thecommand = this;
    }

    ///<summary>The one and only instance of this command</summary>
    public static examples_csCommand TheCommand
    {
      get { return m_thecommand; }
    }

    ///<returns>The command name as it appears on the Rhino command line</returns>
    public override string EnglishName
    {
      get { return "examples_cs"; }
    }

    delegate Rhino.Commands.Result TestFunc(RhinoDoc doc);
    void Test(TestFunc func, RhinoDoc doc)
    {
      RhinoApp.WriteLine("[TEST START] - " + func.Method.ToString());
      Rhino.Commands.Result rc = func(doc);
      RhinoApp.WriteLine("[TEST DONE] - result = " + rc.ToString());
    }


    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      examples_csPlugIn.ThePlugIn.IncrementRunCommandCount();
      //Test(Examples.ActiveViewport, doc);
      //Test(Examples.AddBrepBox, doc);
      //Test(Examples.AddChildLayer, doc);
      //Test(Examples.AddCircle, doc);
      //Test(Examples.AddCone, doc);
      //Test(Examples.AddCylinder, doc);
      //Test(Examples.AddBackgroundBitmap, doc);
      //Test(Examples.AddClippingPlane, doc);
      //Test(Examples.AddLayer, doc);
      //Test(Examples.AddLine, doc);
      //Test(Examples.AddLinearDimension, doc);
      //Test(Examples.AddLinearDimension2, doc);
      //Test(Examples.AddMaterial, doc);
      //Test(Examples.AddMesh, doc);
      //Test(Examples.AddNamedView, doc);
      //Test(Examples.AddNurbsCircle, doc);
      //Test(Examples.AddNurbsCurve, doc);
      //Test(Examples.AddObjectsToGroup, doc);
      //Test(Examples.AddSphere, doc);
      //Test(Examples.AddAnnotationText, doc);
      //Test(Examples.AddTorus, doc);
      //Test(Examples.AddTruncatedCone, doc);
      //Test(Examples.AdvancedDisplay, doc);
      //Test(Examples.ArcLengthPoint, doc);
      //Test(Examples.BooleanDifference, doc);
      //Test(Examples.BlockInsertionPoint, doc);
      //Test(Examples.CommandLineOptions, doc);
      //Test(Examples.DivideByLengthPoints, doc);
      //Test(Examples.DetermineObjectLayer, doc);
      //Test(Examples.DupBorder, doc);
      //Test(Examples.FindObjectsByName, doc);
      //Test(Examples.InsertKnot, doc);
      //Test(Examples.IntersectLines, doc);
      Test(Examples.MoveCPlane, doc);
      //Test(Examples.ObjectDisplayMode, doc);
      //Test(Examples.OrientOnSrf, doc);
      //Test(Examples.SelLayer, doc);
      //Test(Examples.UnrollSurface, doc);
      //Test(Examples.UnrollSurface2, doc);
      return Rhino.Commands.Result.Success;
    }
  }
}


namespace RhinoGold.Commands._01_UI
{
  [System.Runtime.InteropServices.Guid("ec197a46-e9ba-42b4-a9d2-167189ffda00")]
  public class ShowTripod : Rhino.Commands.Command
  {
    static ShowTripod m_thecommand;

    public ShowTripod()
    {
      // Rhino only creates one instance of each command class defined in a plug-in, so it is
      // safe to hold on to a static reference.
      m_thecommand = this;
    }


    ///<summary>The one and only instance of this command</summary>
    public static ShowTripod TheCommand
    {
      get { return m_thecommand; }
    }

    ///<returns>The command name as it appears on the Rhino command line</returns>
    public override string EnglishName
    {
      get { return "ShowTripod"; }
    }

    Rhino.Display.DisplayMaterial m_goldmaterial;
    Mesh m_MeshToDraw;
    bool m_conduits_on = false;
    BoundingBox m_mesh_bounds;
    Point3d m_Origin;
    double m_Scale;

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      // might as well not create objects until we need them
      if (m_MeshToDraw == null)
      {
        m_goldmaterial = new Rhino.Display.DisplayMaterial();
        m_goldmaterial.Diffuse = System.Drawing.Color.FromArgb(255, 155, 0);
        //m_goldmaterial.Transparency = 0.9;
        m_goldmaterial.Shine = 0.1;
        m_goldmaterial.Emission = System.Drawing.Color.FromArgb(50, 25, 0);

        Torus torus = new Torus(Plane.WorldXY, 10, 3);
        NurbsSurface nurb = torus.ToNurbsSurface();
        Mesh[] myMesh = Mesh.CreateFromBrep(nurb.ToBrep());
        m_MeshToDraw = myMesh[0];
        m_mesh_bounds = m_MeshToDraw.GetBoundingBox(true);
      }
      m_conduits_on = !m_conduits_on;
      if (m_conduits_on)
      {
        RhinoApp.WriteLine("Turning tripod display on");
        Rhino.Display.DisplayPipeline.CalculateBoundingBox += ConduitCalculateBoundingBox;
        Rhino.Display.DisplayPipeline.PostDrawObjects += ConduitDraw;
      }
      else
      {
        RhinoApp.WriteLine("Turning tripod display off");
        Rhino.Display.DisplayPipeline.CalculateBoundingBox -= ConduitCalculateBoundingBox;
        Rhino.Display.DisplayPipeline.PostDrawObjects -= ConduitDraw;
      }
      doc.Views.Redraw();
      return Rhino.Commands.Result.Success;
    }


    private void ComputeScene(Rhino.Display.DrawEventArgs e)
    {
      //Retrieve clipping planes
      Plane nPlane;
      e.Display.Viewport.GetFrustumNearPlane(out nPlane);
      Plane fPlane;
      e.Display.Viewport.GetFrustumNearPlane(out fPlane);

      //Calculate projection plane
      Plane projection = fPlane;
      projection.Origin = (nPlane.Origin + fPlane.Origin) * 0.5;

      //Retrieve inverse drawing XForm
      Transform s2wXform = e.Display.Viewport.GetTransform(Rhino.DocObjects.CoordinateSystem.Screen, Rhino.DocObjects.CoordinateSystem.World);

      //Retrieve Viewport size, in pixels
      int W = e.Viewport.ScreenPortBounds.Width;

      // Create custom axis system origin, in screen coordinates
      Point3d pt = new Point3d(W + 60, 60, 0.0);
      //Project the origin back into world space
      pt.Transform(s2wXform);

      //Create a ray from the camera, through the custom axis origin
      Line Origin_Ray;
      if (e.Display.Viewport.IsPerspectiveProjection)//(dp.GetRhinoVP.VP.Projection = IOn.view_projection.perspective_view) Then
      {
        //Perform camera based intersection
        Origin_Ray = new Line(e.Display.Viewport.CameraLocation, pt);
      }
      else
      {
        //Perform parallel intersection
        Origin_Ray = new Line(pt, pt + e.Display.Viewport.CameraDirection);
      }

      //Intersect it with the projection plane
      double p = 0;
      Rhino.Geometry.Intersect.Intersection.LinePlane(Origin_Ray, projection, out p);

      //Set the origin, in world coordinates
      this.m_Origin = Origin_Ray.PointAt(p);

      //Calculate the screen scale at the origin
      double pix_per_unit;
      e.Display.Viewport.GetWorldToScreenScale(this.m_Origin, out pix_per_unit);

      //Calculate the scale of the axis system (in units per pixel)
      this.m_Scale = 1 / pix_per_unit;
    }

    void ConduitCalculateBoundingBox(object sender, Rhino.Display.CalculateBoundingBoxEventArgs e)
    {
      ComputeScene(e);
      Transform xform = Transform.Scale(Point3d.Origin, m_Scale * 2);
      xform *= Transform.Translation(m_Origin.X, m_Origin.Y, m_Origin.Z);
      BoundingBox bbox = xform.TransformBoundingBox(m_mesh_bounds);
      bbox.Inflate(20);
      e.IncludeBoundingBox(bbox);
    }

    void ConduitDraw(object sender, Rhino.Display.DrawEventArgs e)
    {
      ComputeScene(e);

      Transform xform = Transform.Scale(Point3d.Origin, m_Scale);
      xform *= Transform.Translation(new Vector3d(m_Origin));
      e.Display.PushModelTransform(xform);
      e.Display.DrawMeshShaded(m_MeshToDraw, m_goldmaterial);
      e.Display.PopModelTransform();
    }
  }
}

