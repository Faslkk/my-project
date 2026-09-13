using System.Collections.Generic;

namespace WashingSimulator.Logic
{

    public class FrontLoadMachine : WashingMachine
    {
        private readonly int _spinSpeedRpm;
        private readonly bool _hasSteam;

        public bool HasSteam => _hasSteam;

        public override string MachineType => "Фронтальна машина";

        public override string Description =>
            $"Фронтальне завантаження. Швидкість віджиму: {_spinSpeedRpm} об/хв." +
            (_hasSteam ? " Має функцію пару." : "");

        public FrontLoadMachine(string name, float capacityKg, int spinSpeedRpm, bool hasSteam = false)
            : base(name, capacityKg)
        {
            _spinSpeedRpm = spinSpeedRpm;
            _hasSteam     = hasSteam;
        }

        public override int GetSpinSpeedRpm() => _spinSpeedRpm;

        public override bool SupportsMode(WashingMode mode) => true;

        public override int GetWashDurationSeconds(WashingMode mode) => mode switch
        {
            WashingMode.Cotton    => 180,  // 3 хв (симуляція)
            WashingMode.Delicate  => 120,
            WashingMode.QuickWash =>  60,
            WashingMode.Wool      => 150,
            WashingMode.Sports    => 200,
            _ => 120
        };

        public override string GetModeDescription(WashingMode mode) => mode switch
        {
            WashingMode.Cotton    => "Бавовна: 60°C, повний цикл",
            WashingMode.Delicate  => "Делікатне: 30°C, м'яке обертання",
            WashingMode.QuickWash => "Швидке: 40°C, скорочений цикл",
            WashingMode.Wool      => "Вовна: 30°C, щадний режим",
            WashingMode.Sports    => "Спорт: 60°C, посилене полоскання",
            _ => "Стандартний режим"
        };
        protected override List<(MachineState state, int percent, string label)> GetWashPhases()
        {
            if (_hasSteam)
            {
                return new List<(MachineState, int, string)>
                {
                    (MachineState.Washing,  35, "Прання"),
                    (MachineState.Rinsing,  30, "Полоскання"),
                    (MachineState.Washing,  10, "Парова обробка"),   
                    (MachineState.Spinning, 25, "Віджим")
                };
            }
            return base.GetWashPhases();
        }

        protected override string GetPhaseDetail(MachineState phase, int step, int total)
        {
            if (phase == MachineState.Washing && _hasSteam && step == 1 && total == 1)
                return "Обробка парою для видалення бактерій";
            return base.GetPhaseDetail(phase, step, total);
        }
    }
}
