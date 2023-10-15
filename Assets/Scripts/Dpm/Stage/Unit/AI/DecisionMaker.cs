using System;
using System.Collections.Generic;
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

        private readonly Dictionary<Type, IAICalculator> _skillCalculators = new();

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

                calculator.Init(character, moveInfo);

                _moveCalculators.Add(calculator.GetType(), calculator);
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

                calculator.Init(character, attackInfo);

                _attackCalculators.Add(calculator.GetType(), calculator);
            }
        }

        public void Dispose()
        {
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

            foreach (var kv in _skillCalculators)
            {
                kv.Value.Dispose();
            }

            _skillCalculators.Clear();

            _character = null;
        }

        public void UpdateFrame(float dt)
        {
            IAICalculator maxScoredCalculator = null;
            var maxScore = -1f;

            foreach (var kv in _attackCalculators)
            {
                var calculator = kv.Value;

                var score = calculator.Calculate();

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