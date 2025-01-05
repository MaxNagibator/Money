namespace Money.Api.Tests.TestTools;

public abstract class TestObject
{
    private readonly List<TestObject> _objects = [];

    public DatabaseClient Environment { get; private set; } = null!;
    protected bool IsNew { get; set; } = true;

    public virtual void Attach(DatabaseClient env)
    {
        Environment = env;
        AfterAttach();
        env.AddObject(this);
    }

    public virtual void AfterAttach()
    {
    }

    public abstract void LocalSave();

    public TestObject SaveObject()
    {
        LocalSave();

        foreach (var testObject in _objects)
        {
            testObject.SaveObject();
        }

        return this;
    }
}
