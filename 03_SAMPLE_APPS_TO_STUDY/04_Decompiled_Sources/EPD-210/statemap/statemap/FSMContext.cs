using System;
using System.Collections;

namespace statemap
{
	[Serializable]
	public abstract class FSMContext
	{
		protected string name_;

		protected State state_;

		protected Stack stateStack_;

		[NonSerialized]
		protected string transition_;

		[NonSerialized]
		protected State previousState_;

		public string Name
		{
			get
			{
				return name_;
			}
			set
			{
				name_ = value;
			}
		}

		public bool InTransition => (state_ == null) ? true : false;

		public State PreviousState
		{
			get
			{
				if (previousState_ != null)
				{
					return previousState_;
				}
				throw new NullReferenceException("Previous state not set.");
			}
		}

		public event StateChangeEventHandler StateChange;

		public FSMContext(State state)
		{
			name_ = "FSMContext";
			state_ = state;
			transition_ = string.Empty;
			previousState_ = null;
			stateStack_ = new Stack();
		}

		public FSMContext()
			: this(null)
		{
		}

		public abstract void EnterStartState();

		public void SetState(State state)
		{
			StateChangeEventArgs e = new StateChangeEventArgs(name_, "SET", state_, state);
			if (state_ != null)
			{
				previousState_ = state_;
			}
			state_ = state;
			OnStateChange(e);
		}

		public void ClearState()
		{
			previousState_ = state_;
			state_ = null;
		}

		public void PushState(State state)
		{
			StateChangeEventArgs e = new StateChangeEventArgs(name_, "PUSH", state_, state);
			if (state_ != null)
			{
				stateStack_.Push(state_);
			}
			previousState_ = state_;
			state_ = state;
			OnStateChange(e);
		}

		public void PopState()
		{
			if (stateStack_.Count == 0)
			{
				throw new InvalidOperationException("popping an empty state stack");
			}
			State newState = (State)stateStack_.Pop();
			StateChangeEventArgs e = new StateChangeEventArgs(name_, "POP", state_, newState);
			previousState_ = state_;
			state_ = newState;
			OnStateChange(e);
		}

		public void EmptyStateStack()
		{
			stateStack_.Clear();
		}

		public string GetTransition()
		{
			return transition_;
		}

		~FSMContext()
		{
			name_ = null;
			state_ = null;
			transition_ = null;
			previousState_ = null;
			stateStack_ = null;
		}

		protected virtual void OnStateChange(StateChangeEventArgs e)
		{
			if (this.StateChange != null)
			{
				this.StateChange(this, e);
			}
		}
	}
}
