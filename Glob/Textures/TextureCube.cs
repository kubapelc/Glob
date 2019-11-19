using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public class TextureCube : Texture
	{
		public int Width { get { return base.StorageWidth; } }
		public int Height { get { return base.StorageHeight; } }

		public TextureCube(Device device, string name, SizedInternalFormatGlob format, int width, int height, int levels = 0)
			: base(TextureTarget.TextureCubeMap, name)
		{
			device.BindTexture(Target, Handle);
			TexStorage2D(format, width, height, levels);
		}
	}
}
