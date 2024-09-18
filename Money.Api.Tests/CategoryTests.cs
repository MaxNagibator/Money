using Microsoft.AspNetCore.Mvc.Testing;

namespace Money.Api.Tests;

public class CategoryTests
{
    [Test]
    public async Task GetModifiedTest()
    {
        var apiClient = new CategoryClient(Integration.GetHttpClient(), Console.WriteLine);
        var result = (await apiClient.Get(1)).IsSuccess().Content;

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Categories, Is.Not.Null);
        Assert.That(result.Categories.Length, Is.GreaterThanOrEqualTo(1));
    }
}
