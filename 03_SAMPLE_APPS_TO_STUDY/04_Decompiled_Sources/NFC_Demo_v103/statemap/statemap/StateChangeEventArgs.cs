using System;

namespace statemap
{
	public class StateChangeEventArgs : EventArgs
	{
		private readonly string _fsmName;

		private readonly string _transitionType;

		private readonly State _previousState;

		private readonly State _newState;

		public StateChangeEventArgs(string fsmName, string transitionType, State previousState, State newState)
		{
			_fsmName = fsmName;
			_transitionType = transitionType;
			_previousState = previousState;
			_newState = newState;
		}

		public string FSMName()
		{
			return _fsmName;
		}

		public string TransitionType()
		{
			return _transitionType;
		}

		public State PreviousState()
		{
			return _previousState;
		}

		public State NewState()
		{
			return _newState;
		}
	}
}
