using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace TranslationCoverage;

class Program
{
	static int Main(string[] args)
	{
		var dirFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.*", SearchOption.AllDirectories);

		if (!dirFiles.Any(f => f.EndsWith(".csproj")))
		{
			Console.WriteLine("No C# project could be detected.");
			return 1;
		}

		var csFiles = TraverseAllDirs(".cs");
		var translationFile = TraverseAllDirs(".json").FirstOrDefault(f => f.EndsWith("translations.json"));

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

			var regexPattern =
				"WithLocalizedTitle\\(\"([a-zA-Z._-]*)\"|WithLocalizedDescription\\(\"([a-zA-Z._-]*)\"|Translations\\.GetTranslation\\(\"([a-zA-Z._-]*)\"|AddLocalizedTextInput\\(\"([a-zA-Z._-]*)\"";
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
			var dict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, string>>>(
				File.ReadAllText(translationFile)) ?? [];

			return dict.Keys.ToArray();
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Could not load translation.json: {ex.Message}");
			return [];
		}
	}

	private static string[] TraverseAllDirs(string ending)
	{
		var currentDir = Directory.GetCurrentDirectory();
		return Directory.GetFiles(currentDir, $"*{ending}", SearchOption.AllDirectories);
	}
}