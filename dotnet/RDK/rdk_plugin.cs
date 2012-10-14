#pragma warning disable 1591
using System;
using System.Collections.Generic;

#if RDK_CHECKED
namespace Rhino.Render
{
  // Not public
  sealed class RdkPlugIn : IDisposable
  {
    #region statics
    static bool m_callbacks_set;
    internal static void SetRdkCallbackFunctions(bool on)
    {
      // All of the RDK callback functions - gets called every time a new RdkPlugIn is created
      if (on)
      {
#if RDK_UNCHECKED
        UnsafeNativeMethods.Rdk_SetNewTextureCallback(RenderTexture.m_NewTextureCallback);
        UnsafeNativeMethods.Rdk_SetNewMaterialCallback(RenderMaterial.m_NewMaterialCallback);
        UnsafeNativeMethods.Rdk_SetNewEnvironmentCallback(RenderEnvironment.m_NewEnvironmentCallback);

        UnsafeNativeMethods.Rdk_SetRenderContentDeleteThisCallback(RenderContent.m_DeleteThis);
        UnsafeNativeMethods.Rdk_SetContentStringCallback(RenderContent.m_GetRenderContentString);
        UnsafeNativeMethods.Rdk_SetNewTextureEvaluatorCallback(RenderTexture.m_NewTextureEvaluator);
        UnsafeNativeMethods.Rdk_SetTextureEvaluatorCallbacks(TextureEvaluator.m_GetColor, TextureEvaluator.m_OnDeleteThis);
        UnsafeNativeMethods.Rdk_SetSimulateTextureCallback(RenderTexture.m_SimulateTexture);
        UnsafeNativeMethods.Rdk_SetAddUISectionsCallback(RenderContent.m_AddUISections);
        UnsafeNativeMethods.Rdk_SetIsContentTypeAcceptableAsChildCallback(RenderContent.m_IsContentTypeAcceptableAsChild);
        UnsafeNativeMethods.Rdk_SetHarvestDataCallback(RenderContent.m_HarvestData);
        UnsafeNativeMethods.Rdk_SetGetShaderCallback(RenderContent.m_GetShader);

        //Materials
        UnsafeNativeMethods.Rdk_SetTextureChildSlotNameCallback(RenderMaterial.m_TextureChildSlotName);
        UnsafeNativeMethods.Rdk_SetSimulateMaterialCallback(RenderMaterial.m_SimulateMaterial);

        //Environments
        UnsafeNativeMethods.Rdk_SetSimulateEnvironmentCallback(RenderEnvironment.m_SimulateEnvironment);

        //CustomRenderMeshes
        UnsafeNativeMethods.Rdk_SetCallback_CRMProvider_DeleteThis(CustomRenderMesh.Provider.m_DeleteThis);
        UnsafeNativeMethods.Rdk_SetCallback_CRMProvider_WillBuild(CustomRenderMesh.Provider.m_WillBuild);
        UnsafeNativeMethods.Rdk_SetCallback_CRMProvider_BBox(CustomRenderMesh.Provider.m_BBox);
        UnsafeNativeMethods.Rdk_SetCallback_CRMProvider_Build(CustomRenderMesh.Provider.m_Build);

        //IoPlugins
        UnsafeNativeMethods.Rdk_SetRenderContentIoDeleteThisCallback(IOPlugIn.m_DeleteThis);
        UnsafeNativeMethods.Rdk_SetRenderContentIoLoadCallback(IOPlugIn.m_Load);
        UnsafeNativeMethods.Rdk_SetRenderContentIoSaveCallback(IOPlugIn.m_Save);
        UnsafeNativeMethods.Rdk_SetRenderContentIoStringCallback(Rhino.Render.IOPlugIn.m_GetRenderContentIoString);
#endif
#if RDK_CHECKED
        //SdkRender
        UnsafeNativeMethods.Rdk_SetSdkRenderCallback(RenderPipeline.m_ReturnBoolGeneralCallback);
#endif
        m_callbacks_set = true;
      }
      else
      {
        if (m_callbacks_set)
        {
#if RDK_UNCHECKED
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

          //CustomRenderMeshes
          UnsafeNativeMethods.Rdk_SetCallback_CRMProvider_DeleteThis(null);
          UnsafeNativeMethods.Rdk_SetCallback_CRMProvider_WillBuild(null);
          UnsafeNativeMethods.Rdk_SetCallback_CRMProvider_BBox(null);
          UnsafeNativeMethods.Rdk_SetCallback_CRMProvider_Build(null);

          //IoPlugins
          UnsafeNativeMethods.Rdk_SetRenderContentIoDeleteThisCallback(null);
          UnsafeNativeMethods.Rdk_SetRenderContentIoLoadCallback(null);
          UnsafeNativeMethods.Rdk_SetRenderContentIoSaveCallback(null);
          UnsafeNativeMethods.Rdk_SetRenderContentIoStringCallback(null);
#endif
          //SdkRender
#if RDK_CHECKED
          UnsafeNativeMethods.Rdk_SetSdkRenderCallback(null);
#endif
          m_callbacks_set = false;
        }
      }
    }
    /// <summary>
    /// Dictionary of valid RdkPlugIn's
    /// </summary>
    static readonly Dictionary<Guid, RdkPlugIn> m_rdkPlugInDictionary = new Dictionary<Guid, RdkPlugIn>();
    /// <summary>
    /// Find loaded RdkPlugIn in the Render Development Kit(RDK) plug-in
    /// dictionary.
    /// </summary>
    /// <param name="plugInId">Plug-in Id to search for</param>
    /// <returns>
    /// If a plug-in with the specified Id is found in the dictionary then the
    /// plug-in object is returned, if not found then null is returned.
    /// </returns>
    public static RdkPlugIn FromPlugInId(Guid plugInId)
    {
      RdkPlugIn found;
      m_rdkPlugInDictionary.TryGetValue(plugInId, out found);
      return found;
    }
    /// <summary>
    /// Search the Render Development Kit(RDK) plug-in dictionary for a render
    /// plug-in with the matching Rhino plug-in Id.
    /// </summary>
    /// <param name="plugIn">Rhino plug-in to search for.</param>
    /// <returns>
    /// If the plug-in is in the RDK dictionary then the dictionary plug-in
    /// object is returned, if not then null is returned.
    /// </returns>
    public static RdkPlugIn FromRhinoPlugIn(PlugIns.PlugIn plugIn)
    {
      return ((null == plugIn) ? null : FromPlugInId(plugIn.Id));
    }
    /// <summary>
    /// Check each RdkPlugIn in the Render Development Kit(RDK) dictionary and
    /// return the first one found that contains the specified content contentType.
    /// </summary>
    /// <param name="type">Class contentType to search for</param>
    /// <returns>
    /// If there is a plug-in in the dictionary that registered the requested
    /// contentType then return the plug-in, if not then return null.
    /// </returns>
    public static RdkPlugIn FromRenderConentClassType(Type type)
    {
      foreach (var item in m_rdkPlugInDictionary)
        if (item.Value.m_renderContentTypes.Contains(type))
          return item.Value;
      return null;
    }
    /// <summary>
    /// Search the plug-in's dictionary for the specified plug-in and if it is
    /// not found then add it to the dictionary.
    /// </summary>
    /// <param name="plugIn"></param>
    /// <returns></returns>
    internal static RdkPlugIn GetRdkPlugIn(PlugIns.PlugIn plugIn)
    {
      var found = FromRhinoPlugIn(plugIn);
      if (null != found) return found;
      var pluginPointer = plugIn.NonConstPointer();
      return AddPlugInToDictionary(pluginPointer, plugIn.Id, plugIn.m_runtime_serial_number);
    }
    /// <summary>
    /// If the specified plug-in is not currently in the plug-in dictionary do
    /// nothing otherwise; see AddPlugInToDictionary for a description of what
    /// happens.
    /// </summary>
    /// <param name="rhinoPlugInId">Id of the plug-in to search for or add.</param>
    /// <param name="serialNumber">Plug-in C++ pointer serial number.</param>
    /// <returns>
    /// A plug-in object from the plug-in dictionary or null if there was a
    /// problem adding a new dictionary plug-in.
    /// </returns>
    internal static RdkPlugIn GetRdkPlugIn(Guid rhinoPlugInId, int serialNumber)
    {
      var found = FromPlugInId(rhinoPlugInId);
      if (null != found) return found;

      var pRhinoPlugIn = UnsafeNativeMethods.CRhinoPlugInManager_GetPlugInFromId(rhinoPlugInId, true);

      if (IntPtr.Zero == pRhinoPlugIn)
      {
        var cmnPlugIn = PlugIns.PlugIn.GetLoadedPlugIn(rhinoPlugInId);
        if (null != cmnPlugIn)
          pRhinoPlugIn = cmnPlugIn.NonConstPointer();
      }

      return AddPlugInToDictionary(pRhinoPlugIn, rhinoPlugInId, serialNumber);
    }
    /// <summary>
    /// Create a new C++ runtime RDK plug-in object then create a RhinoCommon
    /// RdkPlugIn, attach the C++ pointer to it and set the C++ callback
    /// function pointers to the RckPlugIn.
    /// </summary>
    /// <param name="rhinoPlugIn">
    /// The C++ pointer to the native CRhinoPlugIn that is causing this RDK
    /// plug-in to get added.
    /// </param>
    /// <param name="rhinoPlugInId">The plug-in Id</param>
    /// <param name="serialNumber">Plug-in C++ pointer serial number</param>
    /// <returns></returns>
    static private RdkPlugIn AddPlugInToDictionary(IntPtr rhinoPlugIn, Guid rhinoPlugInId, int serialNumber)
    {
      if (rhinoPlugIn != IntPtr.Zero)
      {
        var pRdkPlugIn = UnsafeNativeMethods.CRhCmnRdkPlugIn_New(rhinoPlugIn, serialNumber);
        if (pRdkPlugIn != IntPtr.Zero)
        {
          SetRdkCallbackFunctions(true);
          var rdkPlugIn = new RdkPlugIn(pRdkPlugIn, rhinoPlugInId);
          m_rdkPlugInDictionary.Add(rhinoPlugInId, rdkPlugIn);
          return rdkPlugIn;
        }
      }
      return null;
    }
    /// <summary>
    /// If there the plug-in dictionary contains a plug-in that registered the
    /// specified content type then return true otherwise; return false;
    /// </summary>
    /// <param name="contentType">Class type to search for.</param>
    /// <returns>
    /// If there the plug-in dictionary contains a plug-in that registered the
    /// specified content contentType then return true otherwise; return false;
    /// </returns>
    public static bool RenderContentTypeIsRegistered(Type contentType)
    {
      return (null != FromRenderConentClassType(contentType));
    }
    /// <summary>
    /// Search the plug-in dictionary for a plug-in for that registered the
    /// specified class type Guid, if it is found then return the class
    /// type and plug-in ID otherwise; return null.
    /// </summary>
    /// <param name="id">Class GUUID attribute value to search for.</param>
    /// <param name="pluginId">
    /// Output parameter, will be set to the Id of the plug-in that registered
    /// the class type or Guid.Empty if the type is not found.
    /// </param>
    /// <returns>
    /// Returns the class type and Id of the plug-in that registered the class
    /// type if the Id is found otherwise; return null.
    /// </returns>
    public static Type GetRenderContentType(Guid id, out Guid pluginId)
    {
      pluginId = Guid.Empty;
      foreach (var item in m_rdkPlugInDictionary)
      {
        var foundType = item.Value.m_renderContentTypes.Find(t => t.GUID == id);
        if (foundType == null) continue;
        pluginId = item.Key;
        return foundType;
      }
      return null;
    }

#if RDK_UNCHECKED
    /// <summary>
    /// Check the specified class type for a custom attribute of the class type
    /// CustomRenderContentIoAttribute and if one is found ask the C++ SDK if
    /// the type is registered.
    /// </summary>
    /// <param name="type">Class type to test</param>
    /// <returns>
    /// Returns true if the class type contains a CustomRenderContentIoAttribute
    /// with a file extension that is registered with the C++ SDK otherwise; 
    /// returns false.
    /// </returns>
    internal static bool RenderContentIoTypeIsRegistered(Type type)
    {
      // Get the class type custom attribute list
      var attr = type.GetCustomAttributes(typeof(CustomRenderContentIoAttribute), false);
      // Search the attribute list for a CustomRenderContentIoAttribute
      foreach (var att in attr)
        if (att is CustomRenderContentIoAttribute)
        { // if this attribute is a CustomRenderContentIoAttribute
          var custom = att as CustomRenderContentIoAttribute;
          var extension = custom.Extension;
          // Check the C++ SDK to see if the file extension is registered
          return UnsafeNativeMethods.Rdk_RenderContentIo_IsExtensionRegistered(extension);
        }
      // There is no CustomRenderContentIoAttribute associated with the
      // specified class type
      return false;
    }
#endif
    #endregion statics

    #region class members
    /// <summary>
    /// CRhinoPluIn Id that owns this RdkPlugIn
    /// </summary>
    readonly Guid m_rhinoPlugInId;
    /// <summary>
    /// The RDK C++ CRdkPlugIn pointer associated with this object
    /// </summary>
    readonly IntPtr m_rdkPlugInPointer = IntPtr.Zero;
    /// <summary>
    /// List of valid RenderContent class types associated with this plug-in.
    /// </summary>
    readonly List<Type> m_renderContentTypes = new List<Type>();
    #endregion class members

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="rdkPlugInPointer">
    /// C++ Pointer to the CRhinoPlugIn that this plug-in is associated with.
    /// </param>
    /// <param name="rhinoPlugInId">C++ CRhinoPlugIn Id</param>
    private RdkPlugIn(IntPtr rdkPlugInPointer, Guid rhinoPlugInId)
    {
      m_rdkPlugInPointer = rdkPlugInPointer;
      m_rhinoPlugInId = rhinoPlugInId;
    }
    /// <summary>
    /// Add list of class types, the type list has been sanitized and should
    /// only contain valid RenderContent class types, this should only be
    /// called from RenderContent.RegisterContent.
    /// </summary>
    /// <param name="types">Types to add to the plug-ins contenet type list.</param>
    internal void AddRegisteredContentTypes(IEnumerable<Type> types)
    {
      m_renderContentTypes.AddRange(types);
    }

    #region IDisposable Members
    /// <summary>
    /// Required IDisposable method
    /// </summary>
    public void Dispose()
    {
      // We need to find the reference to this thing in the list, un-initialize the C++
      // object, delete it and then remove it to actually make sure thing gets garbage
      // collected.
      RdkPlugIn plugIn;
      if (m_rdkPlugInDictionary.TryGetValue(m_rhinoPlugInId, out plugIn))
      {
        m_rdkPlugInDictionary.Remove(m_rhinoPlugInId);
        UnsafeNativeMethods.CRhCmnRdkPlugIn_Delete(plugIn.m_rdkPlugInPointer);
      }
    }
    #endregion
  }
}
#endif