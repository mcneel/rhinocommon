using System;
using Rhino;
using Rhino.Geometry;
using System.Collections.Generic;

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

      /*
    slices = range(COUNT)
    def parallel_contour(i):
        try:
            rad = math.radians(i)
            plane = Rhino.Geometry.Plane.WorldXY
            axis = Rhino.Geometry.Vector3d(0,1,0)
            plane.Rotate(rad, axis, Rhino.Geometry.Point3d.Origin)
            tol = scriptcontext.doc.ModelAbsoluteTolerance
            rc, crvs, pts = Rhino.Geometry.Intersect.Intersection.BrepPlane(brep, plane, tol)
            if rc: slices[i] = crvs
        except:
            pass

    if parallel:
        tasks.Parallel.ForEach(range(COUNT), parallel_contour)
    else:
        #parallel_contour(COUNT-1)
        for i in range(COUNT): parallel_contour(i)
    if slices:
        for slice in slices:
            for s in slice: scriptcontext.doc.Objects.AddCurve(s)
       */
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
      //Test(Examples.DivideByLengthPoints, doc);
      //Test(Examples.DetermineObjectLayer, doc);
      //Test(Examples.DupBorder, doc);
      //Test(Examples.FindObjectsByName, doc);
      //Test(Examples.InsertKnot, doc);
      //Test(Examples.IntersectLines, doc);
      Test(Examples.IsocurveDensity, doc);
      //Test(Examples.MoveCPlane, doc);
      //Test(Examples.ObjectDecoration, doc);
      //Test(Examples.ObjectDisplayMode, doc);
      //Test(Examples.OrientOnSrf, doc);
      //Test(Examples.SelLayer, doc);
      //Test(Examples.Sweep1, doc);
      //Test(Examples.UnrollSurface, doc);
      //Test(Examples.UnrollSurface2, doc);

/*
      Sphere s = new Sphere(new Point3d(1,2,3), 12);
      var mesh = Mesh.CreateFromSphere(s, 20, 20);
      mesh.SetUserString("serializeIO", "yep it works!");
      
      //read/write binary

      var options = new Rhino.FileIO.SerializationOptions();
      options.RhinoVersion = 4;
      var context = new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All, options);
      var bin_serializer = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter(null, context);
      var bin_stream = new System.IO.FileStream("C:\\TestBinary.bin", System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None);
      bin_serializer.Serialize(bin_stream, mesh);
      bin_stream.Close();
      bin_stream = new System.IO.FileStream("C:\\TestBinary.bin", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
      var a = bin_serializer.Deserialize(bin_stream) as Mesh;
      bin_stream.Close();
      var attr = new Rhino.DocObjects.ObjectAttributes();
      attr.ColorSource = Rhino.DocObjects.ObjectColorSource.ColorFromObject;
      attr.ObjectColor = System.Drawing.Color.Red;
      doc.Objects.AddMesh(a, attr);

      //data contract
      var stream = new System.IO.FileStream("C:\\TestDataContract.xml", System.IO.FileMode.Create);
      var dc_serializer = new System.Runtime.Serialization.DataContractSerializer(mesh.GetType());
      dc_serializer.WriteObject(stream, mesh);
      stream.Close();
      stream = new System.IO.FileStream("C:\\TestDataContract.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
      var b = dc_serializer.ReadObject(stream) as Mesh;
      stream.Close();
      attr.ObjectColor = System.Drawing.Color.Green;
      doc.Objects.AddMesh(b, attr);

      //net contract
      stream = new System.IO.FileStream("C:\\TestNetDataContract.xml", System.IO.FileMode.Create);
      var ndc_serializer = new System.Runtime.Serialization.NetDataContractSerializer(context);
      ndc_serializer.WriteObject(stream, mesh);
      stream.Close();
      stream = new System.IO.FileStream("C:\\TestNetDataContract.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
      var c = ndc_serializer.ReadObject(stream) as Mesh;
      stream.Close();
      attr.ObjectColor = System.Drawing.Color.Pink;
      doc.Objects.AddMesh(c, attr);


      //soap
      stream = new System.IO.FileStream("C:\\TestSoapFormatter.xml", System.IO.FileMode.Create);
      var soap_serializer = new System.Runtime.Serialization.Formatters.Soap.SoapFormatter(null, context);
      soap_serializer.Serialize(stream, mesh);
      stream.Close();
      stream = new System.IO.FileStream("C:\\TestSoapFormatter.xml", System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read);
      var d = soap_serializer.Deserialize(stream) as Mesh;
      stream.Close();
      attr.ObjectColor = System.Drawing.Color.Blue;
      doc.Objects.AddMesh(d, attr);
      doc.Views.Redraw();

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


