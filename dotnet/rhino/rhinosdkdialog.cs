#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

// none of the UI namespace needs to be in the stand-alone opennurbs library
#if RHINO_SDK

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
      static CreatePanelCallback m_create_panel_callback;
      static IntPtr OnCreatePanelCallback(Guid pluginId, Guid tabId, IntPtr hParent)
      {
        IWin32Window panel = FindOrCreatePanel(pluginId, tabId);
        if (panel != null)
          return panel.Handle;
        return IntPtr.Zero;
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
      public static void RegisterPanel(Rhino.PlugIns.PlugIn plugin, Type panelType, string caption, System.Drawing.Icon icon)
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
        UnsafeNativeMethods.RHC_RegisterTabbedDockBar(caption, panelType.GUID, plugin.Id, icon.Handle, m_create_panel_callback);
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
    }
  }
}

#endif