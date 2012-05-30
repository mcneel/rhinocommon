using System;
using Rhino.Geometry;

namespace Rhino.DocObjects
{
#if RHINO_SDK
  /// <summary>
  /// Represents an object in the document.
  /// <para>RhinoObjects should only ever be creatable by the RhinoDoc.</para>
  /// </summary>
  public class RhinoObject //: Runtime.CommonObject - We don't want to allow for any private copies of RhinoObjects
  {
  #region internals
    // All RhinoObject pointer validation is performed against the runtime serial number
    internal uint m_rhinoobject_serial_number;
    internal GeometryBase m_original_geometry;
    internal GeometryBase m_edited_geometry;
    internal ObjectAttributes m_original_attributes;
    internal ObjectAttributes m_edited_attributes;

    // well.. that is not exactly true. As we start adding support for custom Rhino objects
    // we need a way to create RhinoObject from a plug-in
    internal IntPtr m_pRhinoObject = IntPtr.Zero; // this is ONLY set when the object is a custom rhinocommon object

    internal delegate uint CommitGeometryChangesFunc(uint sn, IntPtr pConstGeometry);

    internal IntPtr ConstPointer()
    {
      if (m_pRhinoObject != IntPtr.Zero)
        return m_pRhinoObject;

      IntPtr rc = UnsafeNativeMethods.RHC_LookupObjectBySerialNumber(m_rhinoobject_serial_number);
      if (IntPtr.Zero == rc)
        throw new Rhino.Runtime.DocumentCollectedException();

      return rc;
    }
    // same thing
    internal IntPtr NonConstPointer_I_KnowWhatImDoing()
    {
      return ConstPointer();
    }

    /// <summary>
    /// !!!DO NOT CALL THIS FUNCTION UNLESS YOU ARE WORKING WITH CUSTOM RHINO OBJECTS!!!
    /// </summary>
    /// <returns>A pointer.</returns>
    internal IntPtr NonConstPointer()
    {
      if( IntPtr.Zero==m_pRhinoObject )
        throw new Rhino.Runtime.DocumentCollectedException();
      return m_pRhinoObject;
    }

    // this protected constructor should only be used by "custom" subclasses
    internal RhinoObject()
    {
      m_rhinoobject_serial_number = 0;
      m_theDrawCallback = OnRhinoObjectDraw;
      m_theDuplicateCallback = OnRhinoObjectDuplicate;
      m_theDocNotifyCallback = OnRhinoObjectDocNotify;
      m_theActiveInViewportCallback = OnRhinoObjectActiveInViewport;
      m_theSelectionCallback = OnRhinoObjectSelection;
      m_thePickCallback = OnRhinoObjectPick;
      m_thePickedCallback = OnRhinoObjectPicked;
      m_theTransformCallback = OnRhinoObjectTransform;

      UnsafeNativeMethods.CRhinoObject_SetCallbacks(m_theDuplicateCallback, m_theDrawCallback, m_theDocNotifyCallback, m_theActiveInViewportCallback, m_theSelectionCallback, m_theTransformCallback);
      UnsafeNativeMethods.CRhinoObject_SetPickCallbacks(m_thePickCallback, m_thePickedCallback);
    }

    internal RhinoObject(uint sn)
    {
      m_rhinoobject_serial_number = sn;
    }

    internal delegate void RhinoObjectDrawCallback(IntPtr pConstRhinoObject, IntPtr pDisplayPipeline);
    internal delegate void RhinoObjectDuplicateCallback(int docId, uint sourceObjectSerialNumber, uint newObjectSerialNumber);
    internal delegate void RhinoObjectDocNotifyCallback(int docId, uint serialNumber, int add);
    internal delegate int RhinoObjectActiveInViewportCallback(int docId, uint serialNumber, IntPtr pRhinoViewport);
    internal delegate void RhinoObjectSelectionCallback(int docId, uint serialNumber);
    internal delegate void RhinoObjectTransformCallback(int docId, uint serialNumber, IntPtr pConstTransform);
    internal delegate void RhinoObjectPickCallback(int docId, uint serialNumber, IntPtr pConstRhinoObject, IntPtr pRhinoObjRefArray);
    internal delegate void RhinoObjectPickedCallback(int docId, uint serialNumber, IntPtr pConstRhinoObject, IntPtr pRhinoObjRefArray, int count);
    static RhinoObjectDrawCallback m_theDrawCallback;
    static RhinoObjectDuplicateCallback m_theDuplicateCallback;
    static RhinoObjectDocNotifyCallback m_theDocNotifyCallback;
    static RhinoObjectActiveInViewportCallback m_theActiveInViewportCallback;
    static RhinoObjectSelectionCallback m_theSelectionCallback;
    static RhinoObjectTransformCallback m_theTransformCallback;
    static RhinoObjectPickCallback m_thePickCallback;
    static RhinoObjectPickedCallback m_thePickedCallback;

    static void OnRhinoObjectDraw(IntPtr pConstRhinoObject, IntPtr pDisplayPipeline)
    {
      RhinoObject rhobj = RhinoObject.CreateRhinoObjectHelper(pConstRhinoObject);
      if( rhobj!=null )
        rhobj.OnDraw(new Display.DrawEventArgs(pDisplayPipeline, IntPtr.Zero));
    }

    static void OnRhinoObjectDuplicate(int docId, uint sourceObjectSerialNumber, uint newObjectSerialNumber)
    {
      RhinoDoc doc = RhinoDoc.FromId(docId);
      if (doc != null)
      {
        RhinoObject rhobj = doc.Objects.FindCustomObject(sourceObjectSerialNumber);
        if (rhobj != null)
        {
          Type t = rhobj.GetType();
          RhinoObject newobj = System.Activator.CreateInstance(t) as RhinoObject;
          newobj.m_rhinoobject_serial_number = newObjectSerialNumber;
          doc.Objects.AddCustomObject(newObjectSerialNumber, newobj);
          newobj.OnDuplicate(rhobj);
        }
      }
    }

    static void OnRhinoObjectDocNotify(int docId, uint serialNumber, int add)
    {
      RhinoDoc doc = RhinoDoc.FromId(docId);
      if (doc != null)
      {
        RhinoObject rhobj = doc.Objects.FindCustomObject(serialNumber);
        if (rhobj != null)
        {
          if (add == 1)
            rhobj.OnAddToDocument(doc);
          else
            rhobj.OnDeleteFromDocument(doc);
        }
      }
    }

    static int OnRhinoObjectActiveInViewport(int docId, uint serialNumber, IntPtr pRhinoViewport)
    {
      int rc = -1;
      RhinoDoc doc = RhinoDoc.FromId(docId);
      if (doc != null)
      {
        RhinoObject rhobj = doc.Objects.FindCustomObject(serialNumber);
        if (rhobj != null)
          rc = rhobj.IsActiveInViewport(new Rhino.Display.RhinoViewport(null, pRhinoViewport)) ? 1 : 0;
      }
      return rc;
    }

    static void OnRhinoObjectSelection(int docId, uint serialNumber)
    {
      RhinoDoc doc = RhinoDoc.FromId(docId);
      if (doc != null)
      {
        RhinoObject rhobj = doc.Objects.FindCustomObject(serialNumber);
        if (rhobj != null)
          rhobj.OnSelectionChanged();
      }
    }

    static void OnRhinoObjectTransform(int docId, uint serialNumber, IntPtr pConstTransform)
    {
      RhinoDoc doc = RhinoDoc.FromId(docId);
      if (doc != null)
      {
        RhinoObject rhobj = doc.Objects.FindCustomObject(serialNumber);
        if (rhobj != null)
        {
          Transform xf = (Transform)System.Runtime.InteropServices.Marshal.PtrToStructure(pConstTransform, typeof(Transform));
          rhobj.OnTransform(xf);
        }
      }
    }

    static System.Collections.Generic.IEnumerable<ObjRef> ObjRefCollectionFromIntPtr(IntPtr pRhinoObjRefArray, int count)
    {
      for (int i = count - 1; i >= 0; i--)
      {
        IntPtr pRhinoObjRef = UnsafeNativeMethods.CRhinoObjRefArray_GetLastItem(pRhinoObjRefArray, i);
        if( IntPtr.Zero!=pRhinoObjRef )
        {
          yield return new ObjRef(pRhinoObjRef);
        }
      }
    }

    static void OnRhinoObjectPick(int docId, uint serialNumber, IntPtr pConstRhinoPickContext, IntPtr pRhinoObjRefArray)
    {
      RhinoDoc doc = RhinoDoc.FromId(docId);
      if (doc != null)
      {
        RhinoObject rhobj = doc.Objects.FindCustomObject(serialNumber);
        if (rhobj != null)
        {
          System.Collections.Generic.IEnumerable<ObjRef> objs = rhobj.OnPick(new Input.Custom.PickContext(pConstRhinoPickContext));
          if (objs != null)
          {
            foreach (ObjRef objref in objs)
            {
              IntPtr pConstObjRef = objref.ConstPointer();
              UnsafeNativeMethods.CRhinoObjRefArray_Append(pRhinoObjRefArray, pConstObjRef);
            }
          }
        }
      }
    }

    static void OnRhinoObjectPicked(int docId, uint serialNumber, IntPtr pConstRhinoPickContext, IntPtr pRhinoObjRefArray, int count)
    {
      RhinoDoc doc = RhinoDoc.FromId(docId);
      if (doc != null)
      {
        RhinoObject rhobj = doc.Objects.FindCustomObject(serialNumber);
        var list = ObjRefCollectionFromIntPtr(pRhinoObjRefArray, count);
        if (rhobj != null)
        {
          rhobj.OnPicked(new Input.Custom.PickContext(pConstRhinoPickContext), list);
        }
      }
    }

    //const int idxCRhinoObject = 0;
    const int idxCRhinoPointObject = 1;
    const int idxCRhinoCurveObject = 2;
    const int idxCRhinoMeshObject = 3;
    const int idxCRhinoBrepObject = 4;
    const int idxCRhinoPointCloudObject = 5;
    const int idxCRhinoAnnotationTextObject = 6;
    const int idxCRhinoSurfaceObject = 7;
    const int idxCRhinoInstanceObject = 8;
    const int idxCRhinoHatchObject = 9;
    const int idxCRhinoDetailViewObject = 10;
    const int idxCRhinoClippingPlaneObject = 11;
    const int idxCRhinoTextDot = 12;
    const int idxCRhinoGripObject = 13;
    const int idxCRhinoExtrusionObject = 14;
    const int idxCRhinoLinearDimension = 15;
    const int idxCRhinoAnnotationObject = 16;
    const int idxCRhinoLight = 17;
    const int idxCRhinoMorphControl = 18;
    const int idxCRhinoRadialDimension = 19;
    const int idxCRhinoAngularDimension = 20;

    internal static RhinoObject CreateRhinoObjectHelper(IntPtr pRhinoObject)
    {
      if (IntPtr.Zero == pRhinoObject)
        return null;

      uint sn = UnsafeNativeMethods.CRhinoObject_RuntimeSN(pRhinoObject);
      if (sn < 1)
        return null;

      int doc_id = UnsafeNativeMethods.CRhinoObject_Document(pRhinoObject);
      RhinoDoc doc = RhinoDoc.FromId(doc_id);
      if (doc != null)
      {
        RhinoObject custom = doc.Objects.FindCustomObject(sn);
        if (custom != null)
          return custom;
      }


      int type = UnsafeNativeMethods.CRhinoRhinoObject_GetRhinoObjectType(pRhinoObject);
      if (type < 0)
        return null;
      RhinoObject rc;
      switch (type)
      {
        case idxCRhinoPointObject: //1
          rc = new PointObject(sn);
          break;
        case idxCRhinoCurveObject: //2
          rc = new CurveObject(sn);
          break;
        case idxCRhinoMeshObject: //3
          rc = new MeshObject(sn);
          break;
        case idxCRhinoBrepObject: //4
          rc = new BrepObject(sn);
          break;
        case idxCRhinoPointCloudObject: //5
          rc = new PointCloudObject(sn);
          break;
        case idxCRhinoAnnotationTextObject: //6
          rc = new TextObject(sn);
          break;
        case idxCRhinoSurfaceObject: //7
          rc = new SurfaceObject(sn);
          break;
        case idxCRhinoInstanceObject: //8
          rc = new InstanceObject(sn);
          break;
        case idxCRhinoHatchObject: //9
          rc = new HatchObject(sn);
          break;
        case idxCRhinoDetailViewObject: //10
          rc = new DetailViewObject(sn);
          break;
        case idxCRhinoClippingPlaneObject: //11
          rc = new ClippingPlaneObject(sn);
          break;
        case idxCRhinoTextDot: //12
          rc = new TextDotObject(sn);
          break;
        case idxCRhinoGripObject: //13
          rc = new GripObject(sn);
          break;
#if USING_V5_SDK
        case idxCRhinoExtrusionObject: //14
          rc = new ExtrusionObject(sn);
          break;
#endif
        case idxCRhinoLinearDimension: //15
          rc = new LinearDimensionObject(sn);
          break;
        case idxCRhinoAnnotationObject: //16
          rc = new AnnotationObjectBase(sn);
          break;
        case idxCRhinoLight: //17
          rc = new LightObject(sn);
          break;
        case idxCRhinoMorphControl: //18
          rc = new MorphControlObject(sn);
          break;
        case idxCRhinoRadialDimension: //19
          rc = new RadialDimensionObject(sn);
          break;
        case idxCRhinoAngularDimension: //20
          rc = new AngularDimensionObject(sn);
          break;
        default:
          rc = new RhinoObject(sn);
          break;
      }
      return rc;
    }
    #endregion

  #region statics
    /// <summary>
    /// Gets the runtime serial number that will be assigned to
    /// the next Rhino Object that is created.
    /// </summary>
    [CLSCompliant(false)]
    public static uint NextRuntimeSerialNumber
    {
      get
      {
        return UnsafeNativeMethods.CRhinoObject_NextRuntimeObjectSerialNumber();
      }
    }

    /// <summary>
    /// Gets the render meshes of some objects.
    /// </summary>
    /// <param name="rhinoObjects">An array, a list, or any enumerable set of Rhino objects.</param>
    /// <param name="okToCreate">true if the method is allowed to instantiate new meshes if they do not exist.</param>
    /// <param name="returnAllObjects">true if all objects should be returned.</param>
    /// <returns>An array of object references.</returns>
    public static ObjRef[] GetRenderMeshes(System.Collections.Generic.IEnumerable<RhinoObject> rhinoObjects, bool okToCreate, bool returnAllObjects)
    {
      ObjRef[] rc = null;
      Runtime.INTERNAL_RhinoObjectArray rhinoobject_array = new Rhino.Runtime.INTERNAL_RhinoObjectArray(rhinoObjects);
      IntPtr pRhObjectArray = rhinoobject_array.NonConstPointer();
      IntPtr pObjRefArray = UnsafeNativeMethods.RHC_RhinoGetRenderMeshes(pRhObjectArray, okToCreate, returnAllObjects);
      rhinoobject_array.Dispose();

      if (IntPtr.Zero != pObjRefArray)
      {
        int count = UnsafeNativeMethods.RhinoObjRefArray_Count(pObjRefArray);
        if (count > 0)
        {
          rc = new ObjRef[count];
          for (int i = 0; i < count; i++)
          {
            IntPtr pConstObjRef = UnsafeNativeMethods.RhinoObjRefArray_GetItem(pObjRefArray, i);
            rc[i] = new ObjRef(pConstObjRef);
          }
        }
      }
      UnsafeNativeMethods.RhinoObjRefArray_Delete(pObjRefArray);
      return rc;
    }
    #endregion

  #region properties
    /// <summary>
    /// Gets the Rhino-based object type.
    /// </summary>
    [CLSCompliant(false)]
    public ObjectType ObjectType
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return (ObjectType)UnsafeNativeMethods.CRhinoObject_ObjectType(ptr);
      }
    }

    /// <summary>
    /// Tests an object to see if its data members are correctly initialized.
    /// </summary>
    public bool IsValid
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Object_IsValid(ptr, IntPtr.Zero);
      }
    }

    /// <summary>
    /// Gets the document that owns this object.
    /// </summary>
    public RhinoDoc Document
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int id = UnsafeNativeMethods.CRhinoObject_Document(ptr);
        return RhinoDoc.FromId(id);
      }
    }

    /// <summary>
    /// Gets the underlying geometry for this object.
    /// <para>All rhino objects are composed of geometry and attributes.</para>
    /// </summary>
    public Rhino.Geometry.GeometryBase Geometry
    {
      get
      {
        if (null != m_edited_geometry)
          return m_edited_geometry;

        if (null == m_original_geometry)
        {
          ComponentIndex ci = new ComponentIndex();
          // use the "const" geometry that is associated with this RhinoObject
          IntPtr pConstThis = ConstPointer();
          IntPtr pGeometry = UnsafeNativeMethods.CRhinoObject_Geometry(pConstThis, ci);
          if (IntPtr.Zero == pGeometry)
            return null;

          m_original_geometry = GeometryBase.CreateGeometryHelper(pGeometry, this);
        }
        return m_original_geometry;
      }
      // We can also implement set if it makes sense.
      // One thing we would have to do is make sure that the geometry is of the correct type
      // It might be better to place this type of functionality on the subclass specific
      // geometry getters (CurveObject.CurveGeometry)
    }

    /// <summary>
    /// Gets or sets the object attributes.
    /// </summary>
    public ObjectAttributes Attributes
    {
      get
      {
        if (null != m_edited_attributes)
          return m_edited_attributes;

        return m_original_attributes ?? (m_original_attributes = new ObjectAttributes(this));
      }
      set
      {
        // make sure this object is still valid - ConstPointer will throw a DocumentCollectedException
        // if the object is no longer in existance
        ConstPointer();
        if (m_edited_attributes == value)
          return;

        m_edited_attributes = value.Duplicate();
      }
    }

    /// <summary>
    /// Gets the objects runtime serial number.
    /// </summary>
    [CLSCompliant(false)]
    public uint RuntimeSerialNumber
    {
      get
      {
        if (m_rhinoobject_serial_number != 0)
          return m_rhinoobject_serial_number;
        IntPtr pThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoObject_RuntimeSN(pThis);
      }
    }

    const int idxIsDeletable = 0;
    const int idxIsDeleted = 1;
    const int idxIsInstanceDefinitionGeometry = 2;
    const int idxIsReference = 3;
    //const int idxIsVisible = 4;
    const int idxIsNormal = 5;
    const int idxIsLocked = 6;
    const int idxIsHidden = 7;
    //const int idxIsSolid = 8;
    const int idxGripsSelected = 9;
    const int idxHasDynamicTransform = 10;
    bool GetBool(int which)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_GetBool(ptr, which);
    }

    /// <summary>
    /// Some objects cannot be deleted, like grips on lights and annotation objects. 
    /// </summary>
    public bool IsDeletable
    {
      get { return GetBool(idxIsDeletable); }
      protected set
      {
        // only custom subclasses can set this flag
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.CRhinoCustomObject_SetIsDeletable(pConstThis, value);
      }
    }

    /// <summary>
    /// true if the object is deleted. Deleted objects are kept by the document
    /// for undo purposes. Call RhinoDoc.UndeleteObject to undelete an object.
    /// </summary>
    public bool IsDeleted
    {
      get { return GetBool(idxIsDeleted); }
    }

    /// <summary>
    /// true if the object is used as part of an instance definition.   
    /// </summary>
    public bool IsInstanceDefinitionGeometry
    {
      get { return GetBool(idxIsInstanceDefinitionGeometry); }
    }

    /// <summary>
    /// An object must be in one of three modes: normal, locked or hidden.
    /// If an object is in normal mode, then the object's layer controls visibility
    /// and selectability. If an object is locked, then the object's layer controls
    /// visibility by the object cannot be selected. If the object is hidden, it is
    /// not visible and it cannot be selected.
    /// </summary>
    public bool IsNormal
    {
      get { return GetBool(idxIsNormal); }
    }

    /// <summary>
    /// An object must be in one of three modes: normal, locked or hidden.
    /// If an object is in normal mode, then the object's layer controls visibility
    /// and selectability. If an object is locked, then the object's layer controls
    /// visibility by the object cannot be selected. If the object is hidden, it is
    /// not visible and it cannot be selected.
    /// </summary>
    public bool IsLocked
    {
      get { return GetBool(idxIsLocked); }
    }

    /// <summary>
    /// An object must be in one of three modes: normal, locked or hidden.
    /// If an object is in normal mode, then the object's layer controls visibility
    /// and selectability. If an object is locked, then the object's layer controls
    /// visibility by the object cannot be selected. If the object is hidden, it is
    /// not visible and it cannot be selected.
    /// </summary>
    public bool IsHidden
    {
      get { return GetBool(idxIsHidden); }
    }

    /// <summary>
    /// Gets a value indicating if an object is a reference object. An object from a work session
    /// reference model is a reference object and cannot be modified. An object is
    /// a reference object if, and only if, it is on a reference layer.
    /// </summary>
    public bool IsReference
    {
      get { return GetBool(idxIsReference); }
    }

    /// <summary>Gets the object visibility.</summary>
    public bool Visible
    {
      get
      {
        return Attributes.Visible;
      }
    }

    //internal bool InternalIsSolid()
    //{
    //  return GetBool(idxIsSolid);
    //}

    #endregion

    /// <summary>
    /// Constructs a deep (full) copy of the geometry.
    /// </summary>
    /// <returns>A copy of the internal geometry.</returns>
    public Rhino.Geometry.GeometryBase DuplicateGeometry()
    {
      if (null != m_edited_geometry)
        return m_edited_geometry.Duplicate();

      GeometryBase g = Geometry;
      if (null != g)
        return g.Duplicate();

      return null;
    }

    /// <summary>
    /// Moves changes made to this RhinoObject into the RhinoDoc.
    /// </summary>
    /// <returns>
    /// true if changes were made.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayout.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayout.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayout.py' lang='py'/>
    /// </example>
    public bool CommitChanges()
    {
      bool rc = false;

      if (null != m_edited_geometry)
      {
        CommitGeometryChangesFunc func = GetCommitFunc();
        if (null == func)
          return false;
        IntPtr pConstGeometry = m_edited_geometry.ConstPointer();
        uint serial_number = func(m_rhinoobject_serial_number, pConstGeometry);
        if (serial_number > 0)
        {
          rc = true;
          m_rhinoobject_serial_number = serial_number;
          m_edited_geometry = null;
        }
        else
          return false;
      }

      if (null != m_edited_attributes)
      {
        rc = Document.Objects.ModifyAttributes(this, m_edited_attributes, false);
        if (rc)
          m_edited_attributes = null;
      }

      return rc;
    }

    internal virtual CommitGeometryChangesFunc GetCommitFunc()
    {
      return null;
    }

    /// <summary>
    /// Computes an estimate of the number of bytes that this object is using in memory.
    /// Note that this is a runtime memory estimate and does not directly compare to the
    /// amount of space take up by the object when saved to a file.
    /// </summary>
    /// <returns>The estimated number of bytes this object occupies in memory.</returns>
    [CLSCompliant(false)]
    public uint MemoryEstimate()
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Object_SizeOf(pConstThis);
    }

    //const int idxLayerIndex = 0;
    //const int idxGroupCount = 1;
    //const int idxSpace = 2;
    const int idxUnselectAllSubObjects = 3;
    const int idxUnhighightAllSubObjects = 4;
    int GetInt(int which)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_GetInt(ptr, which);
    }

    /// <summary>
    /// Every object has a Guid (globally unique identifier, also known as UUID, or universally
    /// unique identifier). The default value is Guid.Empty.
    /// <para>
    /// When an object is added to a model, the value is checked.  If the value is Guid.Empty, a
    /// new Guid is created. If the value is not null but it is already used by another object
    /// in the model, a new Guid is created. If the value is not Guid.Empty and it is not used by
    /// another object in the model, then that value persists. When an object is updated, by
    /// a move for example, the value of ObjectId persists.
    /// </para>
    /// <para>This value is the same as the one returned by this.Attributes.ObjectId.</para>
    /// </summary>
    public Guid Id
    {
      get
      {
        return Attributes.ObjectId;
      }
    }

    /// <summary>
    /// Rhino objects have optional text names.  More than one object in
    /// a model can have the same name and some objects may have no name.
    /// </summary>
    public string Name
    {
      get
      {
        return Attributes.Name;
      }
    }

    /// <summary>Number of groups object belongs to.</summary>
    public int GroupCount
    {
      get
      {
        return Attributes.GroupCount;
      }
    }

    /// <summary>
    /// Allocates an array of group indices of length GroupCount.
    /// If <see cref="GroupCount"/> is 0, then this method returns null.
    /// </summary>
    /// <returns>An array of group indices, or null if <see cref="GroupCount"/> is 0.</returns>
    public int[] GetGroupList()
    {
      int count = GroupCount;
      if (count < 1)
        return null;
      int[] rc = new int[count];
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.CRhinoObject_GetGroupList(ptr, ref rc[0]);
      return rc;
    }

    /// <summary>Check selection state.</summary>
    /// <param name="checkSubObjects">
    /// (false is good default)
    /// If true and the entire object is not selected, and some subset of the object
    /// is selected, like some edges of a surface, then 3 is returned.
    /// If false and the entire object is not selected, then zero is returned.
    /// </param>
    /// <returns>
    /// 0 = object is not selected.
    /// 1 = object is selected.
    /// 2 = entire object is selected persistently.
    /// 3 = one or more proper sub-objects are selected.
    /// </returns>
    public int IsSelected(bool checkSubObjects)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsSelected(ptr, checkSubObjects);
    }

    /// <summary>Check sub-object selection state.</summary>
    /// <param name="componentIndex">Index of subobject to check.</param>
    /// <returns>true if the subobject is selected.</returns>
    /// <remarks>A subobject cannot be persistently selected.</remarks>
    public bool IsSubObjectSelected(ComponentIndex componentIndex)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsSubObjectSelected(ptr, componentIndex);
    }

    /// <summary>Get a list of all selected sub-objects.</summary>
    /// <returns>An array of subobject indices, or null if there are none.</returns>
    public ComponentIndex[] GetSelectedSubObjects()
    {
      IntPtr ptr = ConstPointer();
      Runtime.INTERNAL_ComponentIndexArray arr = new Runtime.INTERNAL_ComponentIndexArray();
      IntPtr pArray = arr.NonConstPointer();
      int count = UnsafeNativeMethods.CRhinoObject_GetSelectedSubObjects(ptr, pArray, true);
      ComponentIndex[] rc = null;
      if (count > 0)
      {
        rc = arr.ToArray();
      }
      arr.Dispose();
      return rc;
    }

    /// <summary>Reports if an object can be selected.</summary>
    /// <param name="ignoreSelectionState">
    /// If true, then selected objects are selectable.
    /// If false, then selected objects are not selectable.
    /// </param>
    /// <param name="ignoreGripsState">
    /// If true, then objects with grips on can be selected.
    /// If false, then the value returned by the object's IsSelectableWithGripsOn() function decides if the object can be selected.
    /// </param>
    /// <param name="ignoreLayerLocking">
    /// If true, then objects on locked layers are selectable.
    /// If false, then objects on locked layers are not selectable.
    /// </param>
    /// <param name="ignoreLayerVisibility">
    /// If true, then objects on hidden layers are selectable.
    /// If false, then objects on hidden layers are not selectable.
    /// </param>
    /// <returns>true if object is capable of being selected.</returns>
    /// <remarks>
    /// Objects that are locked, hidden, or on locked or hidden layers
    /// cannot be selected. If IsSelectableWithGripsOn() returns false,
    /// then an that object is not selectable if it has grips turned on.
    /// </remarks>
    public bool IsSelectable(bool ignoreSelectionState, bool ignoreGripsState, bool ignoreLayerLocking, bool ignoreLayerVisibility)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsSelectable(ptr, ignoreSelectionState, ignoreGripsState, ignoreLayerLocking, ignoreLayerVisibility);
    }
    /// <summary>Reports if an object can be selected.</summary>
    /// <returns>true if object is capable of being selected.</returns>
    /// <remarks>
    /// Objects that are locked, hidden, or on locked or hidden layers
    /// cannot be selected. If IsSelectableWithGripsOn() returns false,
    /// then an that object is not selectable if it has grips turned on.
    /// </remarks>
    public bool IsSelectable()
    {
      return IsSelectable(false, false, false, false);
    }

    /// <summary>Reports if a subobject can be selected.</summary>
    /// <param name="componentIndex">index of subobject to check.</param>
    /// <param name="ignoreSelectionState">
    /// If true, then selected objects are selectable.
    /// If false, then selected objects are not selectable.
    /// </param>
    /// <returns>true if the specified subobject can be selected.</returns>
    /// <remarks>
    /// Objects that are locked, hidden, or on locked or hidden layers
    /// cannot be selected. If IsSelectableWithGripsOn() returns false,
    /// then that object is not selectable if it has grips turned on.
    /// </remarks>
    public bool IsSubObjectSelectable(ComponentIndex componentIndex, bool ignoreSelectionState)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsSubObjectSelectable(ptr, componentIndex, ignoreSelectionState);
    }

    /// <summary>Selects an object.</summary>
    /// <param name="on">The new selection state; true activates selection.</param>
    /// <param name="syncHighlight">
    /// If true, then the object is highlighted if it is selected 
    /// and unhighlighted if is is not selected.
    /// <para>Highlighting can be and stay out of sync, as its specification is independent.</para>
    /// </param>
    /// <param name="persistentSelect">
    /// Objects that are persistently selected stay selected when a command terminates.
    /// </param>
    /// <param name="ignoreGripsState">
    /// If true, then objects with grips on can be selected.
    /// If false, then the value returned by the object's IsSelectableWithGripsOn() function
    /// decides if the object can be selected when it has grips turned on.
    /// </param>
    /// <param name="ignoreLayerLocking">
    /// If true, then objects on locked layers can be selected.
    /// If false, then objects on locked layers cannot be selected.
    /// </param>
    /// <param name="ignoreLayerVisibility">
    /// If true, then objects on hidden layers can be selectable.
    /// If false, then objects on hidden layers cannot be selected.
    /// </param>
    /// <returns>
    /// <para>0: object is not selected.</para>
    /// <para>1: object is selected.</para>
    /// <para>2: object is selected persistently.</para>
    /// </returns>
    /// <remarks>
    /// Objects that are locked, hidden, or on locked or hidden layers
    /// cannot be selected. If IsSelectableWithGripsOn() returns false,
    /// then an that object is not selectable if it has grips turned on.
    /// </remarks>
    public int Select(bool on, bool syncHighlight, bool persistentSelect, bool ignoreGripsState, bool ignoreLayerLocking, bool ignoreLayerVisibility)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_Select(ptr, on, syncHighlight, persistentSelect, ignoreGripsState, ignoreLayerLocking, ignoreLayerVisibility);
    }

    /// <summary>Selects an object.</summary>
    /// <param name="on">The new selection state; true activates selection.</param>
    /// <returns>
    /// <para>0: object is not selected.</para>
    /// <para>1: object is selected.</para>
    /// <para>2: object is selected persistently.</para>
    /// </returns>
    /// <remarks>
    /// Objects that are locked, hidden, or on locked or hidden layers
    /// cannot be selected. If IsSelectableWithGripsOn() returns false,
    /// then an that object is not selectable if it has grips turned on.
    /// </remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    public int Select(bool on)
    {
      return Select(on, true);
    }

    /// <summary>Selects an object.</summary>
    /// <param name="on">The new selection state; true activates selection.</param>
    /// <param name="syncHighlight">
    /// If true, then the object is hightlighted if it is selected
    /// and not hightlighted if is is not selected.
    /// <para>Highlighting can be and stay out of sync, as its specification is independent.</para>
    /// </param>
    /// <returns>
    /// <para>0: object is not selected.</para>
    /// <para>1: object is selected.</para>
    /// <para>2: object is selected persistently.</para>
    /// </returns>
    /// <remarks>
    /// Objects that are locked, hidden, or on locked or hidden layers
    /// cannot be selected. If IsSelectableWithGripsOn() returns false,
    /// then an that object is not selectable if it has grips turned on.
    /// </remarks>
    public int Select(bool on, bool syncHighlight)
    {
      return Select(on, syncHighlight, true, false, false, false);
    }

    /// <summary>Reports if an object can be selected.</summary>
    /// <param name="componentIndex">Index of subobject to check.</param>
    /// <param name="select">The new selection state; true activates selection.</param>
    /// <param name="syncHighlight">
    /// (default=true)
    /// If true, then the object is highlighted if it is selected 
    /// and unhighlighted if is is not selected.
    /// </param>
    /// <returns>
    /// 0: object is not selected
    /// 1: object is selected
    /// 2: object is selected persistently.
    /// </returns>
    /// <remarks>
    /// Objects that are locked, hidden, or on locked or hidden layers
    /// cannot be selected. If IsSelectableWithGripsOn() returns false,
    /// then an that object is not selectable if it has grips turned on.
    /// </remarks>
    public int SelectSubObject(ComponentIndex componentIndex, bool select, bool syncHighlight)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_SelectSubObject(ptr, componentIndex, select, syncHighlight);
    }

    /// <summary>
    /// Removes selection from all subobjects.
    /// </summary>
    /// <returns>The number of unselected subobjects.</returns>
    public int UnselectAllSubObjects()
    {
      // 20 Jan 2010 - S. Baer
      // The Rhino SDK function CRhinoObject::UnselectAllSubObjects is not const, but we shouldn't have to
      // be copying objects around in order to unselect subobjects (especially when SelectSubObject is
      // considered a const operation.)
      //
      // I dug through the Rhino core source code and passing a const_cast pointer appears to be okay in
      // this situation
      return GetInt(idxUnselectAllSubObjects);
    }

    /// <summary>Check highlight state.</summary>
    /// <param name="checkSubObjects">
    /// If true and the entire object is not highlighted, and some subset of the object
    /// is highlighted, like some edges of a surface, then 3 is returned.
    /// If false and the entire object is not highlighted, then zero is returned.
    /// </param>
    /// <returns>
    /// <para>0: object is not highlighted.</para>
    /// <para>1: entire object is highlighted.</para>
    /// <para>3: one or more proper sub-objects are highlighted.</para>
    /// </returns>
    public int IsHighlighted(bool checkSubObjects)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsHighlighted(ptr, checkSubObjects);
    }

    /// <summary>
    /// Modifies the highlighting of the object.
    /// </summary>
    /// <param name="enable">true if highlighting should be enabled.</param>
    /// <returns>true if the object is now highlighted.</returns>
    public bool Highlight(bool enable)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_Highlight(ptr, enable);
    }

    /// <summary>
    /// Determines if a subobject is highlighted.
    /// </summary>
    /// <param name="componentIndex">A subobject component index.</param>
    /// <returns>true if the subobject is highlighted.</returns>
    public bool IsSubObjectHighlighted(ComponentIndex componentIndex)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsSubObjectHighlighted(ptr, componentIndex);
    }

    /// <summary>
    /// Gets a list of all highlighted subobjects.
    /// </summary>
    /// <returns>An array of all highlighted subobjects; or null is there are none.</returns>
    public ComponentIndex[] GetHighlightedSubObjects()
    {
      IntPtr ptr = ConstPointer();
      Runtime.INTERNAL_ComponentIndexArray arr = new Runtime.INTERNAL_ComponentIndexArray();
      IntPtr pArray = arr.NonConstPointer();
      int count = UnsafeNativeMethods.CRhinoObject_GetSelectedSubObjects(ptr, pArray, false);
      ComponentIndex[] rc = null;
      if (count > 0)
      {
        rc = arr.ToArray();
      }
      arr.Dispose();
      return rc;
    }

    /// <summary>
    /// Highlights a subobject.
    /// </summary>
    /// <param name="componentIndex">A subobject component index.</param>
    /// <param name="highlight">true if the subobject should be highlighted.</param>
    /// <returns>true if the subobject is now highlighted.</returns>
    public bool HighlightSubObject(ComponentIndex componentIndex, bool highlight)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_HighlightSubObject(ptr, componentIndex, highlight);
    }

    /// <summary>
    /// Removes highlighting from all subobjects.
    /// </summary>
    /// <returns>The number of changed subobjects.</returns>
    public int UnhighlightAllSubObjects()
    {
      // 20 Jan 2010 - S. Baer
      // See my comments in UnselectAllSubObjects. The same goes for UnhighlightAllSubObjects
      return GetInt(idxUnhighightAllSubObjects);
    }

    /// <summary>Gets or sets the activation state of object default editing grips.</summary>
    public bool GripsOn
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int rc = UnsafeNativeMethods.CRhinoObject_GripsOn(ptr);
        return rc != 0;
      }
      set
      {
        // 20 Jan 2010 - S. Baer
        // Made enabling grips a const operation. The PointsOn command performs
        // a const_cast on CRhinoObject when turning grips on
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.CRhinoObject_EnableGrips(ptr, value);
      }
    }

    /// <summary>
    /// true if grips are turned on and at least one is selected.
    /// </summary>
    public bool GripsSelected
    {
      get
      {
        return GetBool(idxGripsSelected);
      }
    }

    /// <summary>Turns on/off the object's editing grips.</summary>
    /// <param name="customGrips">The custom object grips.</param>
    /// <returns>
    /// true if the call succeeded.  If you attempt to add custom grips to an
    /// object that does not support custom grips, then false is returned.
    /// </returns>
    public bool EnableCustomGrips(Rhino.DocObjects.Custom.CustomObjectGrips customGrips)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pGrips = customGrips==null?IntPtr.Zero:customGrips.NonConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoObject_EnableCustomGrips(pConstThis, pGrips);
      if (rc && customGrips != null)
      {
        customGrips.OnAttachedToRhinoObject(this);
      }
      return rc;
    }

    /// <summary>
    /// Returns grips for this object If grips are enabled. If grips are not
    /// enabled, returns null.
    /// </summary>
    /// <returns>An array of grip objects; or null if there are no grips.</returns>
    public GripObject[] GetGrips()
    {
      IntPtr pThis = ConstPointer();
      IntPtr pGripList = UnsafeNativeMethods.ON_GripList_New();
      int count = UnsafeNativeMethods.CRhinoObject_GetGrips(pThis, pGripList);
      GripObject[] rc = null;
      if (count > 0)
      {
        System.Collections.Generic.List<GripObject> grips = new System.Collections.Generic.List<GripObject>();
        for (int i = 0; i < count; i++)
        {
          IntPtr pGrip = UnsafeNativeMethods.ON_GripList_Get(pGripList, i);
          uint sn = UnsafeNativeMethods.CRhinoObject_RuntimeSN(pGrip);
          if (IntPtr.Zero != pGrip && sn > 0)
          {
            GripObject g = new GripObject(sn);
            grips.Add(g);
          }
        }
        if (grips.Count > 0)
          rc = grips.ToArray();
      }
      UnsafeNativeMethods.ON_GripList_Delete(pGripList);
      return rc;
    }

    /// <summary>
    /// Used to turn analysis modes on and off.
    /// </summary>
    /// <param name="mode">A visual analysis mode.</param>
    /// <param name="enable">true if the mode should be activated; false otherwise.</param>
    /// <returns>true if this object supports the analysis mode.</returns>
    public bool EnableVisualAnalysisMode(Rhino.Display.VisualAnalysisMode mode, bool enable)
    {
      IntPtr pConstThis = ConstPointer();
      Guid id = mode.Id;
      return UnsafeNativeMethods.CRhinoObject_EnableVisualAnalysisMode(pConstThis, id, enable);
    }

    /// <summary>
    /// Reports if any visual analysis mode is currently active for an object.
    /// </summary>
    /// <returns>true if an analysis mode is active; otherwise false.</returns>
    public bool InVisualAnalysisMode()
    {
      return InVisualAnalysisMode(null);
    }

    /// <summary>
    /// Reports if a visual analysis mode is currently active for an object.
    /// </summary>
    /// <param name="mode">
    /// The mode to check for.
    /// <para>Use null if you want to see if any mode is active.</para>
    /// </param>
    /// <returns>true if the specified analysis mode is active; otherwise false.</returns>
    public bool InVisualAnalysisMode(Rhino.Display.VisualAnalysisMode mode)
    {
      IntPtr pConstThis = ConstPointer();
      Guid id = Guid.Empty;
      if (mode != null)
        id = mode.Id;
      return UnsafeNativeMethods.CRhinoObject_InVisualAnalysisMode(pConstThis, id);
    }

    /// <summary>
    /// Gets a list of currently enabled analysis modes for this object.
    /// </summary>
    /// <returns>An array of visual analysis modes. The array can be empty, but not null.</returns>
    public Rhino.Display.VisualAnalysisMode[] GetActiveVisualAnalysisModes()
    {
      IntPtr pConstThis = ConstPointer();
      int count = UnsafeNativeMethods.CRhinoObject_AnalysisModeList_Count(pConstThis);
      Rhino.Display.VisualAnalysisMode[] rc = new Display.VisualAnalysisMode[count];
      for (int i = 0; i < count; i++)
      {
        Guid id = UnsafeNativeMethods.CRhinoObject_AnalysisModeListId(pConstThis, i);
        rc[i] = Rhino.Display.VisualAnalysisMode.Find(id);
      }
      return rc;
    }

    /// <summary>
    /// Gets a localized short descriptive name of the object.
    /// </summary>
    /// <param name="plural">true if the descriptive name should in plural.</param>
    /// <returns>A string with the short localized descriptive name.</returns>
    public virtual string ShortDescription(bool plural)
    {
      using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoObject_ShortDescription(pConstThis, pString, plural);
        return sh.ToString();
      }
    }

    /// <summary>
    /// Returns true if the object is capable of having a mesh of the specified type
    /// </summary>
    /// <param name="meshType"></param>
    /// <returns></returns>
    public virtual bool IsMeshable(Rhino.Geometry.MeshType meshType)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsMeshable(pConstThis, (int)meshType);
    }

    /// <summary>
    /// Meshing parameters that this object uses for generating render meshes. If the
    /// object's attributes do not have custom meshing parameters, then the document's
    /// meshing parameters are used.
    /// </summary>
    /// <returns></returns>
    public MeshingParameters GetRenderMeshParameters()
    {
      MeshingParameters rc = new MeshingParameters();
      IntPtr pMeshingParameters = rc.NonConstPointer();
      IntPtr pConstThis = ConstPointer();
      UnsafeNativeMethods.CRhinoObject_GetRenderMeshParameters(pConstThis, pMeshingParameters);
      return rc;
    }

    /// <summary>
    /// RhinoObjects can have several different types of meshes and 
    /// different numbers of meshes.  A b-rep can have a render and 
    /// an analysis mesh on each face.  A mesh object has a single 
    /// render mesh and no analysis mesh. Curve, point, and annotation
    /// objects have no meshes.
    /// </summary>
    /// <param name="meshType">type of mesh to count</param>
    /// <param name="parameters">
    /// if not null and if the object can change its mesh (like a brep),
    /// then only meshes that were created with these mesh parameters are counted.
    /// </param>
    /// <returns>number of meshes</returns>
    public virtual int MeshCount(MeshType meshType, MeshingParameters parameters)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pMeshParameters = IntPtr.Zero;
      if (parameters != null)
        pMeshParameters = parameters.ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_MeshCount(pConstThis, (int)meshType, pMeshParameters);
    }

    /// <summary>
    /// Create meshes used to render and analyze surface and polysrf objects.
    /// </summary>
    /// <param name="meshType">type of meshes to create</param>
    /// <param name="parameters">
    /// in parameters that control the quality of the meshes that are created
    /// </param>
    /// <param name="ignoreCustomParameters">
    /// Default should be false. Should the object ignore any custom meshing
    /// parameters on the object's attributes
    /// </param>
    /// <returns>number of meshes created</returns>
    public virtual int CreateMeshes(MeshType meshType, MeshingParameters parameters, bool ignoreCustomParameters)
    {
      IntPtr pThis = NonConstPointer_I_KnowWhatImDoing();
      IntPtr pConstMeshParameters = parameters.ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_CreateMeshes(pThis, (int)meshType, pConstMeshParameters, ignoreCustomParameters);
    }

    /// <summary>
    /// Get existing meshes used to render and analyze surface and polysrf objects.
    /// </summary>
    /// <param name="meshType"></param>
    /// <returns></returns>
    public virtual Mesh[] GetMeshes(MeshType meshType)
    {
      using (var mesh_array = new Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer())
      {
        IntPtr pMeshArray = mesh_array.NonConstPointer();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.CRhinoObject_GetMeshes(pConstThis, pMeshArray, (int)meshType);
        return mesh_array.ToConstArray(this);
      }
    }

    /// <summary>
    /// Gets an array of subobjects.
    /// </summary>
    /// <returns>An array of subobjects, or null if there are none.</returns>
    public DocObjects.RhinoObject[] GetSubObjects()
    {
      Runtime.INTERNAL_RhinoObjectArray arr = new Runtime.INTERNAL_RhinoObjectArray();
      IntPtr ptr = ConstPointer();
      IntPtr pArray = arr.NonConstPointer();
      int count = UnsafeNativeMethods.CRhinoObject_GetSubObjects(ptr, pArray);
      DocObjects.RhinoObject[] rc = null;
      if (count > 0)
        rc = arr.ToArray();
      arr.Dispose();
      return rc;
    }

    /// <summary>
    /// True if the object has a dynamic transformation
    /// </summary>
    public bool HasDynamicTransform
    {
      get { return GetBool(idxHasDynamicTransform); }
    }

    /// <summary>
    /// While an object is being dynamically tranformed (dragged, rotated, ...),
    /// the current transformation can be retrieved and used for creating
    /// dynamic display.
    /// </summary>
    /// <param name="transform"></param>
    /// <returns>
    /// True if the object is being edited and its transformation
    /// is available.  False if the object is not being edited,
    /// in which case the identity xform is returned.
    /// </returns>
    public bool GetDynamicTransform(out Transform transform)
    {
      transform = Transform.Identity;
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_GetDynamicTransform(pConstThis, ref transform);
    }

#if RDK_UNCHECKED
    /// <summary>
    /// Gets the instance ID of the render material associated with this object.
    /// </summary>
    public Guid RenderMaterialInstanceId
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.Rdk_RenderContent_ObjectMaterialInstanceId(pConstThis);
      }
      //set
      //{
      //  IntPtr pThis = NonConstPointer();
      //  UnsafeNativeMethods.Rdk_RenderContent_SetObjectMaterialInstanceid(pThis, value);
      //}
    }

    /// <summary>
    /// Gets the render material associated with this object.
    /// </summary>
    public Rhino.Render.RenderMaterial RenderMaterial
    {
      get
      {
        return Rhino.Render.RenderContent.FromInstanceId(RenderMaterialInstanceId) as Rhino.Render.RenderMaterial;
      }
      //set
      //{
      //  RenderMaterialInstanceId = value.InstanceId;
      //}
    }

    /// <summary>
    /// Gets all object decals associated with this object.
    /// </summary>
    public Rhino.Render.ObjectDecals Decals
    {
      get
      {
        Rhino.Render.ObjectDecals decals = new Rhino.Render.ObjectDecals(this);
        return decals;
      }
    }
#endif
    /// <summary>
    /// Gets material that this object uses based on it's attributes and the document
    /// that the object is associated with.  In the rare case that a document is not
    /// associated with this object, null will be returned.
    /// </summary>
    /// <param name="frontMaterial">
    /// If true, gets the material used to render the object's front side
    /// </param>
    /// <returns></returns>
    public Material GetMaterial(bool frontMaterial)
    {
      IntPtr pConstThis = ConstPointer();
      var doc = Document;
      if (doc == null)
        return null;
      int index = UnsafeNativeMethods.CRhinoObject_GetMaterial(pConstThis, frontMaterial);
      if( index<-2 ) // -1 and -2 are valid since the material may be the "default" or "locked default" material
        return null;

      if( -1==index )
        return Material.DefaultMaterial;
      return doc.Materials[index];
    }

    /// <summary>
    /// Called when Rhino wants to draw this object
    /// </summary>
    /// <param name="e"></param>
    protected virtual void OnDraw(Rhino.Display.DrawEventArgs e)
    {
      IntPtr pConstThis = ConstPointer();
      UnsafeNativeMethods.CRhinoObject_Draw(pConstThis, e.m_pDisplayPipeline);
    }

    /// <summary>
    /// Called when this a new instance of this object is created and copied from
    /// an existing object
    /// </summary>
    /// <param name="source"></param>
    protected virtual void OnDuplicate(RhinoObject source) { }

    /// <summary>
    /// This call informs an object it is about to be deleted.
    /// Some objects, like clipping planes, need to do a little extra cleanup
    /// before they are deleted.
    /// </summary>
    /// <param name="doc"></param>
    protected virtual void OnDeleteFromDocument(RhinoDoc doc) { }

    /// <summary>
    /// This call informs an object it is about to be added to the list of
    /// active objects in the document.
    /// </summary>
    /// <param name="doc"></param>
    protected virtual void OnAddToDocument(RhinoDoc doc) { }

    /// <summary>
    /// Determine if this object is active in a particular viewport.
    /// </summary>
    /// <param name="viewport"></param>
    /// <remarks>
    /// The default implementation tests for space and viewport id. This
    /// handles things like testing if a page space object is visible in a
    /// modeling view.
    /// </remarks>
    /// <returns>True if the object is active in viewport</returns>
    public virtual bool IsActiveInViewport(Rhino.Display.RhinoViewport viewport)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsActiveInViewport(pConstThis, viewport.ConstPointer());
    }

    /// <summary>
    /// Called to determine if this object or some sub-portion of this object should be
    /// picked given a pick context.
    /// </summary>
    /// <param name="context"></param>
    protected virtual System.Collections.Generic.IEnumerable<ObjRef> OnPick(Rhino.Input.Custom.PickContext context)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pConstContext = context.ConstPointer();
      IntPtr pObjRefArray = UnsafeNativeMethods.CRhinoObject_Pick(pConstThis, pConstContext);
      if (IntPtr.Zero == pObjRefArray)
        return null;
      return new Runtime.INTERNAL_RhinoObjRefArray(pObjRefArray);
    }

    /// <summary>
    /// Called when this object has been picked
    /// </summary>
    /// <param name="context"></param>
    /// <param name="pickedItems">
    /// Items that were picked. This parameter is enumerable because there may
    /// have been multiple sub-objects picked
    /// </param>
    protected virtual void OnPicked(Rhino.Input.Custom.PickContext context, System.Collections.Generic.IEnumerable<ObjRef> pickedItems)
    {
    }

    /// <summary>
    /// Called when the selection state of this object has changed
    /// </summary>
    protected virtual void OnSelectionChanged()
    {
    }

    /// <summary>
    /// Called when a transformation has been applied to the geometry
    /// </summary>
    /// <param name="transform"></param>
    protected virtual void OnTransform(Rhino.Geometry.Transform transform)
    {
    }
  }
#endif
  /// <summary>
  /// Defines enumerated values for several kinds of selection methods.
  /// </summary>
  public enum SelectionMethod : int
  {
    /// <summary>
    /// Selected by non-mouse method (SelAll, etc.)
    /// </summary>
    Other = 0,

    /// <summary>
    /// Selected by a mouse click on the object.
    /// </summary>
    MousePick = 1,

    /// <summary>
    /// Selected by a mouse selection window box. 
    /// Window selection indicates the object is completely contained by the selection rectangle.
    /// </summary>
    WindowBox = 2,

    /// <summary>
    /// Selected by a mouse selection crossing box. 
    /// A crossing selection indicates the object intersects with the selection rectangle.
    /// </summary>
    CrossingBox = 3
  }

  // skipping CRhinoPhantomObject, CRhinoProxyObject

#if RHINO_SDK
  // all ObjRef's are created in .NET
  /// <summary>
  /// Represents a reference to a Rhino object.
  /// </summary>
  public class ObjRef : IDisposable
  {
    private IntPtr m_ptr; // pointer to unmanaged CRhinoObjRef
    internal IntPtr ConstPointer() { return m_ptr; }
    internal IntPtr NonConstPointer() { return m_ptr; }
    internal ObjRef()
    {
      m_ptr = UnsafeNativeMethods.CRhinoObjRef_New();
    }

    internal ObjRef(ObjRef other)
    {
      m_ptr = UnsafeNativeMethods.CRhinoObjRef_Copy(other.m_ptr);
    }
    internal ObjRef(IntPtr pOtherObjRef)
    {
      m_ptr = UnsafeNativeMethods.CRhinoObjRef_Copy(pOtherObjRef);
    }

    internal ObjRef(RhinoObject parent, IntPtr pGeometry)
    {
      IntPtr pParent = parent.ConstPointer();
      m_ptr = UnsafeNativeMethods.CRhinoObjRef_New3(pParent, pGeometry);
    }

    /// <summary>
    /// Initializes a new object reference from a globally unique identifier (<see cref="Guid"/>).
    /// </summary>
    /// <param name="id">The ID.</param>
    public ObjRef(Guid id)
    {
      m_ptr = UnsafeNativeMethods.CRhinoObjRef_New1(id);
    }

    /// <summary>
    /// Initializes a new object reference from a Rhino object.
    /// </summary>
    /// <param name="rhinoObject">The Rhino object.</param>
    public ObjRef(DocObjects.RhinoObject rhinoObject)
    {
      IntPtr pObject = rhinoObject.ConstPointer();
      m_ptr = UnsafeNativeMethods.CRhinoObjRef_New2(pObject);
    }

    /// <summary>
    /// Initialized a new object reference from a Rhino object and pick context
    /// </summary>
    /// <param name="rhinoObject"></param>
    /// <param name="pickContext"></param>
    public ObjRef(DocObjects.RhinoObject rhinoObject, Rhino.Input.Custom.PickContext pickContext)
    {
      IntPtr pObject = rhinoObject.ConstPointer();
      IntPtr pPickContext = pickContext.ConstPointer();
      m_ptr = UnsafeNativeMethods.CRhinoObjRef_New4(pObject, pPickContext);
    }

    /// <summary>Returns the id of the referenced Rhino object.</summary>
    public Guid ObjectId
    {
      get { return UnsafeNativeMethods.CRhinoObjRef_ObjectUuid(m_ptr); }
    }

    /// <summary>
    /// If &gt; 0, then this is the value of a Rhino object's serial number field.
    /// The serial number is used instead of the pointer to prevent crashes in
    /// cases when the RhinoObject is deleted but an ObjRef continues to reference
    /// the Rhino object. The value of RuntimeSerialNumber is not saved in archives
    /// because it generally changes if you save and reload an archive.
    /// </summary>
    [CLSCompliant(false)]
    public uint RuntimeSerialNumber
    {
      get { return UnsafeNativeMethods.CRhinoObjRef_RuntimeSN(m_ptr); }
    }

    /// <summary>
    /// Gets the component index of the referenced (sub) geometry.
    /// Some objects have subobjects that are valid pieces of geometry. For
    /// example, breps have edges and faces that are valid curves and surfaces.
    /// Each subobject has a component index that is &gt; 0. The parent
    /// geometry has a component index = -1.
    /// </summary>
    public Geometry.ComponentIndex GeometryComponentIndex
    {
      get
      {
        ComponentIndex ci = new ComponentIndex();
        UnsafeNativeMethods.CRhinoObjRef_GeometryComponentIndex(m_ptr, ref ci);
        return ci;
      }
    }

    internal const int idxON_Geometry = 0;
    internal const int idxON_Curve = 1;
    internal const int idxON_NurbsCurve = 2;
    internal const int idxON_Mesh = 3;
    internal const int idxON_Point = 4;
    internal const int idxON_TextDot = 5;
    internal const int idxON_Surface = 6;

    internal IntPtr GetGeometryConstPointer(Rhino.Geometry.GeometryBase geometry)
    {
      if (geometry is Surface)
        return UnsafeNativeMethods.CRhinoObjRef_Surface(m_ptr);
      if (geometry is Curve)
        return UnsafeNativeMethods.CRhinoObjRef_Curve(m_ptr);
      if (geometry is Point)
        return UnsafeNativeMethods.CRhinoObjRef_Point(m_ptr);
      if (geometry is Brep)
        return UnsafeNativeMethods.CRhinoObjRef_Brep(m_ptr);

      return UnsafeNativeMethods.CRhinoObjRef_Geometry(m_ptr);
    }

    private Rhino.Geometry.GeometryBase ObjRefToGeometryHelper(IntPtr pGeometry)
    {
      if (pGeometry == IntPtr.Zero)
        return null;
      object parent;
      if (UnsafeNativeMethods.CRhinoObjRef_IsTopLevelGeometryPointer(m_ptr, pGeometry))
        parent = Object();
      else
        parent = new ObjRef(this); // copy in case user decides to call Dispose on this ObjRef
      return null == parent ? null : Rhino.Geometry.GeometryBase.CreateGeometryHelper(pGeometry, parent);
    }

    /// <summary>
    /// Gets the geometry linked to the object targeted by this reference.
    /// </summary>
    /// <returns>The geometry.</returns>
    public Geometry.GeometryBase Geometry()
    {
      IntPtr pGeometry = UnsafeNativeMethods.CRhinoObjRef_Geometry(m_ptr);
      return ObjRefToGeometryHelper(pGeometry);
    }

    /// <summary>
    /// Gets the clipping plane surface if this reference targeted one.
    /// </summary>
    /// <returns>A clipping plane surface, or null if this reference targeted something else.</returns>
    public Geometry.ClippingPlaneSurface ClippingPlaneSurface()
    {
      IntPtr pClippingPlaneSurface = UnsafeNativeMethods.CRhinoObjRef_ClippingPlaneSurface(m_ptr);
      return ObjRefToGeometryHelper(pClippingPlaneSurface) as Rhino.Geometry.ClippingPlaneSurface;
    }

    /// <example>
    /// <code source='examples\vbnet\ex_intersectcurves.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_intersectcurves.cs' lang='cs'/>
    /// <code source='examples\py\ex_intersectcurves.py' lang='py'/>
    /// </example>
    /// <summary>
    /// Gets the curve if this reference targeted one.
    /// </summary>
    /// <returns>A curve, or null if this reference targeted something else.</returns>
    public Geometry.Curve Curve()
    {
      IntPtr pCurve = UnsafeNativeMethods.CRhinoObjRef_Curve(m_ptr);
      return ObjRefToGeometryHelper(pCurve) as Rhino.Geometry.Curve;
    }

    /// <summary>
    /// Gets the edge if this reference geometry is one.
    /// </summary>
    /// <returns>A boundary representation edge; or null on error.</returns>
    public Geometry.BrepEdge Edge()
    {
      IntPtr pBrepEdge = UnsafeNativeMethods.CRhinoObjRef_Edge(m_ptr);
      return ObjRefToGeometryHelper(pBrepEdge) as BrepEdge;
    }

    /// <summary>
    /// If the referenced geometry is a brep face, a brep with one face, or
    /// a surface, this returns the brep face.
    /// </summary>
    /// <returns>A boundary representation face; or null on error.</returns>
    public Geometry.BrepFace Face()
    {
      IntPtr pBrepFace = UnsafeNativeMethods.CRhinoObjRef_Face(m_ptr);
      return ObjRefToGeometryHelper(pBrepFace) as BrepFace;
    }

    /// <summary>
    ///  Gets the brep if this reference geometry is one.
    /// </summary>
    /// <returns>A boundary representation; or null on error.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_booleandifference.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_booleandifference.cs' lang='cs'/>
    /// <code source='examples\py\ex_booleandifference.py' lang='py'/>
    /// </example>
    public Geometry.Brep Brep()
    {
      IntPtr pBrep = UnsafeNativeMethods.CRhinoObjRef_Brep(m_ptr);
      return ObjRefToGeometryHelper(pBrep) as Brep;
    }

    /// <summary>
    /// Gets the surface if the referenced geometry is one.
    /// </summary>
    /// <returns>A surface; or null if the referenced object is not a surface, or on error.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    public Geometry.Surface Surface()
    {
      IntPtr pSurface = UnsafeNativeMethods.CRhinoObjRef_Surface(m_ptr);
      return ObjRefToGeometryHelper(pSurface) as Surface;
    }

    /// <summary>
    /// Gets the text dot if the referenced geometry is one.
    /// </summary>
    /// <returns>A text dot; or null if the referenced object is not a text dot, or on error.</returns>
    public Geometry.TextDot TextDot()
    {
      IntPtr pTextDot = UnsafeNativeMethods.CRhinoObjRef_TextDot(m_ptr);
      return ObjRefToGeometryHelper(pTextDot) as TextDot;
    }

    /// <summary>
    /// Gets the mesh if the referenced geometry is one.
    /// </summary>
    /// <returns>A mesh; or null if the referenced object is not a mesh, or on error.</returns>
    public Geometry.Mesh Mesh()
    {
      IntPtr pMesh = UnsafeNativeMethods.CRhinoObjRef_Mesh(m_ptr);
      return ObjRefToGeometryHelper(pMesh) as Mesh;
    }

    /// <summary>
    /// Gets the point if the referenced geometry is one.
    /// </summary>
    /// <returns>A point; or null if the referenced object is not a point, or on error.</returns>
    public Geometry.Point Point()
    {
      IntPtr pPoint = UnsafeNativeMethods.CRhinoObjRef_Point(m_ptr);
      return ObjRefToGeometryHelper(pPoint) as Point;
    }

    /// <summary>
    /// Gets the point cloud if the referenced geometry is one.
    /// </summary>
    /// <returns>A point cloud; or null if the referenced object is not a point cloud, or on error.</returns>
    public Geometry.PointCloud PointCloud()
    {
      IntPtr pPointCloud = UnsafeNativeMethods.CRhinoObjRef_PointCloud(m_ptr);
      return ObjRefToGeometryHelper(pPointCloud) as PointCloud;
    }

    /// <summary>
    /// Gets the text entity if the referenced geometry is one.
    /// </summary>
    /// <returns>A text entity; or null if the referenced object is not a text entity, or on error.</returns>
    public Geometry.TextEntity TextEntity()
    {
      IntPtr pTextEntity = UnsafeNativeMethods.CRhinoObjRef_Annotation(m_ptr);
      return ObjRefToGeometryHelper(pTextEntity) as TextEntity;
    }

    /// <summary>
    /// Gets the light if the referenced geometry is one.
    /// </summary>
    /// <returns>A light; or null if the referenced object is not a light, or on error.</returns>
    public Geometry.Light Light()
    {
      IntPtr pLight = UnsafeNativeMethods.CRhinoObjRef_Light(m_ptr);
      return ObjRefToGeometryHelper(pLight) as Light;
    }

    /// <summary>
    /// Gets the hatch if the referenced geometry is one.
    /// </summary>
    /// <returns>A hatch; or null if the referenced object is not a hatch</returns>
    public Geometry.Hatch Hatch()
    {
      return Geometry() as Hatch;
    }
    //private bool IsSubGeometry()
    //{
    //  return UnsafeNativeMethods.CRhinoObjRef_IsSubGeometry(m_ptr);
    //}

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~ObjRef()
    {
      Dispose(false);
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.CRhinoObjRef_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    /// <summary>Returns the referenced Rhino object.</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    public RhinoObject Object()
    {
      IntPtr rc = UnsafeNativeMethods.CRhinoObjRef_Object(m_ptr);
      if (IntPtr.Zero == rc)
        return null;
      return RhinoObject.CreateRhinoObjectHelper(rc);
    }

    /// <summary>
    /// Gets the method used to select this object.
    /// </summary>
    /// <returns>The method used to select this object.</returns>
    public SelectionMethod SelectionMethod()
    {
      int rc = UnsafeNativeMethods.CRhinoObjRef_SelectionMethod(m_ptr);

      switch (rc)
      {
        case 0:
          return Rhino.DocObjects.SelectionMethod.Other;
        case 1:
          return Rhino.DocObjects.SelectionMethod.MousePick;
        case 2:
          return Rhino.DocObjects.SelectionMethod.WindowBox;
        case 3:
          return Rhino.DocObjects.SelectionMethod.CrossingBox;
        default:
          return Rhino.DocObjects.SelectionMethod.Other;
      }
    }

    /// <summary>
    /// If the object was selected by picking a point on it, then
    /// SelectionPoint() returns the point where the selection
    /// occured, otherwise it returns Point3d.Unset.
    /// </summary>
    /// <returns>
    /// The point where the selection occured or Point3d.Unset on failure.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_constrainedcopy.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_constrainedcopy.cs' lang='cs'/>
    /// <code source='examples\py\ex_constrainedcopy.py' lang='py'/>
    /// </example>
    public Point3d SelectionPoint()
    {
      Point3d pt = Point3d.Unset;
      bool rc = UnsafeNativeMethods.CRhinoObjRef_SelectionPoint(m_ptr, ref pt);
      if (rc)
        return pt;

      return Point3d.Unset;
    }

    /// <summary>
    /// If the reference geometry is a curve or edge with a selection
    /// point, then this gets the parameter of the selection point.
    /// </summary>
    /// <param name="parameter">The parameter of the selection point.</param>
    /// <returns>If the selection point was on a curve or edge, then the
    /// curve/edge is returned, otherwise null.</returns>
    /// <remarks>
    /// If a curve was selected and CurveParameter is called and the 
    /// SelectionMethod() is not 1 (point pick on object), the curve will
    /// be returned and parameter will be set to the start parameter of
    /// the picked curve. This can be misleading so it may be necessary
    /// to call SelectionMethod() first, before calling CurveParameter
    /// to get the desired information.</remarks>
    public Curve CurveParameter(out double parameter)
    {
      parameter = 0.0;

      IntPtr pCurve = UnsafeNativeMethods.CRhinoObjRef_CurveParameter(m_ptr, ref parameter);
      return ObjRefToGeometryHelper(pCurve) as Curve;
    }

    /// <summary>
    /// If the reference geometry is a surface, brep with one face,
    /// or surface edge with a selection point, then this gets the 
    /// surface paramters of the selection point.
    /// </summary>
    /// <param name="u">The U value is assigned to this out parameter during the call.</param>
    /// <param name="v">The V value is assigned to this out parameter during the call.</param>
    /// <returns>
    /// If the selection point was on a surface, then the surface is returned.
    /// </returns>
    public Surface SurfaceParameter(out double u, out double v)
    {
      u = 0.0;
      v = 0.0;

      IntPtr pSurface = UnsafeNativeMethods.CRhinoObjRef_SurfaceParameter(m_ptr, ref u, ref v);
      return ObjRefToGeometryHelper(pSurface) as Surface;
    }

    /// <summary>
    /// When an object is selected by picking a sub-object, SetSelectionComponent
    /// may be used to identify the sub-object.
    /// </summary>
    /// <param name="componentIndex"></param>
    public void SetSelectionComponent(ComponentIndex componentIndex)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoObjRef_SetSelectionComponent(pThis, componentIndex);
    }


  }
#endif


  // skipping CRhinoObjRefArray
}

#if RHINO_SDK

namespace Rhino.Runtime
{
  // do not make this class available in the public SDK. It is pretty hairy
  class INTERNAL_RhinoObjectArray : IDisposable
  {
    IntPtr m_ptr; //ON_SimpleArray<CRhinoObject*>*
    readonly System.Collections.Generic.List<DocObjects.RhinoObject> m_rhino_objects;
    //public IntPtr ConstPointer() { return m_ptr; }
    public IntPtr NonConstPointer() { return m_ptr; }

    public INTERNAL_RhinoObjectArray()
    {
      m_ptr = UnsafeNativeMethods.RhinoObjectArray_New(0);
    }
    public INTERNAL_RhinoObjectArray(System.Collections.Generic.IEnumerable<DocObjects.RhinoObject> rhinoObjects)
    {
      m_rhino_objects = new System.Collections.Generic.List<Rhino.DocObjects.RhinoObject>(rhinoObjects);
      int count = m_rhino_objects.Count;
      m_ptr = UnsafeNativeMethods.RhinoObjectArray_New(count);

      for (int i = 0; i < count; i++)
      {
        IntPtr pRhinoObject = m_rhino_objects[i].ConstPointer();
        UnsafeNativeMethods.RhinoObjectArray_Add(m_ptr, pRhinoObject);
      }
    }

    public Rhino.DocObjects.RhinoObject[] ToArray()
    {
      if (null != m_rhino_objects)
        return m_rhino_objects.ToArray();

      int count = UnsafeNativeMethods.RhinoObjectArray_Count(m_ptr);
      Rhino.DocObjects.RhinoObject[] rc = new Rhino.DocObjects.RhinoObject[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr pRhinoObject = UnsafeNativeMethods.RhinoObjectArray_Get(m_ptr, i);
        rc[i] = DocObjects.RhinoObject.CreateRhinoObjectHelper(pRhinoObject);
      }
      return rc;
    }

    public static Rhino.DocObjects.RhinoObject[] ToArrayFromPointer(IntPtr pRhinoObjectArray)
    {
      if (IntPtr.Zero == pRhinoObjectArray)
        return new Rhino.DocObjects.RhinoObject[0];

      int count = UnsafeNativeMethods.RhinoObjectArray_Count(pRhinoObjectArray);
      Rhino.DocObjects.RhinoObject[] rc = new Rhino.DocObjects.RhinoObject[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr pRhinoObject = UnsafeNativeMethods.RhinoObjectArray_Get(pRhinoObjectArray, i);
        rc[i] = DocObjects.RhinoObject.CreateRhinoObjectHelper(pRhinoObject);
      }
      return rc;
    }

    ~INTERNAL_RhinoObjectArray()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.RhinoObjectArray_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
  }

  class INTERNAL_RhinoObjRefArray : System.Collections.Generic.IEnumerable<Rhino.DocObjects.ObjRef>, IDisposable
  {
    IntPtr m_pRhinoObjRefArray; //CRhinoObjRefArray*
    //public IntPtr ConstPointer() { return m_ptr; }
    public IntPtr NonConstPointer() { return m_pRhinoObjRefArray; }

    public INTERNAL_RhinoObjRefArray(IntPtr pRhinoObjRefArray)
    {
      m_pRhinoObjRefArray = pRhinoObjRefArray;
    }

    ~INTERNAL_RhinoObjRefArray()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_pRhinoObjRefArray)
      {
        UnsafeNativeMethods.CRhinoObjRefArray_Delete(m_pRhinoObjRefArray);
        m_pRhinoObjRefArray = IntPtr.Zero;
      }
    }

    System.Collections.Generic.IEnumerator<DocObjects.ObjRef> GetEnumeratorHelper()
    {
      int count = UnsafeNativeMethods.CRhinoObjRefArray_Count(m_pRhinoObjRefArray);
      for (int i = 0; i < count; i++)
      {
        IntPtr pRhinoObjRef = UnsafeNativeMethods.CRhinoObjRefArray_GetItem(m_pRhinoObjRefArray, i);
        if (IntPtr.Zero != pRhinoObjRef)
        {
          yield return new Rhino.DocObjects.ObjRef(pRhinoObjRef);
        }
      }
    }

    public System.Collections.Generic.IEnumerator<DocObjects.ObjRef> GetEnumerator()
    {
      return GetEnumeratorHelper();
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumeratorHelper();
    }
  }
}

#endif