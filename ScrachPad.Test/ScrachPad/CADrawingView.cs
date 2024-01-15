using CoreAnimation;

namespace ScrachPad.Test
{
	/// <summary>
	/// View based drawing canvas which is using GPU resources for drawing (CoreAnimation).
	///
	/// Based on https://medium.com/@almalehdev/high-performance-drawing-on-ios-part-2-2cb2bc957f6
	/// </summary>
	public class CADrawingView : UIImageView
	{
		private CALayer? _drawingLayer;
		private CGPoint? _currentTouchPoint;

		/// <summary>
		/// Create new view instance.
		/// </summary>
		public CADrawingView() : this(default)
		{
		}

		/// <summary>
		/// Create new view instance with specifc frame.
		/// </summary>
		/// <param name="frame"></param>
		public CADrawingView(CGRect frame) : base(frame)
		{
			DrawingLineWidth = 6.0f;
			DrawingColor = UIColor.Black;
			MaxDrawingBufferSize = 400;
			ContentMode = UIViewContentMode.TopLeft;
		}

		/// <summary>
		/// Canvas image
		/// </summary>
		public override UIImage? Image
		{
			get
			{
				return base.Image;
			}

			set
			{
				base.Image = value;

				if (value == null)
				{
					DisposeDrawingLayer();
				}
			}
		}

		/// <summary>
		/// Drawing line color.
		/// </summary>
		public UIColor DrawingColor { get; set; }

		/// <summary>
		/// Drawing line width.
		/// </summary>
		public float DrawingLineWidth { get; set; }

		/// <summary>
		/// Max drawing buffer size before drawing layer compressed into single image.
		///
		/// The greater number the more memory is used to store layers. Lower
		/// number will cause more often compression to free up memory but that
		/// will use more CPU.
		/// </summary>
		public uint MaxDrawingBufferSize { get; set; }

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();
			UserInteractionEnabled = true;
		}

		/// <summary>
		/// Register first point where touch began.
		/// </summary>
		/// <param name="touches"></param>
		/// <param name="evt"></param>
		public override void TouchesBegan(NSSet touches, UIEvent? evt)
		{
			if (touches.FirstOrDefault() is UITouch touch
				&& touch.LocationInView(this) is CGPoint newTouchPoint)
			{
				_currentTouchPoint = newTouchPoint;
			}
		}

		/// <summary>
		/// Initialise drawing between two touch points once touch moved.
		/// </summary>
		/// <param name="touches"></param>
		/// <param name="evt"></param>
		public override void TouchesMoved(NSSet touches, UIEvent? evt)
		{
			if (touches.FirstOrDefault() is UITouch touch
				&& touch.LocationInView(this) is CGPoint newTouchPoint
				&& _currentTouchPoint is CGPoint previousTouchPoint)
			{
				DrawTouches(previousTouchPoint, newTouchPoint);
				_currentTouchPoint = newTouchPoint;
			}
		}

		/// <summary>
		/// Merge drawing layer into image once touch ended.
		/// </summary>
		/// <param name="touches"></param>
		/// <param name="evt"></param>
		public override void TouchesEnded(NSSet touches, UIEvent? evt)
		{
			FlattenToImage();
			_currentTouchPoint = null;
		}

		protected virtual CALayer CreateTouchSublayer(CGPoint start, CGPoint end)
		{
			UIBezierPath linePath = new UIBezierPath();
			linePath.MoveTo(start);
			linePath.AddLineTo(end);

			CAShapeLayer lineLayer = new CAShapeLayer
			{
				ContentsScale = UIScreen.MainScreen.Scale,
				Path = linePath.CGPath,
				FillColor = DrawingColor.CGColor,
				Opacity = 1.0f,
				LineWidth = DrawingLineWidth,
				LineCap = CAShapeLayer.CapRound,
				StrokeColor = DrawingColor.CGColor
			};

			return lineLayer;
		}

		/// <summary>
		/// Create new sublayer for provided touch points.
		/// </summary>
		/// <param name="start">Start point where touch began.</param>
		/// <param name="end">End point where touch ended.</param>
		private void DrawTouches(CGPoint start, CGPoint end)
		{
			SetupDrawingLayerIfNeeded();
			
			CALayer sublayer = CreateTouchSublayer(start, end);

			if (sublayer == null)
			{
				return;
			}

			_drawingLayer?.AddSublayer(sublayer);

			if (_drawingLayer?.Sublayers is CALayer[] sublayers
				&& sublayers.Length > MaxDrawingBufferSize)
			{
				FlattenToImage();
			}
		}

		/// <summary>
		/// Setup drawing layer if not already configured.
		/// </summary>
		private void SetupDrawingLayerIfNeeded()
		{
			if (_drawingLayer != null)
			{
				return;
			}

			CALayer drawingLayer = new CALayer
			{
				ContentsScale = UIScreen.MainScreen.Scale
			};

			Layer.AddSublayer(drawingLayer);
			_drawingLayer = drawingLayer;
		}

		/// <summary>
		/// Merge drawing layer into image.
		/// </summary>
		private void FlattenToImage()
		{
			UIImage? currentImage = Image;
			UIGraphicsImageRendererFormat rendererFromat = UIGraphicsImageRendererFormat.DefaultFormat;
			rendererFromat.Opaque = false;
			float maxSize = Math.Max((float)Bounds.Size.Width, (float)Bounds.Size.Height);
			UIGraphicsImageRenderer renderer = new UIGraphicsImageRenderer(new CGSize(maxSize, maxSize), rendererFromat);
			UIImage newImage = renderer.CreateImage(context => {
				currentImage?.Draw(new CGRect(CGPoint.Empty, currentImage.Size));
				_drawingLayer?.RenderInContext(context.CGContext);
			});
			Image = newImage;
			DisposeDrawingLayer();
		}

		/// <summary>
		/// Dispose drawing layer to clean up drawing canvas.
		/// </summary>
		private void DisposeDrawingLayer()
		{
			_drawingLayer?.RemoveFromSuperLayer();
			_drawingLayer = null;
		}
	}
}
