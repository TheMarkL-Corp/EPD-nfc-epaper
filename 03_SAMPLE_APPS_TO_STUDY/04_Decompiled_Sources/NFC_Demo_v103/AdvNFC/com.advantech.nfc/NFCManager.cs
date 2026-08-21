using com.advantech.nfc.api;
using J_RFID;
using System.Linq;
using System.Threading.Tasks;

namespace com.advantech.nfc
{
	public class NFCManager : NFCSTATEChangeCallback
	{
		private static NFCManager _instance = null;

		private static NFCTagChangeListener _tagChange = null;

		private static RFIDAPI _rfid_api = new RFIDAPI();

		public INFCEDPAPI _epd_api = null;

		private INFCCommand _nfc_command;

		private NFCState _nfc_state;

		private byte[] _tag = null;

		private bool _commEnable = false;

		private NFCTagState _lastState;

		public NFCTagChangeListener TagChange
		{
			get
			{
				return _tagChange;
			}
			set
			{
				_tagChange = value;
			}
		}

		private NFCManager()
		{
		}

		private async void startNFCState()
		{
			await Task.Factory.StartNew(delegate
			{
				_nfc_state.run();
			}, TaskCreationOptions.LongRunning);
		}

		private void stopNFCState()
		{
			if (_nfc_state != null)
			{
				_nfc_state.stop();
			}
		}

		public static NFCManager getInstance()
		{
			if (_instance == null)
			{
				_instance = new NFCManager();
			}
			return _instance;
		}

		public void setNFCCommand(INFCCommand command)
		{
			if (_nfc_command != null)
			{
				stopNFCState();
				_epd_api = null;
				_nfc_state.setNFCCommand(null);
				_nfc_command = null;
				_nfc_state = null;
			}
			if (command != null)
			{
				_nfc_state = new NFCState();
				_nfc_state.setStateChangeCallback(this);
				_nfc_command = command;
				_nfc_state.setNFCCommand(command);
				_epd_api = new LeoD30EPDAPI(command, _nfc_state);
				startNFCState();
			}
		}

		public INFCEDPAPI getNfcAPI()
		{
			return _epd_api;
		}

		public byte[] getTagID()
		{
			return _tag;
		}

		internal void setTagID(byte[] tag)
		{
			_tag = tag?.ToArray();
			if (tag == null)
			{
				doChangeTagListener(NFCTagState.NFC_TAG_STATE_TAG_OFF);
			}
			else
			{
				doChangeTagListener(NFCTagState.NFC_TAG_STATE_TAG_ON);
			}
		}

		public void resetNFCState()
		{
		}

		protected void doChangeTagListener(NFCTagState state)
		{
			if (_tagChange != null && _lastState != state)
			{
				_tagChange.onTagStateChange(state);
				_lastState = state;
			}
		}

		internal void setCommEnable(bool v)
		{
			_commEnable = v;
			if (v)
			{
				doChangeTagListener(NFCTagState.NFC_TAG_STATE_COMM_ON);
			}
			else if (_tag != null)
			{
				doChangeTagListener(NFCTagState.NFC_TAG_STATE_TAG_ON);
			}
			else
			{
				doChangeTagListener(NFCTagState.NFC_TAG_STATE_TAG_OFF);
			}
		}

		public bool getCommEanble()
		{
			return _commEnable;
		}

		public void onNFCStateChange(NFCSTATE new_state)
		{
		}
	}
}
