#pragma warning disable 1591
using System;

#if RHINO_SDK
namespace Rhino.Display
{
  public abstract class DisplayConduit
  {
    bool m_bEnabled = false;
    protected DisplayConduit() {}

    public bool Enabled
    {
      get
      {
        return m_bEnabled;
      }
      set
      {
        m_bEnabled = value;
        if (m_bEnabled)
        {
          Type base_type = typeof(DisplayConduit);
          Type t = this.GetType();

          // 15 Aug 2011 S. Baer
          // https://github.com/mcneel/rhinocommon/issues/29
          // The virtual functions are protected, so we need to call the overload
          // of GetMethod that takes some binding flags
          var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public;

          System.Reflection.MethodInfo mi = t.GetMethod("CalculateBoundingBox", flags);
          if( mi.DeclaringType != base_type )
            DisplayPipeline.CalculateBoundingBox += _CalculateBoundingBox;

          mi = t.GetMethod("DrawForeground", flags);
          if (mi.DeclaringType != base_type)
            DisplayPipeline.DrawForeground += _DrawForeground;

          mi = t.GetMethod("DrawOverlay", flags);
          if (mi.DeclaringType != base_type)
            DisplayPipeline.DrawOverlay += _DrawOverlay;

          mi = t.GetMethod("PostDrawObjects", flags);
          if (mi.DeclaringType != base_type)
            DisplayPipeline.PostDrawObjects += _PostDrawObjects;

          mi = t.GetMethod("PreDrawObjects", flags);
          if (mi.DeclaringType != base_type)
            DisplayPipeline.PreDrawObjects += _PreDrawObjects;
        }
        else
        {
          DisplayPipeline.CalculateBoundingBox -= _CalculateBoundingBox;
          DisplayPipeline.DrawForeground -= _DrawForeground;
          DisplayPipeline.DrawOverlay -= _DrawOverlay;
          DisplayPipeline.PostDrawObjects -= _PostDrawObjects;
          DisplayPipeline.PreDrawObjects -= _PreDrawObjects;
        }
      }
    }

    private void _CalculateBoundingBox(object sender, CalculateBoundingBoxEventArgs e) { CalculateBoundingBox(e); }
    private void _DrawForeground(object sender, DrawEventArgs e) { DrawForeground(e); }
    private void _DrawOverlay(object sender, DrawEventArgs e)  { DrawOverlay(e); }
    private void _PostDrawObjects(object sender, DrawEventArgs e) { PostDrawObjects(e); }
    private void _PreDrawObjects(object sender, DrawEventArgs e) { PreDrawObjects(e); }


    /// <summary>
    /// Use this function to increase the bounding box of scene so it includes the
    /// geometry that you plan to draw in the "Draw" virtual functions
    /// </summary>
    /// <param name="e"></param>
    protected virtual void CalculateBoundingBox(CalculateBoundingBoxEventArgs e) {}

    /// <summary>
    /// Called before objects are been drawn. Depth writing and testing are on.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void PreDrawObjects(DrawEventArgs e) {}

    /// <summary>
    /// Called after all non-highlighted objects have been drawn. Depth writing and testing are
    /// still turned on. If you want to draw without depth writing/testing, see DrawForeground
    /// </summary>
    /// <param name="e"></param>
    protected virtual void PostDrawObjects(DrawEventArgs e) {}

    /// <summary>
    /// Called after all non-highlighted objects have been drawn and PostDrawObjects has been called.
    /// Depth writing and testing are turned OFF. If you want to draw with depth writing/testing,
    /// see PostDrawObjects
    /// </summary>
    /// <param name="e"></param>
    protected virtual void DrawForeground(DrawEventArgs e) {}

    /// <summary>
    /// If Rhino is in a feedback mode, the draw overlay call allows for temporary geometry to be drawn on top of
    /// everything in the scene. This is similar to the dynamic draw routine that occurs with custom get point.
    /// </summary>
    /// <param name="e"></param>
    protected virtual void DrawOverlay(DrawEventArgs e) {}
  }
}
#endif