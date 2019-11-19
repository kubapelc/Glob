using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	/// <summary>
	/// Describes the state of blending and color mask
	/// </summary>
	public class BlendState
	{
		// Color buffer
		public readonly bool ColorMaskR = true;
		public readonly bool ColorMaskG = true;
		public readonly bool ColorMaskB = true;
		public readonly bool ColorMaskA = true;
		public readonly BlendMode ColorBlendMode = BlendMode.Overwrite;

		public BlendState()
		{

		}

		public BlendState(BlendMode colorBlendMode, bool colorMaskR = true, bool colorMaskG = true, bool colorMaskB = true,
			bool colorMaskA = true)
		{
			ColorBlendMode = colorBlendMode;
			ColorMaskA = colorMaskA;
			ColorMaskB = colorMaskB;
			ColorMaskG = colorMaskG;
			ColorMaskR = colorMaskR;
		}
	}

	/// <summary>
	/// Describes blend mode including blending equation mode and separate settings for RGB and alpha
	/// </summary>
	public class BlendMode : IEquatable<BlendMode>
	{
		public readonly BlendingFactorSrc BlendSrcRgb;
		public readonly BlendingFactorDest BlendDstRgb;
		public readonly BlendingFactorSrc BlendSrcAlpha;
		public readonly BlendingFactorDest BlendDstAlpha;

		public readonly BlendEquationMode BlendModeRgb;
		public readonly BlendEquationMode BlendModeAlpha;

		public bool IsBlendingEnabled
		{
			get
			{
				if(BlendSrcRgb == BlendingFactorSrc.One && BlendSrcAlpha == BlendingFactorSrc.One &&
				   BlendDstRgb == BlendingFactorDest.Zero && BlendDstAlpha == BlendingFactorDest.Zero &&
				   BlendModeRgb == BlendEquationMode.FuncAdd && BlendModeAlpha == BlendEquationMode.FuncAdd)
					return false;
				return true;
			}
		}

		public BlendMode(BlendingFactorSrc srcRgb, BlendingFactorDest dstRgb, BlendingFactorSrc srcAlpha, BlendingFactorDest dstAlpha, BlendEquationMode modeRgb = BlendEquationMode.FuncAdd, BlendEquationMode modeAlpha = BlendEquationMode.FuncAdd)
		{
			BlendSrcRgb = srcRgb;
			BlendDstRgb = dstRgb;
			BlendSrcAlpha = srcAlpha;
			BlendDstAlpha = dstAlpha;
			BlendModeRgb = modeRgb;
			BlendModeAlpha = modeAlpha;
		}

		public bool Equals(BlendMode other)
		{
			if(ReferenceEquals(null, other)) return false;
			if(ReferenceEquals(this, other)) return true;
			return BlendSrcRgb == other.BlendSrcRgb && BlendDstRgb == other.BlendDstRgb && BlendSrcAlpha == other.BlendSrcAlpha && BlendDstAlpha == other.BlendDstAlpha && BlendModeRgb == other.BlendModeRgb && BlendModeAlpha == other.BlendModeAlpha;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != this.GetType()) return false;
			return Equals((BlendMode)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (int)BlendSrcRgb;
				hashCode = (hashCode * 397) ^ (int)BlendDstRgb;
				hashCode = (hashCode * 397) ^ (int)BlendSrcAlpha;
				hashCode = (hashCode * 397) ^ (int)BlendDstAlpha;
				hashCode = (hashCode * 397) ^ (int)BlendModeRgb;
				hashCode = (hashCode * 397) ^ (int)BlendModeAlpha;
				return hashCode;
			}
		}

		public static bool operator ==(BlendMode left, BlendMode right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(BlendMode left, BlendMode right)
		{
			return !Equals(left, right);
		}

		/// <summary>
		/// No blending
		/// </summary>
		public static readonly BlendMode Overwrite = new BlendMode(BlendingFactorSrc.One, BlendingFactorDest.Zero, BlendingFactorSrc.One, BlendingFactorDest.Zero);

		/// <summary>
		/// Additive blending of all components including alpha
		/// </summary>
		public static readonly BlendMode Additive = new BlendMode(BlendingFactorSrc.One, BlendingFactorDest.One, BlendingFactorSrc.One, BlendingFactorDest.One);

		/// <summary>
		/// Classical alpha blending of all components including alpha
		/// </summary>
		public static readonly BlendMode AlphaBlending = new BlendMode(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
	}

}
