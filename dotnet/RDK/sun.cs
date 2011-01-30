using System;
using System.Runtime.InteropServices;

#if USING_RDK
namespace Rhino.Render
{
    public class Sun : IDisposable
    {
        private IntPtr m_pSun = IntPtr.Zero;
        private Rhino.RhinoDoc m_doc;

        public Sun()
        {
            m_pSun = UnsafeNativeMethods.Rdk_SunNew();
        }

        internal Sun(Rhino.RhinoDoc doc)
        {
            m_doc = doc;
        }

        public bool Enabled
        {
            get { return UnsafeNativeMethods.Rdk_Sun_Enabled(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetEnabled(NonConstPointer(), value); }
        }

        public bool EnableAllowed
        {
            get { return UnsafeNativeMethods.Rdk_Sun_EnableAllowed(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetEnableAllowed(NonConstPointer(), value); }
        }

        public bool ManualControlAllowed
        {
            get { return UnsafeNativeMethods.Rdk_Sun_ManualControlAllowed(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetManualControlAllowed(NonConstPointer(), value); }
        }

        public bool ManualControlOn
        {
            get { return UnsafeNativeMethods.Rdk_Sun_ManualControlOn(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetManualControlOn(NonConstPointer(), value); }
        }

        public double North
        {
            get { return UnsafeNativeMethods.Rdk_Sun_North(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetNorth(NonConstPointer(), value); }
        }

        public Rhino.Geometry.Vector3d Vector
        {
            get 
            {
                Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
                UnsafeNativeMethods.Rdk_Sun_Vector(ConstPointer(), ref v);
                return v;
            }
            set { UnsafeNativeMethods.Rdk_Sun_SetVector(NonConstPointer(), value); }
        }

        public double Azimuth
        {
            get { return UnsafeNativeMethods.Rdk_Sun_Azimuth(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetAzimuth(NonConstPointer(), value); }
        }

        public double Altitude
        {
            get { return UnsafeNativeMethods.Rdk_Sun_Altitude(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetAltitude(NonConstPointer(), value); }
        }

        public double Latitude
        {
            get { return UnsafeNativeMethods.Rdk_Sun_Latitude(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetLatitude(NonConstPointer(), value); }
        }

        public double Longitude
        {
            get { return UnsafeNativeMethods.Rdk_Sun_Longitude(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetLongitude(NonConstPointer(), value); }
        }

        public double TimeZone
        {
            get { return UnsafeNativeMethods.Rdk_Sun_TimeZone(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetTimeZone(NonConstPointer(), value); }
        }

        public bool DaylightSavingOn
        {
            get { return UnsafeNativeMethods.Rdk_Sun_DaylightSavingOn(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetDaylightSavingOn(NonConstPointer(), value); }
        }

        public int DaylightSavingMinutes
        {
            get { return UnsafeNativeMethods.Rdk_Sun_DaylightSavingMinutes(ConstPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetDaylightSavingMinutes(NonConstPointer(), value); }
        }

        public DateTime LocalDateTime
        {
            get
            {
                int month = 0, year = 0, day = 0, hours = 0, minutes = 0, seconds = 0;

                UnsafeNativeMethods.Rdk_Sun_LocalDateTime(ConstPointer(), ref year, ref month, ref day, ref hours, ref minutes, ref seconds);

                DateTime dt = new DateTime(year, month, day, hours, minutes, seconds);

                return dt;
            }
            set 
            {
                UnsafeNativeMethods.Rdk_Sun_SetLocalDateTime(NonConstPointer(), value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
            }
        }

        public DateTime UTCDateTime
        {
            get
            {
                int month = 0, year = 0, day = 0, hours = 0, minutes = 0, seconds = 0;

                UnsafeNativeMethods.Rdk_Sun_UTCDateTime(ConstPointer(), ref year, ref month, ref day, ref hours, ref minutes, ref seconds);

                DateTime dt = new DateTime(year, month, day, hours, minutes, seconds);

                return dt;
            }
            set
            {
                UnsafeNativeMethods.Rdk_Sun_SetUTCDateTime(NonConstPointer(), value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second);
            }
        }

        public Rhino.Geometry.Light Light()
        {
            Rhino.Geometry.Light light = new Rhino.Geometry.Light();

            IntPtr plight = light.NonConstPointer();

            UnsafeNativeMethods.Rdk_Sun_Light(ConstPointer(), plight);

            return light;
        }

        [CLSCompliant(false)]
        public uint CRC()
        {
            return UnsafeNativeMethods.Rdk_Sun_CRC(ConstPointer());
        }

        public void ShowDialog()
        {
            UnsafeNativeMethods.Rdk_Sun_ShowDialog(NonConstPointer());
        }
        
        ~Sun()
        {
            Dispose(false);
        }

        IntPtr ConstPointer()
        {
            if (m_pSun != IntPtr.Zero)
            {
                return UnsafeNativeMethods.Rdk_SunInterface(m_pSun);
            }

            return UnsafeNativeMethods.Rdk_DocSunInterface(m_doc.m_docId);
        }

        

        IntPtr NonConstPointer()
        {
            if (m_pSun != IntPtr.Zero)
            {
                return UnsafeNativeMethods.Rdk_SunInterface(m_pSun);
            }

            return UnsafeNativeMethods.Rdk_DocSunInterface(m_doc.m_docId);
        }

        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IntPtr.Zero != m_pSun)
            {
                UnsafeNativeMethods.Rdk_SunDelete(m_pSun);
                m_pSun = IntPtr.Zero;
            }
        }
    }

    public class Skylight
    {
        private Rhino.RhinoDoc m_doc;

        internal Skylight(Rhino.RhinoDoc doc)
        {
            m_doc = doc;
        }

        public bool Enabled
        {
            get { return UnsafeNativeMethods.Rdk_Sun_SkylightOn(ConstDocSunPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetSkylightOn(NonConstDocSunPointer(), value); }
        }

        public double ShadowIntensity
        {
            get { return UnsafeNativeMethods.Rdk_Sun_SkylightShadowIntensity(ConstDocSunPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetSkylightShadowIntensity(NonConstDocSunPointer(), value); }
        }

        public bool CustomEnvironmentOn
        {
            get { return UnsafeNativeMethods.Rdk_Sun_SkylightCustomEnvironmentOn(ConstDocSunPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetSkylightCustomEnvironmentOn(NonConstDocSunPointer(), value); }
        }

        public Guid CustomEnvironment
        {
            get { return UnsafeNativeMethods.Rdk_Sun_SkylightCustomEnvironment(ConstDocSunPointer()); }
            set { UnsafeNativeMethods.Rdk_Sun_SetSkylightCustomEnvironment(NonConstDocSunPointer(), value); }
        }


        IntPtr ConstDocSunPointer()
        {
            return UnsafeNativeMethods.Rdk_DocSun(m_doc.m_docId);
        }

        IntPtr NonConstDocSunPointer()
        {
            return UnsafeNativeMethods.Rdk_DocSun(m_doc.m_docId);
        }
    }
}
#endif