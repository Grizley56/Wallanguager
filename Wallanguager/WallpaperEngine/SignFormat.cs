namespace Wallanguager.WallpaperEngine
{
	public class SignFormat
	{
		public static readonly string OriginalToken = "%w";
		public static readonly string TranslateToken = "%t";
		public static readonly SignFormat Default = new SignFormat(OriginalToken + " : " + TranslateToken); //TODO: del

		public string Pattern { get; }

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