using Example.AuthServer.Domain.Entities;

namespace Example.AuthServer.Domain.DataSources.Impl;

public class MockCustomerDataSource
    : ICustomerDataSource
{
    internal static readonly Dictionary<string, Customer> MockCustomers =
        new()
        {
            ["74e9cdd9-674d-456c-81b2-d1430eef12ea"] = new Customer
            {
                Id = Guid.Parse("74e9cdd9-674d-456c-81b2-d1430eef12ea"),
                Partner = MockPartnerDataSource.Get("06972ae6-ef95-4659-afaf-2331acd9f8f7"),
                EmailAddress = "john.doe@example.com",
                FullName = "John Doe",
                PasswordPlain = "password-doe"
            },

            ["df4eeb9c-d106-4656-89b2-23aa33be67de"] = new Customer
            {
                Id = Guid.Parse("df4eeb9c-d106-4656-89b2-23aa33be67de"),
                Partner = MockPartnerDataSource.Get("7ccbbadc-e44b-4680-83bf-34d933a10997"),
                EmailAddress = "applebee@examplecorp.com",
                FullName = "Apple Bee",
                PasswordPlain = "password-bee"
            },

            ["6362f982-5b74-4f5f-9e00-924adb9815cd"] = new Customer
            {
                Id = Guid.Parse("6362f982-5b74-4f5f-9e00-924adb9815cd"),
                Partner = MockPartnerDataSource.Get("06972ae6-ef95-4659-afaf-2331acd9f8f7"),
                EmailAddress = "bella.baxter@name.info",
                FullName = "Bella Baxter",
                PasswordPlain = "password-baxter"
            }
        };

    public static Customer Get(string id)
        => MockCustomers[id] ?? throw new InvalidOperationException($"Customer with id {id} not found");

    public Task<Customer?> GetByEmailAddressAsync(
        string emailAddress, string partnerToken, CancellationToken token = default)
        => Task.FromResult(MockCustomers.Values.FirstOrDefault(
            c => c.EmailAddress == emailAddress && c.Partner.Token == partnerToken));

    public Task<Customer?> GetByUrnAsync(
        string urn, CancellationToken token = default)
        => Task.FromResult(MockCustomers.Values.FirstOrDefault(c => c.Urn == urn));
}