using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Deterministic
{
	[AttributeUsage(AttributeTargets.Method)]
	public class DeterministicMethod : Attribute
	{
	}
}
