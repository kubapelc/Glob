using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL;

namespace Glob
{
	/// <summary>
	/// Contains info about current device, such as supported OpenGL version and extensions. Query extension availability using CheckExtensionAvailable() method.
	/// </summary>
	public class DeviceInfo
	{
		//GlobContext _context;

		public HashSet<string> Extensions { get; private set; }
		//string[] _extensionsOrdered;

		public string OpenGLVersion { get { return _openGlVersion; } }
		public string GLSLVersion { get { return _shadingLanguageVersion; } }
		public string DeviceRenderer { get { return _deviceRenderer; } }
		public string DeviceVendor { get { return _deviceVendor; } }

		string _openGlVersion = "Not available";
		string _shadingLanguageVersion = "Not available";
		string _deviceRenderer = "Not available";
		string _deviceVendor = "Not available";

		public DeviceInfo()
		{
			//_context = context;

			_openGlVersion = GL.GetString(StringName.Version);
			_shadingLanguageVersion = GL.GetString(StringName.ShadingLanguageVersion);
			_deviceRenderer = GL.GetString(StringName.Renderer);
			_deviceVendor = GL.GetString(StringName.Vendor);

			GetAvailableExtensions();
		}

		void GetAvailableExtensions()
		{
			int numExtensions = GL.GetInteger(GetPName.NumExtensions);

			Extensions = new HashSet<string>();

			for(int i = 0; i < numExtensions; i++)
			{
				string ext = GL.GetString(StringNameIndexed.Extensions, i);
				Extensions.Add(ext);
			}
		}

		public bool CheckExtensionAvailable(string ext)
		{
			return Extensions.Contains(ext);
		}
	}
}
