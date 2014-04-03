// Copyright (c) 2014 SIL International
// This software is licensed under the MIT License (http://opensource.org/licenses/MIT)
using NUnit.Framework;
using System;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;

namespace MonoBugFixTests
{
	[TestFixture]
	public class ApplicationSettingsBaseTests
	{
		string tempDir;

		[TestFixtureSetUp]
		public void FixtureSetup ()
		{
			// Use random temp directory to store settings files of tests.
			tempDir = Path.Combine (Path.GetTempPath (), Path.GetRandomFileName ());
			Directory.CreateDirectory (tempDir);
			var localAppData = Path.Combine (tempDir, "LocalAppData");
			Directory.CreateDirectory (localAppData);
			var appData = Path.Combine (tempDir, "AppData");
			Directory.CreateDirectory (appData);

			Environment.SetEnvironmentVariable ("XDG_DATA_HOME", localAppData);
			Environment.SetEnvironmentVariable ("XDG_CONFIG_HOME", appData);
		}

		[TestFixtureTearDown]
		public void FixtureTearDown ()
		{
			Directory.Delete (tempDir);
		}

		#region Bug #2315
		class Bug2315Settings : ApplicationSettingsBase
		{
			public Bug2315Settings () : base ("Bug2315Settings")
			{
			}

			[UserScopedSetting]
			[DefaultSettingValue ("some text")]
			public string Text {
				get { return (string)this ["Text"]; }
				set { this ["Text"] = value; }
			}
		}

		[Test]
		public void SettingSavingEventFired_Bug2315 ()
		{
			bool settingsSavingCalled = false;
			var settings = new Bug2315Settings ();
			settings.SettingsSaving += (sender, e) => {
				settingsSavingCalled = true;
			};

			settings.Text = DateTime.Now.ToString ();
			settings.Save ();

			Assert.IsTrue (settingsSavingCalled);
		}
		#endregion

		#region Bug #15818
		class Bug15818SettingsProvider: SettingsProvider, IApplicationSettingsProvider
		{
			public Bug15818SettingsProvider ()
			{
			}

			public static void ResetUpgradeCalled ()
			{
				UpgradeCalled = false;
			}

			public static bool UpgradeCalled { get; private set; }

			public override void Initialize (string name, NameValueCollection config)
			{
				if (name != null && config != null) {
					base.Initialize (name, config);
				}
			}

			public override string Name
			{
				get { return "Bug15818SettingsProvider"; }
			}

			public override string Description
			{
				get { return "Bug15818SettingsProvider"; }
			}

			public override string ApplicationName
			{
				get { return "Bug15818"; }
				set { }
			}

			public override SettingsPropertyValueCollection GetPropertyValues (SettingsContext context, SettingsPropertyCollection collection)
			{
				return null;
			}

			public override void SetPropertyValues (SettingsContext context, SettingsPropertyValueCollection collection)
			{
			}

			#region IApplicationSettingsProvider implementation

			public SettingsPropertyValue GetPreviousVersion (SettingsContext context, SettingsProperty property)
			{
				return null;
			}

			public void Reset (SettingsContext context)
			{
			}

			public void Upgrade (SettingsContext context, SettingsPropertyCollection properties)
			{
				UpgradeCalled = true;
			}

			#endregion
		}

		class Bug15818Settings : ApplicationSettingsBase
		{
			public Bug15818Settings () : base ("Bug15818Settings")
			{
			}

			[UserScopedSetting]
			[SettingsProvider (typeof (Bug15818SettingsProvider))]
			[DefaultSettingValue ("some text")]
			public string Text {
				get { return (string)this ["Text"]; }
				set { this ["Text"] = value; }
			}
		}

		public class Bug15818Class
		{
			public string Name { get; set; }
			public int Value { get; set; }
		}

		class Bug15818Settings2 : ApplicationSettingsBase
		{
			public Bug15818Settings2 () : base ("Bug15818Settings2")
			{
			}

			[UserScopedSetting]
			[DefaultSettingValue ("default text")]
			public string Text {
				get { return (string)this ["Text"]; }
				set { this ["Text"] = value; }
			}

			[UserScopedSetting]
			public Bug15818Class MyObject {
				get { return (Bug15818Class)this ["MyObject"]; }
				set { this ["MyObject"] = value; }
			}
		}

		[Test]
		public void UpgradeGetsCalled_Bug15818 ()
		{
			Bug15818SettingsProvider.ResetUpgradeCalled ();

			var settings = new Bug15818Settings ();
			settings.Upgrade ();
			Assert.IsTrue (Bug15818SettingsProvider.UpgradeCalled);
		}

		[Test]
		public void CustomClass_Roundtrip ()
		{
			var settings = new Bug15818Settings2
			{
				Text = "foo",
				MyObject = new Bug15818Class { Name = "Some Name", Value = 15818 }
			};
			settings.Save ();

			var settings2 = new Bug15818Settings2 ();
			Assert.AreEqual ("foo", settings2.Text);
			Assert.IsNotNull (settings2.MyObject);
			Assert.AreEqual ("Some Name", settings2.MyObject.Name);
			Assert.AreEqual (15818, settings2.MyObject.Value);
		}

		[Test]
		public void ModifiedObjectsAreSerialized_Bug15818 ()
		{
			var settings = new Bug15818Settings2
			{
				Text = "foo",
				MyObject = new Bug15818Class { Name = "Some Name", Value = 15818 }
			};
			settings.Save ();

			// Modify the value of the object - bug #15818
			settings.Text = "bla";
			settings.MyObject.Name = "xyz";
			settings.MyObject.Value = -1;
			settings.Save ();

			// Verify that the new values got saved
			var settings2 = new Bug15818Settings2 ();
			Assert.AreEqual ("bla", settings2.Text);
			Assert.IsNotNull (settings2.MyObject);
			Assert.AreEqual ("xyz", settings2.MyObject.Name);
			Assert.AreEqual (-1, settings2.MyObject.Value);
		}

		[Test]
		public void Reset_FiresPropChangedOnly_Bug15818 ()
		{
			bool propChangedCalled = false;
			bool settingsLoadedCalled = false;
			bool settingsSavingCalled = false;
			var settings = new Bug15818Settings2 ();
			settings.PropertyChanged += (sender, e) => { propChangedCalled = true; };
			settings.SettingsLoaded += (sender, e) => { settingsLoadedCalled = true; };
			settings.SettingsSaving += (sender, e) => { settingsSavingCalled = true; };

			settings.Reset ();

			Assert.IsTrue (propChangedCalled, "#1");
			Assert.IsFalse (settingsLoadedCalled, "#2");
			Assert.IsFalse (settingsSavingCalled, "#3");
		}
		#endregion
	}
}

