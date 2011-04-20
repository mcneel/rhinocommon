using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

//don't make serializable yet.

namespace Rhino.Geometry
{
  public class InstanceDefinitionGeometry : GeometryBase
  {
    #region internals
    internal InstanceDefinitionGeometry(IntPtr native_ptr, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(native_ptr, parent_object, obj_ref)
    { }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new InstanceDefinitionGeometry(IntPtr.Zero, null, null);
    }
    #endregion

    public InstanceDefinitionGeometry()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_InstanceDefinition_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
    }

    const int idxName = 0;
    const int idxDescription = 1;

    public string Name
    {
      get
      {
        IntPtr ptr = ConstPointer();
        IntPtr pName = UnsafeNativeMethods.ON_InstanceDefinition_GetString(ptr, idxName);
        if (IntPtr.Zero == pName)
          return String.Empty;
        return Marshal.PtrToStringUni(pName);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_SetString(ptr, idxName, value);
      }
    }

    public string Description
    {
      get
      {
        IntPtr ptr = ConstPointer();
        IntPtr pName = UnsafeNativeMethods.ON_InstanceDefinition_GetString(ptr, idxDescription);
        if (IntPtr.Zero == pName)
          return String.Empty;
        return Marshal.PtrToStringUni(pName);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_SetString(ptr, idxDescription, value);
      }
    }
  }

  public class InstanceReferenceGeometry : GeometryBase
  {
    internal InstanceReferenceGeometry(IntPtr native_ptr, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(native_ptr, parent_object, obj_ref)
    { }


    internal override GeometryBase DuplicateShallowHelper()
    {
      return new InstanceReferenceGeometry(IntPtr.Zero, null, null);
    }
  }
}
