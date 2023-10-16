using System;
using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI.Calculator;
using Dpm.Stage.Unit.AI.Calculator.Attack;
using Dpm.Stage.Unit.AI.Calculator.Move;

namespace Dpm.Stage.Unit.AI
{
    public class DecisionMaker : IDecisionMaker, IDebugDrawable
    {
        private readonly Dictionary<Type, IAIMoveCalculator> _moveCalculators = new();

        private readonly Dictionary<Type, IAIAttackCalculator> _attackCalculators = new();

        private readonly Dictionary<Type, IAICalculator> _abilityCalculators = new();

        private readonly Dictionary<IAICalculator, SliderWeightFactorInfo> _factorInfos = new();

        private Character _character;

        public IUnit CurrentAttackTarget { get; private set; }

        public void Init(Character character, MoveSpec moveSpec, AttackSpec attackSpec)
        {
            _character = character;

            foreach (var moveInfo in moveSpec.calculatorInfos)
            {
                // FIXME
                IAIMoveCalculator calculator = moveInfo.type switch
                {
                    MoveType.ApproachToEnemy => new ApproachToEnemyMoveCalculator(),
                    MoveType.Retreat => new RetreatMoveCalculator(),
                    _ => null
                };

                if (calculator == null)
                {
                    continue;
                }

                calculator.Init(character, moveInfo);

                _moveCalculators.Add(calculator.GetType(), calculator);

                _factorInfos.Add(calculator, moveInfo.weightFactorInfo);
            }

            foreach (var attackInfo in attackSpec.calculatorInfos)
            {
                // FIXME
                IAIAttackCalculator calculator = attackInfo.type switch
                {
                    AttackTargetSearchingType.Closest => new ClosestTargetAttackCalculator(),
                    // AttackTargetSearchingType.Strongest => new ClosestTargetAttackCalculator(),
                    // AttackTargetSearchingType.Weakest => new ClosestTargetAttackCalculator(),
                    _ => null
                };

                if (calculator == null)
                {
                    continue;
                }

                calculator.Init(character, attackInfo);

                _attackCalculators.Add(calculator.GetType(), calculator);

                _factorInfos.Add(calculator, attackInfo.weightFactorInfo);
            }

            CoreService.Event.Subscribe<ChangeAICalculatorFactorEvent>(OnChangeAICalculatorFactor);
        }

        public void Dispose()
        {
            CoreService.Event.Unsubscribe<ChangeAICalculatorFactorEvent>(OnChangeAICalculatorFactor);

            foreach (var kv in _moveCalculators)
            {
                kv.Value.Dispose();
            }

            _moveCalculators.Clear();

            foreach (var kv in _attackCalculators)
            {
                kv.Value.Dispose();
            }

            _attackCalculators.Clear();

            foreach (var kv in _abilityCalculators)
            {
                kv.Value.Dispose();
            }

            _abilityCalculators.Clear();

            _character = null;
        }

        private void OnChangeAICalculatorFactor(Core.Interface.Event e)
        {
            if (e is not ChangeAICalculatorFactorEvent cfe || cfe.Character != _character)
            {
                return;
            }

            var calculator = GetCalculatorTyped(cfe.AICalculatorType);

            if (calculator != null && _factorInfos.TryGetValue(calculator, out var factorInfo))
            {
                factorInfo.defaultValue = cfe.Value;

                _factorInfos[calculator] = factorInfo;
            }
        }

        public void UpdateFrame(float dt)
        {
            IAICalculator maxScoredCalculator = null;
            var maxScore = -1f;

            foreach (var kv in _attackCalculators)
            {
                var calculator = kv.Value;

                var score = calculator.Calculate();

                score *= GetScoreFactor(calculator);

                if (maxScore < score)
                {
                    maxScore = score;
                    maxScoredCalculator = calculator;
                    CurrentAttackTarget = calculator.CurrentTarget;
                }
            }

            maxScoredCalculator?.Execute();

            maxScore = -1f;

            foreach (var kv in _moveCalculators)
            {
                var calculator = kv.Value;

                var score = calculator.Calculate();

                score *= GetScoreFactor(calculator);

                if (maxScore < score)
                {
                    maxScore = score;
                    maxScoredCalculator = calculator;
                }
            }

            // FIXME : 움직이며 공격하기 등의 다른 동작을 동시에 실행할 수 있어야 함
            maxScoredCalculator?.Execute();

            CurrentAttackTarget = null;
        }

        private float GetScoreFactor(IAICalculator calculator)
        {
            if (_factorInfos.TryGetValue(calculator, out var factorInfo))
            {
                return factorInfo.defaultValue;
            }

            return 0;
        }

        public float GetScoreFactor(Type calculatorType)
        {
            var calculator = GetCalculatorTyped(calculatorType);

            return calculator != null ? GetScoreFactor(calculator) : 0.5f;
        }

        private IAICalculator GetCalculatorTyped(Type calculatorType)
        {
            IAICalculator calculator = null;

            if (_moveCalculators.TryGetValue(calculatorType, out var moveCalculator))
            {
                calculator = moveCalculator;
            }
            else if (_attackCalculators.TryGetValue(calculatorType, out var attackCalculator))
            {
                calculator = attackCalculator;
            }
            else if (_abilityCalculators.TryGetValue(calculatorType, out var abilityCalculator))
            {
                calculator = abilityCalculator;
            }

            return calculator;
        }

        public bool IsUsingTyped(Type calculatorType)
        {
            return _moveCalculators.ContainsKey(calculatorType) ||
                   _attackCalculators.ContainsKey(calculatorType) ||
                   _abilityCalculators.ContainsKey(calculatorType);
        }

        public void DrawCurrent()
        {
            foreach (var kv in _attackCalculators)
            {
                var calculator = kv.Value;

                calculator.DrawCurrent();
            }

            foreach (var kv in _moveCalculators)
            {
                var calculator = kv.Value;

                calculator.DrawCurrent();
            }
        }
    }
}