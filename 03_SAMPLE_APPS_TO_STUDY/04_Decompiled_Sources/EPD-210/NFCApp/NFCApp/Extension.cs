using System.Windows.Forms;

namespace NFCApp
{
	public static class Extension
	{
		public static void InvokeIfRequired(this Control control, MethodInvoker action)
		{
			if (control != null)
			{
				if (control.InvokeRequired)
				{
					control.Invoke(action);
				}
				else
				{
					action();
				}
			}
		}
	}
}
