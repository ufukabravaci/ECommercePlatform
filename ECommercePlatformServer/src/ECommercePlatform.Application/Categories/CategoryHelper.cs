using ECommercePlatform.Domain.Categories;

namespace ECommercePlatform.Application.Categories;

public static class CategoryTreeHelper
{
    public static int GetDepth(Category category)
    {
        int depth = 1;
        var current = category.Parent;

        while (current != null)
        {
            depth++;
            current = current.Parent;
        }

        return depth;
    }
}
public static class CategoryRules
{
    public const int MaxDepth = 4;
}
