namespace GrotixBackend.BuildingBlocks.Configuration;

public static class DotEnvBootstrap
{
    public static void LoadFromCurrentDirectory(string fileName = ".env")
    {
        var envPath = ResolveEnvPath(Directory.GetCurrentDirectory(), fileName);
        if (envPath == null || !File.Exists(envPath))
            return;

        foreach (var rawLine in File.ReadAllLines(envPath))
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
                continue;

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (value.Length >= 2 &&
                ((value.StartsWith('"') && value.EndsWith('"')) ||
                 (value.StartsWith('\'') && value.EndsWith('\''))))
            {
                value = value[1..^1];
            }

            Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string? ResolveEnvPath(string startDirectory, string fileName)
    {
        var current = new DirectoryInfo(startDirectory);
        while (current != null)
        {
            var candidate = Path.Combine(current.FullName, fileName);
            if (File.Exists(candidate))
                return candidate;
            current = current.Parent;
        }

        return null;
    }
}
