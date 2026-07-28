using System.Text.Json;

namespace Maestro_AI.Api;

using Maestro_AI.Services;

/// <summary>API: documentation — serve markdown docs in multiple languages.</summary>
public static class DocsAPI
{
    public static string GetDoc(string topic, string? lang = "en") => DocRenderer.GetDoc(topic, lang ?? "en");
    public static string GetDocList(string? lang = "en") => DocRenderer.GetDocList(lang ?? "en");
    public static string GetHelpForTab(string tabId, string? lang = "en") => DocRenderer.GetHelpForTab(tabId, lang ?? "en");
    public static string SearchDocs(string query, string? lang = "en") => DocRenderer.SearchDocs(query, lang ?? "en");
}
