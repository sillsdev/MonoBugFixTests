// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using NUnit.Framework;
using System;
using System.Windows.Forms;
using System.Drawing;

namespace MonoBugFixTests
{
	[TestFixture]
	public class FlowPanelTests_AutoSize
	{
		private Form f;

		[SetUp]
		protected void SetUp ()
		{
			f = new Form ();
			f.AutoSize = false;
			f.ClientSize = new Size (100, 300);
			f.ShowInTaskbar = false;
			f.Show ();
		}

		[TearDown]
		protected void TearDown ()
		{
			f.Dispose ();
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfLarger ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 10, 10);
			panel.Dock = DockStyle.None;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (192, panel.Width, "1"); // 2 * 90 + 4 * 3 margin
			Assert.AreEqual (25, panel.Height, "2");
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfLarger_DockBottom ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 10, 10);
			panel.Dock = DockStyle.Bottom;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (250, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (50, panel.Height, "3");
		}

		[Test]
		public void AutoSizeGrowOnly_DontResizeIfSmaller ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.None;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (100, panel.Width, "1");
			Assert.AreEqual (100, panel.Height, "2");
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfSmaller_DockTop ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.Top;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (0, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (25, panel.Height, "3");
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfSmaller_DockBottom ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.Bottom;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (275, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (25, panel.Height, "3");
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfSmaller_DockLeft ()
		{
			f.ClientSize = new Size (300, 100);

			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.Left;

			var c = new Label ();
			c.Size = new Size (25, 90);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (0, panel.Left, "1");
			Assert.AreEqual (f.ClientRectangle.Height, panel.Height, "2");
			Assert.AreEqual (31, panel.Width, "3"); // 25 + 2*3 margin
		}

		[Test]
		public void AutoSizeGrowOnly_ResizeIfSmaller_DockRight ()
		{
			f.ClientSize = new Size (300, 100);

			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowOnly;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.Right;

			var c = new Label ();
			c.Size = new Size (25, 90);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (269, panel.Left, "1");
			Assert.AreEqual (f.ClientRectangle.Height, panel.Height, "2");
			Assert.AreEqual (31, panel.Width, "3"); // 25 + 2*3 margin
		}

		[Test]
		public void AutoSizeGrowAndShrink_ResizeIfSmaller ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 100, 100);
			panel.Dock = DockStyle.None;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (96, panel.Width, "1"); // 90 + 2*3 margin
			Assert.AreEqual (25, panel.Height, "2");
		}

		[Test]
		public void AutoSizeGrowAndShrink_ResizeIfSmaller_DockBottom ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = true;
			panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 100, 100);
			panel.Dock = DockStyle.Bottom;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (275, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (25, panel.Height, "3");
		}

		[Test]
		public void NoAutoSize_DontResizeIfLarger ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = false;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 10, 10);
			panel.Dock = DockStyle.None;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (10, panel.Width, "1");
			Assert.AreEqual (10, panel.Height, "2");
		}

		[Test]
		public void NoAutoSize_DontResizeIfLarger_DockBottom ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = false;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle (5, 5, 10, 10);
			panel.Dock = DockStyle.Bottom;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (290, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (10, panel.Height, "3");
		}

		[Test]
		public void NoAutoSize_DontResizeIfSmaller ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = false;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.None;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add (panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (100, panel.Width, "1");
			Assert.AreEqual (100, panel.Height, "2");
		}

		[Test]
		public void NoAutoSize_DontResizeIfSmaller_DockBottom ()
		{
			var panel = new FlowLayoutPanel ();
			panel.SuspendLayout ();
			panel.AutoSize = false;
			panel.WrapContents = true;
			panel.Bounds = new Rectangle(5, 5, 100, 100);
			panel.Dock = DockStyle.Bottom;

			var c = new Label ();
			c.Size = new Size (90, 25);
			panel.Controls.Add (c);
			f.Controls.Add(panel);
			panel.ResumeLayout (true);

			Assert.AreEqual (200, panel.Top, "1");
			Assert.AreEqual (f.ClientRectangle.Width, panel.Width, "2");
			Assert.AreEqual (100, panel.Height, "3");
		}
	}
}

