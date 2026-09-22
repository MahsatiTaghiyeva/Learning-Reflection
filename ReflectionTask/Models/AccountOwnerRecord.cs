namespace BankAccountSystem.Records;

public record AccountOwnerRecord(
    int Id,
    string FullName,
    string TaxId
);