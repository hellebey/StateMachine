using StateMachine;

namespace StateMachineTests
{
    public class TestEntity
    {
        private bool actionWithoutChange1;
        private bool actionWithoutChange2;
        private bool actionWithoutChange3;
        private bool actionWithChange;
        private bool exit;
        private bool entry;

        private StateMachine<TestState, TestTrigger> _sm;

        public TestState State => _state;
        private TestState _state { get; set; }

        public void ConfigureAsync()
        {
            _sm = new StateMachine<TestState, TestTrigger>(
                _state, 
                s => _state = s);

            _sm.Configure(TestState.State1)
              .NoAction(TestTrigger.NotChangeTrigger1)
              .NoAction<int>(TestTrigger.NotChangeTrigger1)
              .OnActionAsync(TestTrigger.Trigger1, ActionWithoutChangeAsync1)
              .OnActionAsync<string>(TestTrigger.Trigger1, ActionWithoutChangeAsync2)
              .OnActionAsync<int>(TestTrigger.Trigger1, ActionWithoutChangeAsync3)
              .ChangeStateAsync<bool>(TestTrigger.Trigger1, TestState.State2, ActionWithChangeAsync)
              .OnExitAsync(ExitAsync);

            _sm.Configure(TestState.State2)
              .OnEntryAsync(EntryAsync)
              .ChangeState(TestTrigger.ChangeTrigger2, TestState.State3);
        }

        public void ConfigureSync()
        {
            _sm = new StateMachine<TestState, TestTrigger>(
                _state,
                s => _state = s);

            _sm.Configure(TestState.State1)
              .NoAction(TestTrigger.NotChangeTrigger1)
              .NoAction<int>(TestTrigger.NotChangeTrigger1)
              .OnAction(TestTrigger.Trigger1, ActionWithoutChange1)
              .OnAction<string>(TestTrigger.Trigger1, ActionWithoutChange2)
              .OnAction<int>(TestTrigger.Trigger1, ActionWithoutChange3)
              .ChangeState<bool>(TestTrigger.Trigger1, TestState.State2, ActionWithChange1)
              .OnExit(Exit1);

            _sm.Configure(TestState.State2)
              .OnEntry(Entry1)
              .ChangeState(TestTrigger.ChangeTrigger2, TestState.State3);
        }

        private void Entry1()
        {
            entry = true;
        }

        private void ActionWithChange1(bool obj)
        {
            actionWithChange = true;
        }

        private void Exit1()
        {
            exit = true;
        }

        private void ActionWithoutChange3(int obj)
        {
            actionWithoutChange3 = true;
        }

        private void ActionWithoutChange2(string obj)
        {
            actionWithoutChange2 = true;
        }

        private void ActionWithoutChange1()
        {
            actionWithoutChange1 = true;
        }

        private async Task ActionWithChangeAsync(bool arg)
        {
            actionWithChange = true;
        }

        private async Task ActionWithoutChangeAsync3(int arg)
        {
            actionWithoutChange3 = true;
        }

        private async Task ActionWithoutChangeAsync2(string arg)
        {
            actionWithoutChange2 = true;
        }

        private async Task ActionWithoutChangeAsync1()
        {
            actionWithoutChange1 = true;
        }

        private async Task EntryAsync()
        {
            entry = true;
        }

        private async Task ExitAsync()
        {
            exit = true;
        }

        public async Task AsyncAsserts()
        {
            await _sm.TriggerAsync(TestTrigger.NotChangeTrigger1);
            Assert.That(TestState.State1.Equals(_sm.State), message: "Сменился стейт при триггере без смены стейта.");

            await _sm.TriggerAsync(TestTrigger.NotChangeTrigger1, 0);
            Assert.That(TestState.State1.Equals(_sm.State), message: "Сменился стейт при триггере без смены стейта.");

            await _sm.TriggerAsync(TestTrigger.Trigger1);
            Assert.That(TestState.State1.Equals(_sm.State), message: "Сменился стейт при триггере без смены стейта.");
            Assert.That(actionWithoutChange1 == true, message: "Не вызвалось действие на триггер без параметра.");

            await _sm.TriggerAsync(TestTrigger.Trigger1, "string");
            Assert.That(TestState.State1.Equals(_sm.State), message: "Сменился стейт при триггере без смены стейта.");
            Assert.That(actionWithoutChange2 == true, message: "Не вызвалось действие на триггер без параметра.");

            await _sm.TriggerAsync(TestTrigger.Trigger1, 1);
            Assert.That(TestState.State1.Equals(_sm.State), message: "Сменился стейт при триггере без смены стейта.");
            Assert.That(actionWithoutChange3 == true, message: "Не вызвалось действие на триггер без параметра.");

            await _sm.TriggerAsync(TestTrigger.Trigger1, true);
            Assert.That(TestState.State2.Equals(_sm.State), message: "не  сменился стейт при триггере со сменой стейта.");
            Assert.That(actionWithChange == true, message: "Не вызвалось действие при смене статуса.");
            Assert.That(exit == true, message: "Не вызвалось действие при выходе из статуса.");
            Assert.That(entry == true, message: "Не вызвалось действие при входе в статус.");

            await _sm.TriggerAsync(TestTrigger.ChangeTrigger2);
            Assert.That(TestState.State3.Equals(_sm.State), message: "не  сменился стейт при триггере со сменой стейта.");
        }

        public void SyncAserts()
        {
            _sm.Trigger(TestTrigger.NotChangeTrigger1);
            Assert.That(TestState.State1.Equals(_sm.State), message: "Сменился стейт при триггере без смены стейта.");

            _sm.Trigger(TestTrigger.NotChangeTrigger1, 0);
            Assert.That(TestState.State1.Equals(_sm.State), message: "Сменился стейт при триггере без смены стейта.");

            _sm.Trigger(TestTrigger.Trigger1);
            Assert.That(TestState.State1.Equals(_sm.State), message: "Сменился стейт при триггере без смены стейта.");
            Assert.That(actionWithoutChange1 == true, message: "Не вызвалось действие на триггер без параметра.");

            _sm.Trigger(TestTrigger.Trigger1, "string");
            Assert.That(TestState.State1.Equals(_sm.State), message: "Сменился стейт при триггере без смены стейта.");
            Assert.That(actionWithoutChange2 == true, message: "Не вызвалось действие на триггер без параметра.");

            _sm.Trigger(TestTrigger.Trigger1, 1);
            Assert.That(TestState.State1.Equals(_sm.State), message: "Сменился стейт при триггере без смены стейта.");
            Assert.That(actionWithoutChange3 == true, message: "Не вызвалось действие на триггер без параметра.");

            _sm.Trigger(TestTrigger.Trigger1, true);
            Assert.That(TestState.State2.Equals(_sm.State), message: "не  сменился стейт при триггере со сменой стейта.");
            Assert.That(actionWithChange == true, message: "Не вызвалось действие при смене статуса.");
            Assert.That(exit == true, message: "Не вызвалось действие при выходе из статуса.");
            Assert.That(entry == true, message: "Не вызвалось действие при входе в статус.");

            _sm.Trigger(TestTrigger.ChangeTrigger2);
            Assert.That(TestState.State3.Equals(_sm.State), message: "не  сменился стейт при триггере со сменой стейта.");
        }
    }
}
