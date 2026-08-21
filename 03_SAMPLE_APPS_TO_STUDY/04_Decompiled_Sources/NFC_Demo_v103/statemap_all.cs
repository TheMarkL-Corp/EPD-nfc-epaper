using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;

[assembly: AssemblyTitle("SMC statemap .Net Library")]
[assembly: AssemblyDescription("State Machine Compiler .Net runtime library")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("")]
[assembly: AssemblyCopyright("charlesr")]
[assembly: AssemblyTrademark("")]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: AssemblyVersion("1.0.5384.19132")]
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
	public delegate void StateChangeEventHandler(object sender, StateChangeEventArgs args);
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
