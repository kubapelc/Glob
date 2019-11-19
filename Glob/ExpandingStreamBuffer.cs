using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	// TODO: revisit, possibly useful
	/*
	public struct StreamBufferInfo
	{
		public readonly IntPtr Data;
		public readonly int Handle;

		public StreamBufferInfo(IntPtr data, int handle)
		{
			Data = data;
			Handle = handle;
		}
	}

	/// <summary>
	/// Helper object for uploading data to the GPU efficiently using persistent buffer mapping. Buffer size is unlimited, old buffers will be reallocated if their size is insufficient. Buffer size only expands, never shrinks.
	/// </summary>
	public class ExpandingStreamBuffer
	{
		const int BufferingLevel = 3;

		readonly int _chunkSize;
		readonly BufferTarget _target;
		readonly BufferStorageFlags _storageFlags;
		readonly BufferAccessMask _accessMask;
		readonly string _name;

		int _position = 0;
		int _size = 0;

		int[] _bufferSizes;
		StreamBufferInfo[] _bufferInfos;
		FenceSync[] _fences;

		public ExpandingStreamBuffer(string name, BufferTarget target, int chunkSize, bool clientStorage = false)
		{
			_name = name;
			_target = target;
			_chunkSize = chunkSize;
			_storageFlags = BufferStorageFlags.MapWriteBit | BufferStorageFlags.MapPersistentBit;
			_accessMask = BufferAccessMask.MapWriteBit | BufferAccessMask.MapPersistentBit;

			if(clientStorage)
			{
				_storageFlags |= BufferStorageFlags.ClientStorageBit;
			}

			_bufferSizes = new int[BufferingLevel];
			_bufferInfos = new StreamBufferInfo[BufferingLevel];
			_fences = new FenceSync[BufferingLevel];

			_size = _chunkSize;

			for(int i = 0; i < BufferingLevel; i++)
			{
				_bufferSizes[i] = 0;
				_bufferInfos[i] = new StreamBufferInfo(IntPtr.Zero, 0);
				_fences[i] = null;
			}
		}

		int GetChunkedSize(int size)
		{
			return ((size + _chunkSize - 1) / _chunkSize) * _chunkSize;
		}

		/// <summary>
		/// Call before uploading data to the buffer.
		/// </summary>
		/// <param name="sync">Fence sync object that should get signaled after the data you are about to upload to this buffer is no longer needed.</param>
		/// <param name="bytes">Number of bytes that will be stored in this buffer</param>
		/// <returns>Struct containing handle of the current buffer and pointer to its memory</returns>
		public StreamBufferInfo GetBuffer(FenceSync sync, int bytes)
		{
			// Wait for any operation still using the current buffer's previous contents
			_fences[_position]?.ClientWaitSync();
			_fences[_position] = sync;

			// Calculate needed buffer size, rounded up to a multiple of chunk size
			_size = Math.Max(_size, GetChunkedSize(bytes));

			// Reallocate the buffer if the current size is too small
			if(_bufferSizes[_position] < _size)
			{
				if(_bufferInfos[_position].Handle > 0)
				{
					GL.BindBuffer(_target, _bufferInfos[_position].Handle);
					GL.UnmapBuffer(_target);
					GL.BindBuffer(_target, 0);
					GL.DeleteBuffer(_bufferInfos[_position].Handle);
				}

				var handle = GL.GenBuffer();
				GL.BindBuffer(_target, handle);
				GL.BufferStorage(_target, new IntPtr(_size), IntPtr.Zero, _storageFlags);
				Glob.Utils.SetObjectLabel(ObjectLabelIdentifier.Buffer, handle, _name + "-" + _position.ToString());

				var ptr = GL.MapBufferRange(_target, IntPtr.Zero, new IntPtr(_size), _accessMask);

				GL.BindBuffer(_target, 0);

				_bufferInfos[_position] = new StreamBufferInfo(ptr, handle);
				_bufferSizes[_position] = _size;
			}

			var info = _bufferInfos[_position];

			// Advance
			_position++;
			if(_position == BufferingLevel)
				_position = 0;

			return info;
		}
	}
	*/
}
