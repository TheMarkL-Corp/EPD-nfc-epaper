#define DEBUG
using com.advantech.nfc.cmd;
using System;
using System.Diagnostics;
using System.Threading;

namespace com.advantech.nfc.api
{
	public enum NFCSTATE
	{
		NFCSTATE_NONE,
		NFCSTATE_INIT,
		NFCSTATE_TEST,
		NFCSTATE_READY,
		NFCSTATE_BUSY
	}
	public class NFCState
	{
		private bool _enableLog = false;

		private int _last_state;

		private bool _interrupted;

		internal NFCBuffer nfcBuffer;

		private NFCStateContext _fsm;

		private D30Command _command;

		private NFCSTATEChangeCallback stateChangeCB = null;

		private NFCSTATE current_state = NFCSTATE.NFCSTATE_NONE;

		private BlockingQueue<FTMEventRec> eventQ = new BlockingQueue<FTMEventRec>(20, "Q", true);

		private bool txInQ = false;

		private FTMEventRec currentEvent;

		private Timer timer;

		private const int maxTickTimer = 3;

		private int[] tickTimers = new int[3];

		private bool[] waitInQ = new bool[3];

		private int waitQIndex = -1;

		private bool _stopped;

		public NFCState()
		{
			StartTickTimer();
			nfcBuffer = new NFCBuffer();
			_command = null;
			ResetContext();
		}

		private void ResetContext()
		{
			_fsm = new NFCStateContext(this);
		}

		private void log(string v)
		{
			if (_enableLog)
			{
				Debug.Print(v);
			}
		}

		internal void setNFCCommand(INFCCommand command)
		{
			_command = (D30Command)command;
			if (_command == null)
			{
				ResetContext();
			}
		}

		internal bool readyToTx()
		{
			if (_command != null)
			{
				if (!txInQ)
				{
					if (nfcBuffer.getWriteBufferLength() <= 0)
					{
						NFCSTATE nFCSTATE = current_state;
						if ((uint)nFCSTATE > 1u && nFCSTATE != NFCSTATE.NFCSTATE_BUSY)
						{
							return true;
						}
						return false;
					}
					return false;
				}
				return false;
			}
			return false;
		}

		internal byte[] getRx()
		{
			if (_command != null)
			{
				if (nfcBuffer.getReadBufferLength() != 0)
				{
					return nfcBuffer.getDataReceived();
				}
				return null;
			}
			return null;
		}

		internal byte[] buildNFCPacket(byte command, byte[] data)
		{
			NFCManager instance = NFCManager.getInstance();
			if (_command != null)
			{
				int num = (data != null) ? data.Length : 0;
				byte[] array = new byte[num + 3];
				array[0] = command;
				array[1] = (byte)num;
				int num2 = (array[0] & 0xFF) + (array[1] & 0xFF);
				for (int i = 0; i < num; i++)
				{
					array[i + 2] = data[i];
					num2 += (data[i] & 0xFF);
				}
				array[num + 2] = (byte)((256 - num2) & 0xFF);
				return array;
			}
			return null;
		}

		public void setStateChangeCallback(NFCSTATEChangeCallback cb)
		{
			stateChangeCB = cb;
		}

		public void removeStateChangeCallback()
		{
			stateChangeCB = null;
		}

		public NFCSTATE getNFCState()
		{
			return current_state;
		}

		public void SetNFCState(NFCSTATE s)
		{
			current_state = s;
			if (stateChangeCB != null)
			{
				stateChangeCB.onNFCStateChange(s);
			}
		}

		public void addEvent(FTMEventRec rec)
		{
			if (rec.type == FTMEventType.FTMEVENT_WAIT)
			{
				if (waitInQ[(int)rec.data])
				{
					return;
				}
				waitInQ[(int)rec.data] = true;
			}
			else if (rec.type == FTMEventType.FTMEVENT_TX_MESSAGE)
			{
				txInQ = true;
			}
			eventQ.Enqueue(rec);
		}

		public void addEvent(FTMEventType type, object data)
		{
			addEvent(new FTMEventRec(type, data));
		}

		public FTMEventRec popEvent()
		{
			FTMEventRec item = null;
			eventQ.Dequeue(ref item);
			if (item.type == FTMEventType.FTMEVENT_WAIT)
			{
				waitInQ[(int)item.data] = false;
			}
			else if (item.type == FTMEventType.FTMEVENT_TX_MESSAGE)
			{
				txInQ = false;
			}
			return item;
		}

		internal void StartTickTimer()
		{
			timer = new Timer(timerCallback, null, 0, 10);
			for (int i = 0; i < 3; i++)
			{
				tickTimers[i] = -1;
				waitInQ[i] = false;
			}
		}

		internal void StopTickTimer()
		{
			timer.Dispose();
			timer = null;
		}

		internal void SetWaitTimer(int index, int ms10)
		{
			if (index < 3)
			{
				tickTimers[index] = ms10;
			}
		}

		internal void ClearWaitTimer(int index)
		{
			if (index < 3)
			{
				tickTimers[index] = -1;
			}
		}

		internal void ClearAllWaitTimers()
		{
			for (int i = 0; i < 3; i++)
			{
				tickTimers[i] = -1;
			}
		}

		private void timerCallback(object state)
		{
			for (int i = 0; i < 3; i++)
			{
				if (tickTimers[i] >= 0)
				{
					tickTimers[i]--;
				}
				if (tickTimers[i] == 0)
				{
					addEvent(FTMEventType.FTMEVENT_WAIT, i);
				}
			}
		}

		internal void S(int state, string s)
		{
			log($"{state:0000}: " + s);
			_last_state = state;
		}

		internal void E(int state, string s)
		{
			log($"{_last_state:0000}->{state:0000}: " + s);
		}

		internal void T(int state, string s)
		{
			log($"{_last_state:0000}->{state:0000}: " + s);
		}

		internal void C(string s)
		{
			log($"{_last_state:0000}: " + s);
		}

		internal void run()
		{
			_interrupted = false;
			_fsm.EnterStartState();
			_stopped = false;
			while (true)
			{
				currentEvent = popEvent();
				if (_interrupted)
				{
					break;
				}
				switch (currentEvent.type)
				{
				case FTMEventType.FTMEVENT_WAIT:
					waitQIndex = (int)currentEvent.data;
					_fsm.Wait();
					break;
				case FTMEventType.FTMEVENT_TX_MESSAGE:
					if (currentEvent.data != null)
					{
						nfcBuffer.putWriteBuffer((byte[])currentEvent.data);
						_fsm.TxMessage();
					}
					break;
				case FTMEventType.FTMEVENT_RX_MESSAGE:
					_fsm.RxMessage();
					break;
				case FTMEventType.FTMEVENT_TAG_FOUND:
					_fsm.TagFound();
					break;
				case FTMEventType.FTMEVENT_EXCEPTION:
					_fsm.Exception();
					break;
				case FTMEventType.FTMEVENT_RESET:
					_fsm.Reset();
					break;
				}
			}
			_stopped = true;
		}

		public void stop()
		{
			_interrupted = true;
			addEvent(FTMEventType.FTMEVENT_RESET, 0);
			while (!_stopped)
			{
				Thread.Sleep(1);
			}
		}

		internal void InitQ()
		{
		}

		internal void ClearTag()
		{
			NFCManager.getInstance().setTagID(null);
		}

		internal void CheckTag()
		{
			if (_command != null && _command.isValid())
			{
				addEvent(FTMEventType.FTMEVENT_TAG_FOUND, null);
			}
		}

		internal void setCommEnable(bool v)
		{
			NFCManager.getInstance().setCommEnable(v);
		}

		internal void CreateCommander()
		{
		}

		internal int getWaitIndex()
		{
			return waitQIndex;
		}

		private void dumpMBCtrl(byte mbCtrlDyn)
		{
			string text = "MBCtrlDyn=";
			if ((mbCtrlDyn & 1) != 0)
			{
				text += " MB_EN";
			}
			if ((mbCtrlDyn & 2) != 0)
			{
				text += " HOST_PUT_MSG";
			}
			if ((mbCtrlDyn & 4) != 0)
			{
				text += " RF_PUT_MSG";
			}
			if ((mbCtrlDyn & 8) != 0)
			{
				text += " RFU";
			}
			if ((mbCtrlDyn & 0x10) != 0)
			{
				text += " HOST_MISS_MSG";
			}
			if ((mbCtrlDyn & 0x20) != 0)
			{
				text += " RF_MISS_MSG";
			}
			if ((mbCtrlDyn & 0x40) != 0)
			{
				text += " HOST_CURRENT_MSG";
			}
			if ((mbCtrlDyn & 0x80) != 0)
			{
				text += " RF_CURRENT_MSG";
			}
			log(text);
		}

		private void dumpEHCtrl(byte ehCtrlDyn)
		{
			string text = "EHCtrlDyn=";
			if ((ehCtrlDyn & 1) != 0)
			{
				text += " EH_EN";
			}
			if ((ehCtrlDyn & 2) != 0)
			{
				text += " EH_ON";
			}
			if ((ehCtrlDyn & 4) != 0)
			{
				text += " FIELD_ON";
			}
			if ((ehCtrlDyn & 8) != 0)
			{
				text += " VCC_ON";
			}
			log(text);
		}

		private void dumpData(string m, byte[] recv)
		{
			string text = "";
			for (int i = 0; i < recv.Length; i++)
			{
				text += $"{recv[i]:X2} ";
			}
			log($"{m}={text}");
		}

		internal void SetupFTM()
		{
			if (_command != null)
			{
				try
				{
					_command.presentPassword();
					byte b = _command.readDynConfig(2);
					dumpEHCtrl(b);
					Thread.Sleep(5);
					if (true)
					{
						byte b2 = _command.ReadConfiguration(2);
						if (b2 != 1)
						{
							_command.WriteConfiguration(2, 1);
						}
						if ((b & 1) == 0)
						{
							_command.writeDynConfig(2, 1);
						}
					}
					else if ((b & 1) == 0 && _command.ReadConfiguration(2) != 0)
					{
						_command.WriteConfiguration(2, 0);
					}
					byte b3 = _command.readDynConfig(13);
					dumpMBCtrl(b3);
					try
					{
						if ((b3 & 1) == 0)
						{
							for (int num = 10; num > 0; num--)
							{
								try
								{
									_command.writeDynConfig(13, 1);
									Thread.Sleep(5);
									b3 = _command.readDynConfig(13);
									dumpMBCtrl(b3);
								}
								catch (NFCException)
								{
								}
							}
						}
						if ((b3 & 1) == 1)
						{
							log("check FTM enable ok");
						}
						else
						{
							log("check FTM enable not ok");
						}
						byte b4 = _command.readMessageLength();
						if (b4 > 0)
						{
							byte[] recv = _command.readMessage(0, 0);
							log("msg len=" + b4.ToString());
							dumpData("RX ", recv);
						}
					}
					catch (NFCException)
					{
						log("Setup FTM fails");
					}
				}
				catch (NFCException)
				{
					ResetContext();
				}
			}
		}

		internal void CheckMBCtrl()
		{
			try
			{
				byte b;
				if (_command != null)
				{
					b = _command.readDynConfig(13);
					if ((b & 1) != 0)
					{
						if ((b & 2) != 0)
						{
							addEvent(FTMEventType.FTMEVENT_RX_MESSAGE, null);
						}
						if ((b & 4) == 0)
						{
							goto IL_0055;
						}
						goto IL_0055;
					}
				}
				goto end_IL_0001;
				IL_0064:
				if ((b & 0x20) == 0)
				{
					goto IL_0073;
				}
				goto IL_0073;
				IL_0055:
				if ((b & 0x10) == 0)
				{
					goto IL_0064;
				}
				goto IL_0064;
				IL_0073:
				if ((b & 6) == 0 && nfcBuffer.getWriteBufferLength() > 0)
				{
					byte[] dataTransmitted = nfcBuffer.getDataTransmitted();
					_command.writeMessage(dataTransmitted);
					dumpData("TX", dataTransmitted);
				}
				end_IL_0001:;
			}
			catch (NFCException)
			{
				addEvent(FTMEventType.FTMEVENT_EXCEPTION, null);
			}
		}

		internal void HandleRxMessage()
		{
			try
			{
				int num = _command.readMessageLength();
				byte[] sourceArray = _command.readMessage(0, num);
				byte[] array = new byte[num + 2];
				Array.Copy(sourceArray, 0, array, 0, num + 2);
				dumpData($"HRX({num})", array);
				nfcBuffer.putReadBuffer(array);
			}
			catch (NFCException)
			{
			}
		}

		internal void HandleTxMessage()
		{
			try
			{
				if (nfcBuffer.getWriteBufferLength() > 0)
				{
					byte[] dataTransmitted = nfcBuffer.getDataTransmitted();
					_command.writeMessage(dataTransmitted);
					dumpData("HTX", dataTransmitted);
				}
			}
			catch (NFCException)
			{
			}
		}

		internal void TestAPI()
		{
			log("TestAPI");
			INFCEDPAPI nfcAPI = NFCManager.getInstance().getNfcAPI();
			nfcAPI.TestAPI();
		}
	}
}
