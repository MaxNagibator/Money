using Money.Data;

namespace Money.Api.Tests.TestTools;

public class DatabaseClient
{
    public DatabaseClient(Func<ApplicationDbContext> createWebDbContext)
    {
        CreateApplicationDbContext = createWebDbContext;
    }

    public Func<ApplicationDbContext> CreateApplicationDbContext { get; }
    private ApplicationDbContext _context;
    public ApplicationDbContext Context => _context ?? (_context = CreateApplicationDbContext());
    public List<TestObject> TestObjects = new List<TestObject>();

    public TestUser WithUser()
    {
        var obj = new TestUser();
        obj.Attach(this);

        return obj;
    }

    private static object lockObject = new object();
    public DatabaseClient Save()
    {
        if (TestObjects != null)
        {
            // поскольку тесты в несколько потоков это выполняют, а политика партии пока не рассматривает конкаранси случаи
            lock (lockObject)
            {
                foreach (var o in TestObjects)
                {
                    o.SaveObject();
                }
            }
        }

        return this;
    }

    public DatabaseClient Clear()
    {
        TestObjects = new List<TestObject>();
        return this;
    }
}
