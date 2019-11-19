using System;
using System.IO;

namespace Glob
{
	public interface IFileManagerGlob
	{
		/// <summary>
		/// Returns stream of the specified file
		/// </summary>
		/// <param name="file">File path</param>
		/// <returns></returns>
		Stream GetStream(string file);

		/// <summary>
		/// Returns a path to a shader source file that can be passed to GetStream method.
		/// For example, this may return a combined path of "Shaders/file"
		/// </summary>
		/// <param name="file">Filename of the shader source file</param>
		/// <returns>Path to the shader source file</returns>
		string GetPathShaderSource(string file);

		/// <summary>
		/// Returns a path to a resolved shader file that can be passed to GetStream method.
		/// For example, this may return a combined path of "Shaders/Resolved/file"
		/// </summary>
		/// <param name="file">Filename of the shader source file</param>
		/// <returns>Path to the resolved shader source file</returns>
		string GetPathShaderCacheResolved(string file);

		/// <summary>
		/// Returns a path to a compiled shader that can be passed to GetStream method.
		/// For example, this may return a combined path of "Shaders/Compiled/file"
		/// </summary>
		/// <param name="file">Filename of the compiled shader file</param>
		/// <returns>Path to the compiled shader file</returns>
		string GetPathShaderCacheCompiled(string file);
	}
}
