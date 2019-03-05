using System;

namespace GetEnv
{
	class GetEnv
	{
		private static void Main(string[] args)
		{
			foreach (System.Collections.DictionaryEntry entry in Environment.GetEnvironmentVariables())
			{
				Console.WriteLine(entry.Key + "=" + entry.Value);
			}
		}
	}
}