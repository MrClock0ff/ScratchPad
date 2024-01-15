using System.Drawing;

namespace ScrachPad.Test;

public class ScratchPadCGDrawingViewController : CGDrawingViewController
{
	private readonly List<DrawingPointGroup> _pointsToDraw;
	private readonly float _screenScale;
	private DrawingPointGroup _currentPoints;
	private UIColor _color;
	private UIImage _snapshot;

	public ScratchPadCGDrawingViewController()
	{
		_pointsToDraw = new List<DrawingPointGroup>(128);
		_screenScale = (float)UIScreen.MainScreen.Scale;
		_color = UIColor.Black;
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		View!.Frame = default;
		DrawingView.BackgroundColor = UIColor.White;
		SetImage(_snapshot);
	}

	public override void ViewWillTransitionToSize(CGSize toSize, IUIViewControllerTransitionCoordinator coordinator)
	{
		// In order to get this working parent view controller has to be set
		coordinator.AnimateAlongsideTransitionInView(View!, context => { _snapshot = GetImage(); }, context => { SetImage(_snapshot); } );
		base.ViewWillTransitionToSize(toSize, coordinator);
	}

	public override void TouchesBegan(NSSet touches, UIEvent? evt)
	{
		base.TouchesBegan(touches, evt);

		DrawingPointGroup points = new DrawingPointGroup(_color);
		_pointsToDraw.Add(points);
		_currentPoints = points;

		if (evt?.AllTouches.AnyObject is UITouch touch)
		{
			CGPoint point = touch.LocationInView(DrawingView);
			_currentPoints.Add(ToPointF(point));
		}
	}

	public override void TouchesMoved(NSSet touches, UIEvent? evt)
	{
		base.TouchesMoved(touches, evt);

		if (evt?.AllTouches.AnyObject is UITouch touch)
		{
			CGPoint point = touch.LocationInView(DrawingView);
			_currentPoints?.Add(ToPointF(point));
			ForceDraw();
		}
	}

	public override void TouchesEnded(NSSet touches, UIEvent? evt)
	{
		base.TouchesEnded(touches, evt);

		if (evt?.AllTouches.AnyObject is UITouch touch)
		{
			CGPoint point = touch.LocationInView(DrawingView);
			_currentPoints?.Add(ToPointF(point));
			ForceDraw();
		}
	}

	#region CGDrawingViewController abstract class implementation
	protected override void OnDrawInRect(CGContext context, CGRect rect)
	{
		foreach (DrawingPointGroup points in _pointsToDraw.ToList())
		{
			if (points.Count > 1)
			{
				DrawPoints(context, points);
			}
		}
	}
	#endregion

	private void DrawPoints(CGContext context, DrawingPointGroup points)
	{
		context.SetLineWidth(2f * _screenScale);
		context.SetLineCap(CGLineCap.Round);
		context.SetStrokeColor(points.Color.CGColor);
		List<PointF> pointsSnapshot = points.ToList();

		for (int i = 1; i < pointsSnapshot.Count; i++)
		{
			PointF startPoint = pointsSnapshot[i - 1];
			PointF endPoint = pointsSnapshot[i];

			context.MoveTo(startPoint.X, startPoint.Y);
			context.AddLineToPoint(endPoint.X, endPoint.Y);
		}

		context.StrokePath();
	}

	private UIImage GetImage()
	{
		return GetSnapshot();
	}

	private void SetImage(UIImage? image)
	{
		SetSnapshot(image, true);
		image?.Dispose();
		_pointsToDraw.Clear();
		ForceDraw();
	}

	private static PointF ToPointF(CGPoint point)
	{
		return new PointF((float)point.X, (float)point.Y);
	}

	private class DrawingPointGroup : List<PointF>
	{
		public DrawingPointGroup(UIColor color)
		{
			Color = color ?? UIColor.Black;
		}

		public UIColor Color { get; }
	}
}

