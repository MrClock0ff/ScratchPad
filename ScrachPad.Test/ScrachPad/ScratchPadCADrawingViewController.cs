namespace ScrachPad.Test;

public class ScratchPadCADrawingViewController : CADrawingViewController
{
	public ScratchPadCADrawingViewController()
	{
	}

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		View!.Frame = default;
		DrawingView.BackgroundColor = UIColor.White;
	}
}

