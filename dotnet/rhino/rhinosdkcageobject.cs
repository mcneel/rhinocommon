#pragma warning disable 1591
//public class WireframeObject : RhinoObject { }
//public class CageObject : WireframeObject { }

#if RHINO_SDK
namespace Rhino.DocObjects
{
  public class MorphControlObject : RhinoObject
  {
    internal MorphControlObject(uint serialNumber)
      : base(serialNumber) { }

    //internal override CommitGeometryChangesFunc GetCommitFunc()
    //{
    //  return UnsafeNativeMethods.CRhinoMorphControlObject_InternalCommitChanges;
    //}
  }
}
#endif