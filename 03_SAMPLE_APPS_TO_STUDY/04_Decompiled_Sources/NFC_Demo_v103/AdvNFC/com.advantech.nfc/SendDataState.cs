namespace com.advantech.nfc
{
	public enum SendDataState
	{
		SDState_Getinfo,
		SDState_Reboot_FW_App,
		SDState_Erase,
		SDState_Unlock,
		SDState_SendData,
		SDState_Checksum_APP,
		SDState_Finish,
		SDState_Error,
		SDState_Erase_Error,
		SDState_NOAPP_Error,
		SDState_UPGRADEAPP_Error,
		SDState_BLTAPP_Error,
		SDState_DEV_VOLT_Error,
		SDState_Checksum_Error,
		SDState_Compare_Error
	}
}
