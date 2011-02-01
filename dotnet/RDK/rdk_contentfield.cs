using System;
using System.Runtime.InteropServices;

#if USING_RDK
namespace Rhino.Render
{
    public abstract class Field : IDisposable
    {
        public enum eChangeContext : int
        {
            ccUI = 0, // Change occurred as a result of user activity in the content's UI.
            ccDrop = 1, // Change occurred as a result of drag and drop.
            ccProgram = 2, // Change occurred as a result of internal program activity.
            ccIgnore = 3, // Change can be disregarded.
            ccTree = 4, // Change occurred within the content tree (e.g., nodes reordered).
            ccUndo = 5, // Change occurred as a result of an undo.
            ccFieldInit = 6, // Change occurred as a result of a field initialization.
            ccSerialize = 7, // Change occurred during serialization (loading).
        }

        protected IntPtr m_pField = IntPtr.Zero;
        private bool m_bAutoDelete = true;

        protected string m_sInternal;
        protected string m_sFriendly;

        internal Field(string sInternal, string sFriendly)
        {
            //RenderContent.m_fields.Add(this);

            m_sInternal = sInternal;
            m_sFriendly = sFriendly;
        }

        ~Field()
        {
            Dispose(false);
        }

        internal virtual void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            m_pField = UnsafeNativeMethods.Rdk_ContentField_New(parentContent.ConstPointer(), m_sInternal, m_sFriendly, bVisibleToAutoUI ? 0 : 0x8001);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
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
        private String m_default_value;

        public StringField(string sInternal, string sFriendly, string default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetStringValue(m_pField, m_default_value, (int)eChangeContext.ccFieldInit);
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
                    UnsafeNativeMethods.Rdk_ContentField_SetStringValue(pField, value, (int)eChangeContext.ccProgram);
                }

            }
        }
    }




    public class BoolField : Field
    {
        private bool m_default_value;

        public BoolField(string sInternal, string sFriendly, bool default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetBoolValue(m_pField, m_default_value ? 1 : 0, (int)eChangeContext.ccFieldInit);
        }

        public bool Value
        {
            get { return 1 == UnsafeNativeMethods.Rdk_ContentField_BoolValue(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_ContentField_SetBoolValue(ConstPointer(), value ? 1 : 0, (int)eChangeContext.ccProgram); }
        }
    }


    public class IntField : Field
    {
        private int m_default_value;

        public IntField(string sInternal, string sFriendly, int default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetIntValue(m_pField, m_default_value, (int)eChangeContext.ccFieldInit);
        }

        public int Value
        {
            get { return UnsafeNativeMethods.Rdk_ContentField_IntValue(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_ContentField_SetIntValue(ConstPointer(), value, (int)eChangeContext.ccProgram); }
        }
    }



    public class DoubleField : Field
    {
        private double m_default_value;

        public DoubleField(string sInternal, string sFriendly, double default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetDoubleValue(m_pField, m_default_value, (int)eChangeContext.ccFieldInit);
        }

        public double Value
        {
            get { return UnsafeNativeMethods.Rdk_ContentField_DoubleValue(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_ContentField_SetDoubleValue(ConstPointer(), value, (int)eChangeContext.ccProgram); }
        }
    }



    public class FloatField : Field
    {
        private float m_default_value;

        public FloatField(string sInternal, string sFriendly, float default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetFloatValue(m_pField, m_default_value, (int)eChangeContext.ccFieldInit);
        }

        public float Value
        {
            get { return UnsafeNativeMethods.Rdk_ContentField_FloatValue(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_ContentField_SetFloatValue(ConstPointer(), value, (int)eChangeContext.ccProgram); }
        }
    }



    public class ColorField : Field
    {
        private Rhino.Display.Color4f m_default_value;

        public ColorField(string sInternal, string sFriendly, Rhino.Display.Color4f default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetColorValue(m_pField, m_default_value, (int)eChangeContext.ccFieldInit);
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
                UnsafeNativeMethods.Rdk_ContentField_SetColorValue(ConstPointer(), value, (int)eChangeContext.ccProgram); 
            }
        }
    }


    public class Vector2dField : Field
    {
        private Rhino.Geometry.Vector2d m_default_value;

        public Vector2dField(string sInternal, string sFriendly, Rhino.Geometry.Vector2d default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetVector2dValue(m_pField, m_default_value, (int)eChangeContext.ccFieldInit);
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
                UnsafeNativeMethods.Rdk_ContentField_SetVector2dValue(ConstPointer(), value, (int)eChangeContext.ccProgram);
            }
        }
    }

    public class Vector3dField : Field
    {
        private Rhino.Geometry.Vector3d m_default_value;

        public Vector3dField(string sInternal, string sFriendly, Rhino.Geometry.Vector3d default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetVector3dValue(m_pField, m_default_value, (int)eChangeContext.ccFieldInit);
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
                UnsafeNativeMethods.Rdk_ContentField_SetVector3dValue(ConstPointer(), value, (int)eChangeContext.ccProgram);
            }
        }
    }

    public class Point4dField : Field
    {
        private Rhino.Geometry.Point4d m_default_value;

        public Point4dField(string sInternal, string sFriendly, Rhino.Geometry.Point4d default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetPoint4dValue(m_pField, m_default_value, (int)eChangeContext.ccFieldInit);
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
                UnsafeNativeMethods.Rdk_ContentField_SetPoint4dValue(ConstPointer(), value, (int)eChangeContext.ccProgram);
            }
        }
    }



    public class GuidField : Field
    {
        private Guid m_default_value;

        public GuidField(string sInternal, string sFriendly, Guid default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetUUIDValue(m_pField, m_default_value, (int)eChangeContext.ccFieldInit);
        }

        public Guid Value
        {
            get { return UnsafeNativeMethods.Rdk_ContentField_UUIDValue(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_ContentField_SetUUIDValue(ConstPointer(), value, (int)eChangeContext.ccProgram); }
        }
    }



    public class TransformField : Field
    {
        private Rhino.Geometry.Transform m_default_value;

        public TransformField(string sInternal, string sFriendly, Rhino.Geometry.Transform default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetXformValue(m_pField, m_default_value, (int)eChangeContext.ccFieldInit);
        }

        public Rhino.Geometry.Transform Value
        {
            get 
            { 
                Rhino.Geometry.Transform x = new Rhino.Geometry.Transform();
                UnsafeNativeMethods.Rdk_ContentField_XformValue(ConstPointer(), ref x);
                return x;
            }
            set { UnsafeNativeMethods.Rdk_ContentField_SetXformValue(ConstPointer(), value, (int)eChangeContext.ccProgram); }
        }
    }



    public class DateTimeField : Field
    {
        private DateTime m_default_value;

        public DateTimeField(string sInternal, string sFriendly, DateTime default_value)
            : base(sInternal, sFriendly)
        {
            m_default_value = default_value;
        }

        internal override void CreateCppPointer(RenderContent parentContent, bool bVisibleToAutoUI)
        {
            base.CreateCppPointer(parentContent, bVisibleToAutoUI);
            UnsafeNativeMethods.Rdk_ContentField_SetTimeValue(m_pField, m_default_value.Year, m_default_value.Month, m_default_value.Day, m_default_value.Hour, m_default_value.Minute, m_default_value.Second, (int)eChangeContext.ccFieldInit);
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
                UnsafeNativeMethods.Rdk_ContentField_SetTimeValue(ConstPointer(), value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second, (int)eChangeContext.ccProgram);
            }
        }
    }
}


#endif
