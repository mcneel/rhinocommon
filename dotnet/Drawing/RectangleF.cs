//************************************************************************************
//
// Author: Colin Wade
//
// Copyright © 2013-2014 OMAX Corporation
//
//************************************************************************************

namespace Rhino.Drawing
{
    public struct RectangleF
    {
        float x, y, width, height;

        public RectangleF(float x, float y, float width, float height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public float X { get { return x; } set { x = value; } }
        public float Y { get { return y; } set { y = value; } }

        public float Width { get { return width; } set { width = value; } }
        public float Height { get { return height; } set { height = value; } }

        public float Left { get { return x; } }
        public float Top { get { return y; } }

        public static RectangleF Empty = new RectangleF();
    }
}
