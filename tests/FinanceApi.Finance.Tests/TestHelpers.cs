using FinanceApi.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Finance.Tests;

internal static class TestHelpers
{
    internal static FinanceDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<FinanceDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new FinanceDbContext(options);
    }

    internal static Guid UserId => Guid.Parse("00000000-0000-0000-0000-000000000001");
    internal static Guid OtherUserId => Guid.Parse("00000000-0000-0000-0000-000000000002");
}
