namespace ScrachPad.Test;

public abstract class CGDrawingViewController : UIViewController
{
	private CGDrawingView _drawingView;
	private bool _disposed;

	public CGDrawingViewController()
	{
	}

	public UIView DrawingView
	{
		get
		{
			return _drawingView;
		}
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		_drawingView = new CGDrawingView()
		{
			AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight,
			Delegate = OnDrawInRect
		};

		View!.AddSubview(_drawingView);
	}

	public UIImage GetSnapshot()
	{
		return _drawingView.GetSnapshot();
	}

	public void SetSnapshot(UIImage image, bool deferDrawing = false)
	{
		_drawingView.SetSnapshot(image, deferDrawing);
	}

	public void ForceDraw()
	{
		_drawingView.SetNeedsDisplay();
	}

	protected abstract void OnDrawInRect(CGContext context, CGRect rect);

	protected override void Dispose(bool disposing)
	{
		if (_disposed)
		{
			return;
		}

		_disposed = true;

		if (disposing)
		{
			_drawingView?.Dispose();
			_drawingView = null;
		}

		base.Dispose(disposing);
	}
}

