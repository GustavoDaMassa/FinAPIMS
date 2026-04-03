using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Exceptions;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Application.Services;
using FinanceApi.Finance.Domain.Models;

namespace FinanceApi.Finance.Tests;

public class CategoryServiceTests
{
    private static ICategoryService CreateService(out FinanceDbContext db)
    {
        db = TestHelpers.CreateDb();
        return new CategoryService(db);
    }

    [Fact]
    public async Task Create_ShouldPersistCategoryAndReturnDto()
    {
        var service = CreateService(out var db);

        var result = await service.CreateAsync(new CreateCategoryRequest("Alimentação", TestHelpers.UserId));

        Assert.Equal("Alimentação", result.Name);
        Assert.Equal(TestHelpers.UserId, result.UserId);
        Assert.Equal(1, await db.Categories.CountAsync());
    }

    [Fact]
    public async Task Create_DuplicateNameForSameUser_ShouldThrow()
    {
        var service = CreateService(out _);
        await service.CreateAsync(new CreateCategoryRequest("Alimentação", TestHelpers.UserId));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.CreateAsync(new CreateCategoryRequest("Alimentação", TestHelpers.UserId)));
    }

    [Fact]
    public async Task Create_SameNameDifferentUser_ShouldSucceed()
    {
        var service = CreateService(out _);
        await service.CreateAsync(new CreateCategoryRequest("Alimentação", TestHelpers.UserId));

        var result = await service.CreateAsync(new CreateCategoryRequest("Alimentação", TestHelpers.OtherUserId));

        Assert.Equal("Alimentação", result.Name);
    }

    [Fact]
    public async Task FindById_WhenExists_ShouldReturnDto()
    {
        var service = CreateService(out var db);
        var category = Category.Create("Lazer", TestHelpers.UserId);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        var result = await service.FindByIdAsync(category.Id);

        Assert.Equal(category.Id, result.Id);
        Assert.Equal("Lazer", result.Name);
    }

    [Fact]
    public async Task FindById_WhenNotFound_ShouldThrow()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
            service.FindByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ListByUser_ShouldReturnOnlyUserCategories()
    {
        var service = CreateService(out var db);
        db.Categories.Add(Category.Create("C1", TestHelpers.UserId));
        db.Categories.Add(Category.Create("C2", TestHelpers.UserId));
        db.Categories.Add(Category.Create("C3", TestHelpers.OtherUserId));
        await db.SaveChangesAsync();

        var result = await service.ListByUserAsync(TestHelpers.UserId);

        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal(TestHelpers.UserId, c.UserId));
    }

    [Fact]
    public async Task Delete_ShouldRemoveCategory()
    {
        var service = CreateService(out var db);
        var category = Category.Create("Viagem", TestHelpers.UserId);
        db.Categories.Add(category);
        await db.SaveChangesAsync();

        await service.DeleteAsync(category.Id);

        Assert.Equal(0, await db.Categories.CountAsync());
    }

    [Fact]
    public async Task Delete_WhenNotFound_ShouldThrow()
    {
        var service = CreateService(out _);

        await Assert.ThrowsAsync<CategoryNotFoundException>(() =>
            service.DeleteAsync(Guid.NewGuid()));
    }
}
