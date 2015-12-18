using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Deterministic
{
	class Graph<T>
	{
		public IEnumerable<T> Nodes
		{
			get { return relations.Keys; }
		}

		Dictionary<T, List<T>> relations;

		public Graph()
		{
			relations = new Dictionary<T, List<T>>();
		}

		public void AddNode(T node)
		{
			relations[node] = new List<T>();
		}

		public void Link(T from, T to)
		{
			if (!relations.ContainsKey(from)) relations[from] = new List<T>();
			relations[from].Add(to);
		}

		public List<T> ConnectedTo(T node)
		{
			return relations[node];
		}

		public List<T> ConnectedFrom(T node)
		{
			return relations.Keys.Where(n => relations[n].Contains(node)).ToList();
		}
	}
}
