using System.Text.Json;
using BankAccountSystem.Models;

namespace BankAccountSystem.Services;

public class LedgerDataManager
{
    private readonly string _path;

    public LedgerDataManager(string path)
    {
        _path = path;
    }

    public async Task SaveTransactionsAsync(
        IEnumerable<TransactionDetail> transactions)
    {
        string? directory = Path.GetDirectoryName(_path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json = JsonSerializer.Serialize(
            transactions,
            new JsonSerializerOptions
            {
                WriteIndented = true
            }
        );

        using FileStream fileStream = new FileStream(
            _path,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None,
            4096,
            useAsync: true
        );

        using StreamWriter writer = new StreamWriter(fileStream);

        await writer.WriteAsync(json);
    }

    public async Task<List<TransactionDetail>> LoadTransactionsAsync()
    {
        if (!File.Exists(_path))
        {
            return new List<TransactionDetail>();
        }

        using FileStream fileStream = new FileStream(
            _path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            4096,
            useAsync: true
        );

        using StreamReader reader = new StreamReader(fileStream);

        string json = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(json))
        {
            return new List<TransactionDetail>();
        }

        return JsonSerializer.Deserialize<List<TransactionDetail>>(json)
               ?? new List<TransactionDetail>();
    }
}