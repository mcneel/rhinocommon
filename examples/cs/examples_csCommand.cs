using System;
using Rhino;
using Rhino.Geometry;
using System.Collections.Generic;

namespace examples_cs
{
  class GetScaleXform : Rhino.Input.Custom.GetTransform
  {
    public Plane Plane { get; set; }
    public Point3d RefPoint { get; set; }
    public double Scale { get; set; }

    public GetScaleXform()
    {
      Plane = Plane.WorldXY;
      RefPoint = Point3d.Origin;
      Scale = 1;
    }

    protected override void OnDynamicDraw(Rhino.Input.Custom.GetPointDrawEventArgs e)
    {
      Point3d basePoint;
      if (TryGetBasePoint(out basePoint))
      {
        e.Display.DrawLine(basePoint, RefPoint, System.Drawing.Color.Black);
        e.Display.DrawPoint(RefPoint, System.Drawing.Color.Black);
        base.OnDynamicDraw(e);
      }
    }

    public Transform CalculateTransform( Rhino.Display.RhinoViewport viewport, double d )
    {
      Point3d basePoint;
      if (!TryGetBasePoint(out basePoint))
        return Transform.Identity;
      Plane plane = viewport.ConstructionPlane();
      plane.Origin = basePoint;
      Vector3d v = RefPoint - basePoint;

      double len1 = v.Length;
      if (Math.Abs(len1) < 0.000001)
        return Transform.Identity;

      v.Unitize();
      v = v * d;
      double len2 = v.Length;
      if (Math.Abs(len2) < 0.000001)
        return Transform.Identity;

      Scale = len2 / len1; ;
      return Transform.Scale(plane, Scale, Scale, Scale);
    }
    public override Transform CalculateTransform(Rhino.Display.RhinoViewport viewport, Point3d point)
    {
      Point3d basePoint;
      if (!TryGetBasePoint(out basePoint))
        return Transform.Identity;
      double len2 = (point - basePoint).Length;
      double len1 = (RefPoint - basePoint).Length;
      if (Math.Abs(len1) < 0.000001 || Math.Abs(len2) < 0.000001)
        return Transform.Identity;

      Scale = len2 / len1;

      Plane plane = viewport.ConstructionPlane();
      plane.Origin = basePoint;

      return Transform.Scale(plane, Scale, Scale, Scale);
    }

  }

  public class csTransformCommand : Rhino.Commands.TransformCommand
  {
    public override string EnglishName
    {
      get { return "examples_csTransform"; }
    }

    double m_scale = 1;
    bool m_copy = false;
    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      // Select objects to scale
      var list = new Rhino.Collections.TransformObjectList();
      var rc = SelectObjects("Select objects to scale", list);
      if (rc != Rhino.Commands.Result.Success)
        return rc;

      var anchor = new Point3d();
      var _ref = new Point3d();
      Plane plane = new Plane();

      // Origin point
      var gp = new Rhino.Input.Custom.GetPoint();
      gp.SetCommandPrompt("Origin point");
      var copy = new Rhino.Input.Custom.OptionToggle(m_copy,"No", "Yes");
      gp.AddOptionToggle("Copy", ref copy);
      for (; ; )
      {
        var res = gp.Get();
        if (res == Rhino.Input.GetResult.Point)
        {
          var view = gp.View();
          if (view == null)
            return Rhino.Commands.Result.Failure;
          plane = view.ActiveViewport.ConstructionPlane();
          anchor = gp.Point();
          break;
        }
        if (res == Rhino.Input.GetResult.Option)
          continue;

        return Rhino.Commands.Result.Cancel;
      }

      bool bAcceptDefault = true;

      // Scale factor or first reference point
      gp.SetCommandPrompt("Scale factor or first reference point");
      gp.SetBasePoint(anchor, true);
      gp.DrawLineFromPoint(anchor, true);
      gp.ConstrainToConstructionPlane(true);
      for (; ; )
      {
        if (bAcceptDefault)
        {
          gp.AcceptNumber(true, false);
          gp.SetDefaultNumber(m_scale);
        }
        else
        {
          gp.AcceptNothing(true);
          gp.ClearDefault();
        }

        var res = gp.Get();
        if (res == Rhino.Input.GetResult.Point)
        {
          _ref = gp.Point();
          break;
        }
        if (res == Rhino.Input.GetResult.Number)
        {
          double scale = Math.Abs(gp.Number());
          const double EPS_DIVIDE = 0.000001;
          if (scale < EPS_DIVIDE)
            continue;
          m_scale = scale;
          plane.Origin = anchor;

          var xform = Transform.Scale(plane, m_scale, m_scale, m_scale);
          TransformObjects(list, xform, copy.CurrentValue, copy.CurrentValue);
          m_copy = copy.CurrentValue;
          if (m_copy)
          {
            bAcceptDefault = false;
            continue;
          }
          doc.Views.Redraw();
          return Rhino.Commands.Result.Success;
        }

        if (res == Rhino.Input.GetResult.Nothing)
        {
          if (bAcceptDefault == false)
            return Rhino.Commands.Result.Success;

          plane.Origin = anchor;
          var xform = Transform.Scale(plane, m_scale, m_scale, m_scale);
          TransformObjects(list, xform, copy.CurrentValue, copy.CurrentValue);

          m_copy = copy.CurrentValue;
          if (m_copy)
          {
            bAcceptDefault = false;
            continue;
          }
          doc.Views.Redraw();
          return Rhino.Commands.Result.Success;
        }
        if (res == Rhino.Input.GetResult.Option)
          continue;
        return Rhino.Commands.Result.Cancel;
      }

      // Second reference point
      var gx = new GetScaleXform();
      gx.SetCommandPrompt("Second reference point");
      gx.AddOptionToggle("copy", ref copy);
      gx.AddTransformObjects(list);
      gx.SetBasePoint(anchor, true);
      gx.DrawLineFromPoint(anchor, true);
      gx.ConstrainToConstructionPlane(true);
      gx.Plane = plane;
      gx.RefPoint = _ref;
      gx.AcceptNothing(true);
      gx.AcceptNumber(true, false);

      rc = Rhino.Commands.Result.Cancel;
      for (; ; )
      {
        var res = gx.GetXform();
        if (res == Rhino.Input.GetResult.Point)
        {
          var view = gx.View();
          if (view == null)
          {
            rc = Rhino.Commands.Result.Failure;
            break;
          }
          var xform = gx.CalculateTransform(view.ActiveViewport, gx.Point());
          if (xform.IsValid && xform != Transform.Identity)
          {
            TransformObjects(list, xform, copy.CurrentValue, copy.CurrentValue);
            rc = Rhino.Commands.Result.Success;
            m_scale = gx.Scale;
          }
          m_copy = copy.CurrentValue;
          if (m_copy)
            continue;

          break;
        }

        if (res == Rhino.Input.GetResult.Number)
        {
          var view = gx.View();
          if (view == null)
          {
            rc = Rhino.Commands.Result.Failure;
            break;
          }

          var xform = gx.CalculateTransform(view.ActiveViewport, gx.Number());
          if (xform.IsValid && xform != Transform.Identity)
          {
            TransformObjects(list, xform, copy.CurrentValue, copy.CurrentValue);
            rc = Rhino.Commands.Result.Success;
            m_scale = gx.Scale;
          }
          m_copy = copy.CurrentValue;
          if (m_copy)
            continue;
          break;
        }

        if (res == Rhino.Input.GetResult.Option)
          continue;

        if (res == Rhino.Input.GetResult.Nothing)
          break;

        break;
      }

      doc.Views.Redraw();
      return rc;
    }
  }

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

    static Brep m_brep;
    static double m_tol;
    static void RadialContourHelper(int i)
    {
      double radians = RhinoMath.ToRadians(i);
      var plane = Rhino.Geometry.Plane.WorldXY;
      var axis = Rhino.Geometry.Vector3d.YAxis;
      plane.Rotate(radians, axis, Rhino.Geometry.Point3d.Origin);
      Curve[] curves;
      Point3d[] points;
      Rhino.Geometry.Intersect.Intersection.BrepPlane(m_brep, plane, m_tol, out curves, out points);
    }

    static void RadialContour(bool parallel, Brep brep)
    {
      const int COUNT = 360;
      m_brep = brep;
      m_tol = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
      if (parallel)
      {
        System.Threading.Tasks.Parallel.For(0, COUNT, RadialContourHelper);
      }
      else
      {
        for (int i = 0; i < COUNT; i++)
        {
          RadialContourHelper(i);
        }
      }
      m_brep = null;
    }

    Rhino.Commands.Result TestMt(RhinoDoc doc)
    {
      Rhino.DocObjects.ObjRef objref;
      var rc = Rhino.Input.RhinoGet.GetOneObject("Select brep", true, Rhino.DocObjects.ObjectType.Brep, out objref);
      if( rc!=Rhino.Commands.Result.Success)
        return rc;
      var brep = objref.Brep();
      if( brep==null )
        return Rhino.Commands.Result.Cancel;
      brep.EnsurePrivateCopy(); // make our copy local so we absolutely know the doc won't be mesing with it

      var start = DateTime.Now;
      RadialContour(false, brep);
      var span = DateTime.Now - start;
      RhinoApp.WriteLine(" serial = {0}", span.Milliseconds);
      start = DateTime.Now;
      RadialContour(true, brep);
      span = DateTime.Now - start;
      RhinoApp.WriteLine(" parallel = {0}", span.Milliseconds);
      doc.Views.Redraw();
      return Rhino.Commands.Result.Success;
    }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      //return TestMt(doc);
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
      //Test(Examples.ConstrainedCopy, doc);
      //Test(Examples.CreateBlock, doc);
      //Test(Examples.CurveBoundingBox, doc);
      //Test(Examples.DivideByLengthPoints, doc);
      //Test(Examples.DetermineObjectLayer, doc);
      //Test(Examples.DupBorder, doc);
      //Test(Examples.FindObjectsByName, doc);
      //Test(Examples.IntersectCurves, doc);
      //Test(Examples.InsertKnot, doc);
      //Test(Examples.IntersectLines, doc);
      //Test(Examples.InstanceDefinitionObjects, doc);
      //Test(Examples.IsocurveDensity, doc);
      //Test(Examples.MoveCPlane, doc);
      //Test(Examples.ObjectDecoration, doc);
      //Test(Examples.ObjectDisplayMode, doc);
      //Test(Examples.OrientOnSrf, doc);
      //Test(Examples.SelLayer, doc);
      //Test(Examples.Sweep1, doc);
      //Test(Examples.UnrollSurface, doc);
      //Test(Examples.UnrollSurface2, doc);
      //Test(Examples.ZoomToObject, doc);


      var arc = new Arc(Plane.WorldXY, new Point3d(1, 2, 3), 10, RhinoMath.ToRadians(40));
      
      //read/write binary

      var options = new Rhino.FileIO.SerializationOptions();
      options.RhinoVersion = 4;
      var context = new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All, options);
      var bin_serializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter(null, context);
      var bin_stream = new System.IO.FileStream("C:\\TestBinary.bin", System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None);
      bin_serializer.Serialize(bin_stream, arc);
      bin_stream.Close();
      bin_stream = new System.IO.FileStream("C:\\TestBinary.bin", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
      var a = (Arc)bin_serializer.Deserialize(bin_stream);
      bin_stream.Close();
      var attr = new Rhino.DocObjects.ObjectAttributes();
      attr.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
      attr.ObjectColor = System.Drawing.Color.Red;
      //doc.Objects.AddMesh(a, attr);
      
      //data contract
      var stream = new System.IO.FileStream("C:\\TestDataContract.xml", System.IO.FileMode.Create);
      var dc_serializer = new System.Runtime.Serialization.DataContractSerializer(arc.GetType());
      dc_serializer.WriteObject(stream, arc);
      stream.Close();
      stream = new System.IO.FileStream("C:\\TestDataContract.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
      var b = (Arc)dc_serializer.ReadObject(stream);
      stream.Close();
      attr.ObjectColor = System.Drawing.Color.Green;
      //doc.Objects.AddMesh(b, attr);
      
      //net contract
      stream = new System.IO.FileStream("C:\\TestNetDataContract.xml", System.IO.FileMode.Create);
      var ndc_serializer = new System.Runtime.Serialization.NetDataContractSerializer(context);
      ndc_serializer.WriteObject(stream, arc);
      stream.Close();
      stream = new System.IO.FileStream("C:\\TestNetDataContract.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
      var c = (Arc)ndc_serializer.ReadObject(stream);
      stream.Close();
      attr.ObjectColor = System.Drawing.Color.Pink;
      //doc.Objects.AddMesh(c, attr);


      //soap
      stream = new System.IO.FileStream("C:\\TestSoapFormatter.xml", System.IO.FileMode.Create);
      var soap_serializer = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter(null, context);
      soap_serializer.Serialize(stream, arc);
      stream.Close();
      stream = new System.IO.FileStream("C:\\TestSoapFormatter.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
      var d = (Arc)soap_serializer.Deserialize(stream);
      stream.Close();
      attr.ObjectColor = System.Drawing.Color.Blue;
      //doc.Objects.AddMesh(d, attr);
      //doc.Views.Redraw();

      /*
      //xml
      stream = new System.IO.FileStream("C:\\SteveTestXml.xml", System.IO.FileMode.Create);
      var xml_serializer = new System.Xml.Serialization.XmlSerializer(typeof(Rhino.Geometry.NurbsCurve));
      xml_serializer.Serialize(stream, c);
      stream.Close();
      stream = new System.IO.FileStream("C:\\SteveTestXml.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
      var d = soap_serializer.Deserialize(stream);
      stream.Close();
      */

      
      return Rhino.Commands.Result.Success;
    }
  }
}


