namespace AIAgent.Skills.ReadExcel;

public sealed class ProcessExcelInput
{
	[Required]
	[MinLength(3)]
	public string FileName { get; set; } = string.Empty;

	[Required]
	public string Base64File { get; set; } = string.Empty;

	public int HeaderRow { get; set; } = 1;
}

public sealed class ProcessExcelResult
{
	public bool Success { get; init; }
	public string Message { get; init; } = string.Empty;
	public List<Dictionary<string, string?>> Rows { get; init; } = new();
}

public sealed class ReadExcel : ISkill
{
	// ISkill implementation: accept raw JsonElement payload and delegate to typed RunAsync
	public async Task<object?> RunAsync(
		JsonElement payload,
		CancellationToken cancellationToken = default)
	{
		try
		{
			var input = JsonSerializer.Deserialize<ProcessExcelInput>(
				payload.GetRawText(),
				new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			if (input is null)
			{
				return new ProcessExcelResult { Success = false, Message = "Invalid payload." };
			}

			return await RunAsync(input, cancellationToken).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			return new ProcessExcelResult { Success = false, Message = ex.Message };
		}
	}

	// Accepts a DTO suitable for Kernel direct invocation (FileName + Base64File).
	public async Task<ProcessExcelResult> RunAsync(ProcessExcelInput input, CancellationToken cancellationToken = default)
	{

		if (!IsBase64String(input.Base64File))
		{
			return new ProcessExcelResult { Success = false, Message = "Base64File is not valid Base64." };
		}

		byte[] fileBytes;
		try
		{
			fileBytes = Convert.FromBase64String(input.Base64File);
		}
		catch (Exception ex)
		{
			return new ProcessExcelResult { Success = false, Message = $"Failed to decode Base64 file: {ex.Message}" };
		}

		var ext = System.IO.Path.GetExtension(input.FileName ?? string.Empty) ?? string.Empty;
		try
		{
			if (ext.Equals(".csv", StringComparison.OrdinalIgnoreCase) || ext.Equals(".txt", StringComparison.OrdinalIgnoreCase))
			{
				var text = Encoding.UTF8.GetString(fileBytes);
				var rows = ParseCsv(text, input.HeaderRow);
				return new ProcessExcelResult { Success = true, Message = $"Parsed {rows.Count} rows from CSV.", Rows = rows };
			}

			if (ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
			{
				var rows = await ParseXlsxWithClosedXmlAsync(fileBytes, input.HeaderRow, cancellationToken).ConfigureAwait(false);
				return new ProcessExcelResult { Success = true, Message = $"Parsed {rows.Count} rows from XLSX (first worksheet).", Rows = rows };
			}

			return new ProcessExcelResult { Success = false, Message = $"Unsupported file type '{ext}'. Supported: .csv, .txt, .xlsx." };
		}
		catch (OperationCanceledException)
		{
			return new ProcessExcelResult { Success = false, Message = "Operation cancelled." };
		}
		catch (Exception ex)
		{
			return new ProcessExcelResult { Success = false, Message = $"Error parsing file: {ex.Message}" };
		}
	}

	private static bool IsBase64String(string s)
	{
		if (string.IsNullOrWhiteSpace(s))
			return false;

		// Trim padding/newlines
		s = s.Trim();
		// Quick checks
		if (s.Length % 4 != 0) return false;
		try
		{
			Convert.FromBase64String(s);
			return true;
		}
		catch
		{
			return false;
		}
	}

	// Simple CSV parser (handles basic quoted fields)
	private static List<Dictionary<string, string?>> ParseCsv(string csv, int headerRow)
	{
		var lines = csv.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
		var result = new List<Dictionary<string, string?>>();
		if (lines.Length == 0) return result;

		var headerIndex = Math.Max(1, headerRow) - 1;
		if (headerIndex >= lines.Length) headerIndex = 0;

		var headers = SplitCsvLine(lines[headerIndex]).Select(h => string.IsNullOrWhiteSpace(h) ? "Column" : h.Trim()).ToArray();
		for (var i = headerIndex + 1; i < lines.Length; i++)
		{
			var line = lines[i];
			if (string.IsNullOrWhiteSpace(line)) continue;
			var cols = SplitCsvLine(line);
			var row = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
			for (var c = 0; c < headers.Length; c++)
			{
				var key = headers[c];
				var val = c < cols.Length ? cols[c] : null;
				row[key] = val;
			}
			result.Add(row);
		}
		return result;
	}

	private static string[] SplitCsvLine(string line)
	{
		if (string.IsNullOrEmpty(line)) return Array.Empty<string>();
		var values = new List<string>();
		var sb = new StringBuilder();
		var inQuotes = false;
		for (var i = 0; i < line.Length; i++)
		{
			var ch = line[i];
			if (ch == '"')
			{
				if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
				{
					sb.Append('"');
					i++;
				}
				else
				{
					inQuotes = !inQuotes;
				}
				continue;
			}

			if (ch == ',' && !inQuotes)
			{
				values.Add(sb.ToString());
				sb.Clear();
				continue;
			}

			sb.Append(ch);
		}

		values.Add(sb.ToString());
		return values.ToArray();
	}

	// Use ClosedXML to parse XLSX into rows (first worksheet only)
	private static Task<List<Dictionary<string, string?>>> ParseXlsxWithClosedXmlAsync(byte[] fileBytes, int headerRow, CancellationToken cancellationToken)
	{
		return Task.Run(() =>
		{
			using var ms = new MemoryStream(fileBytes);
			using var workbook = new XLWorkbook(ms);
			var worksheet = workbook.Worksheets.First();

			var rows = new List<Dictionary<string, string?>>();

			var headerRowIndex = Math.Max(1, headerRow);
			var header = worksheet.Row(headerRowIndex);
			var lastColumn = header.LastCellUsed()?.Address.ColumnNumber ?? worksheet.LastColumnUsed()?.ColumnNumber() ?? 0;
			if (lastColumn == 0)
				return rows;

			var headers = new List<string>();
			for (var c = 1; c <= lastColumn; c++)
			{
				var hv = header.Cell(c).GetString();
				headers.Add(string.IsNullOrWhiteSpace(hv) ? $"Column{c}" : hv);
			}

			var lastRow = worksheet.LastRowUsed()?.RowNumber() ?? headerRowIndex;
			for (var r = headerRowIndex + 1; r <= lastRow; r++)
			{
				cancellationToken.ThrowIfCancellationRequested();
				var dict = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
				var row = worksheet.Row(r);
				for (var c = 1; c <= lastColumn; c++)
				{
					var key = headers[c - 1];
					var cell = row.Cell(c);
					var val = cell.GetString();
					dict[key] = string.IsNullOrEmpty(val) ? null : val;
				}
				rows.Add(dict);
			}

			return rows;
		}, cancellationToken);
	}
}
