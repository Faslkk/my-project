using System.Collections.Generic;

namespace WashingSimulator.Logic
{
    public class TopLoadMachine : WashingMachine
    {
        private readonly bool _hasAgitator;  

        public bool HasAgitator => _hasAgitator;

        public override string MachineType => "Вертикальна машина";

        public override string Description =>
            $"Вертикальне завантаження. " +
            (_hasAgitator
                ? "Активаторний барабан — краще відпирає, але жорсткіше для тканини."
                : "Без активатора — щадить тканину.");

        public TopLoadMachine(string name, float capacityKg, bool hasAgitator = true)
            : base(name, capacityKg)
        {
            _hasAgitator = hasAgitator;
        }

        public override int GetSpinSpeedRpm() => _hasAgitator ? 800 : 700;

        public override bool SupportsMode(WashingMode mode) =>
            mode != WashingMode.Delicate && mode != WashingMode.Wool;

        public override int GetWashDurationSeconds(WashingMode mode) => mode switch
        {
            WashingMode.Cotton    => 150,
            WashingMode.QuickWash =>  50,
            WashingMode.Sports    => 170,
            _ => 130
        };

        public override string GetModeDescription(WashingMode mode) => mode switch
        {
            WashingMode.Cotton    => "Бавовна: 50°C, стандартний цикл",
            WashingMode.QuickWash => "Швидке: 40°C, 30 хвилин",
            WashingMode.Sports    => "Спорт: 50°C, потужне прання",
            _ => "Режим недоступний"
        };
        protected override List<(MachineState state, int percent, string label)> GetWashPhases()
        {
            return new List<(MachineState, int, string)>
            {
                (MachineState.Washing,  50, "Прання"),
                (MachineState.Rinsing,  25, "Полоскання"),
                (MachineState.Spinning, 25, "Віджим")
            };
        }

        protected override string GetPhaseDetail(MachineState phase, int step, int total)
        {
            if (phase == MachineState.Washing && _hasAgitator)
                return $"Активаторне обертання {step}/{total}";
            return base.GetPhaseDetail(phase, step, total);
        }
    }
}
