using System;
using System.Collections.Generic;
using System.Text;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public class Device
	{
		// Quick-access to currently bound shaders

		/// <summary>
		/// Currently bound vertex shader
		/// </summary>
		public Shader ShaderVertex { get { return ShaderPipeline.Description.Vertex; } }

		/// <summary>
		/// Currently bound tesselation control shader
		/// </summary>
		public Shader ShaderTesselationControl { get { return ShaderPipeline.Description.TesselationControl; } }

		/// <summary>
		/// Currently bound tesselation evaulation shader
		/// </summary>
		public Shader ShaderTesselationEvaluation { get { return ShaderPipeline.Description.TesselationEvaluation; } }

		/// <summary>
		/// Currently bound geometry shader
		/// </summary>
		public Shader ShaderGeometry { get { return ShaderPipeline.Description.Geometry; } }

		/// <summary>
		/// Currently bound fragment shader
		/// </summary>
		public Shader ShaderFragment { get { return ShaderPipeline.Description.Fragment; } }

		/// <summary>
		/// Currently bound compute shader
		/// </summary>
		public Shader ShaderCompute { get { return ShaderPipeline.Description.Compute; } }

		/// <summary>
		/// Information about the OpenGL device - GL and GLSL versions and extensions
		/// </summary>
		public DeviceInfo DeviceInfo;

		/// <summary>
		/// Manages debug messages from OpenGL such as errors or performance warnings
		/// </summary>
		public DebugMessageManager DebugMessageManager;

		internal ITextOutputGlob TextOutput;
		internal IFileManagerGlob FileManager;

		internal ShaderRepository ShaderRepository;
		internal ShaderPipeline ShaderPipeline = null;

		TextureUnitState _textureUnit;
		BufferBindingManager _bufferBindingManager;

		VertexBufferFormat _boundVertexFormat;
		VertexBufferSource _boundVertexSource;

		RasterizerState _currentRasterizerState;
		DepthState _currentDepthState;
		BlendState _currentBlendState;

		int _boundReadFramebuffer = 0;
		int _boundDrawFramebuffer = 0;
		int _boundBothFramebuffer = 0;

		int _vaoGlobal = 0;

		/// <summary>
		/// Creates a new Glob Device. Can only be called once the OpenGL context is already created, for example by creating an OpenTK GameWindow.
		/// </summary>
		/// <param name="textOutput">Text output that will be used by Glob</param>
		/// <param name="fileManager">File manager that will be used by Glob</param>
		public Device(ITextOutputGlob textOutput, IFileManagerGlob fileManager)
		{
			TextOutput = textOutput;
			FileManager = fileManager;

			DeviceInfo = new DeviceInfo();

			// TODO: move these to DeviceInfo.ToString()
			TextOutput.Print(OutputTypeGlob.Notify, "OpenGL: " + DeviceInfo.OpenGLVersion + " GLSL: " + DeviceInfo.GLSLVersion);
			TextOutput.Print(OutputTypeGlob.Notify, "Renderer: " + DeviceInfo.DeviceRenderer + ", " + DeviceInfo.DeviceVendor);

			// Print extensions
			StringBuilder allExtensionsStr = new StringBuilder();

			foreach(string extension in DeviceInfo.Extensions)
			{
				allExtensionsStr.Append(extension);
				allExtensionsStr.Append(" ");
			}
			TextOutput.Print(OutputTypeGlob.Notify, DeviceInfo.Extensions.Count.ToString() + " GL extensions found");
			TextOutput.Print(OutputTypeGlob.LogOnly, allExtensionsStr.ToString());

			ShaderRepository = new ShaderRepository(this, "#extension GL_ARB_separate_shader_objects : enable\n");
			ShaderRepository.StartFileWatcher(FileManager.GetPathShaderSource(""));

			_bufferBindingManager = new BufferBindingManager();
			DebugMessageManager = new DebugMessageManager(true);
			_textureUnit = new TextureUnitState();
			_currentRasterizerState = new RasterizerState();
			_currentDepthState = new DepthState();
			_currentBlendState = new BlendState();

			// Create and bind a global vertex array object
			_vaoGlobal = GL.GenVertexArray();
			GL.BindVertexArray(_vaoGlobal);
		}

		/// <summary>
		/// Creates a shader from the input filename and list of define macros
		/// </summary>
		/// <param name="filename">Shader source filename</param>
		/// <param name="macros">List of macros to define. One tuple is one marco: the first item is the name, the second item is the value.</param>
		/// <returns>A compiled Shader object.</returns>
		public Shader GetShader(string filename, List<Tuple<string, string>> macros = null)
		{
			return ShaderRepository.GetShader(filename, macros);
		}

		/// <summary>
		/// Updates all shaders: if a source file changed, all shaders that use it will be recompiled.
		/// Returns immediately if no source files changed, thus can be called every frame. 
		/// </summary>
		public void Update()
		{
			ShaderRepository.Update();
		}

		/// <summary>
		/// Changes all current states to null, so any future bind or state change will effectively skip redundant state change checks and should execute its OpenGL calls.
		/// </summary>
		public void Invalidate()
		{
			ShaderPipeline = null;
			_textureUnit.Invalidate();
			_currentRasterizerState = null;
			_currentDepthState = null;
			_currentBlendState = null;
			_boundReadFramebuffer = -1;
			_boundDrawFramebuffer = -1;
			_boundBothFramebuffer = -1;
			_boundVertexFormat = null;
			_boundVertexSource = null;
			_bufferBindingManager.Invalidate();
		}

		/// <summary>
		/// Binds a texture.
		/// </summary>
		/// <param name="texture">Texture object</param>
		/// <param name="unit">Texture unit to bind the texture to. Useful when binding textures for use by a shader.</param>
		public void BindTexture(Texture texture, int unit = -1)
		{
			_textureUnit.BindTexture(texture.Target, texture.Handle, unit);
		}

		/// <summary>
		/// Binds a texture.
		/// </summary>
		/// <param name="target">Texture target to use.</param>
		/// <param name="texture">The texture's OpenGL handle</param>
		/// <param name="unit">Texture unit to bind the texture to. Useful when binding textures for use by a shader.</param>
		public void BindTexture(TextureTarget target, int texture, int unit = -1)
		{
			_textureUnit.BindTexture(target, texture, unit);
		}

		/// <summary>
		/// Binds a buffer to a binding point. Equivalent to glBindBufferBase with a check for redundant state changes.
		/// </summary>
		public void BindBufferBase(BufferRangeTarget target, int bindingPoint, int buffer)
		{
			_bufferBindingManager.BindBufferBase(target, bindingPoint, buffer);
		}

		/// <summary>
		/// Binds a buffer range to a binding point. Equivalent to glBindBufferRange with a check for redundant state changes.
		/// </summary>
		public void BindBufferRange(BufferRangeTarget target, int bindingPoint, int buffer, IntPtr offset, IntPtr size)
		{
			_bufferBindingManager.BindBufferRange(target, bindingPoint, buffer, offset, size);
		}

		/// <summary>
		/// Dispatches the given number of work groups of the currently bound compute shader in each dimension.
		/// </summary>
		public void DispatchComputeGroups(int groupsX, int groupsY = 1, int groupsZ = 1)
		{
			if(ShaderCompute.Valid)
				GL.DispatchCompute(groupsX, groupsY, groupsZ);
		}

		/// <summary>
		/// Dispatches at least the given number of threads of the currently bound compute shader in each dimension.
		/// Dispatches enough work groups, so that at least the input number of threads will be executed in that dimension.
		/// </summary>
		public void DispatchComputeThreads(int threadsX, int threadsY = 1, int threadsZ = 1)
		{
			if(!ShaderCompute.Valid)
				return;

			int w, h, d;
			w = (threadsX + ShaderCompute.WorkGroupSizeX - 1) / ShaderCompute.WorkGroupSizeX;
			h = (threadsY + ShaderCompute.WorkGroupSizeY - 1) / ShaderCompute.WorkGroupSizeY;
			d = (threadsZ + ShaderCompute.WorkGroupSizeZ - 1) / ShaderCompute.WorkGroupSizeZ;

			DispatchComputeGroups(w, h, d);
		}

		/// <summary>
		/// Binds a framebuffer object to a FramebufferTarget. Equivalent to glBindFramebuffer with a check for redundant state changes.
		/// </summary>
		/// <param name="fbo"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		public bool BindFrameBuffer(FrameBuffer fbo, FramebufferTarget target)
		{
			bool changed = false;
			if(target == FramebufferTarget.ReadFramebuffer)
			{
				if(_boundReadFramebuffer != fbo.Handle)
				{
					_boundReadFramebuffer = fbo.Handle;
					GL.BindFramebuffer(target, fbo.Handle);
					changed = true;
				}
			}
			if(target == FramebufferTarget.DrawFramebuffer)
			{
				if(_boundDrawFramebuffer != fbo.Handle)
				{
					_boundDrawFramebuffer = fbo.Handle;
					GL.BindFramebuffer(target, fbo.Handle);
					changed = true;
				}
			}
			if(target == FramebufferTarget.Framebuffer)
			{
				if(_boundBothFramebuffer != fbo.Handle)
				{
					_boundBothFramebuffer = fbo.Handle;
					GL.BindFramebuffer(target, fbo.Handle);
					changed = true;
				}
			}
			return changed;
		}

		/// <summary>
		/// Binds this framebuffer for rendering, also pushes viewport state and sets new viewport according to the parameters.
		/// Optionally creates and pushes a debug group marker.
		/// The viewport state and debug group marker must be popped later by disposing the returned object.
		/// Usage: call this method in a using statement and do all rendering in the using block. After the rendering is finished, the using statement automatically pops viewport and debug group marker.
		/// </summary>
		/// /// <param name="fbo">The FrameBuffer object to bind</param>
		/// <param name="message">Optional debug group name</param>
		/// <returns>A disposable object that, upon being disposed, pops viewport state and also pops a debug group marker.</returns>
		public IDisposable BindFrameBufferPushViewport(FrameBuffer fbo, string message = null)
		{
			return BindFrameBufferPushViewport(fbo, 0, 0, -1, -1, message);
		}

		/// <summary>
		/// Binds this framebuffer for rendering, also pushes viewport state and sets new viewport according to the parameters.
		/// Optionally creates and pushes a debug group marker.
		/// The viewport state and debug group marker must be popped later by disposing the returned object.
		/// Usage: call this method in a using statement and do all rendering in the using block. After the rendering is finished, the using statement automatically pops viewport and debug group marker.
		/// </summary>
		/// <param name="fbo">The FrameBuffer object to bind</param>
		/// <param name="x0">Viewport parameter x0</param>
		/// <param name="y0">Viewport parameter y0</param>
		/// <param name="x1">Viewport parameter x1</param>
		/// <param name="y1">Viewport parameter y1</param>
		/// <param name="message">Optional debug group name</param>
		/// <returns>A disposable object that, upon being disposed, pops viewport state and also pops a debug group marker.</returns>
		public IDisposable BindFrameBufferPushViewport(FrameBuffer fbo, int x0, int y0 = 0, int x1 = -1, int y1 = -1, string message = null)
		{
			return fbo.BindPushViewport(this, x0, y0, x1, y1, message);
		}

		/// <summary>
		/// Binds a graphics pipeline object, including all state it defines.
		/// </summary>
		public void BindPipeline(GraphicsPipeline pipeline)
		{
			pipeline.Bind(this);
		}

		/// <summary>
		/// Binds a compute pipeline object, including all state it defines, which is only the shader in the case of compute pipelines.
		/// </summary>
		public void BindPipeline(ComputePipeline pipeline)
		{
			pipeline.Bind(this);
		}

		/// <summary>
		/// Binds vertex buffer format
		/// </summary>
		public void BindVertexFormat(VertexBufferFormat format)
		{
			if(_boundVertexFormat == format)
				return;

			format.Bind(_boundVertexFormat);
			_boundVertexFormat = format;
		}

		/// <summary>
		/// Binds vertex buffer source
		/// </summary>
		public void BindVertexBufferSource(VertexBufferSource vertexBufferSource)
		{
			if(_boundVertexSource != vertexBufferSource)
				vertexBufferSource.Bind();
			_boundVertexSource = vertexBufferSource;
		}

		/// <summary>
		/// Binds a single vertex buffer, similarly to how BindVertexBufferSource() binds several at once
		/// </summary>
		/// <param name="binding">VertexBufferBinding object describing how to bind the buffer</param>
		/// <param name="bindingIndex">The binding index to use</param>
		public void BindVertexBuffer(VertexBufferBinding binding, int bindingIndex)
		{
			binding.Bind(bindingIndex);
		}

		/// <summary>
		/// Binds a rasterization state object
		/// </summary>
		public void BindRasterizerState(RasterizerState state)
		{
			if(state == null)
				throw new NullReferenceException("Rasterizer state cannot be null!");

			if(_currentRasterizerState == state)
				return;

			if(_currentRasterizerState == null || _currentRasterizerState.CullfaceState != state.CullfaceState)
			{
				if(_currentRasterizerState == null ||
				   (_currentRasterizerState.CullfaceState == CullfaceState.None) != (state.CullfaceState == CullfaceState.None))
				{
					if(state.CullfaceState == CullfaceState.None)
					{
						GL.Disable(EnableCap.CullFace);
					}
					else
					{
						GL.Enable(EnableCap.CullFace);
					}
				}

				if(state.CullfaceState == CullfaceState.Front)
					GL.CullFace(CullFaceMode.Front);
				if(state.CullfaceState == CullfaceState.Back)
					GL.CullFace(CullFaceMode.Back);
				if(state.CullfaceState == CullfaceState.FrontAndBack)
					GL.CullFace(CullFaceMode.FrontAndBack);

				if(state.PolygonModeFront == state.PolygonModeBack)
				{
					if(_currentRasterizerState == null || _currentRasterizerState.PolygonModeFront != state.PolygonModeFront || _currentRasterizerState.PolygonModeBack != state.PolygonModeBack)
					{
						GL.PolygonMode(MaterialFace.FrontAndBack, state.PolygonModeFront);
					}
				}
				else
				{
					if(_currentRasterizerState == null || _currentRasterizerState.PolygonModeFront != state.PolygonModeFront)
						GL.PolygonMode(MaterialFace.Front, state.PolygonModeFront);
					if(_currentRasterizerState == null || _currentRasterizerState.PolygonModeBack != state.PolygonModeBack)
						GL.PolygonMode(MaterialFace.Back, state.PolygonModeBack);
				}
			}

			_currentRasterizerState = state;
		}

		/// <summary>
		/// Binds a depth state object
		/// </summary>
		public void BindDepthState(DepthState state)
		{
			if(_currentDepthState == null || _currentDepthState.DepthMask != state.DepthMask)
			{
				GL.DepthMask(state.DepthMask);
			}

			// Commented out if conditions are base on the assumtion that depthFunc is only affected if depthmask is enabled, which seems to not be the case

			//if(DepthMask != newState.DepthMask && newState.DepthMask)
			//{
			//	DepthMask = newState.DepthMask;
			//	GL.DepthMask(newState.DepthMask);
			//}

			if(_currentDepthState == null || _currentDepthState.DepthFunction != state.DepthFunction)
			{
				GL.DepthFunc(state.DepthFunction);
			}

			//if(DepthMask != newState.DepthMask && !newState.DepthMask)
			//{
			//	DepthMask = newState.DepthMask;
			//	GL.DepthMask(newState.DepthMask);
			//}

			_currentDepthState = state;
		}

		/// <summary>
		/// Binds a blend state object
		/// </summary>
		public void BindBlendState(BlendState state)
		{
			if(_currentBlendState == null ||
				_currentBlendState.ColorMaskR != state.ColorMaskR ||
				_currentBlendState.ColorMaskG != state.ColorMaskG ||
				_currentBlendState.ColorMaskB != state.ColorMaskB ||
				_currentBlendState.ColorMaskA != state.ColorMaskA)
			{
				GL.ColorMask(state.ColorMaskR, state.ColorMaskG, state.ColorMaskB, state.ColorMaskA);
			}

			if(_currentBlendState == null || !_currentBlendState.ColorBlendMode.Equals(state.ColorBlendMode))
			{
				if(_currentBlendState == null || (!_currentBlendState.ColorBlendMode.IsBlendingEnabled && state.ColorBlendMode.IsBlendingEnabled))
					GL.Enable(EnableCap.Blend);
				if(_currentBlendState == null || (_currentBlendState.ColorBlendMode.IsBlendingEnabled && !state.ColorBlendMode.IsBlendingEnabled))
					GL.Disable(EnableCap.Blend);

				if(_currentBlendState == null
					|| (_currentBlendState.ColorBlendMode.BlendSrcRgb != state.ColorBlendMode.BlendSrcRgb)
					|| (_currentBlendState.ColorBlendMode.BlendDstRgb != state.ColorBlendMode.BlendDstRgb)
					|| (_currentBlendState.ColorBlendMode.BlendSrcAlpha != state.ColorBlendMode.BlendSrcAlpha)
					|| (_currentBlendState.ColorBlendMode.BlendDstAlpha != state.ColorBlendMode.BlendDstAlpha))
					GL.BlendFuncSeparate(state.ColorBlendMode.BlendSrcRgb, state.ColorBlendMode.BlendDstRgb, state.ColorBlendMode.BlendSrcAlpha,
					state.ColorBlendMode.BlendDstAlpha);

				if(_currentBlendState == null
					|| (_currentBlendState.ColorBlendMode.BlendModeRgb != state.ColorBlendMode.BlendModeRgb)
					|| (_currentBlendState.ColorBlendMode.BlendModeAlpha != state.ColorBlendMode.BlendModeAlpha))
					GL.BlendEquationSeparate(state.ColorBlendMode.BlendModeRgb, state.ColorBlendMode.BlendModeAlpha);
			}

			_currentBlendState = state;
		}

		/// <summary>
		/// Binds a Image2D for use in shaders using image load store.
		/// </summary>
		/// <param name="unit">Image unit to bind the texture to</param>
		/// <param name="texture">The texture object to bind, can be null to unbind a texture</param>
		/// <param name="access">Texture access mode</param>
		/// <param name="level">The mip level of the texture to bind</param>
		/// <param name="layer">The layer of the texture to bind - only relevant when binding a single layer of a 2D array or 3D texture as a 2D image.</param>
		public void BindImage2D(int unit, Texture texture, TextureAccess access, int level = 0, int layer = 0)
		{
			BindImage(unit, texture, access, false, level, layer);
		}


		/// <summary>
		/// Binds a Image3D for use in shaders using image load store.
		/// </summary>
		/// <param name="unit">Image unit to bind the texture to</param>
		/// <param name="texture">The texture object to bind, can be null to unbind a texture</param>
		/// <param name="access">Texture access mode</param>
		/// <param name="level">The mip level of the texture to bind</param>
		public void BindImage3D(int unit, Texture texture, TextureAccess access, int level = 0)
		{
			BindImage(unit, texture, access, true, level, 0);
		}

		private void BindImage(int unit, Texture texture, TextureAccess access, bool layered, int level, int layer)
		{
			GL.BindImageTexture(unit, texture?.Handle ?? 0, level, layered, layer, access, (SizedInternalFormat)(texture != null ? texture.Format : SizedInternalFormatGlob.R32UI));
		}
	}
}
