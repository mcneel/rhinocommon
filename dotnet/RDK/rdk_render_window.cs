#pragma warning disable 1591
using System;

#if RDK_CHECKED

namespace Rhino.Render
{
  public class RenderWindow
  {
    private readonly Rhino.Render.RenderPipeline m_renderPipe;
    internal RenderWindow(Rhino.Render.RenderPipeline pipe)
    {
      m_renderPipe = pipe;
    }

    [Flags]
    public enum StandardChannels : int
    {
      None    = 0x0000,
      Red     = 0x0001,
      Green   = 0x0002,
      Blue    = 0x0004,
      Alpha   = 0x0008,
      RGBA    = 0x000F,

      DistanceFromCamera  = 0x0010,
      NormalX             = 0x0020,
      NormalY             = 0x0040,
      NormalZ             = 0x0080,

      LuminanceRed        = 0x0100,
      LuminanceGreen      = 0x0200,
      LuminanceBlue       = 0x0400,

      BackgroundLuminanceRed      = 0x1000,
      BackgroundLuminanceGreen    = 0x2000,
      BackgroundLuminanceBlue     = 0x4000,

      MaterialIds       = 0x00010000,
      ObjectIds         = 0x00020000,
      Wireframe         = 0x00040000,
    }

    public static Guid ChannelId(StandardChannels ch)
    {
      return UnsafeNativeMethods.Rdk_RenderWindow_StandardChannelId((int)ch);
    }

    public Rhino.Drawing.Size Size()
    {
      int width = 0;int height = 0;
      UnsafeNativeMethods.Rdk_RenderWindow_Size(ConstPointer(), ref width, ref height);
      return new Rhino.Drawing.Size(width, height);
    }

    public void SetSize(Rhino.Drawing.Size size)
    {
      int width = size.Width, height = size.Height;
      UnsafeNativeMethods.Rdk_RenderWindow_SetSize(ConstPointer(), width, height);
    }

    /// <summary>
    /// Accepts a rendering progress value to inform the user of the rendering advances.
    /// </summary>
    /// <param name="text">The progress text.</param>
    /// <param name="progress">A progress value in the domain [0.0f; 1.0f].</param>
    public void SetProgress(string text, float progress)
    {
      UnsafeNativeMethods.Rdk_RenderWindow_SetProgress(ConstPointer(), text, (int)(progress * 100.0f));
    }

    public Channel OpenChannel(StandardChannels id)
    {
      IntPtr pChannel = UnsafeNativeMethods.Rdk_RenderWindow_OpenChannel(ConstPointer(), (int)id);
      if (pChannel != IntPtr.Zero)
      {
        return new Channel(pChannel);
      }
      return null;
    }


    /// <summary>
    /// A wireframe channel will not be added if none of the document properties settings
    /// indicate that one is needed. In other words, Rhino will not generate an empty wireframe channel
    /// just for the fun of it.
    /// </summary>
    /// <param name="doc">The document to display</param>
    /// <param name="viewport">The view to display</param>
    /// <param name="size">The size of the image without clipping (ie - if you have a region, it was the
    /// size of the image before you cut the region out.</param>
    /// <param name="region">The area of the rendering you want to display.  This should match the size
    /// of the render window itself (ie - the one set using SetSize)</param>
    /// <returns>Returns true if the wireframe channel was successfully added.</returns>
    public bool AddWireframeChannel(Rhino.RhinoDoc doc, Rhino.DocObjects.ViewportInfo viewport, Rhino.Drawing.Size size, Rhino.Drawing.Rectangle region)
    {
      int[] xy = { size.Width, size.Height };
      int[] lrtb = { region.Left, region.Right, region.Top, region.Bottom };
      return UnsafeNativeMethods.Rdk_RenderWindow_AddWireframeChannel(ConstPointer(), doc.DocumentId, viewport.ConstPointer(), ref xy[0], ref lrtb[0]);
    }

    /// <summary>
    /// Add a channel to the frame buffer in addition to the fixed Red, Green, Blue and Alpha channels.
    /// </summary>
    /// <param name="channel">Channel to add</param>
    /// <returns>If the channel existed then true is returned otherwise; returns true if the channel was added or false if not.</returns>
    public bool AddChannel(StandardChannels channel)
    {
      uint pixelSize = 0;
      Guid uuidChannel = Guid.Empty;
      switch (channel)
      {
        case StandardChannels.DistanceFromCamera:
        case StandardChannels.Alpha:
        case StandardChannels.NormalX:
        case StandardChannels.NormalY:
        case StandardChannels.NormalZ:
        case StandardChannels.LuminanceRed:
        case StandardChannels.LuminanceGreen:
        case StandardChannels.LuminanceBlue:
          pixelSize = sizeof(float);
          uuidChannel = ChannelId(channel);
          break;
      }
      if (pixelSize < 1 || uuidChannel == Guid.Empty)
        return false;
      return UnsafeNativeMethods.Rdk_RenderWindow_AddChannel(ConstPointer(), ref uuidChannel, pixelSize);
    }

    /// <summary>
    /// Call this method to open the RenderWindow.StandardChannels.RGBA channel and set a block of color values
    /// </summary>
    /// <param name="size">Size of the area to set. No validation is done on this value</param>
    /// <param name="colors">Array of Color4f values used to set the RenderWindow.StandardChannels.RGBA </param>
    public void SetRGBAChannelColors(Rhino.Drawing.Size size, Rhino.Display.Color4f[] colors)
    {
      SetRGBAChannelColors(Rhino.Drawing.Rectangle.FromLTRB(0, 0, size.Width, size.Height), colors);
    }

    /// <summary>
    /// Call this method to open the RenderWindow.StandardChannels.RGBA channel and set a block of color values
    /// </summary>
    /// <param name="rectangle">
    /// rectangle.X is the horizontal pixel position of the left edge. No validation is done on this value.
    ///   The caller is responsible for ensuring that it is within the frame buffer.
    /// rectangle.Y is the vertical pixel position of the top edge. No validation is done on this value.
    ///   The caller is responsible for ensuring that it is within the frame buffer.
    /// rectangle.Width is the width of the rectangle in pixels. No validation is done on this value.
    /// rectangle.Height is the height of the rectangle in pixels. No validation is done on this value.
    /// </param>
    /// <param name="colors">Array of Color4f values used to set the RenderWindow.StandardChannels.RGBA </param>
    public void SetRGBAChannelColors(Rhino.Drawing.Rectangle rectangle, Rhino.Display.Color4f[] colors)
    {
      using (Rhino.Render.RenderWindow.Channel channel = OpenChannel(Rhino.Render.RenderWindow.StandardChannels.RGBA))
        channel.SetValues(rectangle, colors);
    }

    /// <summary>
    /// Invalidate the entire view window so that the pixels get painted.
    /// </summary>
    public void Invalidate()
    {
      UnsafeNativeMethods.Rdk_RenderWindow_Invalidate(ConstPointer());
    }

    public void InvalidateArea(Rhino.Drawing.Rectangle rect)
    {
      UnsafeNativeMethods.Rdk_RenderWindow_InvalidateArea(ConstPointer(), rect.Top, rect.Left, rect.Bottom, rect.Right);
    }

    #region internals
    IntPtr ConstPointer()
    {
      return UnsafeNativeMethods.Rdk_SdkRender_GetRenderWindow(m_renderPipe.ConstPointer());
    }
    #endregion

    public class Channel : IDisposable
    {
      private IntPtr m_pChannel;
      internal Channel(IntPtr pChannel)
      {
        m_pChannel = pChannel;
      }

      /// <summary>
      /// Returns the size of the data in one pixel in the channel. For RDK standard channels, this value is always sizeof(float). 
      /// For the special chanRGBA collective channel,
      /// this value is 4 * sizeof(float).
      /// </summary>
      /// <returns>The size of a pixel.</returns>
      public int PixelSize()
      {
        return UnsafeNativeMethods.Rdk_RenderWindowChannel_PixelSize(ConstPointer());
      }

      /// <summary>
      /// If x or y are out of range, the function will fail and may crash Rhino.
      /// </summary>
      /// <param name="x">The horizontal pixel position. No validation is done on this value. 
      /// The caller is responsible for ensuring that it is within the frame buffer.</param>
      /// <param name="y">the vertical pixel position. No validation is done on this value.
      /// The caller is responsible for ensuring that it is within the frame buffer.</param>
      /// <param name="value">The value to store in the channel at the specified position.</param>
      public void SetValue(int x, int y, float value)
      {
        UnsafeNativeMethods.Rdk_RenderWindowChannel_SetFloatValue(ConstPointer(), x, y, value);
      }

      internal void SetValues(Rhino.Drawing.Rectangle rectangle, Rhino.Display.Color4f[] colors)
      {
        UnsafeNativeMethods.Rdk_RenderWindowChannel_SetValueRect(ConstPointer(), rectangle.Left, rectangle.Top, rectangle.Width, rectangle.Height, colors);
      }

      /// <summary>
      /// If x or y are out of range, the function will fail and may crash Rhino.
      /// </summary>
      /// <param name="x">The horizontal pixel position. No validation is done on this value. 
      /// The caller is responsible for ensuring that it is within the frame buffer.</param>
      /// <param name="y">The vertical pixel position. No validation is done on this value.
      /// The caller is responsible for ensuring that it is within the frame buffer.</param>
      /// <param name="value">The color to store in the channel at the specified position.</param>
      public void SetValue(int x, int y, Rhino.Display.Color4f value)
      {
        UnsafeNativeMethods.Rdk_RenderWindowChannel_SetColorValue(ConstPointer(), x, y, value);
      }

      IntPtr ConstPointer()
      {
        return m_pChannel;
      }

      #region IDisposable Members

      public void Dispose()
      {
        Dispose(true);
        GC.SuppressFinalize(this);
      }

      protected virtual void Dispose(bool disposing)
      {
        if (disposing)
        {
          if (m_pChannel != IntPtr.Zero)
          {
            UnsafeNativeMethods.Rdk_RenderWindowChannel_Close(m_pChannel);
            m_pChannel = IntPtr.Zero;
          }
        }
      }

      #endregion
    }
  }
}

#endif