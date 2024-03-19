using Example.AuthServer.Domain.Entities;

namespace Example.AuthServer.Domain.DataSources;

public interface ICustomerDataSource
{
    Task<Customer?> GetByEmailAddressAsync(string emailAddress, string partnerToken, CancellationToken token = default);

    Task<Customer?> GetByUrnAsync(string urn, CancellationToken token = default);
    
    async Task<Customer> RequireByEmailAddressAsync(
        string emailAddress, string partnerToken,
        CancellationToken token = default)
    {
        var customer = await GetByEmailAddressAsync(emailAddress, partnerToken, token);
        if (customer is null)
        {
            // when the customer is not found, throw an exception
            throw new InvalidOperationException(
                $"Customer with e-mail address {emailAddress} not found for partner with token {partnerToken}");
        }

        return customer;
    }
    
    async Task<Customer> RequireByUrnAsync(string urn, CancellationToken token = default)
    {
        var customer = await GetByUrnAsync(urn, token);
        if (customer is null)
        {
            // when the customer is not found, throw an exception
            throw new InvalidOperationException($"Customer with URN {urn} not found");
        }

        return customer;
    }
}
