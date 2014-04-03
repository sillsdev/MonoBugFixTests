// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Data;
using System.Resources;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using System.Reflection;
using System.Xml.Serialization;
using System.Net.NetworkInformation;
using System.Xml.Schema;
using System.Drawing.Printing;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Collections;

namespace MonoBugFixTests
{
	[TestFixture]
	public class Test
	{
		#region Novell Bug #519648

		// Pull request #920
		[Test]
		public void Bug519648_FindDoesntThrowWithNullObjectInArray () // Novell bug 519648
		{
			var dt = new DataTable ("datatable");

			var column = new DataColumn ();
			dt.Columns.Add (column);
			var columns = new DataColumn [] { column };
			dt.PrimaryKey = columns;

			try {
				Assert.AreEqual (null, dt.Rows.Find (new object [] { null }));
			} catch (IndexOutOfRangeException) {
				Assert.Fail ("Bug #159648 is not fixed.");
			}
		}

		[Test]
		public void Bug519648_FindDoesntThrowWithNullObject () // Novell bug 519648
		{
			var dt = new DataTable ("datatable");

			var column = new DataColumn ();
			dt.Columns.Add (column);
			var columns = new DataColumn [] { column };
			dt.PrimaryKey = columns;

			try {
				Assert.AreEqual (null, dt.Rows.Find ((object)null));
			} catch (IndexOutOfRangeException) {
				Assert.Fail ("Bug #159648 is not fixed.");
			}
		}

		#endregion

		#region Novell Bug #522783

		// We don't have a real patch for that, only a work-around, so there
		// is no pull request for this problem.
		[Test]
		public void Bug522783_ApplyResourcesForTableLayoutPanel ()
		{
			new NovellBug522783.Bug522783Dialog ();
			try {
				Assert.IsNotNull (new NovellBug522783.Bug522783Dialog ());
			} catch (TargetInvocationException e) {
				if (e.InnerException.GetType () == typeof(NotSupportedException))
					Assert.Fail ("Novell bug #522783 is not fixed.");

				// got a different exception not caused by #522783
				throw;
			}
		}

		#endregion

		#region Novell Bug #594490

		// pull request #921 (merged into master)
		// pull request #923 mono-3.2.8 branch
		public class Bug594490Class
		{
			[XmlAttribute ("xml:lang")]
			public string GroupName;
		}

		[Test]
		public void Bug594490_SerializationOfXmlLangAttribute ()
		{
			var mySerializer = new XmlSerializer (typeof(Bug594490Class));

			using (var writer = new StringWriter ()) {
				var myGroup = new Bug594490Class ();

				myGroup.GroupName = "hello world";

				mySerializer.Serialize (writer, myGroup);
				writer.Close ();
				Assert.AreEqual (@"<?xml version=""1.0"" encoding=""utf-16""?>" + Environment.NewLine +
					@"<Bug594490Class xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xml:lang=""hello world"" />",
					writer.ToString (),
					"Novell bug #594490 (https://bugzilla.novell.com/show_bug.cgi?id=594490) not fixed.");
			}
		}

		#endregion

		#region Novell Bug #599689

		// pull request #922 (merged into master)
		// pull request #923 mono-3.2.8 branch
		[Test]
		public void Bug599689_XmlSchemaException ()
		{
			Assert.AreEqual ("System.Xml.Schema.XmlSchemaException: Test",
				new XmlSchemaException ("Test").ToString ());
		}

		#endregion

		#region Novell Bug #602934

		#region CUPS methods and structs

		[StructLayout (LayoutKind.Sequential)]
		struct CUPS_DEST
		{
			public string Name;
			public string Instance;
			public int IsDefault;
			public int NumOptions;
			public IntPtr Options;
		}

		[StructLayout (LayoutKind.Sequential)]
		struct CUPS_OPTION
		{
			public string Name;
			public string Value;
		}

		readonly IntPtr CUPS_HTTP_DEFAULT = IntPtr.Zero;

		[DllImport ("libcups")]
		static extern IntPtr cupsGetNamedDest (IntPtr http, string name, string instance);

		[DllImport ("libcups")]
		static extern void cupsFreeDests (int num_dests, IntPtr dests);

		[DllImport ("libcups")]
		static extern void cupsFreeDests (int num_dests, ref CUPS_DEST dests);

		#endregion

		Dictionary<string, string> GetOptionsOfFirstPrinterThroughCups ()
		{
			var options = new Dictionary<string, string> ();

			var destPtr = cupsGetNamedDest (CUPS_HTTP_DEFAULT, PrinterSettings.InstalledPrinters [0], null);
			var dest = (CUPS_DEST)Marshal.PtrToStructure (destPtr, typeof(CUPS_DEST));
			var optionPtr = dest.Options;
			int cupsOptionSize = Marshal.SizeOf (typeof(CUPS_OPTION));
			for (int i = 0; i < dest.NumOptions; i++) {
				var cupsOption = (CUPS_OPTION)Marshal.PtrToStructure (optionPtr, typeof(CUPS_OPTION));
				options.Add (cupsOption.Name, cupsOption.Value);
				optionPtr = (IntPtr)((long)optionPtr + cupsOptionSize);
			}
			cupsFreeDests (1, destPtr);

			return options;
		}

		[Test]
		[Platform (Exclude = "Win", Reason = "Depends on CUPS which is usually not installed on Windows")]
		public void Bug602934_PrinterSettingsReturnActualValues ()
		{
			if (PrinterSettings.InstalledPrinters.Count < 1)
				Assert.Ignore ("Need at least one printer installed.");

			var options = GetOptionsOfFirstPrinterThroughCups ();

			var settings = new PrinterSettings () { PrinterName = PrinterSettings.InstalledPrinters [0] };
			Assert.AreEqual (options ["PageSize"], settings.DefaultPageSettings.PaperSize.PaperName,
				"Bug #602934 (https://bugzilla.novell.com/show_bug.cgi?id=602934) not fixed (PaperSize)");
			Assert.AreEqual (options ["Resolution"], string.Format ("{0}dpi", settings.DefaultPageSettings.PrinterResolution.X),
				"Bug #602934 (https://bugzilla.novell.com/show_bug.cgi?id=602934) not fixed (Resolution)");
		}

		#endregion

		#region Novell Bug #642227
		public class Sample1
		{
			[DataMember]
			public string Member1;
		}

		[Test]
		public void Bug642227_IgnoreUnknownElement()
		{
			string xml = @"<Test.Sample1 xmlns=""http://schemas.datacontract.org/2004/07/MonoBugFixTests"">" + Environment.NewLine +
@"	<Member2>bla</Member2>" + Environment.NewLine +
@"	<Member1>bar</Member1>" + Environment.NewLine +
@"</Test.Sample1>";
			new System.Runtime.Serialization.DataContractSerializer (typeof (Sample1))
				.ReadObject (System.Xml.XmlReader.Create (new StringReader (xml)));
		}
		#endregion

		#region Xamarin Bug #2394
		[Test] // Xamarin bug #2394
		public void Bug2394_RowHeightLessThanOldMinHeightVirtMode ()
		{
			using (var dgv = new DataGridView ()) {
				dgv.VirtualMode = true;
				dgv.RowCount = 1;
				dgv.Rows [0].MinimumHeight = 5;
				dgv.Rows [0].Height = 10;
				dgv.RowHeightInfoNeeded += (sender, e) => {
					// NOTE: the order is important here.
					e.MinimumHeight = 2;
					e.Height = 2;
				};
				dgv.UpdateRowHeightInfo (0, false);
				Assert.AreEqual (2, dgv.Rows [0].Height);
				Assert.AreEqual (2, dgv.Rows [0].MinimumHeight);
			}
		}

		[Test] // Xamarin bug #2394
		public void Bug2394_RowHeightLessThanMinHeightVirtMode ()
		{
			using (var dgv = new DataGridView ()) {
				dgv.VirtualMode = true;
				dgv.RowCount = 1;
				dgv.Rows [0].Height = 10;
				dgv.Rows [0].MinimumHeight = 5;
				dgv.RowHeightInfoNeeded += (sender, e) => {
					// Setting the height to a value less than the minimum height
					// will be silently ignored and instead set to MinimumHeight.
					e.Height = 2;
				};
				dgv.UpdateRowHeightInfo (0, false);
				Assert.AreEqual(5, dgv.Rows[0].Height);
				Assert.AreEqual(5, dgv.Rows[0].MinimumHeight);
			}
		}

		[Test] // Xamarin bug #2394
		public void Bug2394_MinHeightGreaterThanOldRowHeightVirtMode ()
		{
			using (var dgv = new DataGridView ()) {
				dgv.VirtualMode = true;
				dgv.RowCount = 1;
				dgv.Rows [0].Height = 10;
				dgv.Rows [0].MinimumHeight = 5;
				dgv.RowHeightInfoNeeded += (sender, e) => {
					e.MinimumHeight = 30;
					e.Height = 40;
				};
				dgv.UpdateRowHeightInfo (0, false);
				Assert.AreEqual (40, dgv.Rows [0].Height);
				Assert.AreEqual (30, dgv.Rows [0].MinimumHeight);
			}
		}
		#endregion

		#region Xamarin Bug #18638
		[Test]
		public void XamarinBug18638 ()
		{
			// Spanning items should not have their entire width assigned to the first column in the span.
			TableLayoutPanel tlp = new TableLayoutPanel ();
			tlp.SuspendLayout ();
			tlp.Size = new Size(291, 100);
			tlp.AutoSize = true;
			tlp.ColumnStyles.Add (new ColumnStyle (SizeType.Absolute, 60));
			tlp.ColumnStyles.Add (new ColumnStyle (SizeType.Percent, 100));
			tlp.ColumnStyles.Add (new ColumnStyle (SizeType.Absolute, 45));
			tlp.ColumnCount = 3;
			tlp.RowStyles.Add (new RowStyle (SizeType.AutoSize));
			var label1 = new Label {AutoSize = true, Text = @"This line spans all three columns in the table!"};
			tlp.Controls.Add (label1, 0, 0);
			tlp.SetColumnSpan (label1, 3);
			tlp.RowStyles.Add (new RowStyle (SizeType.AutoSize));
			tlp.RowCount = 1;
			var label2 = new Label {AutoSize = true, Text = @"This line spans columns two and three."};
			tlp.Controls.Add (label2, 1, 1);
			tlp.SetColumnSpan (label2, 2);
			tlp.RowCount = 2;
			AddTableRow (tlp, "First Row", "This is a test");
			AddTableRow (tlp, "Row 2", "This is another test");
			tlp.ResumeLayout ();

			var widths = tlp.GetColumnWidths ();
			Assert.AreEqual (4, tlp.RowCount, "X18638-1");
			Assert.AreEqual (3, tlp.ColumnCount, "X18638-2");
			Assert.AreEqual (60, widths[0], "X18638-3");
			Assert.Greater (label2.Width, widths[1], "X18638-5");
			Assert.AreEqual (45, widths[2], "X18638-4");
		}

		private void AddTableRow(TableLayoutPanel tlp, string label, string text)
		{
			tlp.SuspendLayout ();
			int row = tlp.RowCount;
			tlp.RowStyles.Add (new RowStyle (SizeType.AutoSize));
			var first = new Label {AutoSize = true, Dock = DockStyle.Fill, Text = label};
			tlp.Controls.Add (first, 0, row);
			var second = new TextBox {AutoSize = true, Text = text, Dock = DockStyle.Fill, Multiline = true};
			tlp.Controls.Add (second, 1, row);
			var third = new Button {Text = @"DEL", Dock = DockStyle.Fill};
			tlp.Controls.Add (third, 2, row);
			tlp.RowCount = row + 1;
			tlp.ResumeLayout ();
		}

		#endregion

		#region XmlInclude on abstract class tests (Bug #18558)
		[Test]
		public void Bug18558_TestSerializeIntermediateType()
		{
			string expectedXml = "<ContainerTypeForTest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><XmlIntermediateType intermediate=\"false\"/></ContainerTypeForTest>";
			var obj = new ContainerTypeForTest();
			obj.MemberToUseInclude = new IntermediateTypeForTest();
			Serialize (obj);
			Assert.AreEqual (Infoset (expectedXml), WriterText, "Serialized Output : " + WriterText);
		}

		[Test]
		public void Bug18558_TestSerializeSecondType()
		{
			string expectedXml = "<ContainerTypeForTest xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><XmlSecondType intermediate=\"false\"/></ContainerTypeForTest>";
			var obj = new ContainerTypeForTest();
			obj.MemberToUseInclude = new SecondDerivedTypeForTest();
			Serialize (obj);
			Assert.AreEqual (Infoset (expectedXml), WriterText, "Serialized Output : " + WriterText);
		}

		#region XmlInclude on abstract class test classes
		[XmlType]
		public class ContainerTypeForTest
		{
			[XmlElement ("XmlFirstType", typeof (FirstDerivedTypeForTest))]
			[XmlElement ("XmlIntermediateType", typeof (IntermediateTypeForTest))]
			[XmlElement ("XmlSecondType", typeof (SecondDerivedTypeForTest))]
			public AbstractTypeForTest MemberToUseInclude { get; set; }
		}

		[XmlInclude (typeof (FirstDerivedTypeForTest))]
		[XmlInclude (typeof (IntermediateTypeForTest))]
		[XmlInclude (typeof (SecondDerivedTypeForTest))]
		public abstract class AbstractTypeForTest
		{
		}

		public class IntermediateTypeForTest : AbstractTypeForTest
		{
			[XmlAttribute (AttributeName = "intermediate")]
			public bool IntermediateMember { get; set; }
		}

		public class FirstDerivedTypeForTest : AbstractTypeForTest
		{
			public string FirstMember { get; set; }
		}

		public class SecondDerivedTypeForTest : IntermediateTypeForTest
		{
			public string SecondMember { get; set; }
		}
		#endregion

		#region Helper methods
		StringWriter sw;
		XmlTextWriter xtw;
		XmlSerializer xs;

		private void SetUpWriter ()
		{
			sw = new StringWriter ();
			xtw = new XmlTextWriter (sw);
			xtw.QuoteChar = '\'';
			xtw.Formatting = Formatting.None;
		}

		private string WriterText
		{
			get
			{
				string val = sw.GetStringBuilder ().ToString ();
				int offset = val.IndexOf ('>') + 1;
				val = val.Substring (offset);
				return Infoset (val);
			}
		}

		private void Serialize (object o)
		{
			SetUpWriter ();
			xs = new XmlSerializer (o.GetType ());
			xs.Serialize (xtw, o);
		}

		public static string Infoset (string sx)
		{
			XmlDocument doc = new XmlDocument ();
			doc.LoadXml (sx);
			StringBuilder sb = new StringBuilder ();
			GetInfoset (doc.DocumentElement, sb);
			return sb.ToString ();
		}

		public static string Infoset (XmlNode nod)
		{
			StringBuilder sb = new StringBuilder ();
			GetInfoset (nod, sb);
			return sb.ToString ();
		}

		static void GetInfoset (XmlNode nod, StringBuilder sb)
		{
			switch (nod.NodeType) {
			case XmlNodeType.Attribute:
				if (nod.LocalName == "xmlns" && nod.NamespaceURI == "http://www.w3.org/2000/xmlns/") return;
				sb.Append (" " + nod.NamespaceURI + ":" + nod.LocalName + "='" + nod.Value + "'");
				break;

			case XmlNodeType.Element:
				XmlElement elem = (XmlElement) nod;
				sb.Append ("<" + elem.NamespaceURI + ":" + elem.LocalName);

				ArrayList ats = new ArrayList ();
				foreach (XmlAttribute at in elem.Attributes)
					ats.Add (at.LocalName + " " + at.NamespaceURI);

				ats.Sort ();

				foreach (string name in ats) {
					string[] nn = name.Split (' ');
					GetInfoset (elem.Attributes[nn[0], nn[1]], sb);
				}

				sb.Append (">");
				foreach (XmlNode cn in elem.ChildNodes)
					GetInfoset (cn, sb);
				sb.Append ("</>");
				break;

			default:
				sb.Append (nod.OuterXml);
				break;
			}
		}
		#endregion
		#endregion
	}
}

