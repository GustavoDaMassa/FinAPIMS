using FinanceApi.Finance.Application.Dtos;
using FinanceApi.Finance.Application.Exceptions;
using FinanceApi.Finance.Application.Interfaces;
using FinanceApi.Finance.Domain.Models;
using FinanceApi.Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceApi.Finance.Application.Services;

public class CategoryService(FinanceDbContext db) : ICategoryService
{
    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        var exists = await db.Categories.AnyAsync(c => c.Name == request.Name && c.UserId == request.UserId);
        if (exists)
            throw new InvalidOperationException($"Category '{request.Name}' already exists for this user.");

        var category = Category.Create(request.Name, request.UserId);
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return ToDto(category);
    }

    public async Task<CategoryDto> FindByIdAsync(Guid id)
    {
        var category = await db.Categories.FindAsync(id)
            ?? throw new CategoryNotFoundException(id);
        return ToDto(category);
    }

    public async Task<IReadOnlyList<CategoryDto>> ListByUserAsync(Guid userId)
    {
        var categories = await db.Categories
            .Where(c => c.UserId == userId)
            .ToListAsync();
        return categories.Select(ToDto).ToList();
    }

    public async Task DeleteAsync(Guid id)
    {
        var category = await db.Categories.FindAsync(id)
            ?? throw new CategoryNotFoundException(id);
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
    }

    private static CategoryDto ToDto(Category c) => new(c.Id, c.Name, c.UserId);
}
