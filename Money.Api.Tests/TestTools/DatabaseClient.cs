using Money.Api.Tests.TestTools.Entities;
using Money.Data;

namespace Money.Api.Tests.TestTools;

public class DatabaseClient(Func<ApplicationDbContext> createWebDbContext)
{
    private static readonly object LockObject = new();
    public List<TestObject>? TestObjects = [];
    private ApplicationDbContext? _context;
    public Func<ApplicationDbContext> CreateApplicationDbContext { get; } = createWebDbContext;
    public ApplicationDbContext Context => _context ??= CreateApplicationDbContext();

    public TestUser WithUser()
    {
        TestUser obj = new();
        obj.Attach(this);
        return obj;
    }

    public DatabaseClient Save()
    {
        if (TestObjects != null)
        {
            // поскольку тесты в несколько потоков это выполняют, а политика партии пока не рассматривает конкаранси случаи
            lock (LockObject)
            {
                foreach (TestObject testObject in TestObjects)
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
            TestObjects = [];
        }

        return this;
    }
}
