 using System.Collections.Generic;

namespace WashingSimulator.Logic
{
    public class PortableMachine : WashingMachine
    {
        private readonly bool _hasSpinCycle; 

        public bool HasSpinCycle => _hasSpinCycle;

        public override string MachineType => "Портативна машина";

        public override string Description =>
            $"Компактна машина для невеликого прання. Місткість до {LoadCapacityKg:F1} кг." +
            (_hasSpinCycle ? " Має функцію віджиму." : " Без віджиму — ручне вичавлювання.");

        public PortableMachine(string name, float capacityKg, bool hasSpinCycle = false)
            : base(name, capacityKg < 3.5f ? capacityKg : 3.5f)   
        {
            _hasSpinCycle = hasSpinCycle;
        }

        public override int GetSpinSpeedRpm() => _hasSpinCycle ? 400 : 0;

        public override bool SupportsMode(WashingMode mode) =>
            mode == WashingMode.Delicate || mode == WashingMode.QuickWash;

        public override int GetWashDurationSeconds(WashingMode mode) => mode switch
        {
            WashingMode.Delicate  => 90,
            WashingMode.QuickWash => 40,
            _ => 60
        };

        public override string GetModeDescription(WashingMode mode) => mode switch
        {
            WashingMode.Delicate  => "Делікатне: 30°C, ручний режим",
            WashingMode.QuickWash => "Швидке: 40°C, 15 хвилин",
            _ => "Режим недоступний для портативної машини"
        };
        protected override List<(MachineState state, int percent, string label)> GetWashPhases()
        {
            if (!_hasSpinCycle)
            {
                return new List<(MachineState, int, string)>
                {
                    (MachineState.Washing, 60, "Прання"),
                    (MachineState.Rinsing, 40, "Полоскання")
                };
            }
            return new List<(MachineState, int, string)>
            {
                (MachineState.Washing,  50, "Прання"),
                (MachineState.Rinsing,  30, "Полоскання"),
                (MachineState.Spinning, 20, "Легкий віджим")
            };
        }

        protected override string GetPhaseDetail(MachineState phase, int step, int total)
        {
            if (phase == MachineState.Spinning)
                return $"Легкий відцентровий відтиск {step}/{total} (400 об/хв)";
            return base.GetPhaseDetail(phase, step, total);
        }
    }
}
