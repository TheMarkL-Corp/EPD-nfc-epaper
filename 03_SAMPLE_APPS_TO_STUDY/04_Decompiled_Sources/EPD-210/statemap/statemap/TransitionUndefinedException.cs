using System;

namespace statemap
{
	public sealed class TransitionUndefinedException : ApplicationException
	{
		public TransitionUndefinedException()
		{
		}

		public TransitionUndefinedException(string message)
			: base(message)
		{
		}
	}
}
