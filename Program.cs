using System.Text.RegularExpressions;

namespace TranslationCoverage;

internal class Program
{
	private static int Main(string[] args)
	{
		var dirFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories);

		if (!dirFiles.Any(f => f.EndsWith(".csproj")))
		{
			Console.WriteLine("No C# project could be detected.");
			return 1;
		}

		var csFiles = TraverseAllDirs(".cs");
		var translationFile = TraverseAllDirs(".csv").FirstOrDefault(f => f.EndsWith("translations.csv"));

		if (string.IsNullOrEmpty(translationFile))
			return 1;

		Console.ForegroundColor = ConsoleColor.DarkCyan;
		Console.WriteLine($"Translation file at: {translationFile.Replace(Directory.GetCurrentDirectory(), "")}\n");

		var usedKeys = GetUsedKeys(csFiles);
		var availableKeys = GetAvailableKeys(translationFile);

		Console.WriteLine($"Found {usedKeys.Length} keys in {csFiles.Length} project files.");
		Console.WriteLine($"Found {availableKeys.Length} keys in translation file.\n");

		var covered = usedKeys.Intersect(availableKeys).ToList();
		var notImplemented = usedKeys.Except(availableKeys).ToList();
		var noPurpose = availableKeys.Except(usedKeys).ToList();

		Console.ResetColor();
		Console.Write("Code Coverage: ");
		Console.ForegroundColor = ConsoleColor.Green;
		Console.Write($"{covered.Count / (float)(covered.Count + notImplemented.Count) * 100:F2} %");
		Console.ResetColor();
		Console.Write(" (");
		Console.ForegroundColor = ConsoleColor.Red;
		Console.Write($"{notImplemented.Count / (float)(covered.Count + notImplemented.Count) * 100:F2} % missing");
		Console.ResetColor();
		Console.Write(")\n");

		Console.Write("Not In Use: ");
		Console.ForegroundColor = ConsoleColor.Yellow;
		Console.WriteLine($"{noPurpose.Count}");
		Console.ResetColor();

		if (notImplemented.Count > 0)
			Console.WriteLine("\n- Missing Keys -");
		foreach (var key in notImplemented)
			Console.WriteLine($"{key}");
		if (noPurpose.Count > 0)
			Console.WriteLine("\n\n- Unused Keys -");
		foreach (var unusedKey in noPurpose)
			Console.WriteLine($"{unusedKey}");

		return notImplemented.Count;
	}

	private static string[] GetUsedKeys(string[] csFiles)
	{
		List<string> usedKeys = [];
		foreach (var csFile in csFiles)
		{
			var fileContent = File.ReadAllText(csFile);

			var regexPattern = @"";
			var matches = Regex.Matches(fileContent, regexPattern);

			var results = new List<string>();
			foreach (Match match in matches)
			{
				for (int i = 1; i < match.Groups.Count; i++)
				{
					if (!match.Groups[i].Success) continue;

					results.Add(match.Groups[i].Value);
					break;
				}
			}

			usedKeys.AddRange(results);
		}

		return usedKeys.Distinct().ToArray();
	}

	private static string[] GetAvailableKeys(string translationFile)
	{
		try
		{
			var lines = File.ReadAllLines(translationFile);
			if (lines.Length <= 1) return [];

			var keys = new List<string>();
			for (int i = 1; i < lines.Length; i++)
			{
				var line = lines[i];
				if (string.IsNullOrWhiteSpace(line)) continue;

				var columns = ParseCsvLine(line);
				if (columns.Count > 0)
					keys.Add(columns[0].Trim('"'));
			}

			return keys.Distinct().ToArray();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Could not load translations.csv: {ex.Message}");
			return [];
		}
	}

	private static List<string> ParseCsvLine(string line)
	{
		var result = new List<string>();
		bool inQuotes = false;
		string current = "";
		foreach (var c in line)
		{
			if (c == '\"')
			{
				inQuotes = !inQuotes;
				continue;
			}
			if (c == ',' && !inQuotes)
			{
				result.Add(current);
				current = "";
			}
			else
			{
				current += c;
			}
		}
		result.Add(current);
		return result;
	}

	private static string[] TraverseAllDirs(string ending)
	{
		var currentDir = Directory.GetCurrentDirectory();
		return Directory.GetFiles(currentDir, $"*{ending}", SearchOption.AllDirectories);
	}
}
