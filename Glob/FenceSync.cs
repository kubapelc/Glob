using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public class FenceSync : IDisposable
	{
		// TODO: docs and comments
		public const long DefaultTimeout = 3000000000; // 3 seconds - drivers usually kill the gpu if it doesn't respond for 2 seconds
		IntPtr _handle = IntPtr.Zero;
		long _frameNumber;

		public FenceSync(long frameNumber)
		{
			_frameNumber = frameNumber;
		}

		public void Create()
		{
			_handle = GL.FenceSync(SyncCondition.SyncGpuCommandsComplete, WaitSyncFlags.None);
		}

		public WaitSyncStatus ClientWaitSync(long timeout = DefaultTimeout, bool flush = false)
		{
			var flags = ClientWaitSyncFlags.None;
			if(flush)
				flags = flags | ClientWaitSyncFlags.SyncFlushCommandsBit;
			var result = GL.ClientWaitSync(_handle, flags, timeout);

			return result;
		}

		public void ServerWaitSync(long timeout = DefaultTimeout)
		{
			var flags = WaitSyncFlags.None;
			GL.WaitSync(_handle, flags, timeout);
		}

		public void Dispose()
		{
			GL.DeleteSync(_handle);
			_handle = IntPtr.Zero;
		}
	}
}
