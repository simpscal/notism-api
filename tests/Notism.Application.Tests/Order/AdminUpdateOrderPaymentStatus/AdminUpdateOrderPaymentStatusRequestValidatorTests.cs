using FluentAssertions;

using Notism.Application.Order.AdminUpdateOrderPaymentStatus;

namespace Notism.Application.Tests.Order.AdminUpdateOrderPaymentStatus;

public class AdminUpdateOrderPaymentStatusRequestValidatorTests
{
    private readonly AdminUpdateOrderPaymentStatusRequestValidator _validator = new();

    [Theory]
    [InlineData("paid")]
    [InlineData("unpaid")]
    [InlineData("failed")]
    public async Task Validate_WhenPaymentStatusIsValidEnum_Passes(string paymentStatus)
    {
        var request = new AdminUpdateOrderPaymentStatusRequest
        {
            OrderId = Guid.NewGuid(),
            PaymentStatus = paymentStatus,
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_WhenOrderIdMissing_Fails()
    {
        var request = new AdminUpdateOrderPaymentStatusRequest
        {
            OrderId = Guid.Empty,
            PaymentStatus = "paid",
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateOrderPaymentStatusRequest.OrderId));
    }

    [Fact]
    public async Task Validate_WhenPaymentStatusMissing_Fails()
    {
        var request = new AdminUpdateOrderPaymentStatusRequest
        {
            OrderId = Guid.NewGuid(),
            PaymentStatus = string.Empty,
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateOrderPaymentStatusRequest.PaymentStatus));
    }

    [Fact]
    public async Task Validate_WhenPaymentStatusIsNotValidEnum_Fails()
    {
        var request = new AdminUpdateOrderPaymentStatusRequest
        {
            OrderId = Guid.NewGuid(),
            PaymentStatus = "bogus",
        };

        var result = await _validator.ValidateAsync(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(AdminUpdateOrderPaymentStatusRequest.PaymentStatus));
    }
}