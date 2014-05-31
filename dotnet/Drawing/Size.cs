//************************************************************************************
//
// Author: Colin Wade
//
// Copyright © 2013-2014 OMAX Corporation
//
//************************************************************************************

namespace Rhino.Drawing
{
    public struct Size
    {
        int width, height;

        public Size(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        public int Width { get { return width; } set { width = value; } }
        public int Height { get { return width; } set { width = value; } }

        public static Size Empty = new Size();
    }
}
