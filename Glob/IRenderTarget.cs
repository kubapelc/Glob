using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public interface IRenderTarget
	{
		int Width { get; }
		int Height { get; }

		void AttachToFramebuffer(FramebufferTarget target, FramebufferAttachment attachmentPoint);
	}
}
