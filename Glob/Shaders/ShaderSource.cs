using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Glob
{
	class ShaderSource
	{
		/// <summary>
		/// For debug purposes such as detecting circular dependencies
		/// </summary>
		internal string Filename { get; set; }

		/// <summary>
		/// Source code with include statements commented out. Dependencies are not resolved.
		/// </summary>
		public string ParsedCode { get; private set; }

		public string VersionString { get; private set; }

		// Dependencies, inclusion, usages and updates are all handled by ShaderRepository

		/// <summary>
		/// Files directly included into this shader file
		/// </summary>
		public HashSet<ShaderSource> Dependencies { get; private set; }

		/// <summary>
		/// Other shader files that directly include this file
		/// </summary>
		public HashSet<ShaderSource> Inclusions { get; private set; }

		/// <summary>
		/// List of all shaders that directly use this file as a primary shader file
		/// </summary>
		public List<Shader> Usages { get; private set; }

		Device _device;

		internal ShaderSource(Device device)
		{
			_device = device;
			Dependencies = new HashSet<ShaderSource>();
			Inclusions = new HashSet<ShaderSource>();
			Usages = new List<Shader>();
		}

		internal static ShaderSource CreateNullSource(Device device, string versionString)
		{
			var src = new ShaderSource(device);
			src.Filename = "Null source file";
			src.ParsedCode = "";
			src.VersionString = versionString;
			return src;
		}

		internal void Parse(ShaderRepository repository, string source)
		{
			// Parse include statements
			// TODO: regex match includes all leading new line chars

			var versionMatches = Regex.Matches(source, @"^\s*\#version\s+(.*?)\s*$", RegexOptions.Multiline);

			if(versionMatches.Count > 0)
			{
				VersionString = versionMatches[0].Groups[1].Value;

				if(versionMatches.Count > 1)
				{
					_device.TextOutput.Print(OutputTypeGlob.Warning, "Shader source " + Filename + " contains multiple #version statements!");
				}
			}
			else
			{
				VersionString = repository.DefaultVersionString;

				_device.TextOutput.Print(OutputTypeGlob.Debug, "Shader source " + Filename + " contains no #version statement!");
			}
			
			var matches = Regex.Matches(source, @"^\s*\#include\s+(.*?)\s*$", RegexOptions.Multiline);

			foreach(Match m in matches)
			{
				var dependency = repository.GetShaderSource(m.Groups[1].Value.Trim());
				repository.AddDependency(this, dependency);
			}

			// TODO: version handling

			// Remove include statements from source

			ParsedCode = source;

			ParsedCode = ParsedCode.Replace("\r\n", "\n");

			ParsedCode = Regex.Replace(ParsedCode, @"^\s*\#include\s+(.*?)\s*\r?$", match =>
			{
				return "/* " + match.Value + " */";
			}, RegexOptions.Multiline);

			ParsedCode = Regex.Replace(ParsedCode, @"^\s*\#version\s+(.*?)\s*$", match =>
			{
				return "/* " + match.Value + " */";
			}, RegexOptions.Multiline);
		}

		void GetDependencyOrder(HashSet<ShaderSource> link, Dictionary<ShaderSource, int> order, int level)
		{
			if(!order.ContainsKey(this))
				order.Add(this, level);
			else
				order[this] = Math.Max(level, order[this]);

			if(link.Contains(this))
			{
				_device.TextOutput.Print(OutputTypeGlob.Error, "Shader source file " + this.Filename + " contains cyclical dependency!");
				return;
			}

			link.Add(this);

			foreach(ShaderSource dependency in Dependencies)
			{
				dependency.GetDependencyOrder(link, order, level + 1);
			}

			link.Remove(this);
		}

		/// <summary>
		/// Returns GLSL code with resolved dependencies that is ready to compile.
		/// </summary>
		internal ResolvedShader GetResolvedGlsl(ShaderRepository repository, Shader shader)
		{
			Dictionary<ShaderSource, int> dependencyOrder = new Dictionary<ShaderSource, int>();

			GetDependencyOrder(new HashSet<ShaderSource>(), dependencyOrder, 0);

			var ordered = dependencyOrder.OrderByDescending(x => x.Value).ToList();

			StringBuilder sb = new StringBuilder();

			sb.AppendLine("#version " + VersionString);
			sb.AppendLine(repository.ShaderHeader);

			// Not needed when compiling to Spirv since macros can be specified with args in glsLang 
			foreach(var macro in shader.Macros)
			{
				sb.AppendLine("#define " + macro.Item1 + " " + macro.Item2);
			}

			List<Tuple<int, string>> blocks = new List<Tuple<int, string>>();

			blocks.Add(new Tuple<int, string>(0, "Shader version, header and macros"));

			int lines = CountLines(repository.ShaderHeader) + 1;

			for(int i = 0; i < ordered.Count; i++)
			{
				var dependency = ordered[i].Key;

				sb.AppendLine("/* File: " + dependency.Filename + " */");
				lines += 1;

				blocks.Add(new Tuple<int, string>(lines, dependency.Filename));
				
				sb.AppendLine(dependency.ParsedCode);
				lines += CountLines(dependency.ParsedCode);
			}

			return new ResolvedShader()
			{
				Code = sb.ToString(),
				Blocks = blocks,
			};
		}

		int CountLines(string s)
		{
			int lines = 1;
			for(int i = 0; i < s.Length; i++)
			{
				if(s[i] == '\n')
					lines++;
			}
			return lines;
		}
	}
}
