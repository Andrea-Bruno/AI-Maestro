using System.Text.Json;

namespace Maestro_AI.Services;

/// <summary>Loads and renders Markdown documentation files from docs/{lang}/.</summary>
public static class DocRenderer
{
    private static readonly string DocsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "docs");

    /// <summary>Map tabId → doc topic filename (without extension).</summary>
    private static readonly Dictionary<string, string> TabMap = new()
    {
        ["dashboard"] = "01-quickstart",
        ["roast"] = "02-roast-monitor",
        ["profiles"] = "03-profiles",
        ["analysis"] = "04-analysis",
        ["batches"] = "07-batches",
        ["pid"] = "08-pid",
        ["diagnostics"] = "13-diagnostics",
        ["tools"] = "10-calculator",
        ["settings"] = "14-settings"
    };

    public static string[] SupportedLangs = ["en", "it", "es", "fr", "de", "ru"];

    /// <summary>Get list of available doc topics.</summary>
    public static string GetDocList(string? lang = "en")
    {
        var dir = Path.Combine(DocsDir, NormalizeLang(lang));
        if (!Directory.Exists(dir)) dir = Path.Combine(DocsDir, "en");
        var files = Directory.GetFiles(dir, "*.md")
            .Select(Path.GetFileNameWithoutExtension!)
            .OrderBy(f => f)
            .ToArray();
        return JsonSerializer.Serialize(new { topics = files, lang = Path.GetFileName(dir) });
    }

    /// <summary>Get a doc by topic name and language. Returns markdown + HTML. Null-safe lang fallback to EN.</summary>
    public static string GetDoc(string topic, string? lang = "en")
    {
        lang = NormalizeLang(lang ?? "en");
        string file = Path.Combine(DocsDir, lang, $"{topic}.md");
        if (!File.Exists(file))
        {
            // Fallback to English
            file = Path.Combine(DocsDir, "en", $"{topic}.md");
            if (!File.Exists(file))
                return JsonSerializer.Serialize(new { error = "Topic not found", topic, availableLangs = SupportedLangs });
            lang = "en";
        }
        var md = File.ReadAllText(file);
        var html = RenderToHtml(md, topic);
        return JsonSerializer.Serialize(new { topic, lang, markdown = md, html });
    }

    /// <summary>Get doc for a specific tab.</summary>
    public static string GetHelpForTab(string tabId, string? lang = "en")
    {
        var topic = TabMap.TryGetValue(tabId, out var t) ? t : "01-quickstart";
        return GetDoc(topic, lang);
    }

    /// <summary>Search docs for a query.</summary>
    public static string SearchDocs(string query, string? lang = "en")
    {
        lang = NormalizeLang(lang);
        var dir = Path.Combine(DocsDir, lang);
        if (!Directory.Exists(dir)) dir = Path.Combine(DocsDir, "en");
        var results = new List<object>();
        var q = query.ToLowerInvariant();

        foreach (var file in Directory.GetFiles(dir, "*.md"))
        {
            var content = File.ReadAllText(file);
            if (content.ToLowerInvariant().Contains(q))
            {
                var lines = File.ReadAllLines(file);
                var title = lines.FirstOrDefault(l => l.StartsWith("# "))?.TrimStart('#', ' ') ?? Path.GetFileNameWithoutExtension(file);
                results.Add(new { topic = Path.GetFileNameWithoutExtension(file), title, matchCount = CountOccurrences(content.ToLowerInvariant(), q) });
            }
        }
        return JsonSerializer.Serialize(new { query, results, count = results.Count });
    }

    private static string NormalizeLang(string lang)
    {
        if (string.IsNullOrEmpty(lang) || !SupportedLangs.Contains(lang)) return "en";
        return lang;
    }

    private static int CountOccurrences(string text, string query)
    {
        int count = 0, i = 0;
        while ((i = text.IndexOf(query, i)) != -1) { count++; i += query.Length; }
        return count;
    }

    private static string RenderToHtml(string md, string topic)
    {
        var lines = md.Split('\n');
        var html = new System.Text.StringBuilder();
        html.AppendLine("<div class='doc-content'>");

        foreach (var line in lines)
        {
            var trimmed = line.TrimEnd('\r');
            if (trimmed.StartsWith("# ")) html.AppendLine($"<h1>{Escape(trimmed[2..])}</h1>");
            else if (trimmed.StartsWith("## ")) html.AppendLine($"<h2>{Escape(trimmed[3..])}</h2>");
            else if (trimmed.StartsWith("### ")) html.AppendLine($"<h3>{Escape(trimmed[4..])}</h3>");
            else if (trimmed.StartsWith("|")) html.AppendLine(ParseTableRow(trimmed));
            else if (trimmed.StartsWith("- **")) html.AppendLine($"<li><strong>{Escape(trimmed.TrimStart('-', ' ').Trim('*', ' '))}</strong></li>");
            else if (trimmed.StartsWith("- ")) html.AppendLine($"<li>{Escape(trimmed[2..])}</li>");
            else if (trimmed.StartsWith("```")) { /* skip code fence markers */ }
            else if (trimmed.StartsWith("```")) { /* skip */ }
            else if (!string.IsNullOrEmpty(trimmed) && char.IsDigit(trimmed[0]) && trimmed.Contains("**")) html.AppendLine($"<p>{Escape(trimmed)}</p>");
            else if (!string.IsNullOrWhiteSpace(trimmed)) html.AppendLine($"<p>{Escape(trimmed)}</p>");
        }
        html.AppendLine("</div>");
        return html.ToString();
    }

    private static string ParseTableRow(string line)
    {
        if (line.Contains("---")) return ""; // skip separator row
        var cells = line.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var tag = line.Contains("**") ? "th" : "td";
        var sb = new System.Text.StringBuilder("<tr>");
        foreach (var c in cells) sb.Append($"<{tag}>{Escape(c.Trim())}</{tag}>");
        sb.Append("</tr>");
        return sb.ToString();
    }

    private static string Escape(string s) => System.Net.WebUtility.HtmlEncode(s);
}
