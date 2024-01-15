namespace ScrachPad.Test;

public abstract class CADrawingViewController : UIViewController
{
	private CADrawingView _drawingView;
	private bool _disposed;

	public CADrawingViewController()
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

		_drawingView = new CADrawingView()
		{
			AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
		};

		View!.AddSubview(_drawingView);
	}

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

