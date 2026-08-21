using statemap;
using System;
using System.CodeDom.Compiler;

namespace com.advantech.nfc.api
{
	[GeneratedCode("smc", " v. 6.6.0")]
	public sealed class NFCStateContext : FSMContext
	{
		[GeneratedCode("smc", " v. 6.6.0")]
		public abstract class NFCStateState : State
		{
			internal NFCStateState(string name, int id)
				: base(name, id)
			{
			}

			protected internal virtual void Entry(NFCStateContext context)
			{
			}

			protected internal virtual void Exit(NFCStateContext context)
			{
			}

			protected internal virtual void Exception(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void Reset(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void RxMessage(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void TagFound(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void TxMessage(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void Wait(NFCStateContext context)
			{
				Default(context);
			}

			protected internal virtual void Default(NFCStateContext context)
			{
				throw new TransitionUndefinedException("State: " + context.State.Name + ", Transition: " + context.GetTransition());
			}
		}

		[GeneratedCode("smc", " v. 6.6.0")]
		internal abstract class Map1
		{
			[NonSerialized]
			internal static readonly Map1_Default.Map1__0001 _0001 = new Map1_Default.Map1__0001("Map1._0001", 0);

			[NonSerialized]
			internal static readonly Map1_Default.Map1__0100 _0100 = new Map1_Default.Map1__0100("Map1._0100", 1);

			[NonSerialized]
			internal static readonly Map1_Default.Map1__0500 _0500 = new Map1_Default.Map1__0500("Map1._0500", 2);

			[NonSerialized]
			internal static readonly Map1_Default.Map1__1000 _1000 = new Map1_Default.Map1__1000("Map1._1000", 3);

			[NonSerialized]
			private static readonly Map1_Default Default = new Map1_Default("Map1.Default", -1);
		}

		[GeneratedCode("smc", " v. 6.6.0")]
		internal class Map1_Default : NFCStateState
		{
			[GeneratedCode("smc", " v. 6.6.0")]
			internal class Map1__0001 : Map1_Default
			{
				internal Map1__0001(string name, int id)
					: base(name, id)
				{
				}

				protected internal override void Entry(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					owner.S(1, "Init");
					owner.SetNFCState(NFCSTATE.NFCSTATE_NONE);
					owner.ClearTag();
					owner.InitQ();
					owner.setCommEnable(false);
					owner.SetWaitTimer(0, 50);
				}

				protected internal override void Exit(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					owner.ClearAllWaitTimers();
				}

				protected internal override void Default(NFCStateContext context)
				{
				}

				protected internal override void Exception(NFCStateContext context)
				{
				}

				protected internal override void Reset(NFCStateContext context)
				{
				}

				protected internal override void RxMessage(NFCStateContext context)
				{
				}

				protected internal override void TagFound(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.T(100, "Found A new Tag");
						owner.CreateCommander();
					}
					finally
					{
						context.State = Map1._0100;
						context.State.Entry(context);
					}
				}

				protected internal override void TxMessage(NFCStateContext context)
				{
				}

				protected internal override void Wait(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					NFCStateState state = context.State;
					context.ClearState();
					try
					{
						owner.CheckTag();
						owner.SetWaitTimer(0, 50);
					}
					finally
					{
						context.State = state;
					}
				}
			}

			[GeneratedCode("smc", " v. 6.6.0")]
			internal class Map1__0100 : Map1_Default
			{
				internal Map1__0100(string name, int id)
					: base(name, id)
				{
				}

				protected internal override void Entry(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					owner.S(100, "Setup");
					owner.SetWaitTimer(0, 10);
					owner.SetNFCState(NFCSTATE.NFCSTATE_INIT);
				}

				protected internal override void Default(NFCStateContext context)
				{
					context.State.Exit(context);
					context.State = Map1._0001;
					context.State.Entry(context);
				}

				protected internal override void Reset(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.T(1, "Reset");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void Wait(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					if (owner.getWaitIndex() == 0)
					{
						context.State.Exit(context);
						context.ClearState();
						try
						{
							owner.SetupFTM();
						}
						finally
						{
							context.State = Map1._0500;
							context.State.Entry(context);
						}
					}
					else
					{
						base.Wait(context);
					}
				}
			}

			[GeneratedCode("smc", " v. 6.6.0")]
			internal class Map1__0500 : Map1_Default
			{
				internal Map1__0500(string name, int id)
					: base(name, id)
				{
				}

				protected internal override void Entry(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					owner.S(500, "Test Communication");
					owner.SetNFCState(NFCSTATE.NFCSTATE_TEST);
					owner.CheckMBCtrl();
					owner.SetWaitTimer(0, 1);
					owner.SetWaitTimer(1, 5);
					owner.SetWaitTimer(2, 500);
				}

				protected internal override void Exit(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					owner.ClearAllWaitTimers();
				}

				protected internal override void Default(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.C("0500 Wrong Event");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void Exception(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.E(1, "NFC Exception, reset");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void Reset(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.T(1, "Reset");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void RxMessage(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.SetNFCState(NFCSTATE.NFCSTATE_BUSY);
						owner.HandleRxMessage();
						owner.SetNFCState(NFCSTATE.NFCSTATE_READY);
					}
					finally
					{
						context.State = Map1._1000;
						context.State.Entry(context);
					}
				}

				protected internal override void TxMessage(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					NFCStateState state = context.State;
					context.ClearState();
					try
					{
						owner.SetNFCState(NFCSTATE.NFCSTATE_BUSY);
						owner.HandleTxMessage();
						owner.SetNFCState(NFCSTATE.NFCSTATE_READY);
					}
					finally
					{
						context.State = state;
					}
				}

				protected internal override void Wait(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					if (owner.getWaitIndex() == 0)
					{
						NFCStateState state = context.State;
						context.ClearState();
						try
						{
							owner.CheckMBCtrl();
							owner.SetWaitTimer(0, 5);
						}
						finally
						{
							context.State = state;
						}
					}
					else if (owner.getWaitIndex() == 1)
					{
						NFCStateState state2 = context.State;
						context.ClearState();
						try
						{
							owner.TestAPI();
							owner.SetWaitTimer(1, 50);
						}
						finally
						{
							context.State = state2;
						}
					}
					else if (owner.getWaitIndex() == 2)
					{
						context.State.Exit(context);
						context.ClearState();
						try
						{
							owner.E(1, "NFCAPI didnt send test command");
						}
						finally
						{
							context.State = Map1._0001;
							context.State.Entry(context);
						}
					}
					else
					{
						base.Wait(context);
					}
				}
			}

			[GeneratedCode("smc", " v. 6.6.0")]
			internal class Map1__1000 : Map1_Default
			{
				internal Map1__1000(string name, int id)
					: base(name, id)
				{
				}

				protected internal override void Entry(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					owner.S(1000, "Ready");
					owner.SetNFCState(NFCSTATE.NFCSTATE_READY);
					owner.CheckMBCtrl();
					owner.SetWaitTimer(0, 1);
					owner.setCommEnable(true);
				}

				protected internal override void Exit(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					owner.ClearAllWaitTimers();
				}

				protected internal override void Default(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					NFCStateState state = context.State;
					context.ClearState();
					try
					{
						owner.C("1000 Wrong Event");
					}
					finally
					{
						context.State = state;
					}
				}

				protected internal override void Exception(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.E(1, "NFC Exception, reset");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void Reset(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.T(1, "Reset");
					}
					finally
					{
						context.State = Map1._0001;
						context.State.Entry(context);
					}
				}

				protected internal override void RxMessage(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					NFCStateState state = context.State;
					context.ClearState();
					try
					{
						owner.SetNFCState(NFCSTATE.NFCSTATE_BUSY);
						owner.HandleRxMessage();
						owner.SetNFCState(NFCSTATE.NFCSTATE_READY);
					}
					finally
					{
						context.State = state;
					}
				}

				protected internal override void TagFound(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					context.State.Exit(context);
					context.ClearState();
					try
					{
						owner.T(100, "Re-Found Tag Again");
						owner.CreateCommander();
					}
					finally
					{
						context.State = Map1._0100;
						context.State.Entry(context);
					}
				}

				protected internal override void TxMessage(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					NFCStateState state = context.State;
					context.ClearState();
					try
					{
						owner.SetNFCState(NFCSTATE.NFCSTATE_BUSY);
						owner.HandleTxMessage();
						owner.SetNFCState(NFCSTATE.NFCSTATE_READY);
					}
					finally
					{
						context.State = state;
					}
				}

				protected internal override void Wait(NFCStateContext context)
				{
					NFCState owner = context.Owner;
					if (owner.getWaitIndex() == 0)
					{
						NFCStateState state = context.State;
						context.ClearState();
						try
						{
							owner.CheckMBCtrl();
							owner.SetWaitTimer(0, 1);
						}
						finally
						{
							context.State = state;
						}
					}
					else
					{
						base.Wait(context);
					}
				}
			}

			internal Map1_Default(string name, int id)
				: base(name, id)
			{
			}
		}

		[NonSerialized]
		private NFCState _owner;

		public NFCStateState State
		{
			get
			{
				if (state_ == null)
				{
					throw new StateUndefinedException();
				}
				return (NFCStateState)state_;
			}
			set
			{
				SetState(value);
			}
		}

		public NFCState Owner
		{
			get
			{
				return _owner;
			}
			set
			{
				_owner = value;
			}
		}

		public NFCStateContext(NFCState owner)
			: base(Map1._0001)
		{
			_owner = owner;
		}

		public override void EnterStartState()
		{
			State.Entry(this);
		}

		public void Exception()
		{
			transition_ = "Exception";
			State.Exception(this);
			transition_ = "";
		}

		public void Reset()
		{
			transition_ = "Reset";
			State.Reset(this);
			transition_ = "";
		}

		public void RxMessage()
		{
			transition_ = "RxMessage";
			State.RxMessage(this);
			transition_ = "";
		}

		public void TagFound()
		{
			transition_ = "TagFound";
			State.TagFound(this);
			transition_ = "";
		}

		public void TxMessage()
		{
			transition_ = "TxMessage";
			State.TxMessage(this);
			transition_ = "";
		}

		public void Wait()
		{
			transition_ = "Wait";
			State.Wait(this);
			transition_ = "";
		}
	}
}
