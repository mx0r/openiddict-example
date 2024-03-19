using Example.AuthServer.Domain.Entities;

namespace Example.AuthServer.Domain.DataSources;

public interface IPartnerDataSource
{
    Task<Partner?> GetByTokenAsync(string partnerToken, CancellationToken token = default);
    
    async Task<Partner> RequireByTokenAsync(string partnerToken, CancellationToken token = default)
    {
        var partner = await GetByTokenAsync(partnerToken, token);
        if (partner is null)
        {
            // when the partner is not found, throw an exception
            throw new InvalidOperationException(
                $"Partner with token {partnerToken} not found");
        }

        return partner;
    }
}
