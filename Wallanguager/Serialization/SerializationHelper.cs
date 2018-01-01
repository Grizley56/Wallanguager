using System;
using System.IO;
using System.Runtime.Serialization;
using System.Windows;
using System.Xml;
using Easy.Logger;
using Wallanguager.Windows;

namespace Wallanguager.Serialization
{
	public static class SerializationHelper
	{
		public static XmlWriterSettings WriteSettings { get; set; } = new XmlWriterSettings()
		{
			CloseOutput = true,
			Indent = true,
			IndentChars = "\t"
		};

		public static XmlReaderSettings ReaderSettings { get; set; } = new XmlReaderSettings()
		{
			CloseInput = true,
			IgnoreComments = true
		};

		public static void Serialize<T>(string filePath, T obj, params Type[] additionalTypes)
		{
			XmlWriter writer = XmlWriter.Create(new FileStream(filePath, FileMode.Create, FileAccess.Write), WriteSettings);

			try
			{
				DataContractSerializer serializer =
					new DataContractSerializer(typeof(T), additionalTypes);
				serializer.WriteObject(writer, obj);
			}
			catch (Exception e)
			{
				Log4NetService.Instance.GetLogger<MainWindow>().Error("An error occurred while saving the " +
					                                                    $"{typeof(T).FullName}", e);
				MessageBox.Show("Saving error", "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				writer.Dispose();
			}
		}


		public static T DeSerialize<T>(string filePath, params Type[] additionalTypes)
		{
			if (!File.Exists(filePath))
				return default(T);

			T result;

			XmlReader reader = XmlReader.Create(new FileStream(filePath, FileMode.Open), ReaderSettings);
			try
			{
				DataContractSerializer serializer = new DataContractSerializer(typeof(T), additionalTypes);
				result = (T) serializer.ReadObject(reader);
			}
			catch (Exception e)
			{
				Log4NetService.Instance.GetLogger<MainWindow>().Error("An error occurred while loading the " +
				                                                      $"{typeof(T).FullName}", e);
				MessageBox.Show("Loading error", "Something went wrong", MessageBoxButton.OK, MessageBoxImage.Error);
				return default(T);
			}
			finally
			{
				reader.Dispose();
			}

			return result;
		}

	}
}
