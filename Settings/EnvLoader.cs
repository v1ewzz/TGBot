namespace TGBot.Settings;

public static class EnvLoader
{
    public static void Load(string path = ".env")
    {
        string fullPath = Path.Combine(AppContext.BaseDirectory, path);
        if (!File.Exists(fullPath))
            return;

        foreach (string line in File.ReadAllLines(fullPath)
                     .Select(TrimLine)
                     .Where(l => l.Length > 0 && !l.StartsWith('#')))
        {
            int eqIndex = line.IndexOf('=');
            if (eqIndex <= 0)
                continue;

            string key = line[..eqIndex].Trim();
            string value = line[(eqIndex + 1)..].Trim().Trim('"');

            if (key.Length > 0 && Environment.GetEnvironmentVariable(key) is null)
                Environment.SetEnvironmentVariable(key, value);
        }
    }

    private static string TrimLine(string raw) => raw.Trim();
}