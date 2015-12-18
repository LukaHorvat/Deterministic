using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Reflection;
using System.Reflection;

namespace Deterministic
{
	public class AssemblyChecker
	{
		public static void CheckAssembly(Assembly asm)
		{
			var types = new List<Tuple<Type, List<Tuple<MethodInfo, FunctionType>>>>();
			foreach (var type in asm.GetTypes().Where(t => !t.IsInterface))
			{
				var list = new List<Tuple<MethodInfo, FunctionType>>();
				types.Add(new Tuple<Type, List<Tuple<MethodInfo, FunctionType>>>(type, list));
				var methods = type.GetMethods();
				var deterministic = new Dictionary<MethodInfo, bool>();
				var noSideEffects = new Dictionary<MethodInfo, bool>();
				var graph = new Graph<MethodInfo>();
				foreach (var method in methods.Where(m => m.DeclaringType == type))
				{
					graph.AddNode(method);
					var instructions = method.GetInstructions();

					foreach (var instr in instructions.Where(i => i.OpCode.Name == "call"))
					{
						graph.Link((MethodInfo)instr.Operand, method);
					}
				}
				var deterministicQueue = new Queue<MethodInfo>();
				var sideEffectQueue = new Queue<MethodInfo>();
				foreach (var method in graph.Nodes)
				{
					//If the method cannot be analysed, we assume the worst case
					if (method.GetMethodBody() == null)
					{
						deterministic[method] = false;
						noSideEffects[method] = false;
						continue;
					}

					var instructions = method.GetInstructions();
					if (instructions.Any(i => new[] { "ldfld", "ldsfld", "ldflda", "ldsflda" }.Contains(i.OpCode.Name)))
					{
						deterministic[method] = false;
						deterministicQueue.Enqueue(method);
					}
					if (instructions.Any(i => new[] { "stfld", "stsfld", "stflda", "stsflda" }.Contains(i.OpCode.Name)))
					{
						noSideEffects[method] = false;
						sideEffectQueue.Enqueue(method);
					}
				}
				while (deterministicQueue.Count > 0)
				{
					var current = deterministicQueue.Dequeue();
					foreach (var link in graph.ConnectedTo(current))
					{
						if (deterministic.ContainsKey(link)) continue;
						deterministic[link] = false;
						deterministicQueue.Enqueue(link);
					}
				}
				while (sideEffectQueue.Count > 0)
				{
					var current = sideEffectQueue.Dequeue();
					foreach (var link in graph.ConnectedTo(current))
					{
						if (noSideEffects.ContainsKey(link)) continue;
						noSideEffects[link] = false;
						sideEffectQueue.Enqueue(link);
					}
				}
				foreach (var node in graph.Nodes)
				{
					if (node.DeclaringType != type) continue;
					if (!deterministic.ContainsKey(node)) deterministic[node] = true;
					if (!noSideEffects.ContainsKey(node)) noSideEffects[node] = true;
					list.Add(new Tuple<MethodInfo, FunctionType>
					(
						node,
						deterministic[node] ?
						(
							noSideEffects[node] ?
							FunctionType.DeterministicWithoutSideEffects :
							FunctionType.DeterministicWithSideEffects
						) :
						(
							noSideEffects[node] ?
							FunctionType.NonDeterministicWithoutSideEffects :
							FunctionType.NonDeterministicWithSideEffects
						)
					));
				}
			}

			foreach (var pair in types.SelectMany(t => t.Item2))
			{
				var method = pair.Item1;
				var attribs = method.GetCustomAttributes();
				if (attribs.Any(a => a is DeterministicMethod) &&
					pair.Item2 != FunctionType.DeterministicWithoutSideEffects &&
					pair.Item2 != FunctionType.DeterministicWithSideEffects)
				{
					throw new MethodAttributeException
					(
						method,
						String.Format("The method {0} has the deterministic attribute but is not deterministic.", method),
						"Any call to a non deterministic function or access to a field breaks determinism. \nEven if only incrementing/decrementing a field."
					);
				}
				if (attribs.Any(a => a is SideEffectFreeMethod) &&
					pair.Item2 != FunctionType.DeterministicWithoutSideEffects &&
					pair.Item2 != FunctionType.DeterministicWithSideEffects)
				{
					throw new MethodAttributeException
					(
						method,
						String.Format("The method {0} has the side effect free attribute but has side effects.", method),
						"Any call to a function with side effects or modification of a field makes the method have side effects."
					);
				}
			}
		}

		public static void OutputIL(MethodInfo info)
		{
			foreach (var instrs in info.GetInstructions())
			{
				Console.WriteLine(instrs);
			}
		}
	}
}
