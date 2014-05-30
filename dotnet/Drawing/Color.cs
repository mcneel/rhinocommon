//************************************************************************************
//
// Author: Colin Wade
//
// Copyright © 2013-2014 OMAX Corporation
//
//************************************************************************************

namespace Rhino.Drawing
{
    public struct Color
    {
        public byte A { get { throw new System.NotImplementedException(); } }
        public byte R { get { throw new System.NotImplementedException(); } }
        public byte G { get { throw new System.NotImplementedException(); } }
        public byte B { get { throw new System.NotImplementedException(); } }

        public bool IsEmpty { get { throw new System.NotImplementedException(); } }

        public int ToArgb()
        {
            throw new System.NotImplementedException();
        }

        public static Color Empty = new Color();

        public static Color FromArgb(int argb)
        {
            throw new System.NotImplementedException();
        }

        public static Color FromArgb(int alpha, int red, int green, int blue)
        {
            throw new System.NotImplementedException();
        }

        public static Color FromArgb(int red, int green, int blue)
        {
            throw new System.NotImplementedException();
        }
    }
}
