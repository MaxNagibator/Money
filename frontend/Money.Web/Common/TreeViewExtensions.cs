namespace Money.Web.Common;

public static class TreeViewExtensions
{
    public static List<TreeItemData<Category>> BuildChildren(this List<Category> categories, int? parentId)
    {
        return categories.Where(category => category.ParentId == parentId)
            .Select(child => new TreeItemData<Category>
            {
                Text = child.Name,
                Value = child,
                Children = BuildChildren(categories, child.Id),
            })
            .OrderBy(item => item.Value?.Order == null)
            .ThenBy(item => item.Value?.Order)
            .ThenBy(item => item.Value?.Name)
            .ToList();
    }

    public static void Filter<T>(this IEnumerable<TreeItemData<T>> treeItemData, string text)
    {
        foreach (var itemData in treeItemData)
        {
            if (itemData.HasChildren)
            {
                Filter(itemData.Children!, text);
            }

            itemData.Visible = itemData.IsVisible(text);
        }
    }

    public static bool IsVisible<T>(this TreeItemData<T> treeItemData, string searchTerm)
    {
        if (treeItemData.HasChildren)
        {
            return treeItemData.Text.IsMatch(searchTerm)
                   || treeItemData.Children!.Any(child => child.Text.IsMatch(searchTerm));
        }

        return treeItemData.Text.IsMatch(searchTerm);
    }

    public static bool IsMatch(this string? text, string searchTerm)
    {
        return string.IsNullOrEmpty(text) == false && text.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }
}
