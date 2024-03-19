using Example.AuthServer.Domain.Entities;

namespace Example.AuthServer.Domain.DataSources.Impl;

public class MockPartnerDataSource
    : IPartnerDataSource
{
    internal static readonly Dictionary<string, Partner> MockPartners
        = new()
        {
            ["06972ae6-ef95-4659-afaf-2331acd9f8f7"] = new Partner
            {
                Id = Guid.Parse("06972ae6-ef95-4659-afaf-2331acd9f8f7"),
                Token = "united-machines",
                Name = "United Machines"
            },
            ["7ccbbadc-e44b-4680-83bf-34d933a10997"] = new Partner
            {
                Id = Guid.Parse("7ccbbadc-e44b-4680-83bf-34d933a10997"),
                Token = "internal",
                Name = "ExampleCorp"
            }
        };

    public static Partner Get(string id)
        => MockPartners[id] ?? throw new InvalidOperationException($"Partner with id {id} not found");

    public Task<Partner?> GetByTokenAsync(
        string partnerToken, CancellationToken token = default)
        => Task.FromResult(MockPartners.Values.FirstOrDefault(p => p.Token == partnerToken));
}