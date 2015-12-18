using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Deterministic
{
	public class MethodAttributeException : Exception
	{
		public MethodInfo Method;

		internal MethodAttributeException(MethodInfo info, string msg, string solutions)
			: base(msg + "\n" + solutions)
		{
			Method = info;
		}
	}
}
