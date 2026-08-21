using System;

namespace statemap
{
	public sealed class StateUndefinedException : ApplicationException
	{
		public StateUndefinedException()
		{
		}

		public StateUndefinedException(string message)
			: base(message)
		{
		}
	}
}
