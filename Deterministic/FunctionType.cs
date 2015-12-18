using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deterministic
{
	public enum FunctionType
	{
		DeterministicWithSideEffects,
		DeterministicWithoutSideEffects,
		NonDeterministicWithSideEffects,
		NonDeterministicWithoutSideEffects
	}
}
