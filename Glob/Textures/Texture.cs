using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public abstract class Texture : IDisposable
	{
		public int Handle { get; private set; }
		public TextureTarget Target { get; private set; }
		public string Name { get; private set; }

		public SizedInternalFormatGlob Format { get; private set; }
		public int Levels { get; private set; }
		public int Samples { get; private set; }

		public int StorageWidth { get; private set; }
		public int StorageHeight { get; private set; }
		public int StorageDepth { get; private set; }

		public Texture ViewBase { get; private set; }
		public int ViewMinLevel { get; private set; }
		public int ViewNumLevels { get; private set; }
		public int ViewMinLayer { get; private set; }
		public int ViewNumLayers { get; private set; }

		protected Texture(TextureTarget target, string name)
		{
			Handle = GL.GenTexture();
			Target = target;
			Name = name;
			ViewBase = null;
		}

		/// <summary>
		/// Allocates texture storage. Does not bind the texture to the specified target, it needs to be bound by the user in advance.
		/// </summary>
		/// <param name="format">The internal format of the texture data</param>
		/// <param name="width">Texture image width</param>
		/// <param name="levels">The number of levels. Use 0 to automatically compute level count.</param>
		protected void TexStorage1D(SizedInternalFormatGlob format, int width, int levels = 0)
		{
			if(levels < 1)
				levels = CountLevels(width);

			GL.TexStorage1D((TextureTarget1d)Target, levels, (SizedInternalFormat)format, width);
			OnTextureStorage(format, levels, 1, width);
		}

		/// <summary>
		/// Allocates texture storage. Does not bind the texture to the specified target, it needs to be bound by the user in advance.
		/// </summary>
		/// <param name="format">The internal format of the texture data</param>
		/// <param name="width">Texture image width</param>
		/// <param name="height">Texture image height</param>
		/// <param name="levels">The number of levels. Use 0 to automatically compute level count.</param>
		protected void TexStorage2D(SizedInternalFormatGlob format, int width, int height, int levels = 0)
		{
			if(levels < 1)
				levels = CountLevels(width, height);

			GL.TexStorage2D((TextureTarget2d)Target, levels, (SizedInternalFormat)format, width, height);
			OnTextureStorage(format, levels, 1, width, height);
		}

		/// <summary>
		/// Allocates multisample texture storage. Does not bind the texture to the specified target, it needs to be bound by the user in advance.
		/// </summary>
		/// <param name="format">The internal format of the texture data</param>
		/// <param name="samples">The sample count of each texture texel</param>
		/// <param name="width">Texture image width</param>
		/// <param name="height">Texture image height</param>
		protected void TexStorage2DMultisample(SizedInternalFormatGlob format, int samples, int width, int height, bool fixedLocations)
		{
			GL.TexStorage2DMultisample((TextureTargetMultisample2d)Target, samples, (SizedInternalFormat)format, width, height, fixedLocations);
			OnTextureStorage(format, 1, samples, width, height);
		}

		/// <summary>
		/// Allocates texture storage. Does not bind the texture to the specified target, it needs to be bound by the user in advance.
		/// </summary>
		/// <param name="format">The internal format of the texture data</param>
		/// <param name="width">Texture image width</param>
		/// <param name="height">Texture image height</param>
		/// <param name="depth">Texture image depth</param>
		/// <param name="levels">The number of levels. Use 0 to automatically compute level count.</param>
		protected void TexStorage3D(SizedInternalFormatGlob format, int width, int height, int depth, int levels = 0)
		{
			if(levels < 1)
				levels = CountLevels(width, height, depth);

			GL.TexStorage3D((TextureTarget3d)Target, levels, (SizedInternalFormat)format, width, height, depth);
			OnTextureStorage(format, levels, 1, width, height, depth);
		}

		/// <summary>
		/// Allocates multisample texture storage. Does not bind the texture to the specified target, it needs to be bound by the user in advance.
		/// </summary>
		/// <param name="format">The internal format of the texture data</param>
		/// <param name="samples">The sample count of each texture texel</param>
		/// <param name="width">Texture image width</param>
		/// <param name="height">Texture image height</param>
		/// <param name="depth">Texture image depth</param>
		protected void TexStorage3DMultisample(SizedInternalFormatGlob format, int samples, int width, int height, int depth, bool fixedLocations)
		{
			GL.TexStorage3DMultisample((TextureTargetMultisample3d)Target, samples, (SizedInternalFormat)format, width, height, depth, fixedLocations);
			OnTextureStorage(format, 1, samples, width, height, depth);
		}

		/// <summary>
		/// Clears the texture with a specified value
		/// </summary>
		/// <param name="level">Mip level to clear</param>
		/// <param name="format">Pixel format of the clear value</param>
		/// <param name="type">Pixel type of the clear value</param>
		/// <param name="data">Clear value</param>
		public void Clear(int level, PixelFormat format, PixelType type, IntPtr data)
		{
			GL.ClearTexImage(Handle, level, format, type, data);
		}

		/// <summary>
		/// Clears the texture with a specified value
		/// </summary>
		/// <param name="level">Mip level to clear</param>
		/// <param name="format">Pixel format of the clear value</param>
		/// <param name="type">Pixel type of the clear value</param>
		/// <param name="data">Clear value</param>
		public void Clear<T>(int level, PixelFormat format, PixelType type, T[] data)
			where T : struct
		{
			GL.ClearTexImage(Handle, level, format, type, data);
		}

		/// <summary>
		/// Clears the texture with a specified value
		/// </summary>
		/// <param name="level">Mip level to clear</param>
		/// <param name="format">Pixel format of the clear value</param>
		/// <param name="type">Pixel type of the clear value</param>
		/// <param name="data">Clear value</param>
		public void Clear<T>(int level, PixelFormat format, PixelType type, T[] data, int x, int y, int z, int w, int h, int d)
			where T : struct
		{
			GL.ClearTexSubImage(Handle, level, x, y, z, w, h, d, format, type, data);
		}

		// TODO: test texture views

		/// <summary>
		/// Important: do not assign a TextureTarget to the texture before calling TextureView // TODO: proč?
		/// </summary>
		/// <param name="origTexture"></param>
		/// <param name="format"></param>
		/// <param name="minLevel"></param>
		/// <param name="numLevels"></param>
		/// <param name="minLayer"></param>
		/// <param name="numLayers"></param>
		protected void TextureView(Texture origTexture, SizedInternalFormatGlob format, int minLevel, int numLevels,
			int minLayer, int numLayers)
		{
			GL.TextureView(Handle, Target, origTexture.Handle, (PixelInternalFormat)format, minLevel, numLevels, minLayer, numLayers);
			Format = format;
			Levels = numLevels;
			Samples = origTexture.Samples;

			StorageWidth = origTexture.StorageWidth >> minLevel;
			StorageHeight = origTexture.StorageHeight >> minLevel;
			StorageDepth = origTexture.StorageDepth >> minLevel;
			
			ViewBase = origTexture;

			ViewMinLevel = minLevel;
			ViewNumLevels = numLevels;
			ViewMinLayer = minLayer;
			ViewNumLayers = numLayers;
		}

		void OnTextureStorage(SizedInternalFormatGlob format, int levels, int samples, int width, int height = 0, int depth = 0)
		{
			Format = format;
			Levels = levels;
			Samples = samples;

			StorageWidth = width;
			StorageHeight = height;
			StorageDepth = depth;

			ViewBase = null;

			ViewMinLevel = -1;
			ViewNumLevels = -1;
			ViewMinLayer = -1;
			ViewNumLayers = -1;

			Utils.SetObjectLabel(ObjectLabelIdentifier.Texture, Handle, Name);
		}

		/// <summary>
		/// Deletes the OpenGL texture object
		/// </summary>
		public void Dispose()
		{
			GL.DeleteTexture(Handle);
			Handle = -1;
		}

		/// <summary>
		/// Returns the max number of mip levels of a 1D texture
		/// </summary>
		public static int CountLevels(int width)
		{
			int levels = 1;

			while(width > 1)
			{
				width /= 2;
				levels++;
			}

			return levels;
		}

		/// <summary>
		/// Returns the max number of mip levels of a 2D texture
		/// </summary>
		public static int CountLevels(int width, int height)
		{
			return Math.Max(CountLevels(width), CountLevels(height));
		}

		/// <summary>
		/// Returns the max number of mip levels of a 3D texture
		/// </summary>
		public static int CountLevels(int width, int height, int depth)
		{
			return Math.Max(Math.Max(CountLevels(width), CountLevels(height)), CountLevels(depth));
		}
	}
}
