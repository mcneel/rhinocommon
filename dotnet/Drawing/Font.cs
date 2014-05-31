//************************************************************************************
//
// Author: Colin Wade
//
// Copyright © 2013-2014 OMAX Corporation
//
//************************************************************************************

namespace Rhino.Drawing
{
    public sealed class Font
    {
        FontFamily family;
        float emSize;
        FontStyle style;
        GraphicsUnit unit;
        byte gdiCharSet;
        bool gdiVerticalFont;

        public Font(FontFamily family, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont = false)
        {
            this.family = family;
            this.emSize = emSize;
            this.style = style;
            this.unit = unit;
            this.gdiCharSet = gdiCharSet;
            this.gdiVerticalFont = gdiVerticalFont;
        }

        public Font(string familyName, float emSize, FontStyle style, GraphicsUnit unit, byte gdiCharSet, bool gdiVerticalFont = false)
        {
            //???
            this.emSize = emSize;
            this.style = style;
            this.unit = unit;
            this.gdiCharSet = gdiCharSet;
            this.gdiVerticalFont = gdiVerticalFont;
        }

        public float Size { get { return emSize; } }
        public FontFamily FontFamily { get { return family; } }
        public FontStyle Style { get { return style; } }
        public GraphicsUnit Unit { get { return unit; } }
        public byte GdiCharSet { get { return gdiCharSet; } }
        public bool GdiVerticalFont { get { return gdiVerticalFont; } }
    }
}
