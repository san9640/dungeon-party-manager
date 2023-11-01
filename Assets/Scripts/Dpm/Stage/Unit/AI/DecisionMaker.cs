using System;
using System.Collections.Generic;
using Dpm.CoreAdapter;
using Dpm.Stage.Event;
using Dpm.Stage.Spec;
using Dpm.Stage.Unit.AI.Calculator;
using Dpm.Stage.Unit.AI.Calculator.Attack;
using Dpm.Stage.Unit.AI.Calculator.Move;
using Dpm.Utility.Constants;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dpm.Stage.Unit.AI
{
    public class DecisionMaker : IDecisionMaker, IDebugDrawable
    {
        private readonly Dictionary<Type, IAIMoveCalculator> _moveCalculators = new();

        private readonly Dictionary<Type, IAIAttackCalculator> _attackCalculators = new();

        private readonly Dictionary<Type, IAICalculator> _abilityCalculators = new();

        private readonly Dictionary<IAICalculator, SliderWeightFactorInfo> _factorInfos = new();

#if UNITY_EDITOR
        public readonly Dictionary<IAICalculator, (float score, float sum, int frameCount)> ResultOnThisFrame = new();
#endif

        private Character _character;

        public IUnit CurrentAttackTarget { get; private set; }

        private const float MinMoveCalculateDelay = 0.1f;
        private const float MaxMoveCalculateDelay = 0.3f;

        private float _currentMoveCalculateDelay = 0f;

        private float _moveCalculateTimePassed = 0f;

        private readonly Dictionary<IUnit, float> _attackTargetsBuffer = new();

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
                    MoveType.AwayFromWall => new AwayFromWallMoveCalculator(),
                    _ => null
                };

                if (calculator == null)
                {
                    continue;
                }

                AddMoveCalculator(calculator, moveInfo);

#if UNITY_EDITOR
                ResultOnThisFrame.Add(calculator, (0, 0, 0));
#endif
            }

            // AddMoveCalculator(new DefaultMoveCalculator(), new MoveCalculatorInfo
            // {
            //     type = MoveType.Default,
            //     weightFactorInfo = new SliderWeightFactorInfo
            //     {
            //         defaultValue = 0.5f,
            //     }
            // });

            foreach (var attackInfo in attackSpec.calculatorInfos)
            {
                // FIXME
                IAIAttackCalculator calculator = attackInfo.type switch
                {
                    AttackTargetSearchingType.Closest => new ClosestTargetAttackCalculator(),
                    AttackTargetSearchingType.LowHp => new LowHpTargetAttackCalculator(),
                    AttackTargetSearchingType.HighHp => new HighHpTargetAttackCalculator(),
                    AttackTargetSearchingType.Ranged => new RangedTargetAttackCalculator(),
                    AttackTargetSearchingType.Melee => new MeleeTargetAttackCalculator(),
                    // AttackTargetSearchingType.Strongest => new StrongestTargetAttackCalculator(),
                    // AttackTargetSearchingType.Weakest => new WeakestTargetAttackCalculator(),
                    _ => null
                };

                if (calculator == null)
                {
                    continue;
                }

                AddAttackCalculator(calculator, attackInfo);

#if UNITY_EDITOR
                ResultOnThisFrame.Add(calculator, (0, 0, 0));
#endif
            }

            // AddAttackCalculator(new StrongestTargetAttackCalculator(), new AttackCalculatorInfo()
            // {
            //     type = AttackTargetSearchingType.Strongest,
            //     weightFactorInfo = new SliderWeightFactorInfo
            //     {
            //         defaultValue = 0.5f,
            //     }
            // });
            //
            // AddAttackCalculator(new WeakestTargetAttackCalculator(), new AttackCalculatorInfo()
            // {
            //     type = AttackTargetSearchingType.Strongest,
            //     weightFactorInfo = new SliderWeightFactorInfo
            //     {
            //         defaultValue = 0.5f,
            //     }
            // });

            CoreService.Event.Subscribe<ChangeAICalculatorFactorEvent>(OnChangeAICalculatorFactor);
        }

        private void AddMoveCalculator(IAIMoveCalculator calculator, MoveCalculatorInfo moveInfo)
        {
            calculator.Init(_character, moveInfo);

            _moveCalculators.Add(calculator.GetType(), calculator);

            _factorInfos.Add(calculator, moveInfo.weightFactorInfo);
        }

        private void AddAttackCalculator(IAIAttackCalculator calculator, AttackCalculatorInfo attackInfo)
        {
            calculator.Init(_character, attackInfo);

            _attackCalculators.Add(calculator.GetType(), calculator);

            _factorInfos.Add(calculator, attackInfo.weightFactorInfo);
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

#if UNITY_EDITOR
            ResultOnThisFrame.Clear();
#endif
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

            foreach (var kv in _attackCalculators)
            {
                var calculator = kv.Value;

                var score = calculator.Calculate();

#if UNITY_EDITOR
                var prev = ResultOnThisFrame[calculator];

                ResultOnThisFrame[calculator] = (score, prev.sum + score, prev.frameCount + 1);
#endif

                if (calculator.CurrentTarget != null)
                {
                    score *= GetScoreFactor(calculator);

                    if (!_attackTargetsBuffer.ContainsKey(calculator.CurrentTarget))
                    {
                        _attackTargetsBuffer.Add(calculator.CurrentTarget, 0f);
                    }

                    _attackTargetsBuffer[calculator.CurrentTarget] += score;
                }
            }

            var maxAttackScore = -1f;

            foreach (var kv in _attackTargetsBuffer)
            {
                if (maxAttackScore < kv.Value)
                {
                    maxAttackScore = kv.Value;

                    CurrentAttackTarget = kv.Key;
                }
            }

            if (CurrentAttackTarget != null)
            {
                CoreService.Event.Send(_character, RequestAttackTargetEvent.Create(CurrentAttackTarget));
            }

            _attackTargetsBuffer.Clear();

            _moveCalculateTimePassed += dt;

            if (_moveCalculateTimePassed >= _currentMoveCalculateDelay)
            {
                var targetDiff = Vector2.zero;

                foreach (var kv in _moveCalculators)
                {
                    var calculator = kv.Value;

                    var score = calculator.Calculate();

#if UNITY_EDITOR
                    var prev = ResultOnThisFrame[calculator];

                    ResultOnThisFrame[calculator] = (score, prev.sum + score, prev.frameCount + 1);
#endif

                    score *= GetScoreFactor(calculator);

                    if (calculator.TargetPos.HasValue)
                    {
                        targetDiff += (calculator.TargetPos.Value - _character.Position).normalized * score;
                    }

                    // if (maxScore < score)
                    // {
                    //     maxScore = score;
                    //     maxScoredCalculator = calculator;
                    // }
                }

                // maxScoredCalculator?.Execute();

                CoreService.Event.SendImmediate(_character, RequestMoveEvent.Create(targetDiff.normalized * 4f + _character.Position));

                _moveCalculateTimePassed = 0f;
                _currentMoveCalculateDelay = Random.Range(MinMoveCalculateDelay, MaxMoveCalculateDelay);
            }

            if (CurrentAttackTarget != null)
            {
                if (CurrentAttackTarget.Position.x < _character.Position.x)
                {
                    _character.Animator.LookDirection = Direction.Left;
                }
                else
                {
                    _character.Animator.LookDirection = Direction.Right;
                }
            }

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

        public IAICalculator GetCalculatorTyped(Type calculatorType)
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