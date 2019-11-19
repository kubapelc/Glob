using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public class Texture2D : Texture, IRenderTarget
	{
		public int Width { get { return base.StorageWidth; } }
		public int Height { get { return base.StorageHeight; } }

		// TODO

		/// <summary>
		/// Creates a Texture2D using immutable storage.
		/// </summary>
		/// <param name="device">Glob device</param>
		/// <param name="name">Debug label</param>
		/// <param name="format">Texture internal format</param>
		/// <param name="width">Width in texels</param>
		/// <param name="height">Height in texels</param>
		/// <param name="levels">Number of mip level. Special values: 1 for the base level only, 0 to compute number of mip levels automatically from texture dimensions.</param>
		public Texture2D(Device device, string name, SizedInternalFormatGlob format, int width, int height, int levels = 0)
			: base(TextureTarget.Texture2D, name)
		{
			device.BindTexture(Target, Handle);
			TexStorage2D(format, width, height, levels);
		}

		// TODO: texture views
		public Texture2D(Device device, string name, SizedInternalFormatGlob format, Texture origTexture, int minLevel, int numLevels, int minLayer = 0)
			: base(TextureTarget.Texture2D, name)
		{
			// Texture view - no need to bind
			TextureView(origTexture, format, minLevel, numLevels, minLayer, 1);
		}

		public void TexSubImage2D<T>(Device device, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, T[] pixels)
			where T : struct
		{
			device.BindTexture(Target, Handle);
			GL.TexSubImage2D(Target, level, xoffset, yoffset, width, height, format, type, pixels);
		}

		public void TexSubImage2D(Device device, int level, int xoffset, int yoffset, int width, int height, PixelFormat format, PixelType type, IntPtr pixels)
		{
			device.BindTexture(Target, Handle);
			GL.TexSubImage2D(Target, level, xoffset, yoffset, width, height, format, type, pixels);
		}

		public void CompressedTexSubImage2D<T>(Device device, int level, int xoffset, int yoffset, int width, int height, T[] pixels)
			where T : struct
		{
			device.BindTexture(Target, Handle);
			GL.CompressedTexSubImage2D(Target, level, xoffset, yoffset, width, height, (PixelFormat)this.Format, pixels.Length, pixels);
		}

		public void CompressedTexSubImage2D(Device device, int level, int xoffset, int yoffset, int width, int height, int numBytes, IntPtr pixels)
		{
			device.BindTexture(Target, Handle);
			GL.CompressedTexSubImage2D(Target, level, xoffset, yoffset, width, height, (PixelFormat)this.Format, numBytes, pixels);
		}

		public virtual void AttachToFramebuffer(FramebufferTarget target, FramebufferAttachment attachmentPoint)
		{
			GL.FramebufferTexture2D(target, attachmentPoint, this.Target, this.Handle, 0);
		}
	}
}
