namespace Money.Api.Tests.TestTools;

public abstract class TestObject
{
    public bool IsNew { get; set; }
    public List<TestObject>? Objects { get; set; } = [];
    public DatabaseClient Environment { get; private set; }

    public virtual void Attach(DatabaseClient env)
    {
        Environment = env;
        AfterAttach();
        env.TestObjects?.Add(this);
    }

    public abstract void LocalSave();

    internal virtual void AfterAttach()
    {
    }

    internal TestObject SaveObject()
    {
        LocalSave();

        if (Objects != null)
        {
            foreach (TestObject x in Objects)
            {
                x.SaveObject();
            }
        }

        return this;
    }
}

public abstract class ReadonlyTestObject : TestObject
{
    public override void LocalSave()
    {
    }
}
