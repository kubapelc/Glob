using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	interface IUniformValue : IEquatable<IUniformValue>
	{
		void Set(int program, int loc);
	}

	 abstract class UniformValue<T> : IEquatable<UniformValue<T>>, IUniformValue
		where T : struct, IEquatable<T>
	{
		protected T _value;

		public UniformValue(T value)
		{
			_value = value;
		}

		public abstract void Set(int program, int loc);

		public bool Equals(UniformValue<T> other)
		{
			if(ReferenceEquals(null, other)) return false;
			if(ReferenceEquals(this, other)) return true;
			return _value.Equals(other._value);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != this.GetType()) return false;
			return Equals((UniformValue<T>)obj);
		}

		public override int GetHashCode()
		{
			return _value.GetHashCode();
		}

		public static bool operator ==(UniformValue<T> left, UniformValue<T> right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(UniformValue<T> left, UniformValue<T> right)
		{
			return !Equals(left, right);
		}

		 public bool Equals(IUniformValue other)
		 {
			 return Equals((object)other);
		 }
	}

	class UniformFloat : UniformValue<float>
	{
		public UniformFloat(float value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform1(program, loc, _value);
		}
	}

	class UniformVector2 : UniformValue<Vector2>
	{
		public UniformVector2(Vector2 value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform2(program, loc, _value);
		}
	}

	class UniformVector3 : UniformValue<Vector3>
	{
		public UniformVector3(Vector3 value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform3(program, loc, _value);
		}
	}

	class UniformVector4 : UniformValue<Vector4>
	{
		public UniformVector4(Vector4 value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform4(program, loc, _value);
		}
	}

	class UniformInt : UniformValue<int>
	{
		public UniformInt(int value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform1(program, loc, _value);
		}
	}

	struct Int2 : IEquatable<Int2>
	{
		public int X;
		public int Y;

		public Int2(int x, int y)
		{
			X = x;
			Y = y;
		}

		public bool Equals(Int2 other)
		{
			return (X == other.X && Y == other.Y);
		}
	}

	struct Int3 : IEquatable<Int3>
	{
		public int X;
		public int Y;
		public int Z;

		public Int3(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public bool Equals(Int3 other)
		{
			return (X == other.X && Y == other.Y && Z == other.Z);
		}
	}

	struct Int4 : IEquatable<Int4>
	{
		public int X;
		public int Y;
		public int Z;
		public int W;

		public Int4(int x, int y, int z, int w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public bool Equals(Int4 other)
		{
			return (X == other.X && Y == other.Y && Z == other.Z && W == other.W);
		}
	}

	class UniformVector2Int : UniformValue<Int2>
	{
		public UniformVector2Int(int x, int y) : base(new Int2(x, y))
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform2(program, loc, _value.X, _value.Y);
		}
	}

	class UniformVector3Int : UniformValue<Int3>
	{
		public UniformVector3Int(int x, int y, int z) : base(new Int3(x, y, z))
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform3(program, loc, _value.X, _value.Y, _value.Z);
		}
	}

	class UniformVector4Int : UniformValue<Int4>
	{
		public UniformVector4Int(int x, int y, int z, int w) : base(new Int4(x, y, z, w))
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform4(program, loc, _value.X, _value.Y, _value.Z, _value.W);
		}
	}

	class UniformDouble : UniformValue<double>
	{
		public UniformDouble(double value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform1(program, loc, _value);
		}
	}

	class UniformVector2Double : UniformValue<Vector2d>
	{
		public UniformVector2Double(Vector2d value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform2(program, loc, _value.X, _value.Y);
		}
	}

	class UniformVector3Double : UniformValue<Vector3d>
	{
		public UniformVector3Double(Vector3d value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform3(program, loc, _value.X, _value.Y, _value.Z);
		}
	}

	class UniformVector4Double : UniformValue<Vector4d>
	{
		public UniformVector4Double(Vector4d value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniform4(program, loc, _value.X, _value.Y, _value.Z, _value.W);
		}
	}

	class UniformMatrix2 : UniformValue<Matrix2>
	{
		public UniformMatrix2(Matrix2 value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniformMatrix2(program, loc, false, ref _value);
		}
	}

	class UniformMatrix3 : UniformValue<Matrix3>
	{
		public UniformMatrix3(Matrix3 value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniformMatrix3(program, loc, false, ref _value);
		}
	}

	class UniformMatrix4 : UniformValue<Matrix4>
	{
		public UniformMatrix4(Matrix4 value) : base(value)
		{
		}

		public override void Set(int program, int loc)
		{
			GL.ProgramUniformMatrix4(program, loc, false, ref _value);
		}
	}
}
