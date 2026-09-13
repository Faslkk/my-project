using System;
using System.Collections.Generic;

namespace WashingSimulator.Logic
{
    public enum WashingMode
    {
        Cotton,       
        Delicate,     
        QuickWash,    
        Wool,         
        Sports         
    }

    public enum MachineState
    {
        Idle,         
        Washing,      
        Rinsing,      
        Spinning,     
        Done,         
        Error         
    }

    public class MachineEventArgs : EventArgs
    {
        public string Message { get; }
        public MachineState State { get; }
        public int ProgressPercent { get; }

        public MachineEventArgs(string message, MachineState state, int progress)
        {
            Message = message;
            State = state;
            ProgressPercent = progress;
        }
    }

    public abstract class WashingMachine
    {
        private string _name;
        private float _loadCapacityKg;
        private MachineState _state;
        private int _progressPercent;

        protected WashingMode CurrentMode;
        protected float CurrentLoadKg;

        public string Name
        {
            get => _name;
            protected set
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Назва не може бути порожньою.");
                _name = value;
            }
        }

        public float LoadCapacityKg
        {
            get => _loadCapacityKg;
            protected set
            {
                if (value <= 0)
                    throw new ArgumentException("Місткість повинна бути більше 0.");
                _loadCapacityKg = value;
            }
        }

        public MachineState State
        {
            get => _state;
            protected set
            {
                _state = value;
                OnStateChanged();
            }
        }

        public int ProgressPercent
        {
            get => _progressPercent;
            protected set => _progressPercent = Math.Max(0, Math.Min(100, value));
        }

        public abstract string MachineType { get; }
        public abstract string Description { get; }

        public event EventHandler<MachineEventArgs> StatusUpdated;
        public event EventHandler<MachineEventArgs> WashingCompleted;

        protected WashingMachine(string name, float loadCapacityKg)
        {
            Name = name;
            LoadCapacityKg = loadCapacityKg;
            _state = MachineState.Idle;
            _progressPercent = 0;
        }
        public abstract int GetWashDurationSeconds(WashingMode mode);
        public abstract int GetSpinSpeedRpm();
        public abstract bool SupportsMode(WashingMode mode);
        public abstract string GetModeDescription(WashingMode mode);
        public WashingResult StartWashing(float loadKg, WashingMode mode)
        {
            if (State != MachineState.Idle)
                return WashingResult.Fail("Машина вже працює або має помилку.");

            if (!SupportsMode(mode))
                return WashingResult.Fail($"Режим '{GetModeLocalName(mode)}' не підтримується цим типом машини.");

            if (loadKg > LoadCapacityKg)
                return WashingResult.Fail($"Завантаження {loadKg:F1} кг перевищує місткість {LoadCapacityKg:F1} кг.");

            if (loadKg <= 0)
                return WashingResult.Fail("Вага завантаження повинна бути більше 0.");

            CurrentLoadKg = loadKg;
            CurrentMode = mode;

            BeginWashCycle();
            return WashingResult.Success("Прання розпочато.");
        }
        private void BeginWashCycle()
        {
            int totalSeconds = GetWashDurationSeconds(CurrentMode);
            var phases = GetWashPhases();
            Steps = BuildSteps(phases, totalSeconds);
            CurrentStepIndex = 0;
            ProgressPercent = 0;
            State = MachineState.Washing;

            NotifyStatus($"▶ Запуск: {Name} | Режим: {GetModeLocalName(CurrentMode)} | Завантаження: {CurrentLoadKg:F1} кг");
            NotifyStatus($"⏱ Розрахунковий час: {FormatTime(totalSeconds)}");
        }
        protected virtual List<(MachineState state, int percent, string label)> GetWashPhases()
        {
            return new List<(MachineState, int, string)>
            {
                (MachineState.Washing,  40, "Прання"),
                (MachineState.Rinsing,  35, "Полоскання"),
                (MachineState.Spinning, 25, "Віджим")
            };
        }
        public List<WashStep> Steps { get; private set; } = new List<WashStep>();
        public int CurrentStepIndex { get; private set; }

        public void AdvanceStep()
        {
            if (State == MachineState.Done || State == MachineState.Idle || State == MachineState.Error)
                return;

            if (CurrentStepIndex < Steps.Count)
            {
                var step = Steps[CurrentStepIndex];
                State = step.Phase;
                ProgressPercent = step.ProgressAtEnd;
                NotifyStatus(step.Message);
                CurrentStepIndex++;
            }
            else
            {
                FinishWashing();
            }
        }

        private void FinishWashing()
        {
            ProgressPercent = 100;
            State = MachineState.Done;
            var result = $"✅ Прання завершено! Машина: {Name} | Режим: {GetModeLocalName(CurrentMode)} | Навантаження: {CurrentLoadKg:F1} кг";
            NotifyStatus(result);
            StatusUpdated?.Invoke(this, new MachineEventArgs(result, MachineState.Done, 100));
            WashingCompleted?.Invoke(this, new MachineEventArgs(result, MachineState.Done, 100));
        }

        public void Reset()
        {
            State = MachineState.Idle;
            ProgressPercent = 0;
            CurrentStepIndex = 0;
            Steps = new List<WashStep>();
            NotifyStatus($"🔄 {Name} готова до роботи.");
        }

        public void SetError(string reason)
        {
            State = MachineState.Error;
            NotifyStatus($"❌ Помилка: {reason}");
        }
        protected void NotifyStatus(string message)
        {
            StatusUpdated?.Invoke(this,
                new MachineEventArgs(message, State, ProgressPercent));
        }

        private void OnStateChanged() {}

        private List<WashStep> BuildSteps(
            List<(MachineState state, int percent, string label)> phases,
            int totalSeconds)
        {
            var steps = new List<WashStep>();
            int accumulatedProgress = 0;

            foreach (var (state, pct, label) in phases)
            {
                int phaseSteps = Math.Max(1, pct / 5); 
                int startProgress = accumulatedProgress;
                int endProgress = accumulatedProgress + pct;
                int phaseDurationSec = totalSeconds * pct / 100;

                for (int i = 1; i <= phaseSteps; i++)
                {
                    int prog = startProgress + (pct * i / phaseSteps);
                    string detail = GetPhaseDetail(state, i, phaseSteps);
                    steps.Add(new WashStep(
                        state,
                        Math.Min(prog, 100),
                        $"  [{label}] {detail} ({prog}%)"
                    ));
                }
                accumulatedProgress = endProgress;
            }
            return steps;
        }

        protected virtual string GetPhaseDetail(MachineState phase, int step, int total)
        {
            return phase switch
            {
                MachineState.Washing  => $"Цикл прання {step}/{total}",
                MachineState.Rinsing  => $"Полоскання {step}/{total}",
                MachineState.Spinning => $"Обертів: {GetSpinSpeedRpm()} об/хв — крок {step}/{total}",
                _ => $"Крок {step}/{total}"
            };
        }
        public static string GetModeLocalName(WashingMode mode) => mode switch
        {
            WashingMode.Cotton    => "Бавовна",
            WashingMode.Delicate  => "Делікатне",
            WashingMode.QuickWash => "Швидке прання",
            WashingMode.Wool      => "Вовна",
            WashingMode.Sports    => "Спортивний",
            _ => mode.ToString()
        };

        public static string FormatTime(int totalSeconds)
        {
            int m = totalSeconds / 60;
            int s = totalSeconds % 60;
            return m > 0 ? $"{m} хв {s:D2} с" : $"{s} с";
        }
        public List<WashingMode> GetSupportedModes()
        {
            var modes = new List<WashingMode>();
            foreach (WashingMode m in Enum.GetValues(typeof(WashingMode)))
                if (SupportsMode(m)) modes.Add(m);
            return modes;
        }

        public override string ToString() =>
            $"{MachineType}: {Name} (до {LoadCapacityKg:F1} кг, {GetSpinSpeedRpm()} об/хв)";
    }
    public class WashStep
    {
        public MachineState Phase { get; }
        public int ProgressAtEnd { get; }
        public string Message { get; }

        public WashStep(MachineState phase, int progress, string message)
        {
            Phase = phase;
            ProgressAtEnd = progress;
            Message = message;
        }
    }

    public class WashingResult
    {
        public bool IsSuccess { get; }
        public string Message { get; }

        private WashingResult(bool ok, string msg) { IsSuccess = ok; Message = msg; }

        public static WashingResult Success(string msg) => new WashingResult(true, msg);
        public static WashingResult Fail(string msg)    => new WashingResult(false, msg);
    }
}
