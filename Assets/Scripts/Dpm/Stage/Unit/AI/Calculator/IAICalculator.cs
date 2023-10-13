using System;

namespace Dpm.Stage.Unit.AI.Calculator
{
    public interface IAICalculator : IDisposable
    {
        float Calculate();

        void Execute();
    }
}