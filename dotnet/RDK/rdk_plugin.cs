using System;
using System.Collections.Generic;

#if USING_RDK
namespace Rhino.Render
{
  // Not public
  sealed class RdkPlugIn
  {
    #region statics
    static void SetRdkCallbackFunctions()
    {
      // All of the RDK callback functions - gets called every time a new RdkPlugIn is created
      UnsafeNativeMethods.Rdk_SetNewTextureCallback(Rhino.Render.RenderTexture.m_NewTexture);
      UnsafeNativeMethods.Rdk_SetContentStringCallback(Rhino.Render.RenderTexture.m_GetRenderContentString);
      UnsafeNativeMethods.Rdk_SetNewTextureEvaluatorCallback(Rhino.Render.RenderTexture.m_NewTextureEvaluator);
      UnsafeNativeMethods.Rdk_SetTextureEvaluatorGetColor(Rhino.Render.TextureEvaluator.m_GetColor);
    }

    static List<RdkPlugIn> m_all_rdk_plugins = new List<RdkPlugIn>();

    public static RdkPlugIn GetRdkPlugIn(Rhino.PlugIns.PlugIn plugin)
    {
      for (int i = 0; i < m_all_rdk_plugins.Count; i++)
      {
        if (m_all_rdk_plugins[i].m_rhino_plugin_id == plugin.Id)
          return m_all_rdk_plugins[i];
      }
      IntPtr pRhinoPlugIn = plugin.NonConstPointer();
      return GetRdkPlugInHelper(pRhinoPlugIn, plugin.Id);
    }
    public static RdkPlugIn GetRdkPlugIn(Guid rhino_plugin_id)
    {
      for (int i = 0; i < m_all_rdk_plugins.Count; i++)
      {
        if (m_all_rdk_plugins[i].m_rhino_plugin_id == rhino_plugin_id)
          return m_all_rdk_plugins[i];
      }
      IntPtr pRhinoPlugIn = UnsafeNativeMethods.CRhinoPlugInManager_GetPlugInFromId(rhino_plugin_id, true);
      return GetRdkPlugInHelper(pRhinoPlugIn, rhino_plugin_id);
    }
    static RdkPlugIn GetRdkPlugInHelper(IntPtr pRhinoPlugIn, Guid rhinoPlugInId)
    {
      if (pRhinoPlugIn != IntPtr.Zero)
      {
        IntPtr pRdkPlugIn = UnsafeNativeMethods.CRhCmnRdkPlugIn_New(pRhinoPlugIn);
        if (pRdkPlugIn != IntPtr.Zero)
        {
          SetRdkCallbackFunctions();
          RdkPlugIn rc = new RdkPlugIn(pRdkPlugIn, rhinoPlugInId);
          m_all_rdk_plugins.Add(rc);
          return rc;
        }
      }
      return null;
    }

    public static bool RenderContentTypeIsRegistered(Type t)
    {
      for (int i = 0; i < m_all_rdk_plugins.Count; i++)
      {
        RdkPlugIn rdk = m_all_rdk_plugins[i];
        if( rdk.m_render_content_types.Contains(t) )
          return true;
      }
      return false;
    }

    public static Type GetRenderTextureType(Guid id, out Guid plugin_id)
    {
      plugin_id = Guid.Empty;
      for (int i = 0; i < m_all_rdk_plugins.Count; i++)
      {
        RdkPlugIn rdk = m_all_rdk_plugins[i];
        Type found_t = rdk.m_render_content_types.Find(delegate(Type t) { return t.GUID == id; });
        if (found_t != null)
        {
          plugin_id = rdk.m_rhino_plugin_id;
          return found_t;
        }
      }
      return null;
    }

    #endregion

    Guid m_rhino_plugin_id;
    IntPtr m_pRdkPlugIn = IntPtr.Zero;
    List<Type> m_render_content_types = new List<Type>();
    private RdkPlugIn(IntPtr pRdkPlugIn, Guid rhinoPlugInId)
    {
      m_pRdkPlugIn = pRdkPlugIn;
      m_rhino_plugin_id = rhinoPlugInId;
    }

    public void AddRegisteredContentTypes(IEnumerable<Type> types)
    {
      m_render_content_types.AddRange(types);
    }

    

  }
}
#endif