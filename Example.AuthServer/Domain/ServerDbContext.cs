using Microsoft.EntityFrameworkCore;

namespace Example.AuthServer.Domain;

public class ServerDbContext(DbContextOptions<ServerDbContext> options)
    : DbContext(options);
