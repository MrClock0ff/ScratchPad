namespace ScrachPad.Test;

public class CGDrawingView : UIView
{
	private UIImage? _snapshot;
	private bool _disposed;

	public CGDrawingView()
	{
	}

	public CGDrawingView(CGRect frame) : base(frame)
	{
	}

	public delegate void DrawInRect(CGContext context, CGRect rect);

	public DrawInRect Delegate { get; set; }

	public override void Draw(CGRect rect)
	{
		base.Draw(rect);

		CGContext context = UIGraphics.GetCurrentContext();
		DrawSnapshot(context, rect);
		Delegate?.Invoke(context, rect);
	}

	public UIImage GetSnapshot()
	{
		UIGraphicsImageRenderer renderer = new UIGraphicsImageRenderer(Bounds.Size);
		UIImage image = renderer.CreateImage(context => DrawViewHierarchy(Bounds, true));
		return image;
	}

	public void SetSnapshot(UIImage? image, bool deferDrawing)
	{
		if (image != null)
		{
			UIImage imageCopy = UIImage.FromImage(image.CGImage);
			_snapshot = imageCopy;
		}
		else
		{
			_snapshot = null;
		}

		if (!deferDrawing)
		{
			SetNeedsDisplay();
		}
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
			_snapshot?.Dispose();
			_snapshot = null;
		}

		base.Dispose(disposing);
	}

	private void DrawSnapshot(CGContext context, CGRect rect)
	{
		if (_snapshot != null)
		{
			using (UIImage image = UIImage.FromImage(_snapshot.CGImage))
			{
				// Store context state so it can be restored once drawing is done
				context.SaveState();
				// Modify coordinate system to prevent flipped image
				context.TranslateCTM(0.0f, rect.Location.Y + rect.Size.Height);
				context.ScaleCTM(1.0f, -1.0f);
				CGRect drawRect = new CGRect(new CGPoint(rect.Location.X, 0), rect.Size);
				context.DrawImage(drawRect, image.CGImage);
				// Restore context to it's original state
				context.RestoreState();
			}

		}
	}
}

