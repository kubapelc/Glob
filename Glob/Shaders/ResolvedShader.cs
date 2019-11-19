using System;
using System.Collections.Generic;
using System.Text;

namespace Glob
{
	class ResolvedShader
	{
		public string Code;
		public List<Tuple<int, string>> Blocks;

		public void Print(Device device)
		{
			StringBuilder sb = new StringBuilder();
			var lines = Code.Split('\n');
			for(int i = 0; i < lines.Length; i++)
			{
				sb.AppendLine((i + 1).ToString() + ": " + lines[i]);
			}
			device.TextOutput.Print(OutputTypeGlob.Debug, sb.ToString());
		}

		public string GetLineOrigin(int line)
		{
			int block = 0;
			for(; block < Blocks.Count - 1; block++)
			{
				if(Blocks[block + 1].Item1 > line)
					break;
			}
			return "line " + (line - Blocks[block].Item1) + " in " + Blocks[block].Item2;
		}
	}
}
