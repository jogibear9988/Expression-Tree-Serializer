using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ExpressionSerialization
{
	class Program
	{
		static void Main(string[] args)
		{
			var template = new ExpressionSerializerTemplate();
			string content = template.TransformText();
			string path = @"E:\C#\ExpressionTreeSerialization\ExpressionSerialization.SL\ExpressionSerializer.partial.cs";
			System.IO.File.WriteAllText(path, content);
		}
	}
}
