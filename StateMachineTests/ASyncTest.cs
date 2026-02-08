using StateMachine;

namespace StateMachineTests;

public class ASyncTest
{
    private StateMachine<TestState, TestTrigger> sm;

    [SetUp]
    public void Setup()
    {

    }

    [TestCaseSource(nameof(CaseSource))]
    public async Task Test1(Action<TestEntity> configureFunc, Func<TestEntity, Task> assertFunc)
    {
        var entity = new TestEntity();
        configureFunc(entity);
        await assertFunc(entity);
    }

    public static object[] CaseSource =
    {
        new object[] {(TestEntity te) => te.ConfigureAsync(),(TestEntity te) => Task.Run(te.SyncAserts)},
        new object[] {(TestEntity te) => te.ConfigureAsync(),(TestEntity te) => Task.Run(te.AsyncAsserts)},
        new object[] {(TestEntity te) => te.ConfigureSync(),(TestEntity te) => Task.Run(te.SyncAserts) },
        new object[] {(TestEntity te) => te.ConfigureSync(),(TestEntity te) => Task.Run(te.AsyncAsserts) }
    };
}