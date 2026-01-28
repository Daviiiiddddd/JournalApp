using System.Text;
using JournalApp.Data;

namespace JournalApp.Services;

public class PdfExportService
{
    // "PDF export" without external packages:
    // We generate a clean HTML file and share it.
    // (If you later add a PDF library, you can convert this HTML to PDF.)
    public async Task<string> ExportPdfAsync(
        List<JournalEntry> entries,
        DateTime from,
        DateTime to,
        string fileName = "JournalExport.html")
    {
        entries ??= new List<JournalEntry>();

        var html = BuildExportHtml(entries, from, to);

        var path = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllTextAsync(path, html, Encoding.UTF8);

        // Share the file (works on MAUI)
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Export Journal",
            File = new ShareFile(path)
        });

        return path;
    }

    private static string BuildExportHtml(List<JournalEntry> entries, DateTime from, DateTime to)
    {
        static string Safe(string? s) =>
            System.Net.WebUtility.HtmlEncode(s ?? "");

        var sb = new StringBuilder();

        sb.AppendLine("<!doctype html><html><head><meta charset='utf-8'/>");
        sb.AppendLine("<meta name='viewport' content='width=device-width,initial-scale=1'/>");
        sb.AppendLine("<title>Journal Export</title>");
        sb.AppendLine(@"
<style>
body{font-family:Segoe UI,Arial,sans-serif;margin:24px;color:#111;}
h1{margin:0 0 6px;font-size:22px;}
.small{color:#555;margin:0 0 18px;}
.card{border:1px solid #e5e7eb;border-radius:12px;padding:14px;margin:12px 0;}
.meta{color:#444;font-size:12px;margin-bottom:10px;}
.tags span{display:inline-block;border:1px solid #ddd;border-radius:999px;padding:2px 8px;margin-right:6px;font-size:12px;}
hr{border:none;border-top:1px solid #eee;margin:14px 0;}
</style>");

        sb.AppendLine("</head><body>");
        sb.AppendLine($"<h1>Journal Export</h1>");
        sb.AppendLine($"<p class='small'>Range: <b>{from:yyyy-MM-dd}</b> → <b>{to:yyyy-MM-dd}</b> • Entries: <b>{entries.Count}</b></p>");

        foreach (var e in entries.OrderByDescending(x => x.EntryDate))
        {
            sb.AppendLine("<div class='card'>");
            sb.AppendLine($"<div class='meta'><b>Date:</b> {Safe(e.EntryDate)} • <b>Category:</b> {Safe(e.Category)} • <b>Mood:</b> {Safe(e.PrimaryMood)}</div>");
            sb.AppendLine($"<h3 style='margin:0 0 8px'>{Safe(e.Title)}</h3>");

            if (!string.IsNullOrWhiteSpace(e.TagsCsv))
            {
                sb.AppendLine("<div class='tags'>");
                foreach (var t in e.TagsCsv.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()))
                    sb.AppendLine($"<span>{Safe(t)}</span>");
                sb.AppendLine("</div><hr/>");
            }

            // ContentHtml is already html; keep it as-is (but could be unsafe if user injects)
            sb.AppendLine(string.IsNullOrWhiteSpace(e.ContentHtml) ? "<i>No content</i>" : e.ContentHtml);

            sb.AppendLine("</div>");
        }

        sb.AppendLine("</body></html>");
        return sb.ToString();
    }
}