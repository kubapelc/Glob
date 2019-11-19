using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public enum VertexAttribClass
	{
		Float = 0,
		Integer = 1,
		Double = 2,
	}

	/// <summary>
	/// Describes vertex buffer format without specifying the exact buffers to use
	/// </summary>
	public class VertexBufferFormat
	{
		public const int MaxAttributes = 16;

		readonly IReadOnlyCollection<VertexAttribDescription> _attributes;
		readonly int _enabledAttributesMask;

		/// <summary>
		/// Creates a new vertex buffer format
		/// </summary>
		/// <param name="attributes">Array of vertex attributes</param>
		public VertexBufferFormat(params VertexAttribDescription[] attributes)
		{
			_attributes = attributes;
			if(_attributes == null)
				_attributes = new VertexAttribDescription[0];
			if(_attributes.Count > MaxAttributes)
				throw new Exception("Too many vertex attributes!");

			_enabledAttributesMask = 0;
			foreach(var attribute in _attributes)
			{
				_enabledAttributesMask |= 1 << attribute.AttribIndex;
			}
		}

		internal void Bind(VertexBufferFormat old)
		{
			if(old != null)
			{
				int disable = old._enabledAttributesMask & (~_enabledAttributesMask);
				int enable = (~old._enabledAttributesMask) & _enabledAttributesMask;

				for(int i = 0; i < MaxAttributes; i++)
				{
					if(((disable >> i) & 1) > 0)
					{
						GL.DisableVertexAttribArray(i);
					}
					if(((enable >> i) & 1) > 0)
					{
						GL.EnableVertexAttribArray(i);
					}
				}
			}
			else
			{
				foreach(var attribute in _attributes)
				{
					GL.EnableVertexAttribArray(attribute.AttribIndex);
				}
			}

			foreach(var attribute in _attributes)
			{
				GL.EnableVertexAttribArray(attribute.AttribIndex);
			}
			

			foreach(var attribute in _attributes)
			{
				attribute.Bind();
			}
		}
	}

	public class VertexAttribDescription : IEquatable<VertexAttribDescription>
	{
		public readonly int AttribIndex;
		public readonly int BindingIndex;
		public readonly int Size;
		public readonly int RelativeOffset;
		public readonly VertexAttribType Type;
		public readonly VertexAttribClass DataClass;
		public readonly bool Normalized;

		/// <summary>
		/// Creates a new vertex attribute for use in a VertexBufferFormat object
		/// </summary>
		/// <param name="attribIndex">Attribute index used to access this attribute in the vertex shader</param>
		/// <param name="bindingIndex"></param>
		/// <param name="size">The number of components of this vertex attribute (1 for a scalar, 3 for a vec3...)</param>
		/// <param name="relativeOffset">Relative offset of this attribute's data inside the source buffer</param>
		/// <param name="type">Data type the attribute</param>
		/// <param name="dataClass">Specifies how will this value be interpreted by shaders. Float is the default, use Integer for integer values, Double for double precision floats.</param>
		/// <param name="normalized">Specifies whether the data of this attribute is normalized to 0..1 range or -1..1 range for signed data types. Only relevant when dataClass is set to float.</param>
		public VertexAttribDescription(int attribIndex, int bindingIndex, int size, int relativeOffset, VertexAttribType type, VertexAttribClass dataClass, bool normalized)
		{
			if(AttribIndex >= VertexBufferFormat.MaxAttributes)
				throw new Exception("Vertex attribute index is too high!");
			AttribIndex = attribIndex;
			BindingIndex = bindingIndex;
			Size = size;
			RelativeOffset = relativeOffset;
			Type = type;
			DataClass = dataClass;
			Normalized = normalized;
		}

		internal void Bind()
		{
			switch(DataClass)
			{
				case VertexAttribClass.Integer:
				{
					GL.VertexAttribIFormat(AttribIndex, Size, (VertexAttribIntegerType)Type, RelativeOffset);
					break;
				}
				case VertexAttribClass.Double:
				{
					GL.VertexAttribLFormat(AttribIndex, Size, (VertexAttribDoubleType)Type, RelativeOffset);
					break;
				}
				default:
				{
					GL.VertexAttribFormat(AttribIndex, Size, Type, Normalized, RelativeOffset);
					break;
				}
			}
			GL.VertexAttribBinding(AttribIndex, BindingIndex);
		}

		public bool Equals(VertexAttribDescription other)
		{
			if(ReferenceEquals(null, other)) return false;
			if(ReferenceEquals(this, other)) return true;
			return AttribIndex == other.AttribIndex && BindingIndex == other.BindingIndex && Size == other.Size && RelativeOffset == other.RelativeOffset && Type == other.Type && DataClass == other.DataClass && Normalized == other.Normalized;
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != this.GetType()) return false;
			return Equals((VertexAttribDescription)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = AttribIndex;
				hashCode = (hashCode * 397) ^ BindingIndex;
				hashCode = (hashCode * 397) ^ Size;
				hashCode = (hashCode * 397) ^ RelativeOffset;
				hashCode = (hashCode * 397) ^ (int)Type;
				hashCode = (hashCode * 397) ^ (int)DataClass;
				hashCode = (hashCode * 397) ^ Normalized.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(VertexAttribDescription left, VertexAttribDescription right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(VertexAttribDescription left, VertexAttribDescription right)
		{
			return !Equals(left, right);
		}
	}
}
