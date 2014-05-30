using System.Collections.Generic;
#pragma warning disable 1591
using System;

#if RDK_CHECKED
namespace Rhino.Render
{
#if RDK_CHECKED
  /// <summary>
  /// This class is used to manage the following override:
  /// 
  /// static class CRdkCmnEventWatcher : public CRhRdkEventSink
  /// {
  ///    ...
  ///    virtual void OnDocumentSettingsChanged(UINT uFlags, UINT_PTR uContext);
  ///    ...
  /// };
  /// </summary>
  class RdkCmnEventWatcher
  {
    /// <summary>
    /// Iterate the event list and see if any have been hooked.
    /// </summary>
    private static bool IsEmpty
    {
      get
      {
        // If any of the hooks are not NULL return false
        foreach (var item in g_event_dictionary)
          if (item.Value != null)
            return false;
        // All of the hooks were null
        return true;
      }
    }
    /// <summary>
    /// Called when adding a delegate to a event, see the GroundPlane.Changed
    /// event for an example.
    /// </summary>
    /// <param name="type">
    /// The event watcher type index
    /// </param>
    /// <param name="value">
    /// Delegate to add
    /// </param>
    public static void Add(UnsafeNativeMethods.EventSyncDocumentSettingsChangedFlag type, EventHandler<RenderPropertyChangedEvent> value)
    {
      // If the callback hook has not been set then set it now
      if (g_settings_changed_hook == null)
      {
        // Call into rhcmnrdk_c to set the callback hook
        g_settings_changed_hook = OnSettingsChanged;
        UnsafeNativeMethods.CRdkCmnEventWatcher_SetDocumentSettingsChangedEventCallback(g_settings_changed_hook, Rhino.Runtime.HostUtils.m_rdk_ew_report);
      }
      if (!g_event_dictionary.ContainsKey(type))
        g_event_dictionary.Add(type, null);
      // Need to do this to ensure the delegate does not get added twice
      g_event_dictionary[type] -= value;
      // Add the new delegate to the event list
      g_event_dictionary[type] += value;
    }
    /// <summary>
    /// Called when removing a delegate from an event, see the
    /// GroundPlane.Changed event for an example.
    /// </summary>
    /// <param name="type">
    /// The event watcher type index
    /// </param>
    /// <param name="value">
    /// Delegate to remove
    /// </param>
    public static void Remove(UnsafeNativeMethods.EventSyncDocumentSettingsChangedFlag type, EventHandler<RenderPropertyChangedEvent> value)
    {
      // Remove from the dictionary
      if (g_event_dictionary.ContainsKey(type)) g_event_dictionary[type] -= value;
      // If there are still event hooks set then bail
      if (!IsEmpty) return;
      // There are no event hooks set so call into rhcmnrdk_c to set the
      // callback hook to null.
      UnsafeNativeMethods.CRdkCmnEventWatcher_SetDocumentSettingsChangedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
      g_settings_changed_hook = null;
    }
    /// <summary>
    /// Called after any RDK document settings have been changed.
    /// CRdkCmnEventWatcher::OnDocumentSettingsChanged(UINT uFlags, UINT_PTR uContext)
    /// </summary>
    /// <param name="flags">A bit mask specifying what has changed</param>
    /// <param name="context">
    /// Provides extra information, parameter is usually zero but in the case of 'rendering'
    /// </param>
    private static void OnSettingsChanged(int flags, int context)
    {
      var event_list = new List<UnsafeNativeMethods.EventSyncDocumentSettingsChangedFlag>();
      // Pre-process the event list and build a list of events to fire
      foreach (var item in g_event_dictionary)
      {
        // Flag for this hook not set so ignore
        if (0 == ((int) item.Key & flags)) continue;
        // If the event has not been hooked then ignore
        if (item.Value == null) continue;
        // Add the event to the to-do list
        event_list.Add(item.Key);
      }
      // Now process the list of events to fire, if you invoke the events from
      // the foreach loop above and a event removes itself the iterator will 
      // throw an exception.
      foreach (var key in event_list)
      {
        try
        {
          // get the event from the list and invoke it
          var invoke_event = g_event_dictionary[key];
          invoke_event.Invoke(null, new RenderPropertyChangedEvent(RhinoDoc.ActiveDoc, context));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    /// <summary>
    /// Used by CRdkCmnEventWatcher_SetDocumentSettingsChangedEventCallback
    /// </summary>
    /// <param name="flags"></param>
    /// <param name="context"></param>
    internal delegate void RdkDocumentSettingsChangedCallback(int flags, int context);
    /// <summary>
    /// Gets set as soon as a event delegate is added, will point to
    /// OnSettingsChanged.
    /// </summary>
    private static RdkDocumentSettingsChangedCallback g_settings_changed_hook;
    /// <summary>
    /// Event delegate dictionary
    /// </summary>
    private static Dictionary<UnsafeNativeMethods.EventSyncDocumentSettingsChangedFlag, EventHandler<RenderPropertyChangedEvent>> g_event_dictionary = new Dictionary<UnsafeNativeMethods.EventSyncDocumentSettingsChangedFlag, EventHandler<RenderPropertyChangedEvent>>();
  }
  /// <summary>
  /// Used by Rhino.Render object property value has changed events.
  /// </summary>
  public class RenderPropertyChangedEvent : EventArgs
  {
    internal RenderPropertyChangedEvent(RhinoDoc doc, int context)
    {
      m_doc = doc;
      m_context = context;
    }
    /// <summary>
    /// The document triggering the event.
    /// </summary>
    public RhinoDoc Document { get { return m_doc; } }
    /// <summary>
    /// Optional argument which may specify the property being modified.
    /// </summary>
    public int Context{ get { return m_context; } }
    private readonly RhinoDoc m_doc;
    private readonly int m_context;
  }
#endif

  /// <summary>
  /// Represents an infinite plane for implementation by renderers.
  /// See <see cref="Rhino.PlugIns.RenderPlugIn.SupportsFeature">SupportsFeature</see>.
  /// </summary>
  public class GroundPlane
  {
#if RDK_CHECKED
    /// <summary>
    /// This event is raised when a GroundPlane property value is changed.
    /// </summary>
    public static event EventHandler<RenderPropertyChangedEvent> Changed
    {
      add { RdkCmnEventWatcher.Add(UnsafeNativeMethods.EventSyncDocumentSettingsChangedFlag.GroundPlane, value); }
      remove { RdkCmnEventWatcher.Remove(UnsafeNativeMethods.EventSyncDocumentSettingsChangedFlag.GroundPlane, value); }
    }
#endif

    /// <summary>
    /// Functions/Properties in this class do not need to check for RDK since
    /// the only way to access the RDK is through the GroundPlane property on
    /// the RhinoDoc. That property does the check before returning this class
    /// </summary>
    private readonly RhinoDoc m_doc;

    internal GroundPlane(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document this groundplane is associated with.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>
    /// Determines whether the document ground plane is enabled.
    /// </summary>
    public bool Enabled
    {
      get { return UnsafeNativeMethods.Rdk_GroundPlane_Enabled(); }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetEnabled(value); }
    }

    /// <summary>
    /// Height above world XY plane in model units.
    /// </summary>
    public double Altitude
    {
      get { return UnsafeNativeMethods.Rdk_GroundPlane_Altitude(); }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetAltitude(value); }
    }

    /// <summary>
    /// Id of material in material table for this ground plane.
    /// </summary>
    public Guid MaterialInstanceId
    {
      get { return UnsafeNativeMethods.Rdk_GroundPlane_MaterialInstanceId(); }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetMaterialInstanceId(value); }
    }

    /// <summary>
    /// Texture mapping offset in world units.
    /// </summary>
    public Rhino.Geometry.Vector2d TextureOffset
    {
      get
      {
        Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
        UnsafeNativeMethods.Rdk_GroundPlane_TextureOffset(ref v);
        return v;
      }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetTextureOffset(value); }
    }

    /// <summary>
    /// Texture mapping single UV span size in world units.
    /// </summary>
    public Rhino.Geometry.Vector2d TextureSize
    {
      get
      {
        Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
        UnsafeNativeMethods.Rdk_GroundPlane_TextureSize(ref v);
        return v;
      }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetTextureSize(value); }
    }

    /// <summary>
    /// Texture mapping rotation around world origin + offset in degrees.
    /// </summary>
    public double TextureRotation
    {
      get { return UnsafeNativeMethods.Rdk_GroundPlane_TextureRotation(); }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetTextureRotation(value); }
    }


    // ?? Why is this static ??
    // Queries whether or not the Ground Plane is visible.
    //public static bool IsGroundPlaneVisible { get { return 1 == UnsafeNativeMethods.Rdk_Globals_IsGroundPlaneVisible(); } }

  }
}
#endif