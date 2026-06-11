using MediatR;

using Notism.Application.Common.Persistence;

namespace Notism.Application.Payment.GetBankAccount;

public class GetBankAccountHandler : IRequestHandler<GetBankAccountRequest, GetBankAccountResponse?>
{
    private readonly IReadDbContext _readDbContext;

    public GetBankAccountHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<GetBankAccountResponse?> Handle(GetBankAccountRequest request, CancellationToken cancellationToken)
    {
        var payment = await new GetBankAccountQuery(_readDbContext).ExecuteAsync(cancellationToken);

        if (payment is null)
        {
            return null;
        }

        return GetBankAccountResponse.FromDomain(payment);
    }
}