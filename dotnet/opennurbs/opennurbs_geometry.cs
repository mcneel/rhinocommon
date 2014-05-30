using System;
using System.Runtime.Serialization;
using Rhino.DocObjects;

namespace Rhino.Geometry
{
  /// <summary>
  /// Provides a common base for most geometric classes. This class is abstract.
  /// </summary>
  //[Serializable]
  public abstract class GeometryBase : Runtime.CommonObject
  {
    #region constructors / wrapped pointer manipulation
    GeometryBase m_shallow_parent;

    // make internal so outside DLLs can't directly subclass GeometryBase
    internal GeometryBase() { }

    ///// <summary>
    ///// Protected constructor for internal use.
    ///// </summary>
    ///// <param name="info">Serialization data.</param>
    ///// <param name="context">Serialization stream.</param>
    //protected GeometryBase(SerializationInfo info, StreamingContext context)
    //  :base(info, context)
    //{
    //}

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = true;
      IntPtr pConstPointer = ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
      return rc;
    }

    internal override IntPtr _InternalGetConstPointer()
    {
      if (null != m_shallow_parent)
        return m_shallow_parent.ConstPointer();

#if RHINO_SDK
      Rhino.DocObjects.ObjRef obj_ref = m__parent as Rhino.DocObjects.ObjRef;
      if (null != obj_ref)
        return obj_ref.GetGeometryConstPointer(this);

      Rhino.DocObjects.RhinoObject parent_object = ParentRhinoObject();
      if (parent_object == null)
      {
        Rhino.FileIO.File3dmObject fileobject = m__parent as Rhino.FileIO.File3dmObject;
        if (null != fileobject)
          return fileobject.GetGeometryConstPointer();
      }

      uint serial_number = 0;
      IntPtr pParentRhinoObject = IntPtr.Zero;
      if (null != parent_object)
      {
        serial_number = parent_object.m_rhinoobject_serial_number;
        pParentRhinoObject = parent_object.m_pRhinoObject;
      }
      ComponentIndex ci = new ComponentIndex();
      // There are a few cases (like in ReplaceObject callback) where the parent
      // rhino object temporarily holds onto the CRhinoObject* because the object
      // is not officially in the document yet.
      if (pParentRhinoObject != IntPtr.Zero)
        return UnsafeNativeMethods.CRhinoObject_Geometry(pParentRhinoObject, ci);
      return UnsafeNativeMethods.CRhinoObject_Geometry2(serial_number, ci);
#else
      Rhino.FileIO.File3dmObject fileobject = m__parent as Rhino.FileIO.File3dmObject;
      if (null != fileobject)
        return fileobject.GetGeometryConstPointer();
      return IntPtr.Zero;
#endif
    }

    internal override object _GetConstObjectParent()
    {
      if (!IsDocumentControlled)
        return null;
      if (null != m_shallow_parent)
        return m_shallow_parent;
      return base._GetConstObjectParent();
    }

    /// <summary>
    /// Is called when a non-const operation occurs.
    /// </summary>
    protected override void OnSwitchToNonConst()
    {
      m_shallow_parent = null;
      base.OnSwitchToNonConst();
    }

    /// <summary>
    /// If true this object may not be modified. Any properties or functions that attempt
    /// to modify this object when it is set to "IsReadOnly" will throw a NotSupportedException.
    /// </summary>
    public sealed override bool IsDocumentControlled
    {
      get
      {
        if (null != m_shallow_parent)
          return m_shallow_parent.IsDocumentControlled;
        return base.IsDocumentControlled;
      }
    }

    /// <summary>
    /// Constructs a light copy of this object. By "light", it is meant that the same
    /// underlying data is used until something is done to attempt to change it. For example,
    /// you could have a shallow copy of a very heavy mesh object and the same underlying
    /// data will be used when doing things like inspecting the number of faces on the mesh.
    /// If you modify the location of one of the mesh vertices, the shallow copy will create
    /// a full duplicate of the underlying mesh data and the shallow copy will become a
    /// deep copy.
    /// </summary>
    /// <returns>An object of the same type as this object.
    /// <para>This behavior is overridden by implementing classes.</para></returns>
    public GeometryBase DuplicateShallow()
    {
      GeometryBase rc = DuplicateShallowHelper();
      if (null != rc)
        rc.m_shallow_parent = this;
      return rc;
    }
    internal virtual GeometryBase DuplicateShallowHelper()
    {
      return null;
    }

    /// <summary>
    /// Constructs a deep (full) copy of this object.
    /// </summary>
    /// <returns>An object of the same type as this, with the same properties and behavior.</returns>
    public virtual GeometryBase Duplicate()
    {
      IntPtr ptr = ConstPointer();
      IntPtr pNewGeometry = UnsafeNativeMethods.ON_Object_Duplicate(ptr);
      return CreateGeometryHelper(pNewGeometry, null);
    }


    internal GeometryBase(IntPtr ptr, object parent, int subobject_index)
    {
      if (subobject_index >= 0 && parent == null)
      {
        throw new ArgumentException();
      }

      if (null == parent)
        ConstructNonConstObject(ptr);
      else
        ConstructConstObject(parent, subobject_index);
    }

    #region Object type codes
    internal const int idxON_Geometry = 0;
    internal const int idxON_Curve = 1;
    internal const int idxON_NurbsCurve = 2;
    internal const int idxON_PolyCurve = 3;
    internal const int idxON_PolylineCurve = 4;
    internal const int idxON_ArcCurve = 5;
    internal const int idxON_LineCurve = 6;
    const int idxON_Mesh = 7;
    const int idxON_Point = 8;
    const int idxON_TextDot = 9;
    const int idxON_Surface = 10;
    const int idxON_Brep = 11;
    const int idxON_NurbsSurface = 12;
    const int idxON_RevSurface = 13;
    const int idxON_PlaneSurface = 14;
    const int idxON_ClippingPlaneSurface = 15;
    const int idxON_Annotation2 = 16;
    const int idxON_Hatch = 17;
    const int idxON_TextEntity2 = 18;
    const int idxON_SumSurface = 19;
    const int idxON_BrepFace = 20;
    const int idxON_BrepEdge = 21;
    const int idxON_InstanceDefinition = 22;
    const int idxON_InstanceReference = 23;
    const int idxON_Extrusion = 24;
    const int idxON_LinearDimension2 = 25;
    const int idxON_PointCloud = 26;
    const int idxON_DetailView = 27;
    const int idxON_AngularDimension2 = 28;
    const int idxON_RadialDimension2 = 29;
    const int idxON_Leader = 30;
    const int idxON_OrdinateDimension2 = 31;
    const int idxON_Light = 32;
    const int idxON_PointGrid = 33;
    const int idxON_MorphControl = 34;
    const int idxON_BrepLoop = 35;
    const int idxON_BrepTrim = 36;
    #endregion

    internal static GeometryBase CreateGeometryHelper(IntPtr pGeometry, object parent)
    {
      return CreateGeometryHelper(pGeometry, parent, -1);
    }

    internal static GeometryBase CreateGeometryHelper(IntPtr pGeometry, object parent, int subobject_index)
    {
      if (IntPtr.Zero == pGeometry)
        return null;

      int type = UnsafeNativeMethods.ON_Geometry_GetGeometryType(pGeometry);
      if (type < 0)
        return null;
      GeometryBase rc = null;

      switch (type)
      {
        case idxON_Curve: //1
          rc = new Curve(pGeometry, parent, subobject_index);
          break;
        case idxON_NurbsCurve: //2
          rc = new NurbsCurve(pGeometry, parent, subobject_index);
          break;
        case idxON_PolyCurve: // 3
          rc = new PolyCurve(pGeometry, parent, subobject_index);
          break;
        case idxON_PolylineCurve: //4
          rc = new PolylineCurve(pGeometry, parent, subobject_index);
          break;
        case idxON_ArcCurve: //5
          rc = new ArcCurve(pGeometry, parent, subobject_index);
          break;
        case idxON_LineCurve: //6
          rc = new LineCurve(pGeometry, parent, subobject_index);
          break;
        case idxON_Mesh: //7
          rc = new Mesh(pGeometry, parent);
          break;
        case idxON_Point: //8
          rc = new Point(pGeometry, parent);
          break;
        case idxON_TextDot: //9
          rc = new TextDot(pGeometry, parent);
          break;
        case idxON_Surface: //10
          rc = new Surface(pGeometry, parent);
          break;
        case idxON_Brep: //11
          rc = new Brep(pGeometry, parent);
          break;
        case idxON_NurbsSurface: //12
          rc = new NurbsSurface(pGeometry, parent);
          break;
        case idxON_RevSurface: //13
          rc = new RevSurface(pGeometry, parent);
          break;
        case idxON_PlaneSurface: //14
          rc = new PlaneSurface(pGeometry, parent);
          break;
        case idxON_ClippingPlaneSurface: //15
          rc = new ClippingPlaneSurface(pGeometry, parent);
          break;
        case idxON_Annotation2: // 16
          rc = new AnnotationBase(pGeometry, parent);
          break;
        case idxON_Hatch: // 17
          rc = new Hatch(pGeometry, parent);
          break;
        case idxON_TextEntity2: //18
          rc = new TextEntity(pGeometry, parent);
          break;
        case idxON_SumSurface: //19
          rc = new SumSurface(pGeometry, parent);
          break;
        case idxON_BrepFace: //20
          {
            int faceindex = -1;
            IntPtr pBrep = UnsafeNativeMethods.ON_BrepSubItem_Brep(pGeometry, ref faceindex);
            if (pBrep != IntPtr.Zero && faceindex >= 0)
            {
              Brep b = new Brep(pBrep, parent);
              rc = b.Faces[faceindex];
            }
          }
          break;
        case idxON_BrepEdge: // 21
          {
            int edgeindex = -1;
            IntPtr pBrep = UnsafeNativeMethods.ON_BrepSubItem_Brep(pGeometry, ref edgeindex);
            if (pBrep != IntPtr.Zero && edgeindex >= 0)
            {
              Brep b = new Brep(pBrep, parent);
              rc = b.Edges[edgeindex];
            }
          }
          break;
        case idxON_InstanceDefinition: // 22
          rc = new InstanceDefinitionGeometry(pGeometry, parent);
          break;
        case idxON_InstanceReference: // 23
          rc = new InstanceReferenceGeometry(pGeometry, parent);
          break;
        case idxON_Extrusion: //24
          rc = new Extrusion(pGeometry, parent);
          break;
        case idxON_LinearDimension2: //25
          rc = new LinearDimension(pGeometry, parent);
          break;
        case idxON_PointCloud: // 26
          rc = new PointCloud(pGeometry, parent);
          break;
        case idxON_DetailView: // 27
          rc = new DetailView(pGeometry, parent);
          break;
        case idxON_AngularDimension2: // 28
          rc = new AngularDimension(pGeometry, parent);
          break;
        case idxON_RadialDimension2: // 29
          rc = new RadialDimension(pGeometry, parent);
          break;
        case idxON_Leader: // 30
          rc = new Leader(pGeometry, parent);
          break;
        case idxON_OrdinateDimension2: // 31
          rc = new OrdinateDimension(pGeometry, parent);
          break;
        case idxON_Light: //32
          rc = new Light(pGeometry, parent);
          break;
        case idxON_PointGrid: //33
          rc = new Point3dGrid(pGeometry, parent);
          break;
        case idxON_MorphControl: //34
          rc = new MorphControl(pGeometry, parent);
          break;
        case idxON_BrepLoop: //35
          {
            int loopindex = -1;
            IntPtr pBrep = UnsafeNativeMethods.ON_BrepSubItem_Brep(pGeometry, ref loopindex);
            if (pBrep != IntPtr.Zero && loopindex >= 0)
            {
              Brep b = new Brep(pBrep, parent);
              rc = b.Loops[loopindex];
            }
          }
          break;
        case idxON_BrepTrim: // 36
          {
            int trimindex = -1;
            IntPtr pBrep = UnsafeNativeMethods.ON_BrepSubItem_Brep(pGeometry, ref trimindex);
            if (pBrep != IntPtr.Zero && trimindex >= 0)
            {
              Brep b = new Brep(pBrep, parent);
              rc = b.Trims[trimindex];
            }
          }
          break;
        default:
          rc = new UnknownGeometry(pGeometry, parent, subobject_index);
          break;
      }

      return rc;
    }

    #endregion

    /// <summary>
    /// Useful for switch statements that need to differentiate between
    /// basic object types like points, curves, surfaces, and so on.
    /// </summary>
    [CLSCompliant(false)]
    public ObjectType ObjectType
    {
      get
      {
        IntPtr ptr = ConstPointer();
        uint rc = UnsafeNativeMethods.ON_Object_ObjectType(ptr);
        return (ObjectType)rc;
      }
    }

    #region Transforms
    /// <summary>
    /// Transforms the geometry. If the input Transform has a SimilarityType of
    /// OrientationReversing, you may want to consider flipping the transformed
    /// geometry after calling this function when it makes sense. For example,
    /// you may want to call Flip() on a Brep after transforming it.
    /// </summary>
    /// <param name="xform">
    /// Transformation to apply to geometry.
    /// </param>
    /// <returns>true if geometry successfully transformed.</returns>
    public bool Transform(Transform xform)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_Geometry_Transform(ptr, ref xform);
    }

    /// <summary>Translates the object along the specified vector.</summary>
    /// <param name="translationVector">A moving vector.</param>
    /// <returns>true if geometry successfully translated.</returns>
    public bool Translate(Vector3d translationVector)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_Geometry_Translate(ptr, translationVector);
    }

    /// <summary>Translates the object along the specified vector.</summary>
    /// <param name="x">The X component.</param>
    /// <param name="y">The Y component.</param>
    /// <param name="z">The Z component.</param>
    /// <returns>true if geometry successfully translated.</returns>
    public bool Translate(double x, double y, double z)
    {
      Vector3d t = new Vector3d(x, y, z);
      return Translate(t);
    }

    /// <summary>
    /// Scales the object by the specified factor. The scale is centered at the origin.
    /// </summary>
    /// <param name="scaleFactor">The uniform scaling factor.</param>
    /// <returns>true if geometry successfully scaled.</returns>
    public bool Scale(double scaleFactor)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_Geometry_Scale(ptr, scaleFactor);
    }

    /// <summary>
    /// Rotates the object about the specified axis. A positive rotation 
    /// angle results in a counter-clockwise rotation about the axis (right hand rule).
    /// </summary>
    /// <param name="angleRadians">Angle of rotation in radians.</param>
    /// <param name="rotationAxis">Direction of the axis of rotation.</param>
    /// <param name="rotationCenter">Point on the axis of rotation.</param>
    /// <returns>true if geometry successfully rotated.</returns>
    public bool Rotate(double angleRadians, Vector3d rotationAxis, Point3d rotationCenter)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_Geometry_Rotate(ptr, angleRadians, rotationAxis, rotationCenter);
    }
    #endregion

    /// <summary>
    /// Computes an estimate of the number of bytes that this object is using in memory.
    /// </summary>
    /// <returns>An estimated memory footprint.</returns>
    [CLSCompliant(false)]
    public uint MemoryEstimate()
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Object_SizeOf(pConstThis);
    }

    /// <summary>
    /// Boundingbox solver. Gets the world axis aligned boundingbox for the geometry.
    /// </summary>
    /// <param name="accurate">If true, a physically accurate boundingbox will be computed. 
    /// If not, a boundingbox estimate will be computed. For some geometry types there is no 
    /// difference between the estimate and the accurate boundingbox. Estimated boundingboxes 
    /// can be computed much (much) faster than accurate (or "tight") bounding boxes. 
    /// Estimated bounding boxes are always similar to or larger than accurate bounding boxes.</param>
    /// <returns>
    /// The boundingbox of the geometry in world coordinates or BoundingBox.Empty 
    /// if not bounding box could be found.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_curveboundingbox.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_curveboundingbox.cs' lang='cs'/>
    /// <code source='examples\py\ex_curveboundingbox.py' lang='py'/>
    /// </example>
    public BoundingBox GetBoundingBox(bool accurate)
    {
#if RHINO_SDK
      Rhino.DocObjects.RhinoObject parent_object = ParentRhinoObject();
#endif
      if (accurate)
      {
        BoundingBox bbox = new BoundingBox();
        Transform xf = new Transform();
#if RHINO_SDK
        if (null != parent_object)
        {
          IntPtr pParentObject = parent_object.ConstPointer();
          if (UnsafeNativeMethods.CRhinoObject_GetTightBoundingBox(pParentObject, ref bbox, ref xf, false))
            return bbox;
        }
#endif
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Geometry_GetTightBoundingBox(ptr, ref bbox, ref xf, false) ? bbox : BoundingBox.Empty;
      }
      else
      {
        BoundingBox rc = new BoundingBox();
#if RHINO_SDK
        if (null != parent_object)
        {
          IntPtr pParentObject = parent_object.ConstPointer();
          if (UnsafeNativeMethods.CRhinoObject_BoundingBox(pParentObject, ref rc))
            return rc;
        }
#endif
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.ON_Geometry_BoundingBox(ptr, ref rc);
        return rc;
      }
    }
    /// <summary>
    /// Aligned Boundingbox solver. Gets the world axis aligned boundingbox for the transformed geometry.
    /// </summary>
    /// <param name="xform">Transformation to apply to object prior to the BoundingBox computation. 
    /// The geometry itself is not modified.</param>
    /// <returns>The accurate boundingbox of the transformed geometry in world coordinates 
    /// or BoundingBox.Empty if not bounding box could be found.</returns>
    public BoundingBox GetBoundingBox(Transform xform)
    {
      BoundingBox bbox = BoundingBox.Empty;

#if RHINO_SDK
      // In cases like breps and curves, the CRhinoBrepObject and CRhinoCurveObject
      // can compute a better tight bounding box
      Rhino.DocObjects.RhinoObject parent_object = ParentRhinoObject();
      if (parent_object != null)
      {
        IntPtr pParent = parent_object.ConstPointer();
        if (UnsafeNativeMethods.CRhinoObject_GetTightBoundingBox(pParent, ref bbox, ref xform, true))
          return bbox;
      }
#endif
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Geometry_GetTightBoundingBox(ptr, ref bbox, ref xform, true) ? bbox : BoundingBox.Empty;
    }
    /// <summary>
    /// Aligned Boundingbox solver. Gets the plane aligned boundingbox.
    /// </summary>
    /// <param name="plane">Orientation plane for BoundingBox.</param>
    /// <returns>A BoundingBox in plane coordinates.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_curveboundingbox.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_curveboundingbox.cs' lang='cs'/>
    /// <code source='examples\py\ex_curveboundingbox.py' lang='py'/>
    /// </example>
    public BoundingBox GetBoundingBox(Plane plane)
    {
      if (!plane.IsValid) { return BoundingBox.Unset; }

      Transform xform = Geometry.Transform.ChangeBasis(Plane.WorldXY, plane);
      BoundingBox rc = GetBoundingBox(xform);
      return rc;
    }
    /// <summary>
    /// Aligned Boundingbox solver. Gets the plane aligned boundingbox.
    /// </summary>
    /// <param name="plane">Orientation plane for BoundingBox.</param>
    /// <param name="worldBox">Aligned box in World coordinates.</param>
    /// <returns>A BoundingBox in plane coordinates.</returns>
    public BoundingBox GetBoundingBox(Plane plane, out Box worldBox)
    {
      worldBox = Box.Unset;

      if (!plane.IsValid) { return BoundingBox.Unset; }

      Transform xform = Geometry.Transform.ChangeBasis(Plane.WorldXY, plane);
      BoundingBox rc = GetBoundingBox(xform);

      //Transform unxform;
      //xform.TryGetInverse(out unxform);

      //worldBox = new Box(rc);
      //worldBox.Transform(unxform);

      worldBox = new Box(plane, rc);
      return rc;
    }

    #region GetBool constants
    const int idxIsDeformable = 0;
    const int idxMakeDeformable = 1;
    internal const int idxIsMorphable = 2;
    const int idxHasBrepForm = 3;
    #endregion

    /// <summary>
    /// true if object can be accurately modified with "squishy" transformations like
    /// projections, shears, and non-uniform scaling.
    /// </summary>
    public bool IsDeformable
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Geometry_GetBool(ptr, idxIsDeformable);
      }
    }

    /// <summary>
    /// If possible, converts the object into a form that can be accurately modified
    /// with "squishy" transformations like projections, shears, an non-uniform scaling.
    /// </summary>
    /// <returns>
    /// false if object cannot be converted to a deformable object. true if object was
    /// already deformable or was converted into a deformable object.
    /// </returns>
    public bool MakeDeformable()
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_Geometry_GetBool(ptr, idxMakeDeformable);
    }

    // [skipping] BOOL SwapCoordinates( int i, int j );

    // Not exposed here
    // virtual bool Morph( const ON_SpaceMorph& morph );
    // virtual bool IsMorphable() const;
    // Moved to SpaceMorph class

    /// <summary>
    /// Returns true if the Brep.TryConvertBrep function will be successful for this object
    /// </summary>
    public bool HasBrepForm
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Geometry_GetBool(ptr, idxHasBrepForm);
      }
    }

    // Not exposed here
    // ON_Brep* BrepForm( ON_Brep* brep = NULL ) const;
    // Implemented in static Brep.TryConvertBrep function

    /// <summary>
    /// If this piece of geometry is a component in something larger, like a BrepEdge
    /// in a Brep, then this function returns the component index.
    /// </summary>
    /// <returns>
    /// This object's component index.  If this object is not a sub-piece of a larger
    /// geometric entity, then the returned index has 
    /// m_type = ComponentIndex.InvalidType
    /// and m_index = -1.
    /// </returns>
    public ComponentIndex ComponentIndex()
    {
      ComponentIndex ci = new ComponentIndex();
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.ON_Geometry_ComponentIndex(ptr, ref ci);
      return ci;
    }

    // [skipping]
    // bool EvaluatePoint( const class ON_ObjRef& objref, ON_3dPoint& P ) const;

    #region user strings
    /// <summary>
    /// Attach a user string (key,value combination) to this geometry.
    /// </summary>
    /// <param name="key">id used to retrieve this string.</param>
    /// <param name="value">string associated with key.</param>
    /// <returns>true on success.</returns>
    public bool SetUserString(string key, string value)
    {
      return _SetUserString(key, value);
    }
    /// <summary>
    /// Gets user string from this geometry.
    /// </summary>
    /// <param name="key">id used to retrieve the string.</param>
    /// <returns>string associated with the key if successful. null if no key was found.</returns>
    public string GetUserString(string key)
    {
      return _GetUserString(key);
    }

    /// <summary>
    /// Gets the amount of user strings.
    /// </summary>
    public int UserStringCount
    {
      get
      {
        return _UserStringCount;
      }
    }

    ///// <summary>
    ///// Gets a copy of all (user key string, user value string) pairs attached to this geometry.
    ///// </summary>
    ///// <returns>A new collection.</returns>
    //public System.Collections.Specialized.NameValueCollection GetUserStrings()
    //{
    //  return _GetUserStrings();
    //}
    #endregion
  }

  // DO NOT make public
  class UnknownGeometry : GeometryBase
  {
    public UnknownGeometry(IntPtr ptr, object parent, int subobject_index)
      : base(ptr, parent, subobject_index)
    {
    }
  }
}
