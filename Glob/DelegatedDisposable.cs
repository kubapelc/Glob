using System;

namespace Glob
{
	class DelegatedDisposable : IDisposable
	{
		Action _dispose;

		public DelegatedDisposable(Action dispose)
		{
			_dispose = dispose;
		}

		public void Dispose()
		{
			_dispose();
		}
	}

	class EmptyDisposable : IDisposable
	{
		public void Dispose()
		{
			
		}
	}
}
