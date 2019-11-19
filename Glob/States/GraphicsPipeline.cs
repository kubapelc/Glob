using System;

namespace Glob
{
	public class GraphicsPipeline : IDisposable
	{
		Device _device;

		ShaderPipeline _shaderPipeline;
		VertexBufferFormat _vertexFormat;
		RasterizerState _rasterizerState;
		DepthState _depthState;
		BlendState _blendState;

		public Shader ShaderVertex { get { return _shaderPipeline.Description.Vertex; } }
		public Shader ShaderTesselationControl { get { return _shaderPipeline.Description.TesselationControl; } }
		public Shader ShaderTesselationEvaluation { get { return _shaderPipeline.Description.TesselationEvaluation; } }
		public Shader ShaderGeometry { get { return _shaderPipeline.Description.Geometry; } }
		public Shader ShaderFragment { get { return _shaderPipeline.Description.Fragment; } }

		/// <summary>
		/// Creates a new graphics pipeline
		/// </summary>
		/// <param name="device">Glob device</param>
		/// <param name="vertex">Vertex shader</param>
		/// <param name="fragment">Fragment shader</param>
		/// <param name="vertexFormat">Vertex buffer format - can be null</param>
		/// <param name="rasterizerState">Rasterization state - cull face and polygon mode</param>
		/// <param name="depthState">Depth state - depth test func and depth mask</param>
		/// <param name="blendState">Blend state - color/alpha blending mode - can be null</param>
		public GraphicsPipeline(Device device, Shader vertex, Shader fragment, VertexBufferFormat vertexFormat, RasterizerState rasterizerState, DepthState depthState, BlendState blendState = null)
			: this(device, vertex, fragment, null, null, null, vertexFormat, rasterizerState, depthState, blendState)
		{
		}

		/// <summary>
		/// Creates a new graphics pipeline
		/// </summary>
		/// <param name="device">Glob device</param>
		/// <param name="vertex">Vertex shader</param>
		/// <param name="tesselationControl">Tesselation control shader - can be null</param>
		/// <param name="tesselationEvaluation">Tesselation evaluation shader - can be null</param>
		/// <param name="geometry">Geometry shader - can be null</param>
		/// <param name="fragment">Fragment shader</param>
		/// <param name="vertexFormat">Vertex buffer format - can be null</param>
		/// <param name="rasterizerState">Rasterization state - cull face and polygon mode</param>
		/// <param name="depthState">Depth state - depth test func and depth mask</param>
		/// <param name="blendState">Blend state - color/alpha blending mode - can be null</param>
		public GraphicsPipeline(Device device, Shader vertex, Shader fragment, Shader tesselationControl, Shader tesselationEvaluation, Shader geometry, VertexBufferFormat vertexFormat, RasterizerState rasterizerState, DepthState depthState, BlendState blendState = null)
		{
			_device = device;
			_shaderPipeline = _device.ShaderRepository.GetShaderPipeline(vertex, tesselationControl, tesselationEvaluation, geometry, fragment, null);
			_vertexFormat = vertexFormat;
			_rasterizerState = rasterizerState;
			_depthState = depthState;
			_blendState = blendState;
			if(_blendState == null)
			{
				_blendState = new BlendState();
			}
		}

		internal void Bind(Device device)
		{
			_shaderPipeline.Bind(device);
			if(_vertexFormat != null)
				device.BindVertexFormat(_vertexFormat);
			device.BindRasterizerState(_rasterizerState);
			device.BindDepthState(_depthState);
			device.BindBlendState(_blendState);
		}

		public void Dispose()
		{
			this._shaderPipeline.Dispose();
		}
	}
}
