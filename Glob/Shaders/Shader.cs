using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	public class Shader
	{
		internal ShaderSource BaseSource { get; private set; }
		internal ShaderStage Stage { get { return _stage; } }
		internal int Handle { get { return _handle; } }
		internal bool Valid { get { return _valid; } }
		
		internal List<ShaderPipeline> Pipelines { get; private set; }
		internal List<Tuple<string, string>> Macros { get; private set; }

		public int WorkGroupSizeX { get { return _workGroupSizeX; } }
		public int WorkGroupSizeY { get { return _workGroupSizeY; } }
		public int WorkGroupSizeZ { get { return _workGroupSizeZ; } }

		Dictionary<string, int> _uniformLocations;
		Dictionary<string, IUniformValue> _uniformValues;
		Dictionary<string, int> _uniformBlockBindings;
		Dictionary<string, int> _shaderStorageBlockBindings;

		Device _device;
		int _handle;
		string _lastLog;
		ShaderStage _stage;
		bool _valid = false;

		int _workGroupSizeX;
		int _workGroupSizeY;
		int _workGroupSizeZ;

		internal Shader(Device device, ShaderStage stage, ShaderSource baseSource, List<Tuple<string, string>> macros)
		{
			Pipelines = new List<ShaderPipeline>();
			_device = device;
			BaseSource = baseSource;
			_stage = stage;
			Macros = macros;

			_uniformLocations = new Dictionary<string, int>();
			_uniformValues = new Dictionary<string, IUniformValue>();
			_uniformBlockBindings = new Dictionary<string, int>();
			_shaderStorageBlockBindings = new Dictionary<string, int>();
		}

		internal void CompileFromSource(ResolvedShader resolved)
		{
			if(_handle > 0)
				GL.DeleteProgram(_handle);

			_handle = GL.CreateShaderProgram((ShaderType)_stage, 1, new string[] {resolved.Code});

			string log = GL.GetProgramInfoLog(_handle);
			_lastLog = log;

			int isLinked = -1;
			GL.GetProgram(_handle, GetProgramParameterName.LinkStatus, out isLinked);
			if(isLinked != 1)
			{
				_valid = false;

				_device.TextOutput.Print(OutputTypeGlob.Error, "Error compiling " + _stage.ToString() + " shader " + BaseSource.Filename);
				
				GL.DeleteProgram(_handle);
				_handle = 0;

				var lines = Regex.Split(log, "\r\n|\r|\n");

				StringBuilder outputSb = new StringBuilder();

				// Find error messages and translate resolved code line number to local line number in the responsible file
				foreach(var line in lines)
				{
					var match = Regex.Match(line, @"^ERROR:.*:([0-9]+):.*$");
					var match2 = Regex.Match(line, @"^0\(([0-9]+)\)");
					outputSb.AppendLine(line);
					if(match.Success)
					{
						int lineNum = int.Parse(match.Groups[1].Value);
						outputSb.AppendLine("At: " + resolved.GetLineOrigin(lineNum));
					}
					else if(match2.Success)
					{
						int lineNum = int.Parse(match2.Groups[1].Value) - 1;
						outputSb.AppendLine("At: " + resolved.GetLineOrigin(lineNum));
					}
				}

				_device.TextOutput.Print(OutputTypeGlob.Debug, outputSb.ToString());
			}
			else
			{
				_valid = true;
			}

			// Query the work group size
			_workGroupSizeX = 0;
			_workGroupSizeY = 0;
			_workGroupSizeZ = 0;

			if(_stage == ShaderStage.Compute && _handle != 0)
			{
				int[] wgSize = new int[3];
				GL.GetProgram(_handle, (GetProgramParameterName)0x8267, wgSize);
				_workGroupSizeX = wgSize[0];
				_workGroupSizeY = wgSize[1];
				_workGroupSizeZ = wgSize[2];
			}

			if(_handle != 0)
			{
				Utils.SetObjectLabel(ObjectLabelIdentifier.Program, _handle, BaseSource.Filename);

				FinalizeCompilation();
			}
		}

		void FinalizeCompilation()
		{
			// Set all uniforms again
			_uniformLocations.Clear();

			var uniforms = _uniformValues.ToList();
			_uniformValues.Clear();
			foreach(var uniformValue in uniforms)
			{
				SetUniform(uniformValue.Key, uniformValue.Value);
			}
			var uniformBlockBindings = _uniformBlockBindings.ToList();
			_uniformBlockBindings.Clear();
			foreach(var binding in uniformBlockBindings)
			{
				UniformBlockBinding(binding.Key, binding.Value);
			}

			var shaderStorageBlockBindings = _shaderStorageBlockBindings.ToList();
			_shaderStorageBlockBindings.Clear();
			foreach(var binding in shaderStorageBlockBindings)
			{
				ShaderStorageBlockBinding(binding.Key, binding.Value);
			}
		}

		#region Uniforms and Inputs

		public void UniformBlockBinding(string blockName, int bindingPoint)
		{
			if(_uniformBlockBindings.ContainsKey(blockName))
			{
				_uniformBlockBindings[blockName] = bindingPoint;
			}
			else
			{
				_uniformBlockBindings.Add(blockName, bindingPoint);
			}

			GL.UniformBlockBinding(_handle, GL.GetUniformBlockIndex(_handle, blockName), bindingPoint);
		}

		public void ShaderStorageBlockBinding(string blockName, int bindingPoint)
		{
			if(_shaderStorageBlockBindings.ContainsKey(blockName))
			{
				_shaderStorageBlockBindings[blockName] = bindingPoint;
			}
			else
			{
				_shaderStorageBlockBindings.Add(blockName, bindingPoint);
			}

			GL.ShaderStorageBlockBinding(_handle, GL.GetProgramResourceIndex(_handle, ProgramInterface.ShaderStorageBlock, blockName), bindingPoint);
		}

		internal int GetUniformLocation(string uniform)
		{
			if(_uniformLocations.ContainsKey(uniform))
			{
				return _uniformLocations[uniform];
			}
			else
			{
				// glGetUniformLocation on an invalid shader results in an error
				if(!_valid)
					return 0;

				int loc = GL.GetUniformLocation(_handle, uniform);
				_uniformLocations.Add(uniform, loc);
				return loc;
			}
		}

		void SetUniform(string name, IUniformValue uniformValue)
		{
			bool same = false;

			if(_uniformValues.ContainsKey(name))
			{
				same = _uniformValues[name].Equals(uniformValue);
				_uniformValues[name] = uniformValue;
			}
			else
			{
				_uniformValues.Add(name, uniformValue);
			}

			// Do not attempt to set uniform for invalid shaders
			if(!same && _valid)
			{
				uniformValue.Set(_handle, GetUniformLocation(name));
			}
		}

		public void SetUniformF(string uniform, float value)
		{
			IUniformValue uniformValue = new UniformFloat(value);
			SetUniform(uniform, uniformValue);
		}
		
		public void SetUniformF(string uniform, Vector2 value)
		{
			IUniformValue uniformValue = new UniformVector2(value);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformF(string uniform, Vector3 value)
		{
			IUniformValue uniformValue = new UniformVector3(value);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformF(string uniform, Vector4 value)
		{
			IUniformValue uniformValue = new UniformVector4(value);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformI(string uniform, int x)
		{
			IUniformValue uniformValue = new UniformInt(x);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformI(string uniform, int x, int y)
		{
			IUniformValue uniformValue = new UniformVector2Int(x, y);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformI(string uniform, int x, int y, int z)
		{
			IUniformValue uniformValue = new UniformVector3Int(x, y, z);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformI(string uniform, int x, int y, int z, int w)
		{
			IUniformValue uniformValue = new UniformVector4Int(x, y, z, w);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformD(string uniform, double value)
		{
			IUniformValue uniformValue = new UniformDouble(value);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformD(string uniform, Vector2d value)
		{
			IUniformValue uniformValue = new UniformVector2Double(value);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformD(string uniform, Vector3d value)
		{
			IUniformValue uniformValue = new UniformVector3Double(value);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformD(string uniform, Vector4d value)
		{
			IUniformValue uniformValue = new UniformVector4Double(value);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformF(string uniform, Matrix2 value)
		{
			IUniformValue uniformValue = new UniformMatrix2(value);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformF(string uniform, Matrix3 value)
		{
			IUniformValue uniformValue = new UniformMatrix3(value);
			SetUniform(uniform, uniformValue);
		}

		public void SetUniformF(string uniform, Matrix4 value)
		{
			IUniformValue uniformValue = new UniformMatrix4(value);
			SetUniform(uniform, uniformValue);
		}
		
		#endregion
	}
}
