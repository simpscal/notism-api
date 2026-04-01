using FluentAssertions;

using Notism.Application.Payment.SaveBankAccount;

namespace Notism.Application.Tests.Payment.SaveBankAccount;

public class SaveBankAccountValidatorTests
{
    private readonly SaveBankAccountRequestValidator _validator = new();

    [Fact]
    public async Task Validate_WhenAllFieldsProvided_Passes()
    {
        var request = new SaveBankAccountRequest
        {
            StorerId = Guid.NewGuid(),
            BankCode = "Vietcombank",
            AccountNumber = "123456789",
            AccountHolderName = "Nguyen Van A",
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenBankCodeEmpty_Fails()
    {
        var request = new SaveBankAccountRequest
        {
            StorerId = Guid.NewGuid(),
            BankCode = string.Empty,
            AccountNumber = "123456789",
            AccountHolderName = "Nguyen Van A",
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaveBankAccountRequest.BankCode));
    }

    [Fact]
    public async Task Validate_WhenAccountNumberEmpty_Fails()
    {
        var request = new SaveBankAccountRequest
        {
            StorerId = Guid.NewGuid(),
            BankCode = "Vietcombank",
            AccountNumber = string.Empty,
            AccountHolderName = "Nguyen Van A",
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaveBankAccountRequest.AccountNumber));
    }

    [Fact]
    public async Task Validate_WhenAccountHolderNameEmpty_Fails()
    {
        var request = new SaveBankAccountRequest
        {
            StorerId = Guid.NewGuid(),
            BankCode = "Vietcombank",
            AccountNumber = "123456789",
            AccountHolderName = string.Empty,
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SaveBankAccountRequest.AccountHolderName));
    }
}
