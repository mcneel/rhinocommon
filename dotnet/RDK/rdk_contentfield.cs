using System;


#if RDK_UNCHECKED
namespace Rhino.Render
{
  public abstract class Field : IDisposable
  {
    public enum ChangeContexts : int
    {
      UI = 0, // Change occurred as a result of user activity in the content's UI.
      Drop = 1, // Change occurred as a result of drag and drop.
      Program = 2, // Change occurred as a result of internal program activity.
      Ignore = 3, // Change can be disregarded.
      Tree = 4, // Change occurred within the content tree (e.g., nodes reordered).
      Undo = 5, // Change occurred as a result of an undo.
      FieldInit = 6, // Change occurred as a result of a field initialization.
      Serialize = 7, // Change occurred during serialization (loading).
    }

    protected IntPtr m_pField = IntPtr.Zero;
    private bool m_bAutoDelete = true;

    protected string m_sInternal;
    protected string m_sFriendly;

    internal Field(string internalName, string friendlyName)
    {
      m_sInternal = internalName;
      m_sFriendly = friendlyName;
    }

    ~Field()
    {
      Dispose(false);
    }

    internal virtual void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      m_pField = UnsafeNativeMethods.Rdk_ContentField_New(parentContent.ConstPointer(), m_sInternal, m_sFriendly, isVisibleToAutoUI ? 0 : 0x8001);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
      if (IntPtr.Zero != m_pField)
      {
        if (m_bAutoDelete)
        {
          UnsafeNativeMethods.Rdk_ContentField_Delete(m_pField);
        }
        m_pField = IntPtr.Zero;
      }
    }

    public IntPtr ConstPointer()
    {
      return m_pField;
    }
  }

  public class StringField : Field
  {
    private String m_defaultValue;

    public StringField(string internalName, string friendlyName, string defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetStringValue(m_pField, m_defaultValue, (int)ChangeContexts.FieldInit);
    }

    public String Value
    {
      get
      {
        IntPtr pField = ConstPointer();
        if (pField != IntPtr.Zero)
        {
          using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
          {
            IntPtr pString = sh.NonConstPointer();
            UnsafeNativeMethods.Rdk_ContentField_StringValue(m_pField, pString);
            return sh.ToString();
          }
        }
        return "";
      }
      set
      {
        IntPtr pField = ConstPointer();
        if (pField != IntPtr.Zero)
        {
          UnsafeNativeMethods.Rdk_ContentField_SetStringValue(pField, value, (int)ChangeContexts.Program);
        }

      }
    }
  }




  public class BoolField : Field
  {
    private bool m_defaultValue;

    public BoolField(string internalName, string friendlyName, bool defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetBoolValue(m_pField, m_defaultValue ? 1 : 0, (int)ChangeContexts.FieldInit);
    }

    public bool Value
    {
      get { return 1 == UnsafeNativeMethods.Rdk_ContentField_BoolValue(ConstPointer()); }
      set { UnsafeNativeMethods.Rdk_ContentField_SetBoolValue(ConstPointer(), value ? 1 : 0, (int)ChangeContexts.Program); }
    }
  }


  public class IntField : Field
  {
    private int m_defaultValue;

    public IntField(string internalName, string friendlyName, int defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetIntValue(m_pField, m_defaultValue, (int)ChangeContexts.FieldInit);
    }

    public int Value
    {
      get { return UnsafeNativeMethods.Rdk_ContentField_IntValue(ConstPointer()); }
      set { UnsafeNativeMethods.Rdk_ContentField_SetIntValue(ConstPointer(), value, (int)ChangeContexts.Program); }
    }
  }



  public class DoubleField : Field
  {
    private double m_defaultValue;

    public DoubleField(string internalName, string friendlyName, double defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetDoubleValue(m_pField, m_defaultValue, (int)ChangeContexts.FieldInit);
    }

    public double Value
    {
      get { return UnsafeNativeMethods.Rdk_ContentField_DoubleValue(ConstPointer()); }
      set { UnsafeNativeMethods.Rdk_ContentField_SetDoubleValue(ConstPointer(), value, (int)ChangeContexts.Program); }
    }
  }



  public class FloatField : Field
  {
    private float m_defaultValue;

    public FloatField(string internalName, string friendlyName, float defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetFloatValue(m_pField, m_defaultValue, (int)ChangeContexts.FieldInit);
    }

    public float Value
    {
      get { return UnsafeNativeMethods.Rdk_ContentField_FloatValue(ConstPointer()); }
      set { UnsafeNativeMethods.Rdk_ContentField_SetFloatValue(ConstPointer(), value, (int)ChangeContexts.Program); }
    }
  }



  public class ColorField : Field
  {
    private Rhino.Display.Color4f m_defaultValue;

    public ColorField(string internalName, string friendlyName, Rhino.Display.Color4f defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetColorValue(m_pField, m_defaultValue, (int)ChangeContexts.FieldInit);
    }

    public Rhino.Display.Color4f Value
    {
      get
      {
        Rhino.Display.Color4f c = new Rhino.Display.Color4f();
        UnsafeNativeMethods.Rdk_ContentField_ColorValue(ConstPointer(), ref c);
        return c;
      }
      set
      {
        UnsafeNativeMethods.Rdk_ContentField_SetColorValue(ConstPointer(), value, (int)ChangeContexts.Program);
      }
    }
  }


  public class Vector2dField : Field
  {
    private Rhino.Geometry.Vector2d m_defaultValue;

    public Vector2dField(string internalName, string friendlyName, Rhino.Geometry.Vector2d defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetVector2dValue(m_pField, m_defaultValue, (int)ChangeContexts.FieldInit);
    }

    public Rhino.Geometry.Vector2d Value
    {
      get
      {
        Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
        UnsafeNativeMethods.Rdk_ContentField_Vector2dValue(ConstPointer(), ref v);
        return v;
      }
      set
      {
        UnsafeNativeMethods.Rdk_ContentField_SetVector2dValue(ConstPointer(), value, (int)ChangeContexts.Program);
      }
    }
  }

  public class Vector3dField : Field
  {
    private Rhino.Geometry.Vector3d m_defaultValue;

    public Vector3dField(string internalName, string friendlyName, Rhino.Geometry.Vector3d defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetVector3dValue(m_pField, m_defaultValue, (int)ChangeContexts.FieldInit);
    }

    public Rhino.Geometry.Vector3d Value
    {
      get
      {
        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
        UnsafeNativeMethods.Rdk_ContentField_Vector3dValue(ConstPointer(), ref v);
        return v;
      }
      set
      {
        UnsafeNativeMethods.Rdk_ContentField_SetVector3dValue(ConstPointer(), value, (int)ChangeContexts.Program);
      }
    }
  }

  public class Point4dField : Field
  {
    private Rhino.Geometry.Point4d m_defaultValue;

    public Point4dField(string internalName, string friendlyName, Rhino.Geometry.Point4d defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetPoint4dValue(m_pField, m_defaultValue, (int)ChangeContexts.FieldInit);
    }

    public Rhino.Geometry.Point4d Value
    {
      get
      {
        Rhino.Geometry.Point4d v = new Rhino.Geometry.Point4d();
        UnsafeNativeMethods.Rdk_ContentField_Point4dValue(ConstPointer(), ref v);
        return v;
      }
      set
      {
        UnsafeNativeMethods.Rdk_ContentField_SetPoint4dValue(ConstPointer(), value, (int)ChangeContexts.Program);
      }
    }
  }



  public class GuidField : Field
  {
    private Guid m_defaultValue;

    public GuidField(string internalName, string friendlyName, Guid defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetUUIDValue(m_pField, m_defaultValue, (int)ChangeContexts.FieldInit);
    }

    public Guid Value
    {
      get { return UnsafeNativeMethods.Rdk_ContentField_UUIDValue(ConstPointer()); }
      set { UnsafeNativeMethods.Rdk_ContentField_SetUUIDValue(ConstPointer(), value, (int)ChangeContexts.Program); }
    }
  }



  public class TransformField : Field
  {
    private Rhino.Geometry.Transform m_defaultValue;

    public TransformField(string internalName, string friendlyName, Rhino.Geometry.Transform defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetXformValue(m_pField, m_defaultValue, (int)ChangeContexts.FieldInit);
    }

    public Rhino.Geometry.Transform Value
    {
      get
      {
        Rhino.Geometry.Transform x = new Rhino.Geometry.Transform();
        UnsafeNativeMethods.Rdk_ContentField_XformValue(ConstPointer(), ref x);
        return x;
      }
      set { UnsafeNativeMethods.Rdk_ContentField_SetXformValue(ConstPointer(), value, (int)ChangeContexts.Program); }
    }
  }



  public class DateTimeField : Field
  {
    private DateTime m_defaultValue;

    public DateTimeField(string internalName, string friendlyName, DateTime defaultValue)
      : base(internalName, friendlyName)
    {
      m_defaultValue = defaultValue;
    }

    internal override void CreateCppPointer(RenderContent parentContent, bool isVisibleToAutoUI)
    {
      base.CreateCppPointer(parentContent, isVisibleToAutoUI);
      UnsafeNativeMethods.Rdk_ContentField_SetTimeValue(m_pField, m_defaultValue.Year, m_defaultValue.Month, m_defaultValue.Day, m_defaultValue.Hour, m_defaultValue.Minute, m_defaultValue.Second, (int)ChangeContexts.FieldInit);
    }

    public DateTime Value
    {
      get
      {
        int month = 0, year = 0, day = 0, hours = 0, minutes = 0, seconds = 0;

        UnsafeNativeMethods.Rdk_ContentField_TimeValue(ConstPointer(), ref year, ref month, ref day, ref hours, ref minutes, ref seconds);

        DateTime dt = new DateTime(year, month, day, hours, minutes, seconds);

        return dt;
      }
      set
      {
        UnsafeNativeMethods.Rdk_ContentField_SetTimeValue(ConstPointer(), value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, (int)ChangeContexts.Program);
      }
    }
  }
}


#endif
