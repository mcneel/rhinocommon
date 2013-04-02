#pragma warning disable 1591
using System;
using System.Diagnostics;
using Rhino.Runtime;

#if RDK_CHECKED
namespace Rhino.Render
{
  public abstract class RenderEnvironment : RenderContent
  {
    public static RenderEnvironment CurrentEnvironment
    {
      get
      {
        RhinoDoc doc = RhinoDoc.ActiveDoc;
        return FromPointer(UnsafeNativeMethods.Rdk_RenderEnvironment_CurrentEnvironment(doc.m_docId)) as RenderEnvironment;
      }
      set
      {
        RhinoDoc doc = RhinoDoc.ActiveDoc;
        UnsafeNativeMethods.Rdk_RenderEnvironment_SetCurrentEnvironment(doc.m_docId, value.Id);
      }
    }

    /// <summary>
    /// Constructs a new <see cref="RenderEnvironment"/> from a <see cref="SimulatedEnvironment"/>.
    /// </summary>
    /// <param name="environment">The environment to create the basic environment from.</param>
    /// <returns>A new basic environment.</returns>
    public static RenderEnvironment NewBasicEnvironment(SimulatedEnvironment environment)
    {
      IntPtr pEnvironment = environment == null ? IntPtr.Zero : environment.ConstPointer();
      NativeRenderEnvironment newEnvironment = FromPointer(UnsafeNativeMethods.Rdk_Globals_NewBasicEnvironment(pEnvironment)) as NativeRenderEnvironment;
      if (newEnvironment != null)
        newEnvironment.AutoDelete = true;
      return newEnvironment;
    }

    public virtual void SimulateEnvironment(ref SimulatedEnvironment simualation, bool isForDataOnly)
    {
      if (IsNativeWrapper())
      {
        UnsafeNativeMethods.Rdk_RenderEnvironment_SimulateEnvironment(NonConstPointer(), simualation.ConstPointer(), isForDataOnly);
      }
      else
      {
        UnsafeNativeMethods.Rdk_RenderEnvironment_CallSimulateEnvironmentBase(NonConstPointer(), simualation.ConstPointer(), isForDataOnly);
      }
    }

    #region callbacks from c++

    internal static NewRenderContentCallbackEvent m_NewEnvironmentCallback = OnNewEnvironment;
    static IntPtr OnNewEnvironment(Guid typeId)
    {
      var renderContent = NewRenderContent(typeId, typeof(RenderEnvironment));
      return (null == renderContent ? IntPtr.Zero : renderContent.NonConstPointer());
    }

    internal delegate void SimulateEnvironmentCallback(int serial_number, IntPtr p, int bDataOnly);
    internal static SimulateEnvironmentCallback m_SimulateEnvironment = OnSimulateEnvironment;
    static void OnSimulateEnvironment(int serial_number, IntPtr pSim, int bDataOnly)
    {
      try
      {
        var texture = FromSerialNumber(serial_number) as RenderEnvironment;
        if (texture != null)
        {
          if (pSim != IntPtr.Zero)
          {
            var sim = new SimulatedEnvironment(pSim);
            texture.SimulateEnvironment(ref sim, 1 == bDataOnly);
          }
        }
      }
      catch (Exception exception)
      {
        HostUtils.ExceptionReport(exception);
      }
    }

    #endregion
  }






  #region Native wrapper
  // DO NOT make public
  internal class NativeRenderEnvironment : RenderEnvironment
  {
    readonly Guid m_native_instance_id = Guid.Empty;
    public NativeRenderEnvironment(IntPtr pRenderContent)
    {
      m_native_instance_id = UnsafeNativeMethods.Rdk_RenderContent_InstanceId(pRenderContent);
    }
    public override string TypeName { get { return GetString(StringIds.TypeName); } }
    public override string TypeDescription { get { return GetString(StringIds.TypeDescription); } }
    internal override IntPtr ConstPointer()
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_FindContentInstance(m_native_instance_id);
      return pContent;
    }
    internal override IntPtr NonConstPointer()
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_FindContentInstance(m_native_instance_id);
      return pContent;
    }
  }
  #endregion
}
#endif