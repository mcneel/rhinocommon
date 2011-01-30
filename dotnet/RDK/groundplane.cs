using System;
using System.Runtime.InteropServices;

#if USING_RDK
namespace Rhino.Render
{
    public class GroundPlane
    {
        private Rhino.RhinoDoc m_doc;

        internal GroundPlane(Rhino.RhinoDoc doc)
        {
            m_doc = doc;
        }

        public bool Enabled
        {
            get { return UnsafeNativeMethods.Rdk_GroundPlane_Enabled(); }
            set { UnsafeNativeMethods.Rdk_GroundPlane_SetEnabled(value); }
        }

        public double Altitude
        {
            get { return UnsafeNativeMethods.Rdk_GroundPlane_Altitude(); }
            set { UnsafeNativeMethods.Rdk_GroundPlane_SetAltitude(value); }
        }

        public Guid MaterialInstanceId
        {
            get { return UnsafeNativeMethods.Rdk_GroundPlane_MaterialInstanceId(); }
            set { UnsafeNativeMethods.Rdk_GroundPlane_SetMaterialInstanceId(value); }
        }

        public Rhino.Geometry.Vector2d TextureOffset
        {
            get 
            {
                Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
                UnsafeNativeMethods.Rdk_GroundPlane_TextureOffset(ref v);
                return v;
            }
            set { UnsafeNativeMethods.Rdk_GroundPlane_SetTextureOffset(value); }
        }

        public Rhino.Geometry.Vector2d TextureSize
        {
            get 
            {
                Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
                UnsafeNativeMethods.Rdk_GroundPlane_TextureSize(ref v);
                return v;
            }
            set { UnsafeNativeMethods.Rdk_GroundPlane_SetTextureSize(value); }
        }

        public double TextureRotation
        {
            get { return UnsafeNativeMethods.Rdk_GroundPlane_TextureRotation(); }
            set { UnsafeNativeMethods.Rdk_GroundPlane_SetTextureRotation(value); }
        }
    }
}
#endif