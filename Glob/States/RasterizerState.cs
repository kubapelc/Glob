using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	[Flags]
	public enum CullfaceState
	{
		None = 0,
		Front = 1,
		Back = 2,
		FrontAndBack = 3,
	}

	/// <summary>
	/// Stores rasterization state - cull face mode, polygon mode
	/// </summary>
	public class RasterizerState
	{
		public readonly PolygonMode PolygonModeFront;
		public readonly PolygonMode PolygonModeBack;
		public readonly CullfaceState CullfaceState;

		public RasterizerState(CullfaceState cullfaceState = CullfaceState.Back, PolygonMode polygonModeFront = PolygonMode.Fill, PolygonMode polygonModeBack = PolygonMode.Fill)
		{
			PolygonModeFront = polygonModeFront;
			PolygonModeBack = polygonModeBack;
			CullfaceState = cullfaceState;
		}
	}
}
