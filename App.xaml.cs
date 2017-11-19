using System;
using System.Reflection;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

using Newtonsoft.Json;
using Wallanguager.Learning;

namespace Wallanguager
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary
	public partial class App : Application
	{
		private static string[] GroupThemes;
		private static Language[] Languages;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			try
			{
				GroupThemes = JsonConvert
					.DeserializeObject<string[]>(GetResourceString("Wallanguager.GroupThemes.json"));

				Languages = JsonConvert
					.DeserializeObject<Language[]>(GetResourceString("Wallanguager.Languages.json"));
			}
			catch (Exception ex)
			{
				Easy.Logger.Log4NetService.Instance.GetLogger<App>().Fatal(ex);
				throw;
			}

			Application.Current.Properties.Add("GroupThemes", GroupThemes);
			Application.Current.Properties.Add("Languages", Languages);
		}

		private string GetResourceString(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();

			Stream stream = assembly.GetManifestResourceStream(resourceName);

			using (StreamReader reader = new StreamReader(stream))
			{
				return reader.ReadToEnd();
			}
		}

	}
}
