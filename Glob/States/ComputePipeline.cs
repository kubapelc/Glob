using System;

namespace Glob
{
	public class ComputePipeline : IDisposable
	{
		ShaderPipeline _shaderPipeline;

		public Shader ShaderCompute { get { return _shaderPipeline.Description.Compute; } }

		public ComputePipeline(Device device, Shader compute)
		{
			_shaderPipeline = device.ShaderRepository.GetShaderPipeline(null, null, null, null, null, compute);
		}

		internal void Bind(Device device)
		{
			_shaderPipeline.Bind(device);
		}

		public void Dispose()
		{
			this._shaderPipeline.Dispose();
		}
	}
}
