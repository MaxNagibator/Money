using Money.ApiClient;
using Money.Data;

namespace Money.Api.Tests.TestTools;

public class DatabaseClient(Func<ApplicationDbContext> createWebDbContext, MoneyClient apiClient)
{
    private static readonly object LockObject = new();

    private List<TestObject>? _testObjects = [];
    private ApplicationDbContext? _context;

    public Func<ApplicationDbContext> CreateApplicationDbContext { get; } = createWebDbContext;
    public ApplicationDbContext Context => _context ??= CreateApplicationDbContext();
    public MoneyClient ApiClient { get; } = apiClient;

    public TestUser WithUser()
    {
        var obj = new TestUser();
        obj.Attach(this);
        return obj;
    }

    public void AddObject(TestObject testObject)
    {
        _testObjects?.Add(testObject);
    }

    public DatabaseClient Save()
    {
        if (_testObjects != null)
        {
            // поскольку тесты в несколько потоков это выполняют, а политика партии пока не рассматривает конкаранси случаи
            lock (LockObject)
            {
                foreach (var testObject in _testObjects)
                {
                    testObject.SaveObject();
                }
            }
        }

        return this;
    }

    public DatabaseClient Clear()
    {
        lock (LockObject)
        {
            _testObjects = [];
        }

        return this;
    }
}
