using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dpm.Utility
{
	public interface IGameSpec
	{
		public string Name { get; }
	}

	public abstract class SpecsHolderBase<T> : ScriptableObject where T : IGameSpec
	{
		public List<T> specs;

		private IReadOnlyDictionary<string, T> _nameToSpec;

		public IReadOnlyDictionary<string, T> NameToSpec =>
			_nameToSpec ??= specs.ToDictionary(spec => spec.Name);
	}
}