using System;
using System.Collections.ObjectModel;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public class VertexBufferSource
	{
		readonly ReadOnlyCollection<VertexBufferBinding> _bindings;

		readonly int _first;
		readonly int _count;

		public int Count { get { return _count; } }

		readonly int[] _buffers;
		readonly IntPtr[] _offsets;
		readonly int[] _strides;

		/// <summary>
		/// Describes buffers that will be used as a data source for rendering vertices
		/// </summary>
		/// <param name="firstBindingPoint">Binding point of the first buffer. Subsequent buffers will use sequentially higher binding points.</param>
		/// <param name="bindings">Array of vertex buffer bindings</param>
		public VertexBufferSource(int firstBindingPoint, params VertexBufferBinding[] bindings)
		{
			_first = firstBindingPoint;
			_count = bindings.Length;
			_bindings = new ReadOnlyCollection<VertexBufferBinding>(bindings);

			_buffers = new int[_count];
			_offsets = new IntPtr[_count];
			_strides = new int[_count];

			for(int i = 0; i < _count; i++)
			{
				var binding = _bindings[i];

				if(binding == null)
				{
					_buffers[i] = 0;
					_offsets[i] = IntPtr.Zero;
					_strides[i] = 0;
				}
				else
				{
					_buffers[i] = binding.Buffer;
					_offsets[i] = binding.Offset;
					_strides[i] = binding.Stride;
				}
			}
		}

		/// <summary>
		/// Returns one of this vertex buffer source's buffer binding objects. Index is relative to the number of bindings in this object, not the actual binding points of the buffers.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public VertexBufferBinding GetBinding(int index)
		{
			return _bindings[index];
		}

		internal void Bind()
		{
			for(int i = 0; i < _count; i++)
			{
				if(_bindings[i] != null)
				{
					_bindings[i].Bind(_first + i);
				}
			}
		}
	}

	/// <summary>
	/// Describes a single vertex buffer binding for use as a data source in vertex rendering
	/// </summary>
	public class VertexBufferBinding
	{
		// No need to make these readonly, since vertex buffer bindings are always bound (no redundant state change checks)
		public int Buffer;
		public IntPtr Offset;
		public int Stride;
		public int Divisor;

		// Binding index not included to force successing binding indices

		/// <summary>
		/// Creates a new instance of VertexBufferBinding
		/// </summary>
		/// <param name="buffer">OpenGL buffer handle</param>
		/// <param name="offset">Offset of the first element in the buffer</param>
		/// <param name="stride">Distance between elements in the buffer</param>
		/// <param name="divisor">Vertex attribute divisor for instanced rendering - instance step rate</param>
		public VertexBufferBinding(int buffer, int offset, int stride, int divisor)
		{
			Buffer = buffer;
			Offset = new IntPtr(offset);
			Stride = stride;
			Divisor = divisor;
		}

		internal void Bind(int bindingIndex)
		{
			GL.BindVertexBuffer(bindingIndex, Buffer, Offset, Stride);
			GL.VertexBindingDivisor(bindingIndex, Divisor);
		}
	}
}
