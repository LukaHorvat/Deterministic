using Deterministic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BuildProject
{
	class A
	{
		public int Field;
		public string Temp;

		public void Increment()
		{
			Temp = "asd" + ++Field;
			//Field += 5;
		}
	}

	class Program
	{
		static void Main(string[] args)
		{
			AssemblyChecker.OutputIL(typeof(A).GetMethod("Increment"));
		}
	}
}
