using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dpm.Utility
{
	public interface IGameSpec
	{
		public string Name { get; }
	}

	public interface ISpecTable
	{
		Type SpecType { get; }
	}

	public interface IGameSpecTable
	{
	}

	public abstract class SpecTableBase<T> : ScriptableObject, ISpecTable where T : IGameSpec
	{
		public Type SpecType => typeof(T);

		public List<T> specs;

		private IReadOnlyDictionary<string, T> _nameToSpec;

		public IReadOnlyDictionary<string, T> NameToSpec =>
			_nameToSpec ??= specs.ToDictionary(spec => spec.Name);
	}
}