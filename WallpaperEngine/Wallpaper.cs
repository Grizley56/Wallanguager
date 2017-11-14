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
	public class Wallpaper
	{
		public static FontInfo GeneralDefaultFont { get; set; } = new FontInfo(new FontFamily("Times New Roman"),
			150, FontStyles.Normal, FontStretches.Normal, FontWeights.Normal, new SolidColorBrush(Colors.Black));

		public static WallpaperStyle GeneralDefaultStyle { get; set; } = WallpaperStyle.Center;

		private FontInfo _font = new FontInfo(new FontFamily("Times New Roman"), 150, FontStyles.Normal,
			FontStretches.Normal, FontWeights.Normal, new SolidColorBrush(Colors.Black));

		private WallpaperStyle _style = WallpaperStyle.Center;

		private ImageSource _previousSignedImage = null;
		private Point _previousPosition;

		public bool IsFontByDefault { get; set; } = true;
		public bool IsStyleByDefault { get; set; } = true;

		public WallpaperStyle Style
		{
			get { return IsStyleByDefault ? GeneralDefaultStyle : _style; }
			set { _style = value; }
		}
		public FlowDirection FlowDirection { get; set; } = FlowDirection.LeftToRight;
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
		public bool IsFixed { get; private set; }

		public Wallpaper(Image image, BitmapImage sourceImage)
		{
			Font = GeneralDefaultFont;
			SourceImage = sourceImage;
			UIElementImage = image;
		}

		public Point FixedSignPosition { get; private set; }

		public ImageSource GetSignedImage(Point drawPosition, Signature sign)
		{
			if (sign == null)
				throw new ArgumentNullException(nameof(sign));

			//TODO: possible to select enum flags SignRelative.Left , Right ... Center ;
			
			_previousPosition = drawPosition;

			FormattedText format = new FormattedText(sign.ToString(), CultureInfo.InvariantCulture,
				FlowDirection, new Typeface(Font.Family, Font.Style, Font.Weight, Font.Stretch),
				Font.Size, Font.Color.Brush);

			Size textSize = new Size(format.Width, format.Height);

			Vector offset = new Vector(format.Width / 2, format.Height / 2);
			drawPosition.X -= offset.X;
			drawPosition.Y -= offset.Y;

			if (drawPosition.X + textSize.Width > SourceImage.Width)
				drawPosition.X = SourceImage.Width - textSize.Width;
			else if (drawPosition.X < 0)
				drawPosition.X = 0;

			if (drawPosition.Y + textSize.Height > SourceImage.Height)
				drawPosition.Y = SourceImage.Height - textSize.Height;
			else if (drawPosition.Y < 0)
				drawPosition.Y = 0;


			DrawingVisual drawingVisual = new DrawingVisual();

			DrawingContext context = drawingVisual.RenderOpen();

			context.DrawImage(SourceImage, new Rect(0, 0, SourceImage.Width, SourceImage.Height));
			context.DrawText(format, drawPosition);

			context.Close();

			ImageSource resultImage = new DrawingImage(drawingVisual.Drawing);

			_previousSignedImage = resultImage; // TODO: cahed

			return resultImage;
		}

		public ImageSource GetFixedSignedImage(Signature sign)
		{
			if (!IsFixed)
				throw new InvalidOperationException("UnFixed state");

			return GetSignedImage(FixedSignPosition, sign);
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

		public bool SaveToFile(Uri path)
		{
			return true;
		}

		public bool SaveToFile(string path) { return SaveToFile(new Uri(path)); }
	}
}
