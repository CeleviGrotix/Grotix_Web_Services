namespace GrotixBackend.BuildingBlocks.Configuration;

public static class DotEnvBootstrap
{
    public static void LoadFromCurrentDirectory(string fileName = ".env")
    {
        var filePath = FindUpwards(Directory.GetCurrentDirectory(), fileName);
        if (filePath == null || !File.Exists(filePath))
            return;

        foreach (var rawLine in File.ReadAllLines(filePath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();
            value = StripWrappingQuotes(value);

            if (string.IsNullOrWhiteSpace(key))
                continue;

            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string? FindUpwards(string startDirectory, string fileName)
    {
        var directory = new DirectoryInfo(startDirectory);

        while (directory != null)
        {
            var candidate = Path.Combine(directory.FullName, fileName);
            if (File.Exists(candidate))
                return candidate;

            directory = directory.Parent;
        }

        return null;
    }

    private static string StripWrappingQuotes(string value)
    {
        if (value.Length >= 2)
        {
            var first = value[0];
            var last = value[^1];
            if ((first == '"' && last == '"') || (first == '\'' && last == '\''))
                return value[1..^1];
        }

        return value;
    }
}
