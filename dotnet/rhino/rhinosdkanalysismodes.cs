using System;
using Rhino.Runtime;

namespace Rhino.Display
{
  public static class VisualAnalysisModeIds
  {
    /// <summary>
    /// This ID is used to check for any shaded analysis mode:
    /// false color (like draft angle) or texture based (like zebra and emap).
    /// Pass this Id to ObjectInAnalysisMode() to answer queries about what
    /// types of analysis modes are active.
    /// </summary>
    public static Guid RhinoShadedAnalysisModeId
    {
      get { return new Guid("2E5FE617-7D66-4ea0-8572-A1C3F8F06B84"); }
    }

    /// <summary>
    /// This ID is used to check for any false color analysis mode
    /// like draft angle or curvature.
    /// Pass this Id to ObjectInAnalysisMode() to answer queries about what
    /// types of analysis modes are active.
    /// </summary>
    public static Guid RhinoFalseColorAnalysisModeId
    {
      get { return new Guid("5B3A0840-7899-4e22-987D-4921282558C0"); }
    }

    /// <summary>
    /// This ID is used to check for any texture based analysis mode
    /// like zebra or emap.
    /// Pass this Id to ObjectInAnalysisMode() to answer queries about what
    /// types of analysis modes are active.
    /// </summary>
    public static Guid RhinoTexturedAnalysisModeId
    {
      get { return new Guid("DF0D8626-3BEA-4471-B8D9-1E85A7967985"); }
    }

    /// <summary>
    /// This ID is used to check for any wireframe based analysis mode
    /// Pass this Id to ObjectInAnalysisMode() to answer queries about what
    /// types of analysis modes are active.
    /// </summary>
    public static Guid RhinoWireFrameAnalysisModeId
    {
      get { return new Guid("43BB9491-51E9-493a-B838-89536DD00960"); }
    }



    /////////////////////////////////////////////////////////////////
    //
    // These ids are passed to CRhinoObject::InAnalysisMode() to
    // to determine if an object is in a specfic core analysis mode.
    // Rhino has 6 core in visual analysis modes. Other analysis modes 
    // can be added by plug-ins.  Use the generic queries described above
    // if you need to know if an object is in a shaded mode, etc.

    // In edge anlysis mode brep and mesh edges are shown in 
    // a selected color.
    public static Guid RhinoEdgeAnalysisModeId
    {
      get { return new Guid("197B765D-CDA3-4411-8A0A-AD8E0891A918"); }
    }

    // In curvature graph analysis mode, curvature hair is shown on
    // curves and surfaces.
    public static Guid RhinoCurvatureGraphAnalysisModeId
    {
      get { return new Guid("DF59A9CF-E517-4846-9232-D9AE56A9D13D"); }
    }

    // In zebra stripe analysis mode, zebra stripes are shown 
    // on surfaces and meshes.
    public static Guid RhinoZebraStripeAnalysisModeId
    {
      get { return new Guid("0CCA817C-95D0-4b79-B5D7-CEB5A2975CE0"); }
    }

    // In emap analysis mode, an environment map is shown 
    // on surfaces and meshes.
    public static Guid RhinoEmapAnalysisModeId
    {
      get { return new Guid("DAEF834E-E978-4f7b-9026-A432C678C189"); }
    }

    // In curvature color analysis mode, surface curvature 
    // is shown using false color mapping.
    public static Guid RhinoCurvatureColorAnalyisModeId
    {
      get { return new Guid("639E9144-1C1A-4bba-8248-D330F50D7B69"); }
    }

    // In draft angle analysis mode, draft angle is 
    // displayed using false colors.
    public static Guid RhinoDraftAngleAnalysisModeId
    {
      get { return new Guid("F08463F4-22E2-4cf1-B810-F01925446D71"); }
    }


    public static Guid RhinoThicknessAnalysisModeId
    {
      get { return new Guid("B28E5435-D299-4933-A95D-3783C496FC66"); }
    }
  }

  public abstract class VisualAnalysisMode
  {
    public enum AnalysisStyle : int
    {
      Wireframe = 1,
      Texture = 2,
      FalseColor = 4
    }

    static System.Collections.Generic.List<VisualAnalysisMode> m_registered_modes;

    /// <summary>
    /// Find a VisualAnalysis mode by id
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static VisualAnalysisMode Find(Guid id)
    {
      if (m_registered_modes != null)
      {
        for (int i = 0; i < m_registered_modes.Count; i++)
        {
          if (m_registered_modes[i].GetType().GUID == id)
            return m_registered_modes[i];
        }
      }

      IntPtr pMode = UnsafeNativeMethods.CRhinoVisualAnalysisMode_Mode(id);
      if (pMode != IntPtr.Zero)
      {
        if (m_registered_modes == null)
          m_registered_modes = new System.Collections.Generic.List<VisualAnalysisMode>();
        var native = new NativeVisualAnalysisMode(id);
        m_registered_modes.Add(native);
        return native;
      }
      return null;
    }

    public static VisualAnalysisMode Find(Type t)
    {
      return Find(t.GUID);
    }

    internal IntPtr ConstPointer()
    {
      return UnsafeNativeMethods.CRhinoVisualAnalysisMode_Mode(Id);
    }


    /// <summary>
    /// Name of the analysis mode.  Used by the What command and the object
    /// properties details window to describe the object.
    /// </summary>
    public abstract string Name { get; }
    public abstract AnalysisStyle Style { get; }

    internal Guid m_id = Guid.Empty;
    public Guid Id
    {
      get
      {
        if (m_id != Guid.Empty)
          return m_id;
        return GetType().GUID;
      }
    }

    /// <summary>
    /// Turn the analysis mode's user interface on and off.  For Rhino's built
    /// in modes this opens or closes the modeless dialog that controls the
    /// analysis mode's display settings.
    /// </summary>
    /// <param name="on"></param>
    public virtual void EnableUserInterface(bool on) {}

    /// <summary>
    /// Return true if this visual analysis mode can be used on a given Rhino object
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public virtual bool ObjectSupportsAnalysisMode(Rhino.DocObjects.RhinoObject obj)
    {
      IntPtr pConstPointer = ConstPointer();
      IntPtr pConstRhinoObject = obj.ConstPointer();
      return UnsafeNativeMethods.CRhinoVisualAnalysisMode_ObjectSupportsAnalysisMode(pConstPointer, pConstRhinoObject);
    }

    /// <summary>
    /// True if this visual analysis mode should show isocuves on shaded surface
    /// objects.  Often a mode's user interface will provide a way to change this
    /// setting.
    /// </summary>
    public virtual bool ShowIsoCurves
    {
      get { return false; }
    }

    /// <summary>
    /// If an analysis mode needs to modify display attributes, this is the place
    /// to do it.  In particular, Style==Texture, then this function must be
    /// overridden.
    /// </summary>
    /// <remarks>
    /// Shaded analysis modes that use texture mapping, like zebra and emap,
    /// override this function set the tex, diffuse_color, and EnableLighting
    /// parameter.
    /// </remarks>
    /// <param name="obj"></param>
    /// <param name="attributes"></param>
    protected virtual void SetUpDisplayAttributes(Rhino.DocObjects.RhinoObject obj, DisplayPipelineAttributes attributes) { }

    /// <summary>
    /// If Style==FalseColor, then this virtual function must be overridden.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="meshes"></param>
    protected virtual void UpdateVertexColors(Rhino.DocObjects.RhinoObject obj, Rhino.Geometry.Mesh[] meshes) { }

    /// <summary>
    /// If Style==Wireframe, then the default decomposes the curve object into
    /// nurbs curve segments and calls the virtual DrawNurbsCurve for each segment
    /// </summary>
    /// <param name="curve"></param>
    /// <param name="pipeline"></param>
    protected virtual void DrawCurveObject( Rhino.DocObjects.CurveObject curve, DisplayPipeline pipeline )
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pConstCurve = curve.ConstPointer();
      IntPtr pPipeline = pipeline.NonConstPointer();
      UnsafeNativeMethods.CRhinoVisualAnalysisMode_DrawCurveObject(pConstThis, pConstCurve, pPipeline);
    }

    protected virtual void DrawMeshObject( Rhino.DocObjects.MeshObject mesh, DisplayPipeline pipeline )
    {
    }

    protected virtual void DrawBrepObject( Rhino.DocObjects.BrepObject brep, DisplayPipeline pipeline )
    {
    }

    protected virtual void DrawPointObject( Rhino.DocObjects.PointObject point, DisplayPipeline pipeline )
    {
    }

    protected virtual void DrawPointCloudObject( Rhino.DocObjects.PointCloudObject pointCloud, DisplayPipeline pipeline )
    {
    }

    /// <summary>
    /// The default does nothing. This is a good function to override for
    /// analysis modes like curvature hair display.
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="curve"></param>
    /// <param name="pipeline"></param>
    protected virtual void DrawNurbsCurve( Rhino.DocObjects.RhinoObject obj, Rhino.Geometry.NurbsCurve curve, DisplayPipeline pipeline)
    {
    }

    protected virtual void DrawNurbsSurface( Rhino.DocObjects.RhinoObject obj, Rhino.Geometry.NurbsSurface surface, DisplayPipeline pipeline)
    {
    }

    protected virtual void DrawMesh( Rhino.DocObjects.RhinoObject obj, Rhino.Geometry.Mesh mesh, DisplayPipeline pipeline )
    {
    }
  }

  class NativeVisualAnalysisMode : VisualAnalysisMode
  {
    public NativeVisualAnalysisMode(Guid id)
    {
      m_id = id;
    }

    public override string Name
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        using (StringHolder sh = new StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoVisualAnalysisMode_GetAnalysisModeName(pConstThis, pString);
          return sh.ToString();
        }
      }
    }
    public override AnalysisStyle Style
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return (AnalysisStyle)UnsafeNativeMethods.CRhinoVisualAnalysisMode_Style(pConstThis);
      }
    }

    public override void EnableUserInterface(bool on)
    {
      IntPtr pConstThis = ConstPointer();
      UnsafeNativeMethods.CRhinoVisualAnalysisMode_EnableUserInterface(pConstThis, on);
    }

    public override bool ShowIsoCurves
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoVisualAnalysisMode_ShowIsoCurves(pConstThis);
      }
    }
  }
}
