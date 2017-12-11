using System;
using System.IO;
using System.Reflection;
using System.Windows;
using GoogleTranslateFreeApi;
using Newtonsoft.Json;
using Wallanguager.Learning;

namespace Wallanguager.Windows
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary
	public partial class App : Application
	{
		private static string[] GroupThemes;
		private static Language[] Languages = GoogleTranslator.LanguagesSupported;

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			try
			{
				GroupThemes = JsonConvert
					.DeserializeObject<string[]>(GetResourceString("Wallanguager.Content.Json.GroupThemes.json"));
			}
			catch (Exception ex)
			{
				Easy.Logger.Log4NetService.Instance.GetLogger<App>().Fatal(ex);
				throw;
			}

			Application.Current.Properties.Add("GroupThemes", GroupThemes);
			Application.Current.Properties.Add("Languages", GoogleTranslator.LanguagesSupported);
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
