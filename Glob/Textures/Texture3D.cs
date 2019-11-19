using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public class Texture3D : Texture
	{
		public int Width { get { return base.StorageWidth; } }
		public int Height { get { return base.StorageHeight; } }
		public int Depth { get { return base.StorageDepth; } }

		public Texture3D(Device device, string name, SizedInternalFormatGlob format, int width, int height, int depth, int levels = 0)
			: base(TextureTarget.Texture3D, name)
		{
			device.BindTexture(Target, Handle);
			TexStorage3D(format, width, height, depth, levels);
		}

		public Texture3D(Device device, string name, SizedInternalFormatGlob format, Texture origTexture, int minLevel, int numLevels, int minLayer = 0, int numLayers = 1)
			: base(TextureTarget.Texture3D, name)
		{
			//state.TextureUnit.BindTexture(Target, Handle);
			TextureView(origTexture, format, minLevel, numLevels, minLayer, numLayers);
		}
	}
}
