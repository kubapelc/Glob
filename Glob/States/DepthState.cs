using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public class DepthState
	{
		// Depth buffer
		public readonly DepthFunction DepthFunction;
		public readonly bool DepthMask;

		public DepthState()
		{
			DepthFunction = DepthFunction.Always;
			DepthMask = false;
		}

		public DepthState(DepthFunction depthFunction, bool depthMask)
		{
			DepthFunction = depthFunction;
			DepthMask = depthMask;
		}
	}
}
