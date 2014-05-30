//************************************************************************************
//
// Author: Colin Wade
//
// Copyright © 2013-2014 OMAX Corporation
//
//************************************************************************************

namespace Rhino.Drawing
{
    public struct SizeF
    {
        float width, height;

        public SizeF(float width, float height)
        {
            this.width = width;
            this.height = height;
        }

        public float Width { get { return width; } set { width = value; } }
        public float Height { get { return width; } set { width = value; } }

        public static SizeF Empty = new SizeF();
    }
}
