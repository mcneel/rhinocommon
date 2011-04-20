using System;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  [Serializable]
  public class DetailView : GeometryBase, ISerializable
  {
    internal DetailView(IntPtr native_ptr, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(native_ptr, parent_object, obj_ref)
    { }

    // serialization constructor
    protected DetailView(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new DetailView(IntPtr.Zero, null, null);
    }

    const int idxIsParallelProjection = 0;
    const int idxIsPerspectiveProjection = 1;
    const int idxIsProjectionLocked = 2;
    public bool IsParallelProjection
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_DetailView_GetBool(pConstThis, idxIsParallelProjection);
      }
      set
      {
        if (IsParallelProjection != value)
        {
          IntPtr pThis = NonConstPointer();
          UnsafeNativeMethods.ON_DetailView_SetBool(pThis, idxIsParallelProjection, value);
        }
      }
    }
    public bool IsPerspectiveProjection
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_DetailView_GetBool(pConstThis, idxIsPerspectiveProjection);
      }
      set
      {
        if (IsPerspectiveProjection != value)
        {
          IntPtr pThis = NonConstPointer();
          UnsafeNativeMethods.ON_DetailView_SetBool(pThis, idxIsPerspectiveProjection, value);
        }
      }
    }

    public bool IsProjectionLocked
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_DetailView_GetBool(pConstThis, idxIsProjectionLocked);
      }
      set
      {
        if (IsProjectionLocked != value)
        {
          IntPtr pThis = NonConstPointer();
          UnsafeNativeMethods.ON_DetailView_SetBool(pThis, idxIsProjectionLocked, value);
        }
      }
    }

    public double PageToModelRatio
    {
      get
      {
        if (!IsParallelProjection)
          return 0;
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_DetailView_GetPageToModelRatio(pConstThis);
      }
    }

    /// <summary>
    /// Set the detail viewport's projection so geometry is displayed at a certain scale
    /// </summary>
    /// <param name="modelLength">reference model length</param>
    /// <param name="modelUnits">units for model length</param>
    /// <param name="pageLength">length on page that the modelLength should equal</param>
    /// <param name="pageUnits">units for page length</param>
    /// <returns>
    /// true on success
    /// false if the viewport's projection is perspective or the input values do not make sense
    /// </returns>
    public bool SetScale(double modelLength, Rhino.UnitSystem modelUnits, double pageLength, Rhino.UnitSystem pageUnits)
    {
      // SetScale only works on parallel projections
      if (!IsParallelProjection)
        return false;

      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_DetailView_SetScale(pThis, modelLength, (int)modelUnits, pageLength, (int)pageUnits);
    }
  }
}
