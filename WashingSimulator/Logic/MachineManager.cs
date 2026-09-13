using System;
using System.Collections.Generic;
using System.Linq;

namespace WashingSimulator.Logic
{
    public class MachineManager
    {
        private readonly List<WashingMachine> _machines = new List<WashingMachine>();

        public IReadOnlyList<WashingMachine> Machines => _machines.AsReadOnly();

        public void AddMachine(WashingMachine machine)
        {
            if (machine == null) throw new ArgumentNullException(nameof(machine));
            if (_machines.Any(m => m.Name == machine.Name))
                throw new InvalidOperationException($"Машина з назвою '{machine.Name}' вже існує.");
            _machines.Add(machine);
        }

        public bool RemoveMachine(string name)
        {
            var machine = FindByName(name);
            if (machine == null) return false;
            if (machine.State != MachineState.Idle && machine.State != MachineState.Done && machine.State != MachineState.Error)
                throw new InvalidOperationException("Неможливо видалити машину під час роботи.");
            return _machines.Remove(machine);
        }

        public WashingMachine FindByName(string name) =>
            _machines.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public List<WashingMachine> GetByType<T>() where T : WashingMachine =>
            _machines.OfType<T>().Cast<WashingMachine>().ToList();

        public List<WashingMachine> GetAvailable() =>
            _machines.Where(m => m.State == MachineState.Idle || m.State == MachineState.Done).ToList();

        public int TotalMachines => _machines.Count;
        public int WorkingCount  => _machines.Count(m => m.State == MachineState.Washing ||
                                                          m.State == MachineState.Rinsing ||
                                                          m.State == MachineState.Spinning);
        public int IdleCount     => _machines.Count(m => m.State == MachineState.Idle);
        public int DoneCount     => _machines.Count(m => m.State == MachineState.Done);
        public int ErrorCount    => _machines.Count(m => m.State == MachineState.Error);


        public static MachineManager CreateDemo()
        {
            var manager = new MachineManager();

            manager.AddMachine(new FrontLoadMachine("Samsung EcoBubble", 9.0f, 1400, hasSteam: true));
            manager.AddMachine(new FrontLoadMachine("Bosch Serie 6",     7.0f, 1200, hasSteam: false));
            manager.AddMachine(new TopLoadMachine  ("LG TurboWash",      8.0f, hasAgitator: false));
            manager.AddMachine(new TopLoadMachine  ("Whirlpool Classic", 6.5f, hasAgitator: true));
            manager.AddMachine(new PortableMachine ("Маленька помічниця",2.0f, hasSpinCycle: false));
            manager.AddMachine(new PortableMachine ("Mini Spin Pro",     3.5f, hasSpinCycle: true));

            return manager;
        }
    }
}
