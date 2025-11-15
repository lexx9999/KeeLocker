using KeePass.Plugins;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace KeeLocker.Forms
{
	public partial class KeeLockerScanResults : Form
	{
		private readonly IPluginHost m_host;
		private readonly KeeLockerExt m_plugin;

		public KeeLockerScanResults(KeePass.Plugins.IPluginHost host, KeeLockerExt plugin, System.Collections.Generic.List<BitLockerWMI.VolumeInfo> volumeList)
		{
			InitializeComponent();
			this.m_host = host;
			this.m_plugin = plugin;

			XmlSerializer serializer = new XmlSerializer(volumeList.GetType());
			StringWriter writer = new StringWriter();
			serializer.Serialize(writer, volumeList);
			tx_Scan.Text = writer.ToString();
		}

	}
}
