namespace FinanceApi.Finance.Domain.Models;

public class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Guid UserId { get; private set; }

    private Category() { }

    public static Category Create(string name, Guid userId)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name,
            UserId = userId
        };
    }

    public void Update(string name) => Name = name;
}
