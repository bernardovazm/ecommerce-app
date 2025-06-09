public class GetProductsTests
{
    [Fact]    public async Task ReturnsExistingProducts()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDB").Options;

        await using var context = new AppDbContext(options);
        context.Products.Add(new Product("Teste","Desc",10,"",Guid.NewGuid()));
        await context.SaveChangesAsync();
        var handler = new GetProductsQueryHandler(context);
        var result = await handler.Handle(new GetProductsQuery(), CancellationToken.None);

        result.Should().HaveCount(1);
    }
}