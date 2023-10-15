using System;

namespace Dpm.Stage.Unit.AI.Calculator
{
    public interface IAICalculator : IDisposable, IDebugDrawable
    {
        float Calculate();

        void Execute();
    }
}