using AutoFixture;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Tests.Common;

public class TestFixture : IDisposable
{
    public IFixture Fixture { get; }
    public ILogger<T> GetLogger<T>() => Mock.Of<ILogger<T>>();

    public TestFixture()
    {
        Fixture = new Fixture();

        Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => Fixture.Behaviors.Remove(b));
        Fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        Fixture.Customize<string>(composer => composer.FromFactory(() => Guid.NewGuid().ToString()[..8]));
    }

    public void Dispose()
    {
    }
}

[CollectionDefinition("Test Collection")]
public class TestCollection : ICollectionFixture<TestFixture>
{
}
