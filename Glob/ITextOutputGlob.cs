using System;
namespace Glob
{
	public enum OutputTypeGlob
	{
		LogOnly,
		Debug,
		Notify,
		Warning,
		PerformanceWarning,
		Error,
	}

	public interface ITextOutputGlob
	{
		void Print(OutputTypeGlob type, string message);
	}
}
