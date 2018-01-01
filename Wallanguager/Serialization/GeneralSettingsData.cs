using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Wallanguager.WallpaperEngine;
using WpfColorFontDialog;

namespace Wallanguager.Serialization
{
	[DataContract]
	class GeneralSettingsData
	{
		[DataMember]
		public FileInfo SelectedSound { get; private set; }
		[DataMember]
		public FontInfo GeneralFontSettings { get; private set; }
		[DataMember]
		public WallpaperStyle GeneralWallpaperStyle { get; private set; }
		[DataMember]
		public Signature DefaultSignature { get; private set; }
		[DataMember]
		public TimeSpan UpdateFrequency { get; private set; }
		[DataMember]
		public UpdateOrder WallpaperUpdateOrder { get; private set; }
		[DataMember]
		public UpdateOrder PhraseUpdateOrder { get; private set; }

		public GeneralSettingsData(FileInfo selectedSound, FontInfo generalFontSettings, WallpaperStyle generalWallpaperStyle, 
			Signature defaultSignature, TimeSpan updateFrequency, UpdateOrder wallpaperUpdateOrder, UpdateOrder phraseUpdateOrder)
		{
			SelectedSound = selectedSound;
			GeneralFontSettings = generalFontSettings;
			GeneralWallpaperStyle = generalWallpaperStyle;
			DefaultSignature = defaultSignature;
			UpdateFrequency = updateFrequency;
			WallpaperUpdateOrder = wallpaperUpdateOrder;
			PhraseUpdateOrder = phraseUpdateOrder;
		}
	}
}
