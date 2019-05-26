using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using ICMS.Modules.BaseComponents;
using TLAgent.OpcLibrary;

namespace ICMS.Modules.BaseComponents.Commons
{
	public class KepController
	{
		string _kepServerName = "KEPware.KEPServerEx.V4";
		string _kepServerIp = "192.168.0.150";
		private Thread _kepThread;
		public OpcHelper KepHelper;
		private ArrayList subScribeList = new ArrayList();
		private EQItem _item;
		public KepController(EQItem item, ArrayList valList)
		{
			_item = item;
			subScribeList = valList;
			KepHelper = new OpcHelper();
            KepHelper.ServerShutdownEvent += new OpcHelper.ServerShutdown(KepHelper_ServerShutdownEvent);
			_kepThread = new Thread(ConnectKep);
			_kepThread.Start();
		}

		void KepHelper_ServerShutdownEvent(string reason)
		{
			KepHelper.Disconnected();
			if (this._item != null)
			{
				_item.EQMessage.Text = reason;
			}

		}

		private void ConnectKep()
		{
			try
			{
				Thread.Sleep(5000);
				if (KepHelper != null && !KepHelper.State)
				{
					var result = KepHelper.Connect(_kepServerName, _kepServerIp);
					if (!result.Status)
					{
						if (this._item != null)
						{
							_item.EQMessage.Text = result.Message;
						}
					}
					else if (subScribeList.Count > 0)
					{
						foreach (var val in subScribeList)
						{
							KepHelper.Subscribe(val.ToString());
						}
					}

				}

			}
			catch (Exception e)
			{
				if (this._item != null)
				{
					_item.EQMessage.Text = e.Message;
				}
				this.ConnectKep();
			}
		}

		public void DisConnected()
		{
			if (_kepThread != null)
			{
				_kepThread.Abort();
				_kepThread.Join();
				_kepThread = null;
			}
			if (KepHelper != null)
			{
				KepHelper.Disconnected();
			}
		}
	}
}
