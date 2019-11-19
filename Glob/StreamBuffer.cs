using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	// TODO: docs and comments
	/// <summary>
	/// Buffer that can efficiently pass data to the GPU, internally implemented using a persistently mapped ring buffer.
	/// 
	/// Usage:
	/// 1) Copy source data to CurrentStart of this buffer
	/// 2) Store current value of CurrentOffset somewhere
	/// 3) Call Advance() with a FenceSync object
	/// 4) Call GL.MemoryBarrier(MemoryBarrierFlags.ClientMappedBufferBarrierBit) to make the data visible to the GPU
	/// 5) Copy the data located at the stored offset value to a GPU-side buffer or use it directly
	/// 6) After rendering current frame, call ClientMappedBufferBarrierBit memory barrier again and call the Create() method of the passed FenceSync object
	/// 7) Repeat next frame
	/// </summary>
	public class StreamBuffer
	{
		/// <summary>
		/// Debug counter: total number of bytes that can be passed to the GPU in one frame using all created StreamBuffers.
		/// Used to get a quick approximate measure of CPU->GPU memory traffic.
		/// </summary>
		public static int TotalBytesPerFrame { get; private set; } = 0;

		int _position;
		int _bufferingLevel;
		int _handle;

		/// <summary>
		/// The OpenGL buffer object's handle
		/// </summary>
		public int Handle { get { return _handle; } }

		/// <summary>
		/// The number of bytes that can be used in one frame
		/// </summary>
		public readonly int Size;

		IntPtr _bufferPointer;

		FenceSync[] _fences;

		/// <summary>
		/// The pointer to the start of current buffer segment's storage
		/// </summary>
		public IntPtr CurrentStart { get { return IntPtr.Add(_bufferPointer, CurrentOffset); } }

		/// <summary>
		/// Offset of the current buffer segment in bytes. Copy source data to this offset, after
		/// </summary>
		public int CurrentOffset { get { return Size * _position; } }

		public StreamBuffer(string name, BufferTarget target, int size, BufferStorageFlags flags = BufferStorageFlags.MapWriteBit, BufferAccessMask mask = (BufferAccessMask)0, int bufferingLevel = 3)
		{
			Size = size;

			TotalBytesPerFrame += size;

			_bufferingLevel = bufferingLevel;
			_fences = new FenceSync[bufferingLevel];
			_handle = GL.GenBuffer();
			GL.BindBuffer(target, _handle);
			GL.BufferStorage(target, new IntPtr(size * bufferingLevel), IntPtr.Zero,
				(BufferStorageFlags)flags | BufferStorageFlags.MapPersistentBit);
			Glob.Utils.SetObjectLabel(ObjectLabelIdentifier.Buffer, _handle, name);

			var accessFlags = (BufferAccessMask)0;

			if(flags.HasFlag(BufferStorageFlags.MapWriteBit))
				accessFlags = accessFlags | BufferAccessMask.MapWriteBit;
			if(flags.HasFlag(BufferStorageFlags.MapReadBit))
				accessFlags = accessFlags | BufferAccessMask.MapReadBit;

			_bufferPointer = GL.MapBufferRange(target, IntPtr.Zero, new IntPtr(size * bufferingLevel),
				accessFlags | mask | BufferAccessMask.MapPersistentBit);
		}

		/// <summary>
		/// Advances the CurrentStart and CurrentOffset pointers to the next buffer segment. If the next segment is still in use by the GPU, waits until it the GPU is finished using it. (Achieved using the FenceSync object)
		/// </summary>
		/// <param name="sync">FenceSync object that will be created after</param>
		public void Advance(FenceSync sync)
		{
			_position++;
			if(_position == _bufferingLevel)
				_position = 0;
			_fences[_position]?.ClientWaitSync();
			_fences[_position] = sync;
		}
	}
}
