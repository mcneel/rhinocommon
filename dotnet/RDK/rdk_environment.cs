#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rhino.Geometry;

#if RDK_UNCHECKED
namespace Rhino.Render
{
  public abstract class RenderEnvironment : RenderContent
  {
    protected RenderEnvironment()
      : base(true)
    {
    }

    internal RenderEnvironment(bool isCustom)
      : base(false)
    {
      //This constructor is only used to construct native wrappers
      Debug.Assert(isCustom == false);
    }

    public static RenderEnvironment CurrentEnvironment
    {
      get
      {
        RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
        return FromPointer(UnsafeNativeMethods.Rdk_RenderEnvironment_CurrentEnvironment(doc.m_docId)) as RenderEnvironment;
      }
      set
      {
        RhinoDoc doc = Rhino.RhinoDoc.ActiveDoc;
        UnsafeNativeMethods.Rdk_RenderEnvironment_SetCurrentEnvironment(doc.m_docId, value.InstanceId);
      }
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

    internal delegate IntPtr NewEnvironmentCallback(Guid type_id);
    internal static NewEnvironmentCallback m_NewEnvironment = OnNewEnvironment;
    static IntPtr OnNewEnvironment(Guid type_id)
    {
      IntPtr rc = IntPtr.Zero;
      try
      {
        Guid plugin_id;
        Type t = RdkPlugIn.GetRenderContentType(type_id, out plugin_id);
        if (t != null && plugin_id != Guid.Empty)
        {
          RenderEnvironment Environment = System.Activator.CreateInstance(t) as RenderEnvironment;
          Environment.Construct(plugin_id);
          rc = Environment.NonConstPointer();
        }
      }
      catch
      {
        rc = IntPtr.Zero;
      }
      return rc;
    }

    internal delegate void SimulateEnvironmentCallback(int serial_number, IntPtr p, int bDataOnly);
    internal static SimulateEnvironmentCallback m_SimulateEnvironment = OnSimulateEnvironment;
    static void OnSimulateEnvironment(int serial_number, IntPtr pSim, int bDataOnly)
    {
      try
      {
        RenderEnvironment texture = RenderContent.FromSerialNumber(serial_number) as RenderEnvironment;
        if (texture != null)
        {
          if (pSim != IntPtr.Zero)
          {
            SimulatedEnvironment sim = new SimulatedEnvironment(pSim);
            texture.SimulateEnvironment(ref sim, 1 == bDataOnly);
          }
        }
      }
      catch
      {
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
      : base(false)
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
    protected override bool IsNativeWrapper()
    {
      return true;
    }
  }
  #endregion
}
#endif