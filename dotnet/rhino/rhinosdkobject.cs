using System;
using Rhino.Geometry;

namespace Rhino.DocObjects
{
  /// <summary>
  /// RhinoObjects should only ever be creatable by the RhinoDoc
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

    internal delegate uint CommitGeometryChangesFunc(uint sn, IntPtr pConstGeometry);

    internal IntPtr ConstPointer()
    {
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

    //protected RhinoObject() { }
    internal RhinoObject(uint sn)
    {
      m_rhinoobject_serial_number = sn;
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

    internal static RhinoObject CreateRhinoObjectHelper(IntPtr pRhinoObject)
    {
      if (IntPtr.Zero == pRhinoObject)
        return null;

      uint sn = UnsafeNativeMethods.CRhinoObject_RuntimeSN(pRhinoObject);
      if (sn < 1)
        return null;

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
        default:
          rc = new RhinoObject(sn);
          break;
      }
      return rc;
    }
    #endregion

    #region statics
    /// <summary>
    /// Get the runtime serial number that will be assigned to
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
        return UnsafeNativeMethods.ON_Object_IsValid(ptr);
      }
    }

    /// <summary>
    /// Gets the document that owns this object
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
    /// All rhino objects are composed of geometry and attributes.
    /// Get the underlying geometry for this object
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
          IntPtr pGeometry = UnsafeNativeMethods.CRhinoObject_Geometry(m_rhinoobject_serial_number, ci);
          if (IntPtr.Zero == pGeometry)
            throw new Rhino.Runtime.DocumentCollectedException();

          m_original_geometry = GeometryBase.CreateGeometryHelper(pGeometry, this);
        }
        return m_original_geometry;
      }
      // We can also implement set if it makes sense.
      // One thing we would have to do is make sure that the geometry is of the correct type
      // It might be better to place this type of functionality on the subclass specific
      // geometry getters (CurveObject.CurveGeometry)
    }

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
    }

    /// <summary>
    /// true if the object is deleted. Deleted objects are kept by the document
    /// for undo purposes. Call RhinoDoc.UndeleteObject to undelete an object
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
    /// Determine if an object is a reference object. An object from a work session
    /// reference model is a reference object and cannot be modified. An object is
    /// a reference object if, and only if, it is on a reference layer.
    /// </summary>
    public bool IsReference
    {
      get { return GetBool(idxIsReference); }
    }

    /// <summary>object visibility</summary>
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

    public Rhino.Geometry.GeometryBase DuplicateGeometry()
    {
      if (null != m_edited_geometry)
        return m_edited_geometry.Duplicate();

      GeometryBase g = this.Geometry;
      if (null != g)
        return g.Duplicate();

      return null;
    }

    /// <summary>
    /// Moves changes made to this RhinoObject into the RhinoDoc
    /// </summary>
    /// <returns>
    /// true if changes were made
    /// </returns>
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
    /// Every object has a UUID (universally unique identifier). The default value is Guid.Empty.
    /// When an object is added to a model, the value is checked.  If the value is Guid.Empty, a
    /// new UUID is created. If the value is not NULL but it is already used by another object
    /// in the model, a new UUID is created. If the value is not Guid.Empty and it is not used by
    /// another object in the model, then that value persists. When an object is updated, by
    /// a move for example, the value of ObjectId persists.
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

    /// <summary>number of groups object belongs to</summary>
    public int GroupCount
    {
      get
      {
        return Attributes.GroupCount;
      }
    }

    /// <summary>
    /// Returns an array of GroupCount group indices.  If GroupCount is zero, then GetGroupList() returns null.
    /// </summary>
    /// <returns></returns>
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

    /// <summary>Check selection state</summary>
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

    /// <summary>Check sub-object selection state</summary>
    /// <param name="componentIndex">index of subobject to check</param>
    /// <returns></returns>
    /// <remarks>subobject cannot be persistently selected</remarks>
    public bool IsSubObjectSelected(ComponentIndex componentIndex)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsSubObjectSelected(ptr, componentIndex);
    }

    /// <summary>Get a list of all selected sub-objects</summary>
    /// <returns></returns>
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

    /// <summary>Reports if an object can be selected</summary>
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
    /// <returns>true if object is capable of being selected</returns>
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
    /// <summary>Reports if an object can be selected</summary>
    /// <returns>true if object is capable of being selected</returns>
    /// <remarks>
    /// Objects that are locked, hidden, or on locked or hidden layers
    /// cannot be selected. If IsSelectableWithGripsOn() returns false,
    /// then an that object is not selectable if it has grips turned on.
    /// </remarks>
    public bool IsSelectable()
    {
      return IsSelectable(false, false, false, false);
    }

    /// <summary>Reports if a subobject can be selected</summary>
    /// <param name="componentIndex">index of subobject to check</param>
    /// <param name="ignoreSelectionState">
    /// If true, then selected objects are selectable.
    /// If false, then selected objects are not selectable.
    /// </param>
    /// <returns></returns>
    /// <remarks>
    /// Objects that are locked, hidden, or on locked or hidden layers
    /// cannot be selected. If IsSelectableWithGripsOn() returns false,
    /// then an that object is not selectable if it has grips turned on.
    /// </remarks>
    public bool IsSubObjectSelectable(ComponentIndex componentIndex, bool ignoreSelectionState)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsSubObjectSelectable(ptr, componentIndex, ignoreSelectionState);
    }

    /// <summary>Reports if an object can be selected</summary>
    /// <param name="on"></param>
    /// <param name="syncHighlight">
    /// If true, then the object is highlighted if it is selected 
    /// and unhighlighted if is is not selected.
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
    /// 0: object is not selected
    /// 1: object is selected
    /// 2: object is selected persistently
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

    /// <summary>Reports if an object can be selected</summary>
    /// <param name="on"></param>
    /// <returns>
    /// 0: object is not selected
    /// 1: object is selected
    /// 2: object is selected persistently
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

    /// <summary>Reports if an object can be selected</summary>
    /// <param name="on"></param>
    /// <param name="syncHighlight">
    /// If true, then the object is hightlighted if it is selected
    /// and not hightlighted if is is not selected.
    /// </param>
    /// <returns>
    /// 0: object is not selected
    /// 1: object is selected
    /// 2: object is selected persistently
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

    /// <summary>Reports if an object can be selected</summary>
    /// <param name="componentIndex">index of subobject to check</param>
    /// <param name="select"></param>
    /// <param name="syncHighlight">
    /// (default=true)
    /// If true, then the object is highlighted if it is selected 
    /// and unhighlighted if is is not selected.
    /// </param>
    /// <returns>
    /// 0: object is not selected
    /// 1: object is selected
    /// 2: object is selected persistently
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
    /// Returns number of unselected subobjects
    /// </summary>
    /// <returns></returns>
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

    /// <summary>Check highlight state</summary>
    /// <param name="checkSubObjects">
    /// If true and the entire object is not highlighted, and some subset of the object
    /// is highlighted, like some edges of a surface, then 3 is returned.
    /// If false and the entire object is not highlighted, then zero is returned.
    /// </param>
    /// <returns>
    /// 0 object is not highlighted
    /// 1 entire object is highlighted
    /// 3 one or more proper sub-objects are highlighted
    /// </returns>
    public int IsHighlighted(bool checkSubObjects)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsHighlighted(ptr, checkSubObjects);
    }

    public bool Highlight(bool enable)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_Highlight(ptr, enable);
    }

    public bool IsSubObjectHighlighted(ComponentIndex componentIndex)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_IsSubObjectHighlighted(ptr, componentIndex);
    }

    /// <summary>
    /// Get a list of all highlighted sub-objects
    /// </summary>
    /// <returns></returns>
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

    public bool HighlightSubObject(ComponentIndex componentIndex, bool highlight)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoObject_HighlightSubObject(ptr, componentIndex, highlight);
    }

    /// <summary>
    /// Returns number of changed subobjects
    /// </summary>
    /// <returns></returns>
    public int UnhighlightAllSubObjects()
    {
      // 20 Jan 2010 - S. Baer
      // See my comments in UnselectAllSubObjects. The same goes for UnhighlightAllSubObjects
      return GetInt(idxUnhighightAllSubObjects);
    }

    //[skipping]
    //HighlightRequiresRedraw, IsMarked, Mark

    /// <summary>
    /// State of object's default editing grips
    /// </summary>
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
    /// True if grips are turned on and at least one is selected
    /// </summary>
    public bool GripsSelected
    {
      get
      {
        return GetBool(idxGripsSelected);
      }
    }

    /// <summary>
    /// Returns grips for this object IF grips are enabled. If grips are not
    /// enabled, returns null.
    /// </summary>
    /// <returns></returns>
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
    /// Localized short description os an object
    /// </summary>
    /// <param name="plural"></param>
    /// <returns></returns>
    public string ShortDescription(bool plural)
    {
      using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoObject_ShortDescription(pConstThis, pString, plural);
        return sh.ToString();
      }
    }

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

#if USING_RDK
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
    public Rhino.Render.ObjectDecals Decals
    {
      get
      {
        Rhino.Render.ObjectDecals decals = new Rhino.Render.ObjectDecals(this);
        return decals;
      }
    }
#endif
  }

  /// <summary>
  /// Enumerates different kinds of selection methods.
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

  // all ObjRef's are created in .NET
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

    public ObjRef(Guid id)
    {
      m_ptr = UnsafeNativeMethods.CRhinoObjRef_New1(id);
    }

    public ObjRef(DocObjects.RhinoObject rhinoObject)
    {
      IntPtr pObject = rhinoObject.ConstPointer();
      m_ptr = UnsafeNativeMethods.CRhinoObjRef_New2(pObject);
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
        parent = this.Object();
      else
        parent = new ObjRef(this); // copy in case user decides to call Dispose on this ObjRef
      return null == parent ? null : Rhino.Geometry.GeometryBase.CreateGeometryHelper(pGeometry, parent);
    }

    public Geometry.GeometryBase Geometry()
    {
      IntPtr pGeometry = UnsafeNativeMethods.CRhinoObjRef_Geometry(m_ptr);
      return ObjRefToGeometryHelper(pGeometry);
    }

    public Geometry.ClippingPlaneSurface ClippingPlaneSurface()
    {
      IntPtr pClippingPlaneSurface = UnsafeNativeMethods.CRhinoObjRef_ClippingPlaneSurface(m_ptr);
      return ObjRefToGeometryHelper(pClippingPlaneSurface) as Rhino.Geometry.ClippingPlaneSurface;
    }

    public Geometry.Curve Curve()
    {
      IntPtr pCurve = UnsafeNativeMethods.CRhinoObjRef_Curve(m_ptr);
      return ObjRefToGeometryHelper(pCurve) as Rhino.Geometry.Curve;
    }

    public Geometry.BrepFace Face()
    {
      IntPtr pBrepFace = UnsafeNativeMethods.CRhinoObjRef_Face(m_ptr);
      return ObjRefToGeometryHelper(pBrepFace) as BrepFace;
    }

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

    public Geometry.TextDot TextDot()
    {
      IntPtr pTextDot = UnsafeNativeMethods.CRhinoObjRef_TextDot(m_ptr);
      return ObjRefToGeometryHelper(pTextDot) as TextDot;
    }
    public Geometry.Mesh Mesh()
    {
      IntPtr pMesh = UnsafeNativeMethods.CRhinoObjRef_Mesh(m_ptr);
      return ObjRefToGeometryHelper(pMesh) as Mesh;
    }
    public Geometry.Point Point()
    {
      IntPtr pPoint = UnsafeNativeMethods.CRhinoObjRef_Point(m_ptr);
      return ObjRefToGeometryHelper(pPoint) as Point;
    }
    public Geometry.PointCloud PointCloud()
    {
      IntPtr pPointCloud = UnsafeNativeMethods.CRhinoObjRef_PointCloud(m_ptr);
      return ObjRefToGeometryHelper(pPointCloud) as PointCloud;
    }
    public Geometry.TextEntity TextEntity()
    {
      IntPtr pTextEntity = UnsafeNativeMethods.CRhinoObjRef_Annotation(m_ptr);
      return ObjRefToGeometryHelper(pTextEntity) as TextEntity;
    }
    public Geometry.Light Light()
    {
      IntPtr pLight = UnsafeNativeMethods.CRhinoObjRef_Light(m_ptr);
      return ObjRefToGeometryHelper(pLight) as Light;
    }

    private bool IsSubGeometry()
    {
      return UnsafeNativeMethods.CRhinoObjRef_IsSubGeometry(m_ptr);
    }

    ~ObjRef()
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
    /// Get the method used to select this object.
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
    /// <returns>The point where the selection occured or Point3d.Unset
    /// on failure.</returns>
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
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <returns>
    /// If the selection point was on a surface, the the surface is returned.
    /// </returns>
    public Surface SurfaceParameter(out double u, out double v)
    {
      u = 0.0;
      v = 0.0;

      IntPtr pSurface = UnsafeNativeMethods.CRhinoObjRef_SurfaceParameter(m_ptr, ref u, ref v);
      return ObjRefToGeometryHelper(pSurface) as Surface;
    }
  }



  // skipping CRhinoObjRefArray
}

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
}
