using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	/// <summary>
	/// Glob texture binding:
	/// For every TextureUnit there is at most one texture bound to at most one texture target at all times. So having multiple textures bound to same unit but different targets is not possible.
	/// </summary>
	internal class TextureUnitState
	{
		const int NumTextureUnits = 32;

		int _activeTextureUnit;
		TextureBinding[] _textureBindings;

		public TextureUnitState()
		{
			_textureBindings = new TextureBinding[NumTextureUnits];

			Invalidate();
		}

		public void Invalidate()
		{
			_activeTextureUnit = -1;
			for(int i = 0; i < NumTextureUnits; i++)
			{
				_textureBindings[i] = null;
			}
		}

		internal void GetCurrentBinding(int unit, out int texture, out TextureTarget target)
		{
			texture = 0;
			target = TextureTarget.Texture2D;

			if(_textureBindings[unit] == null)
				return;

			texture = _textureBindings[unit].Texture;
			target = _textureBindings[unit].Target;
		}

		public void BindTexture(TextureTarget target, int texture, int unit = -1)
		{
			if(unit < 0)
				unit = _textureBindings.Length - 1;

			TextureBinding binding = new TextureBinding(target, texture);

			UseTextureUnit(unit);

			if(_textureBindings[unit] != binding)
			{
				_textureBindings[unit] = binding;
				GL.BindTexture(target, texture);
			}
		}

		void UseTextureUnit(int unit)
		{
			if(_activeTextureUnit != unit && unit >= 0)
			{
				GL.ActiveTexture((TextureUnit)((int)TextureUnit.Texture0 + unit));
			}
			_activeTextureUnit = unit;
		}

		class TextureBinding : IEquatable<TextureBinding>
		{
			public readonly TextureTarget Target;
			public readonly int Texture;

			public TextureBinding(TextureTarget target, int handle)
			{
				Target = target;
				Texture = handle;
			}

			public bool Equals(TextureBinding other)
			{
				if(ReferenceEquals(null, other)) return false;
				if(ReferenceEquals(this, other)) return true;
				return Target == other.Target && Texture == other.Texture;
			}

			public override bool Equals(object obj)
			{
				if(ReferenceEquals(null, obj)) return false;
				if(ReferenceEquals(this, obj)) return true;
				if(obj.GetType() != this.GetType()) return false;
				return Equals((TextureBinding)obj);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					return ((int)Target * 397) ^ Texture;
				}
			}

			public static bool operator ==(TextureBinding left, TextureBinding right)
			{
				return Equals(left, right);
			}

			public static bool operator !=(TextureBinding left, TextureBinding right)
			{
				return !Equals(left, right);
			}
		}
	}
}
