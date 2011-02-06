using System;
using Rhino;

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
      //Test(Examples.AddLayer, doc);
      //Test(Examples.AddBackgroundBitmap, doc);
      //Test(Examples.AddClippingPlane, doc);
      //Test(Examples.AddLine, doc);
      //Test(Examples.AddLinearDimension, doc);
      //Test(Examples.AddLinearDimension2, doc);
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
      //Test(Examples.BlockInsertionPoint, doc);
      //Test(Examples.CommandLineOptions, doc);
      //Test(Examples.DivideByLengthPoints, doc);
      //Test(Examples.DetermineObjectLayer, doc);
      //Test(Examples.FindObjectsByName, doc);
      //Test(Examples.OrientOnSrf, doc);
      //Test(Examples.SelLayer, doc);
      Test(Examples.UnrollSurface, doc);
      return Rhino.Commands.Result.Success;
    }
  }
}

