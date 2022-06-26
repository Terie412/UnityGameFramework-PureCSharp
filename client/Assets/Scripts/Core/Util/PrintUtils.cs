using System.Collections.Generic;
using System.Text;

public static class PrintUtils
{
	public static string Print<T>(IEnumerable<T> list)
	{
		StringBuilder sb = new StringBuilder();
		sb.Append("{\n");
		foreach (var x1 in list)
		{
			sb.Append("\t").Append(x1).Append(",\n");
		}

		sb.Append("}");
		return sb.ToString();
	}
}