using System;
using System.Collections.Generic;
using Dpm.Stage.Unit.AI.Calculator;
using Dpm.Stage.Unit.AI.Calculator.Move;

namespace Dpm.Stage.Unit.AI
{
    public class DecisionMaker : IDecisionMaker
    {
        private readonly Dictionary<Type, IAICalculator> _moveCalculators = new();

        private readonly Dictionary<Type, IAICalculator> _attackCalculators = new();
        
        private readonly Dictionary<Type, IAICalculator> _skillCalculators = new();
        
        public DecisionMaker()
        {
            // Spec에 따라 Calculator의 값을 
            AddCalculator(_moveCalculators, new NormalMoveCalculator());
        }
        
        public void Dispose()
        {
            _moveCalculators.Clear();
            _attackCalculators.Clear();
            _skillCalculators.Clear();
        }
        
        public void UpdateFrame(float dt)
        {
            IAICalculator maxScoredCalculator = null;
            var maxScore = 0f;
            
            CalculateAt(_moveCalculators, ref maxScore, ref maxScoredCalculator);
            CalculateAt(_attackCalculators, ref maxScore, ref maxScoredCalculator);
            CalculateAt(_skillCalculators, ref maxScore, ref maxScoredCalculator);
            
            // FIXME : 움직이며 공격하기 등의 다른 동작을 동시에 실행할 수 있어야 함
            maxScoredCalculator?.Execute();
        }

        private void AddCalculator(Dictionary<Type, IAICalculator> calculators, IAICalculator calculator)
        {
            calculators.Add(calculator.GetType(), calculator);
        }

        private void CalculateAt(Dictionary<Type, IAICalculator> calculators, ref float maxScore,
            ref IAICalculator maxScoredCalculator)
        {
            foreach (var kv in calculators)
            {
                var calculator = kv.Value;

                var score = calculator.Calculate();

                if (maxScore < score)
                {
                    maxScore = score;
                    maxScoredCalculator = calculator;
                }
            }
        }
    }
}