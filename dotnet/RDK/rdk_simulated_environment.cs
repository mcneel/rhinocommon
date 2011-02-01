using System;
using System.Runtime.InteropServices;

#if USING_RDK
namespace Rhino.Render
{
    public class SimulatedEnvironment : IDisposable
    {
        private IntPtr m_pSim = IntPtr.Zero;

        public SimulatedEnvironment()
        {
            m_pSim = UnsafeNativeMethods.Rdk_SimulatedEnvironment_New();
        }

        ~SimulatedEnvironment()
        {
            Dispose(false);
        }

        public System.Drawing.Color BackgroundColor
        {
            get
            {
                return System.Drawing.Color.FromArgb(UnsafeNativeMethods.Rdk_SimulatedEnvironment_BackgroundColor(m_pSim));
            }
            set
            {
                UnsafeNativeMethods.Rdk_SimulatedEnvironment_SetBackgroundColor(m_pSim, value.ToArgb());
            }
        }

        public Rhino.Render.SimulatedTexture BackgroundImage
        {
            get
            {
                return new Rhino.Render.SimulatedTexture(this);
            }
            set
            {
                IntPtr p = value.ConstPointer();
                UnsafeNativeMethods.Rdk_SimulatedEnvironment_SetBackgroundImage(m_pSim, p);

            }
        }

        public enum eBackgroundProjection : int
        {
            planar      = 0,
		    spherical   = 1,	//equirectangular projection
		    emap		= 2,	//mirrorball
		    box         = 3,
		    automatic	= 4,
		    lightprobe  = 5,
		    cubemap	    = 6,
		    vertical_cross_cubemap      = 7,
		    horizontal_cross_cubemap    = 8,
        }


        public eBackgroundProjection BackgroundProjection
        {
            get
            {
                return (eBackgroundProjection)UnsafeNativeMethods.Rdk_SimulatedEnvironment_BackgroundProjection(m_pSim);
            }
            set
            {
                UnsafeNativeMethods.Rdk_SimulatedEnvironment_SetBackgroundProjection(m_pSim, (int)value);
            }
        }

        public static eBackgroundProjection ProjectionFromString(string sProjection)
        {
            return (eBackgroundProjection)UnsafeNativeMethods.Rdk_SimulatedEnvironment_ProjectionFromString(sProjection);
        }

        public static string StringFromProjection(eBackgroundProjection proj)
        {
            using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
            {
                IntPtr pString = sh.NonConstPointer();
                UnsafeNativeMethods.Rdk_SimulatedTexture_StringFromProjection(pString, (int)proj);
                return sh.ToString();
            }
        }

        //public static eBackgroundProjection AutomaticProjectionFromChildTexture(Rhino.Render.RenderTexture texture)
        //{
            //TODO:return (eBackgroundProjection)UnsafeNativeMethods.Rdk_SimulatedEnvironment_AutomaticProjectionFromChildTexture(texture.Id);
        //}

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IntPtr.Zero != m_pSim)
            {
                UnsafeNativeMethods.Rdk_SimulatedTexture_Delete(m_pSim);
                m_pSim = IntPtr.Zero;
            }
        }

        public IntPtr ConstPointer()
        {
            return m_pSim;
        }
    }
}



#endif

