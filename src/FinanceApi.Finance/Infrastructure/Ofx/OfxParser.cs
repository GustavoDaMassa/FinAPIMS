using System.Globalization;
using System.Text.RegularExpressions;
using FinanceApi.Finance.Domain.Enums;

namespace FinanceApi.Finance.Infrastructure.Ofx;

public class OfxParser
{
    private static readonly Regex TagValue =
        new(@"<(\w+)>([^<\r\n]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public IReadOnlyList<OfxTransactionRow> Parse(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            return [];

        var body = ExtractBody(content);
        var chunks = Regex.Split(body, @"<STMTTRN>", RegexOptions.IgnoreCase);
        var result = new List<OfxTransactionRow>();

        foreach (var chunk in chunks.Skip(1))
        {
            var end = chunk.IndexOf("</STMTTRN>", StringComparison.OrdinalIgnoreCase);
            var block = end >= 0 ? chunk[..end] : chunk;

            var tags = ParseTags(block);

            if (!tags.TryGetValue("FITID", out var fitId) ||
                !tags.TryGetValue("TRNAMT", out var amtStr) ||
                !tags.TryGetValue("DTPOSTED", out var dateStr))
                continue;

            if (!decimal.TryParse(amtStr.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var rawAmount))
                continue;

            var type = rawAmount >= 0 ? TransactionType.Inflow : TransactionType.Outflow;
            var amount = Math.Abs(rawAmount);
            var date = ParseDate(dateStr.Trim());

            tags.TryGetValue("MEMO", out var memo);
            tags.TryGetValue("NAME", out var name);
            var description = (memo ?? name)?.Trim();

            result.Add(new OfxTransactionRow(fitId.Trim(), amount, type, date, description));
        }

        return result;
    }

    private static string ExtractBody(string content)
    {
        var trimmed = content.TrimStart();

        // OFX 2.x: starts with a processing instruction <?OFX ... ?> or <?xml ... ?>
        if (trimmed.StartsWith("<?", StringComparison.Ordinal))
        {
            var piEnd = trimmed.IndexOf("?>", StringComparison.Ordinal);
            return piEnd >= 0 ? trimmed[(piEnd + 2)..] : trimmed;
        }

        // OFX 1.x SGML: skip header lines, body starts at <OFX>
        var ofxStart = trimmed.IndexOf("<OFX>", StringComparison.OrdinalIgnoreCase);
        return ofxStart >= 0 ? trimmed[ofxStart..] : trimmed;
    }

    private static Dictionary<string, string> ParseTags(string block)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (Match m in TagValue.Matches(block))
            dict.TryAdd(m.Groups[1].Value, m.Groups[2].Value);
        return dict;
    }

    private static DateOnly ParseDate(string raw)
    {
        // OFX date: YYYYMMDD[HHMMSS[.XXX]][TZ]  — first 8 chars are always the date
        if (raw.Length >= 8 &&
            int.TryParse(raw[..4], out var y) &&
            int.TryParse(raw[4..6], out var m) &&
            int.TryParse(raw[6..8], out var d))
            return new DateOnly(y, m, d);

        return DateOnly.FromDateTime(DateTime.Today);
    }
}
