using System.ComponentModel;
using System.Windows.Forms;

namespace NovellBug522783
{
	public class Bug522783Dialog
	{
		public Bug522783Dialog ()
		{
			InitializeComponent ();
		}

		private void InitializeComponent ()
		{
			var resources = new ComponentResourceManager (typeof (Bug522783Dialog));
			var tableLayoutRevision = new TableLayoutPanel ();
			resources.ApplyResources (tableLayoutRevision, "tableLayoutRevision");
		}
	}
}
