using System;
using Core.Interface;
using Dpm.Stage.Spec;

namespace Dpm.Stage.Unit.AI
{
    public enum AICalculatorType
    {
        Move,
        Attack,
        Ability,
    }

    public interface IDecisionMaker : IUpdatable, IDisposable
    {
        IUnit CurrentAttackTarget { get; }

        void Init(Character character, MoveSpec moveSpec, AttackSpec attackSpec);

        float GetScoreFactor(Type calculatorType);

        bool IsUsingTyped(Type calculatorType);
    }
}