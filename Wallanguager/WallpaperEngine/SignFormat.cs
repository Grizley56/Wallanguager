using System.Runtime.Serialization;

namespace Wallanguager.WallpaperEngine
{
	[DataContract]
	public class SignFormat
	{
		public static readonly string OriginalToken = "%w";
		public static readonly string TranslateToken = "%t";
		public static readonly SignFormat Default = new SignFormat(OriginalToken + " : " + TranslateToken);

		[DataMember]
		public string Pattern { get; private set; }

		public SignFormat(string pattern)
		{
			Pattern = pattern;
		}

		public string GetFormattedString(string original, string translated)
		{
			string formattedString;
			formattedString = Pattern.Replace(OriginalToken, original);
			formattedString = formattedString.Replace(TranslateToken, translated);
			return formattedString;
		}
	}
}