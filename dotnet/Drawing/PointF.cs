//************************************************************************************
//
// Author: Colin Wade
//
// Copyright © 2013-2014 OMAX Corporation
//
//************************************************************************************

namespace Rhino.Drawing
{
    public struct PointF
    {
        float x, y;

        public PointF(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float X { get { return x; } set { x = value; } }
        public float Y { get { return y; } set { y = value; } }

        public static PointF Empty = new PointF();
    }
}
