using System;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	/// <summary>
	/// Wraps OpenGL program pipeline object, which aggregates several separable shader stages into a single pipeline
	/// </summary>
	class ShaderPipeline : IDisposable
	{
		bool _rebuildShaderPipeline;

		internal ShaderPipelineDescription Description;

		int _handle = 0;

		public ShaderPipeline(ShaderPipelineDescription description)
		{
			Description = description;
			_rebuildShaderPipeline = true;
		}

		public void Bind(Device device)
		{
			if(_rebuildShaderPipeline)
			{
				Build();

				GL.BindProgramPipeline(_handle);
				device.ShaderPipeline = this;
			}
			else
			{
				if(device.ShaderPipeline != this)
				{
					GL.BindProgramPipeline(_handle);
					device.ShaderPipeline = this;
				}
			}
		}

		void Build()
		{
			_rebuildShaderPipeline = false;
			if(_handle > 0)
				GL.DeleteProgramPipeline(_handle);

			_handle = GL.GenProgramPipeline();

			if(Description.Vertex != null)
				GL.UseProgramStages(_handle, ProgramStageMask.VertexShaderBit, Description.Vertex.Handle);
			if(Description.TesselationControl != null)
				GL.UseProgramStages(_handle, ProgramStageMask.TessControlShaderBit, Description.TesselationControl.Handle);
			if(Description.TesselationEvaluation != null)
				GL.UseProgramStages(_handle, ProgramStageMask.TessEvaluationShaderBit, Description.TesselationEvaluation.Handle);
			if(Description.Geometry != null)
				GL.UseProgramStages(_handle, ProgramStageMask.GeometryShaderBit, Description.Geometry.Handle);
			if(Description.Fragment != null)
				GL.UseProgramStages(_handle, ProgramStageMask.FragmentShaderBit, Description.Fragment.Handle);
			if(Description.Compute != null)
				GL.UseProgramStages(_handle, ProgramStageMask.ComputeShaderBit, Description.Compute.Handle);
		}

		public void RebuildShaderPipeline()
		{
			_rebuildShaderPipeline = true;
		}

		public void Dispose()
		{
			if(_handle > 0)
				GL.DeleteProgramPipeline(_handle);
			_handle = 0;
			_rebuildShaderPipeline = false;
		}
	}

	class ShaderPipelineDescription : IEquatable<ShaderPipelineDescription>
	{
		public Shader Vertex;
		public Shader TesselationControl;
		public Shader TesselationEvaluation;
		public Shader Geometry;
		public Shader Fragment;
		public Shader Compute;

		public bool Equals(ShaderPipelineDescription other)
		{
			if(ReferenceEquals(null, other)) return false;
			if(ReferenceEquals(this, other)) return true;
			return Equals(Vertex, other.Vertex) && Equals(TesselationControl, other.TesselationControl) && Equals(TesselationEvaluation, other.TesselationEvaluation) && Equals(Geometry, other.Geometry) && Equals(Fragment, other.Fragment) && Equals(Compute, other.Compute);
		}

		public override bool Equals(object obj)
		{
			if(ReferenceEquals(null, obj)) return false;
			if(ReferenceEquals(this, obj)) return true;
			if(obj.GetType() != this.GetType()) return false;
			return Equals((ShaderPipelineDescription)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Vertex != null ? Vertex.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (TesselationControl != null ? TesselationControl.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (TesselationEvaluation != null ? TesselationEvaluation.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Geometry != null ? Geometry.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Fragment != null ? Fragment.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Compute != null ? Compute.GetHashCode() : 0);
				return hashCode;
			}
		}

		public static bool operator ==(ShaderPipelineDescription left, ShaderPipelineDescription right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ShaderPipelineDescription left, ShaderPipelineDescription right)
		{
			return !Equals(left, right);
		}
	}
}
