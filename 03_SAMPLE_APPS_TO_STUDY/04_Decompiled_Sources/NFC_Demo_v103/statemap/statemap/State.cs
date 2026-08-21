using System;

namespace statemap
{
	[Serializable]
	public abstract class State
	{
		private string _name;

		private int _id;

		public string Name => _name;

		public int Id => _id;

		protected State(string name, int id)
		{
			_name = name;
			_id = id;
		}

		public override string ToString()
		{
			return _name;
		}
	}
}
