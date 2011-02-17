using System;
using System.Collections.Generic;

#if USING_RDK
namespace Rhino.Render
{
  // Not public
  sealed class RdkPlugIn
  {
    #region statics
    internal static void SetRdkCallbackFunctions(bool on)
    {
      // All of the RDK callback functions - gets called every time a new RdkPlugIn is created
      if (on)
      {
        UnsafeNativeMethods.Rdk_SetNewTextureCallback(Rhino.Render.RenderTexture.m_NewTexture);
        UnsafeNativeMethods.Rdk_SetNewMaterialCallback(Rhino.Render.RenderMaterial.m_NewMaterial);
        UnsafeNativeMethods.Rdk_SetNewEnvironmentCallback(Rhino.Render.RenderEnvironment.m_NewEnvironment);

        UnsafeNativeMethods.Rdk_SetRenderContentDeleteThisCallback(RenderContent.m_DeleteThis);
        UnsafeNativeMethods.Rdk_SetContentStringCallback(Rhino.Render.RenderTexture.m_GetRenderContentString);
        UnsafeNativeMethods.Rdk_SetNewTextureEvaluatorCallback(Rhino.Render.RenderTexture.m_NewTextureEvaluator);
        UnsafeNativeMethods.Rdk_SetTextureEvaluatorCallbacks(TextureEvaluator.m_GetColor, TextureEvaluator.m_OnDeleteThis);
        UnsafeNativeMethods.Rdk_SetSimulateTextureCallback(Rhino.Render.RenderTexture.m_SimulateTexture);
        UnsafeNativeMethods.Rdk_SetAddUISectionsCallback(Rhino.Render.RenderContent.m_AddUISections);
        UnsafeNativeMethods.Rdk_SetIsContentTypeAcceptableAsChildCallback(Rhino.Render.RenderContent.m_IsContentTypeAcceptableAsChild);
        UnsafeNativeMethods.Rdk_SetHarvestDataCallback(Rhino.Render.RenderContent.m_HarvestData);
        UnsafeNativeMethods.Rdk_SetGetShaderCallback(Rhino.Render.RenderContent.m_GetShader);

        //Materials
        UnsafeNativeMethods.Rdk_SetTextureChildSlotNameCallback(Rhino.Render.RenderMaterial.m_TextureChildSlotName);
        UnsafeNativeMethods.Rdk_SetSimulateMaterialCallback(Rhino.Render.RenderMaterial.m_SimulateMaterial);

        //Environments
        UnsafeNativeMethods.Rdk_SetSimulateEnvironmentCallback(Rhino.Render.RenderEnvironment.m_SimulateEnvironment);

        //SdkRender
        UnsafeNativeMethods.Rdk_SetSdkRenderCallback(Rhino.Render.RenderPipeline.m_ReturnBoolGeneralCallback);
        
      }
      else
      {
        UnsafeNativeMethods.Rdk_SetNewTextureCallback(null);
        UnsafeNativeMethods.Rdk_SetNewMaterialCallback(null);
        UnsafeNativeMethods.Rdk_SetNewEnvironmentCallback(null);

        UnsafeNativeMethods.Rdk_SetRenderContentDeleteThisCallback(null);
        UnsafeNativeMethods.Rdk_SetContentStringCallback(null);
        UnsafeNativeMethods.Rdk_SetNewTextureEvaluatorCallback(null);
        UnsafeNativeMethods.Rdk_SetTextureEvaluatorCallbacks(null, null);
        UnsafeNativeMethods.Rdk_SetSimulateTextureCallback(null);
        UnsafeNativeMethods.Rdk_SetAddUISectionsCallback(null);
        UnsafeNativeMethods.Rdk_SetIsContentTypeAcceptableAsChildCallback(null);
        UnsafeNativeMethods.Rdk_SetHarvestDataCallback(null);
        UnsafeNativeMethods.Rdk_SetGetShaderCallback(null);

        //Materials
        UnsafeNativeMethods.Rdk_SetTextureChildSlotNameCallback(null);
        UnsafeNativeMethods.Rdk_SetSimulateMaterialCallback(null);

        //Environments
        UnsafeNativeMethods.Rdk_SetSimulateEnvironmentCallback(null);

        //SdkRender
        UnsafeNativeMethods.Rdk_SetSdkRenderCallback(null);
      }
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
      return GetRdkPlugInHelper(pRhinoPlugIn, plugin.Id, plugin.m_runtime_serial_number);
    }

    public static RdkPlugIn GetRdkPlugIn(Guid rhino_plugin_id, int serial_number)
    {
      for (int i = 0; i < m_all_rdk_plugins.Count; i++)
      {
        if (m_all_rdk_plugins[i].m_rhino_plugin_id == rhino_plugin_id)
          return m_all_rdk_plugins[i];
      }
      IntPtr pRhinoPlugIn = UnsafeNativeMethods.CRhinoPlugInManager_GetPlugInFromId(rhino_plugin_id, true);

      if (IntPtr.Zero == pRhinoPlugIn)
      {
        Rhino.PlugIns.PlugIn cmnPlugIn = Rhino.PlugIns.PlugIn.GetLoadedPlugIn(rhino_plugin_id);
        if (null != cmnPlugIn)
        {
          pRhinoPlugIn = cmnPlugIn.NonConstPointer();
        }
      }

      return GetRdkPlugInHelper(pRhinoPlugIn, rhino_plugin_id, serial_number);
    }

    static RdkPlugIn GetRdkPlugInHelper(IntPtr pRhinoPlugIn, Guid rhinoPlugInId, int serial_number)
    {
      if (pRhinoPlugIn != IntPtr.Zero)
      {
        IntPtr pRdkPlugIn = UnsafeNativeMethods.CRhCmnRdkPlugIn_New(pRhinoPlugIn, serial_number);
        if (pRdkPlugIn != IntPtr.Zero)
        {
          SetRdkCallbackFunctions(true);
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

    public static Type GetRenderContentType(Guid id, out Guid plugin_id)
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

    internal void AddRegisteredContentTypes(IEnumerable<Type> types)
    {
      m_render_content_types.AddRange(types);
    }

    

  }
}
#endif