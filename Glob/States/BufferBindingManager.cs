using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	internal class BufferBindingManager
	{
		const int BufferBindingPoints = 32;
		Dictionary<BufferRangeTarget, BufferBinding[]> _bufferBindings = new Dictionary<BufferRangeTarget, BufferBinding[]>();

		public BufferBindingManager()
		{
			_bufferBindings = new Dictionary<BufferRangeTarget, BufferBinding[]>();
		}

		public void Invalidate()
		{
			_bufferBindings.Clear();
		}

		public void BindBufferBase(BufferRangeTarget target, int bindingPoint, int buffer)
		{
			if(!_bufferBindings.ContainsKey(target))
				_bufferBindings[target] = new BufferBinding[BufferBindingPoints];

			var binding = new BufferBinding(buffer, IntPtr.Zero, IntPtr.Zero);
			if(IsBindingDifferent(target, bindingPoint, binding))
			{
				GL.BindBufferBase(target, bindingPoint, buffer);
				_bufferBindings[target][bindingPoint] = binding;
			}
		}

		public void BindBufferRange(BufferRangeTarget target, int bindingPoint, int buffer, IntPtr offset, IntPtr size)
		{
			if(!_bufferBindings.ContainsKey(target))
				_bufferBindings[target] = new BufferBinding[BufferBindingPoints];

			var binding = new BufferBinding(buffer, offset, size);
			if(IsBindingDifferent(target, bindingPoint, binding))
			{
				GL.BindBufferRange(target, bindingPoint, buffer, offset, size);
				_bufferBindings[target][bindingPoint] = binding;
			}
		}

		bool IsBindingDifferent(BufferRangeTarget target, int bindingPoint, BufferBinding binding)
		{
			if(_bufferBindings[target][bindingPoint] == null)
				return true;

			var old = _bufferBindings[target][bindingPoint];
			if(old.Buffer != binding.Buffer)
				return true;
			if(old.Offset != binding.Offset)
				return true;
			if(old.Size != binding.Size)
				return true;
			return false;
		}

		class BufferBinding
		{
			public readonly int Buffer;
			public readonly IntPtr Offset;
			public readonly IntPtr Size;

			public BufferBinding(int buffer, IntPtr offset, IntPtr size)
			{
				Buffer = buffer;
				Offset = offset;
				Size = size;
			}
		}
	}
}
