using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Wallanguager.WallpaperEngine;
using WpfColorFontDialog;

namespace Wallanguager.WallpaperEngine
{
	[DataContract]
	public class Wallpaper
	{
		public static FontInfo GeneralDefaultFont { get; set; } = new FontInfo(new FontFamily("Times New Roman"),
			70, FontStyles.Normal, FontStretches.Normal, FontWeights.Normal, new SolidColorBrush(Colors.Black));

		public static WallpaperStyle GeneralDefaultStyle { get; set; } = WallpaperStyle.Center;

		[DataMember] private FontInfo _font = new FontInfo(new FontFamily("Times New Roman"), 70, FontStyles.Normal,
			FontStretches.Normal, FontWeights.Normal, new SolidColorBrush(Colors.Black));

		[DataMember] private WallpaperStyle _style = GeneralDefaultStyle;

		[DataMember] private readonly Uri _soruceImageUri;

		[DataMember] public FlowDirection FlowDirection { get; set; } = FlowDirection.LeftToRight;
	
		[DataMember] public Point FixedSignPosition { get; private set; }

		[DataMember] public bool IsFixed { get; private set; }

		[DataMember] public bool IsFontByDefault { get; set; } = true;

		[DataMember] public bool IsStyleByDefault { get; set; } = true;

		private DrawingImage _previousSignedImage;

		private Point _previousPosition;

		public WallpaperStyle Style
		{
			get { return IsStyleByDefault ? GeneralDefaultStyle : _style; }
			set { _style = value; }
		}
		public BitmapImage SourceImage { get; private set; }
		public Image UIElementImage { get; private set; }
		public FontInfo Font
		{
			get { return IsFontByDefault ? GeneralDefaultFont : _font; }
			set
			{
				if (value == GeneralDefaultFont)
				{
					IsFontByDefault = true;
					return;
				}

				if (_font == value)
					return;

				_font = value;
				IsFontByDefault = (value == GeneralDefaultFont);
			}
		}

		public Wallpaper(Image image, BitmapImage sourceImage)
		{
			Font = GeneralDefaultFont;
			SourceImage = sourceImage;
			UIElementImage = image;

			_soruceImageUri = sourceImage.UriSource;
		}

		public async Task<DrawingImage> GetSignedImage(Point drawPosition, Signature sign)
		{
			if (sign == null)
				throw new ArgumentNullException(nameof(sign));

			//TODO: possible to select enum flags SignRelative.Left , Right ... Center ;
			
			_previousPosition = drawPosition;

			FormattedText format = new FormattedText(sign.ToString(), CultureInfo.InvariantCulture,
				FlowDirection, new Typeface(Font.Family, Font.Style, Font.Weight, Font.Stretch),
				Font.Size, Font.BrushColor);

			Size textSize = new Size(format.Width, format.Height);

			Vector offset = new Vector(format.Width / 2, format.Height / 2);
			drawPosition.X -= offset.X;
			drawPosition.Y -= offset.Y;

			double sourceImageWidth = 0, sourceImageHeight = 0;

			SourceImage.Dispatcher.Invoke(() => {
				sourceImageWidth = SourceImage.Width;
				sourceImageHeight = SourceImage.Height;
			});

			if (drawPosition.X + textSize.Width > sourceImageWidth)
				drawPosition.X = sourceImageWidth - textSize.Width;
			else if (drawPosition.X < 0)
				drawPosition.X = 0;

			if (drawPosition.Y + textSize.Height > sourceImageHeight)
				drawPosition.Y = sourceImageHeight - textSize.Height;
			else if (drawPosition.Y < 0)
				drawPosition.Y = 0;


			_previousSignedImage = await SourceImage.Dispatcher.InvokeAsync(() =>
			{
				DrawingVisual drawingVisual = new DrawingVisual();
				DrawingContext dc = drawingVisual.RenderOpen();
				dc.DrawImage(SourceImage, new Rect(0, 0, SourceImage.Width, SourceImage.Height));
				dc.DrawText(format, drawPosition);
				dc.Close();

				return new DrawingImage(drawingVisual.Drawing);
			});

			return _previousSignedImage;
		}

		public async Task<DrawingImage> GetFixedSignedImage(Signature sign)
		{
			if (!IsFixed)
				throw new InvalidOperationException("UnFixed state");

			return await GetSignedImage(FixedSignPosition, sign);
		}

		public void Fix(Point position)
		{
			IsFixed = true;
			FixedSignPosition = position;
		}

		public void Fix() { Fix(_previousPosition); }

		public void UnFix()
		{
			IsFixed = false;
		}

		public void SaveToFile(ImageSource image, string path)
		{
			var drawingVisual = new DrawingVisual();
			var drawingContext = drawingVisual.RenderOpen();

			drawingContext.DrawImage(image, new Rect(0, 0, image.Width, image.Height));
			drawingContext.Close();

			RenderTargetBitmap rtb = new RenderTargetBitmap(SourceImage.PixelWidth, SourceImage.PixelHeight, 
				SourceImage.DpiX, SourceImage.DpiY, PixelFormats.Default);

			rtb.Render(drawingVisual);

			JpegBitmapEncoder encoder = new JpegBitmapEncoder();
			encoder.Frames.Add(BitmapFrame.Create(rtb));

			using (FileStream stream = new FileStream(path, FileMode.Create))
				encoder.Save(stream);
		}

		[OnDeserialized]
		private void OnDeserialized(StreamingContext context)
		{
			Image img = new Image { Source = new BitmapImage(_soruceImageUri) };
			SourceImage = img.Source as BitmapImage;
			UIElementImage = img;
		}
	}
}
