using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public class Texture2DArray : Texture
	{
		public int Width { get { return base.StorageWidth; } }
		public int Height { get { return base.StorageHeight; } }
		public int Layers { get { return base.StorageDepth; } }

		public Texture2DArray(Device device, string name, SizedInternalFormatGlob format, int width, int height, int layers, int levels = 0)
			: base(TextureTarget.Texture2DArray, name)
		{
			device.BindTexture(Target, Handle);
			TexStorage3D(format, width, height, layers, levels);
		}

		public Texture2DArray(Device device, string name, SizedInternalFormatGlob format, Texture origTexture, int minLevel, int numLevels, int minLayer, int numLayers)
			: base(TextureTarget.Texture2DArray, name)
		{
			//state.TextureUnit.BindTexture(Target, Handle);
			TextureView(origTexture, format, minLevel, numLevels, minLayer, numLayers);
		}
	}
}
