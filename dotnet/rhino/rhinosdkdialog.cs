#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// none of the UI namespace needs to be in the stand-alone opennurbs library
#if RHINO_SDK
using System.Windows.Forms;

namespace Rhino
{
  namespace UI
  {
    //public interface IPanel
    //{
    //  IntPtr GetHandle();
    //}

    class PanelData
    {
      public Type PanelType { get; set; }
      public IWin32Window Panel { get; set; }
      public Guid PlugInId { get; set; }
    }

    public static class PanelIds
    {
      public static Guid Materials { get { return new Guid("{ 0x6df2a957, 0xf12d, 0x42ea, { 0x9f, 0xa6, 0x95, 0xd7, 0x92, 0x0c, 0x1b, 0x76 } }"); } }
      public static Guid Environment { get { return new Guid("{ 0x7df2a957, 0xf12d, 0x42ea, { 0x9f, 0xa6, 0x95, 0xd7, 0x92, 0x0c, 0x1b, 0x76 } }"); } }
      public static Guid Texture { get { return new Guid("{0x8df2a957, 0xf12d, 0x42ea, { 0x9f, 0xa6, 0x95, 0xd7, 0x92, 0x0c, 0x1b, 0x76 } }"); } }
      public static Guid LightManager { get { return new Guid("{ 0x86777b3d, 0x3d68, 0x4965, { 0x84, 0xf8, 0x9e, 0x1, 0x9c, 0x40, 0x24, 0x33 } }"); } }
      public static Guid Sun { get { return new Guid("{ 0x1012681e, 0xd276, 0x49d3, { 0x9c, 0xd9, 0x7d, 0xe9, 0x2d, 0xc2, 0x40, 0x4a } }"); } }
      public static Guid GroundPlane { get { return new Guid("{ 0x987b1930, 0xecde, 0x4e62, { 0x82, 0x82, 0x97, 0xab, 0x4a, 0xd3, 0x25, 0xfe } }"); } }
      public static Guid Layers { get { return new Guid("{ 0x3610bf83, 0x47d, 0x4f7f, { 0x93, 0xfd, 0x16, 0x3e, 0xa3, 0x5, 0xb4, 0x93 } }"); } }
      public static Guid ObjectProperties { get { return new Guid("{ 0x34ffb674, 0xc504, 0x49d9, { 0x9f, 0xcd, 0x99, 0xcc, 0x81, 0x1d, 0xcd, 0xa2 } }"); } }
      public static Guid Display { get { return new Guid("{ 0xb68e9e9f, 0xc79c, 0x473c, { 0xa7, 0xef, 0x84, 0x6a, 0x11, 0xdc, 0x4e, 0x7b } }"); } }
      public static Guid ContextHelp { get { return new Guid("{ 0xf8fb4f9, 0xc213, 0x4a6e, { 0x8e, 0x79, 0xb, 0xec, 0xe0, 0x2d, 0xf8, 0x2a } }"); } }
      public static Guid Notes { get { return new Guid("{ 0x1d55d702, 0x28c, 0x4aab, { 0x99, 0xcc, 0xac, 0xfd, 0xd4, 0x41, 0xfe, 0x5f } }"); } }
      public static Guid Libraries { get { return new Guid("{ 0xb70a4973, 0x99ca, 0x40c0, { 0xb2, 0xb2, 0xf0, 0x34, 0x17, 0xa5, 0xff, 0x1d } }"); } }
    }

    public static class Panels
    {
      static List<PanelData> m_existing_panels;

      static PanelData FindExistingPanelData(Guid pluginId, Guid tabId)
      {
        if (m_existing_panels != null)
        {
          for (int i = 0; i < m_existing_panels.Count; i++)
          {
            PanelData pd = m_existing_panels[i];
            if (pd != null && pd.PlugInId==pluginId && pd.PanelType.GUID==tabId)
              return pd;
          }
        }
        return null;
      }

      static IWin32Window FindOrCreatePanel(Guid pluginId, Guid tabId)
      {
        PanelData data = FindExistingPanelData(pluginId, tabId);
        if( data==null )
          return null;
        if( data.Panel== null )
          data.Panel = System.Activator.CreateInstance(data.PanelType) as IWin32Window;

        return data.Panel;
      }

      internal delegate IntPtr CreatePanelCallback(Guid pluginId, Guid tabId, IntPtr hParent);
      internal delegate int VisiblePanelCallback(Guid pluginId, Guid tabId, uint state);

      static CreatePanelCallback m_create_panel_callback;
      static VisiblePanelCallback m_visible_panel_callback;

      static IntPtr OnCreatePanelCallback(Guid pluginId, Guid tabId, IntPtr hParent)
      {
        IWin32Window panel = FindOrCreatePanel(pluginId, tabId);
        if (panel != null)
          return panel.Handle;
        return IntPtr.Zero;
      }
      static int OnVisiblePanelCallback(Guid pluginId, Guid tabId, uint state)
      {
        PanelData data = FindExistingPanelData(pluginId, tabId);
        if( data==null )
          return 0;
        System.Windows.Forms.Control panel = data.Panel as System.Windows.Forms.Control;
        if (panel == null)
          return 0;
        panel.Visible = (state != 0);
        return 1;
      }

      /// <summary>
      /// You typically register your panel class in your plug-in's OnLoad function.
      /// This informs Rhino of the existence of your panel class
      /// </summary>
      /// <param name="plugin">Plug-in this panel is associated with</param>
      /// <param name="panelType">
      /// Class type to construct when a panel is shown.  Currently only types
      /// that implement the IWin32Window interface are supported. The requirements
      /// for the class are that it has a parameterless constructor and have a
      /// GuidAttribute applied with a unique Guid
      /// </param>
      /// <param name="caption"></param>
      /// <param name="icon">Use a 32bit depth icon in order to get proper transparency</param>
      public static void RegisterPanel(Rhino.PlugIns.PlugIn plugin, Type panelType, string caption, Rhino.Drawing.Icon icon)
      {
        if (!typeof(IWin32Window).IsAssignableFrom(panelType))
          throw new ArgumentException("panelType must implement IWin32Window interface", "panelType");
        System.Reflection.ConstructorInfo constructor = panelType.GetConstructor(System.Type.EmptyTypes);
        if (!panelType.IsPublic || constructor == null)
          throw new ArgumentException("panelType must be a public class and have a parameterless constructor", "panelType");
        object[] attr = panelType.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
        if (attr.Length != 1)
          throw new ArgumentException("panelType must have a GuidAttribute", "panelType");

        if (m_existing_panels == null)
          m_existing_panels = new List<PanelData>();
        // make sure the type is not already registered
        for (int i = 0; i < m_existing_panels.Count; i++)
        {
          var pd = m_existing_panels[i];
          if (pd != null && pd.PlugInId == plugin.Id && pd.PanelType == panelType)
            return;
        }
        PanelData panel_data = new PanelData();
        panel_data.PlugInId = plugin.Id;
        panel_data.PanelType = panelType;
        m_existing_panels.Add(panel_data);


        m_create_panel_callback = OnCreatePanelCallback;
        m_visible_panel_callback = OnVisiblePanelCallback;
        UnsafeNativeMethods.RHC_RegisterTabbedDockBar(caption, panelType.GUID, plugin.Id, icon.Handle, m_create_panel_callback, m_visible_panel_callback);
      }

      public static object GetPanel(Guid panelId)
      {
        if (m_existing_panels == null)
          return null;

        for (int i = 0; i < m_existing_panels.Count; i++)
        {
          Type t = m_existing_panels[i].PanelType;
          if (t.GUID == panelId)
            return m_existing_panels[i].Panel;
        }
        return null;
      }

      public static bool IsPanelVisible(Guid panelId)
      {
        return UnsafeNativeMethods.RHC_RhinoUiIsTabVisible(panelId);
      }

      public static bool IsPanelVisible(Type panelType)
      {
        return IsPanelVisible(panelType.GUID);
      }

      public static bool OpenPanelAsSibling(Guid panelId, Guid existingSiblingId)
      {
        return UnsafeNativeMethods.RHC_OpenTabOnDockBar(panelId, existingSiblingId);
      }

      public static void OpenPanel(Guid panelId)
      {
        UnsafeNativeMethods.RHC_RhinoUiOpenCloseDockbarTab(panelId, true);
      }

      public static void OpenPanel(Type panelType)
      {
        OpenPanel(panelType.GUID);
      }

      public static void ClosePanel(Guid panelId)
      {
        UnsafeNativeMethods.RHC_RhinoUiOpenCloseDockbarTab(panelId, false);
      }

      public static void ClosePanel(Type panelType)
      {
        ClosePanel(panelType.GUID);
      }

      public static Guid[] GetOpenPanelIds()
      {
        using (Runtime.InteropWrappers.SimpleArrayGuid ids = new Runtime.InteropWrappers.SimpleArrayGuid())
        {
          IntPtr pIds = ids.NonConstPointer();
          UnsafeNativeMethods.RHC_GetOpenTabIds(pIds);
          return ids.ToArray();
        }
      }
    }
  }
}

#endif
