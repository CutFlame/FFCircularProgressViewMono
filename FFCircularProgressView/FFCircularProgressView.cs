using System;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FFCircularProgressView
{
	/// <summary>
	/// Circular progress view.
	/// Translated from: https://github.com/elbryan/FFCircularProgressView
	/// </summary>
	public class FFCircularProgressView : UIView
	{
		public static readonly UIColor IOS7Blue = UIColor.FromRGBA (0f, 122f / 255f, 1f, 1f);
		public static readonly UIColor IOS7Gray = UIColor.FromRGBA (101f/255f, 182f/255f, 183f/255f, 1f);

		const float ArrowSizeRatio = .12f;
		const float StopSizeRatio = .3f;
		const float TickWidthRatio = .3f;
		const float PI = (float)Math.PI;

		readonly CAShapeLayer _progressBackgroundLayer;
		readonly CAShapeLayer _progressLayer;
		readonly CAShapeLayer _iconLayer;

		float _progress;
		float _lineWidth;
		UIView _iconView;

		#region Properties

		/// <summary>
		/// Value of the progress to show between 0 and 1
		/// </summary>
		public float Progress
		{
			get
			{
				return _progress;
			}
			set
			{
				SetProgress (value);
			}
		}

		/// <summary>
		/// The thickness of the lines that will be drawn.
		/// Ignores values smaller than 1.
		/// </summary>
		public float LineWidth
		{
			get
			{
				return _lineWidth;
			}
			set
			{
				SetLineWidth (value);
			}
		}

		/// <summary>
		/// A View to be shown in the middle of the circle
		/// </summary>
		public UIView IconView
		{
			get
			{
				return _iconView;
			}
			set
			{
				SetIconView (value);
			}
		}

		/// <summary>
		/// Color for the circle.
		/// Default value is iOS7 Blue
		/// </summary>
		public override UIColor TintColor
		{
			get
			{
				return base.TintColor;
			}
			set
			{
				SetTintColor (value);
			}
		}

		/// <summary>
		/// Gets or sets the color of the background.
		/// </summary>
		public override UIColor BackgroundColor
		{
			get
			{
				return base.BackgroundColor;
			}
			set
			{
				SetBackgroundColor (value);
			}
		}

		/// <summary>
		/// A BezierPath to be shown in the middle of the circle
		/// </summary>
		public UIBezierPath IconPath { get; set; }

		/// <summary>
		/// Color for the checkmark when the progress is done.
		/// Default value is White
		/// </summary>
		public UIColor TickColor { get; set; }

		/// <summary>
		/// Whether the circle is spinning or not
		/// </summary>
		public bool IsSpinning { get; protected set; }

		#endregion

		public FFCircularProgressView (RectangleF frame) : base (frame)
		{
			_progressBackgroundLayer = new CAShapeLayer ();
			_progressBackgroundLayer.LineCap = CAShapeLayer.CapRound;
			Layer.AddSublayer (_progressBackgroundLayer);

			_progressLayer = new CAShapeLayer ();
			_progressLayer.FillColor = null;
			_progressLayer.LineCap = CAShapeLayer.CapSquare;
			Layer.AddSublayer (_progressLayer);

			_iconLayer = new CAShapeLayer ();
			_iconLayer.FillColor = null;
			_iconLayer.FillRule = CAShapeLayer.FillRuleNonZero;
			_iconLayer.LineCap = CAShapeLayer.CapButt;
			Layer.AddSublayer (_iconLayer);

			LineWidth = Frame.Width * .025f;
			TintColor = IOS7Blue;
			TickColor = UIColor.White;
			BackgroundColor = UIColor.Clear;
		}

		/// <summary>
		/// Starts the infinite spin.
		/// </summary>
		public void StartInfiniteSpin ()
		{
			Progress = 0f;
			IsSpinning = true;
			DrawBackgroundCircle (true);

			using (var rotationAnimation = CABasicAnimation.FromKeyPath (@"transform.rotation.z"))
			{
				rotationAnimation.Duration = 1.0;
				rotationAnimation.Cumulative = true;
				rotationAnimation.RepeatCount = float.PositiveInfinity;

				rotationAnimation.To = NSNumber.FromFloat (PI * 2f);

				_progressBackgroundLayer.AddAnimation (rotationAnimation, @"rotationAnimation");
			}
		}

		/// <summary>
		/// Stops the infinite spin.
		/// </summary>
		public void StopInfiniteSpin ()
		{
			DrawBackgroundCircle (false);

			_progressBackgroundLayer.RemoveAllAnimations ();
			IsSpinning = false;
		}

		void SetIconView (UIView iconView)
		{
			if (_iconView != null)
			{
				_iconView.RemoveFromSuperview ();
				Dispose (_iconView);
			}
			_iconView = iconView;
			AddSubview (_iconView);
		}

		void SetBackgroundColor (UIColor value)
		{
			base.BackgroundColor = value;
			_progressBackgroundLayer.FillColor = BackgroundColor.CGColor;
		}

		void SetTintColor (UIColor value)
		{
			base.TintColor = value;
			_progressBackgroundLayer.StrokeColor = TintColor.CGColor;
			_progressLayer.StrokeColor = TintColor.CGColor;
			_iconLayer.StrokeColor = TintColor.CGColor;
		}

		void SetLineWidth (float lineWidth)
		{
			_lineWidth = Math.Max (lineWidth, 1f);

			_progressBackgroundLayer.LineWidth = _lineWidth;
			_progressLayer.LineWidth = _lineWidth * 2f;
			_iconLayer.LineWidth = _lineWidth;
		}

		void SetProgress (float progress)
		{
			if (progress > 1f)
			{
				progress = 1f;
			}
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (_progress != progress)
			{
				_progress = progress;

				// Analysis disable once CompareOfFloatsByEqualityOperator
				if (_progress == 1f)
				{
					AnimateProgressBackgroundLayerFillColor ();
				}

				// Analysis disable once CompareOfFloatsByEqualityOperator
				if (_progress == 0f)
				{
					_progressBackgroundLayer.FillColor = BackgroundColor.CGColor;
				}

				SetNeedsDisplay ();
			}
		}

		#region Draw and Animate methods

		public override void Draw (RectangleF rect)
		{
			_progressBackgroundLayer.Frame = Bounds;
			_progressLayer.Frame = Bounds;
			_iconLayer.Frame = Bounds;

			DrawBackgroundCircle (IsSpinning);

			const float startAngle = -(PI / 2f); //90 degrees
			float endAngle = ((_progress * 2f * PI) + startAngle);
			var processPath = new UIBezierPath ();
			processPath.LineCapStyle = CGLineCap.Butt;
			processPath.LineWidth = _lineWidth;

			var center = new PointF (Bounds.Width / 2f, Bounds.Height / 2f);
			float radius = (Bounds.Width - _lineWidth * 3f) / 2f;
			processPath.AddArc (center, radius, startAngle, endAngle, true);
			_progressLayer.Path = processPath.CGPath;

			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (_progress == 1f)
			{
				DrawTick ();
			}
			else if (_progress > 0 && _progress < 1.0)
			{
				DrawStop ();
			}
			else
			{
				if (_iconView == null && IconPath == null)
				{
					DrawArrow ();
				}
				else if (IconPath != null)
				{
					_iconLayer.Path = IconPath.CGPath;
					_iconLayer.FillColor = null;
				}
			}
		}

		void DrawBackgroundCircle (bool partial)
		{
			const float startAngle = -(PI / 2f); //90 degrees
			float endAngle = (2f * PI) + startAngle;
			var center = new PointF (Bounds.Width / 2f, Bounds.Height / 2f);
			float radius = (Bounds.Width - _lineWidth) / 2f;

			//Draw background//
			using (var processBackgroundPath = new UIBezierPath ())
			{
				processBackgroundPath.LineWidth = _lineWidth;
				processBackgroundPath.LineCapStyle = CGLineCap.Round;

				// Recompute the end angle to make it at 90% of the progress
				if (partial)
				{
					endAngle = (1.8F * PI) + startAngle;
				}

				processBackgroundPath.AddArc (center, radius, startAngle, endAngle, true);
				_progressBackgroundLayer.Path = processBackgroundPath.CGPath;
			}
		}

		void DrawTick ()
		{
			float radius = Math.Min (Frame.Width, Frame.Height) / 2f;

			/*
		     First draw a tick that looks like this:
		     
		     A---F
		     |   |
		     |   E-------D
		     |           |
		     B-----------C
		     
		     (Remember: (0,0) is top left)
		     */

			using (var tickPath = new UIBezierPath ())
			{
				float tickWidth = radius * TickWidthRatio;
				tickPath.MoveTo (PointF.Empty);
				tickPath.AddLineTo (new PointF (0, tickWidth * 2));
				tickPath.AddLineTo (new PointF (tickWidth * 3, tickWidth * 2));
				tickPath.AddLineTo (new PointF (tickWidth * 3, tickWidth));
				tickPath.AddLineTo (new PointF (tickWidth, tickWidth));
				tickPath.AddLineTo (new PointF (tickWidth, 0));
				tickPath.ClosePath ();

				// Now rotate it through -45 degrees...
				tickPath.ApplyTransform (CGAffineTransform.MakeRotation ((-PI / 4f)));

				// ...and move it into the right place.
				tickPath.ApplyTransform (CGAffineTransform.MakeTranslation (radius * .46f, 1.02f * radius));

				_iconLayer.Path = tickPath.CGPath;
				_iconLayer.FillColor = TickColor.CGColor;
				_progressBackgroundLayer.FillColor = _progressLayer.StrokeColor;
			}
		}

		void DrawStop ()
		{
			float radius = Bounds.Width / 2;
			float ratio = StopSizeRatio;
			float sideSize = Bounds.Width * ratio;

			using (var stopPath = new UIBezierPath ())
			{
				stopPath.MoveTo (PointF.Empty);
				stopPath.AddLineTo (new PointF (sideSize, 0f));
				stopPath.AddLineTo (new PointF (sideSize, sideSize));
				stopPath.AddLineTo (new PointF (0f, sideSize));

				// ...and move it into the right place.
				stopPath.ApplyTransform (CGAffineTransform.MakeTranslation (radius * (1 - ratio), radius * (1 - ratio)));

				_iconLayer.Path = stopPath.CGPath;
				_iconLayer.StrokeColor = _progressLayer.StrokeColor;
				_iconLayer.FillColor = TintColor.CGColor;
			}
		}

		void DrawArrow ()
		{
			float radius = Bounds.Width / 2;
			float ratio = ArrowSizeRatio;
			float segmentSize = Bounds.Width * ratio;

			// Draw icon

			using (var path = new UIBezierPath ())
			{
				path.MoveTo (PointF.Empty);
				path.AddLineTo (new PointF (segmentSize * 2f, 0f));
				path.AddLineTo (new PointF (segmentSize * 2f, segmentSize));
				path.AddLineTo (new PointF (segmentSize * 3f, segmentSize));
				path.AddLineTo (new PointF (segmentSize, segmentSize * 3.3f));
				path.AddLineTo (new PointF (-segmentSize, segmentSize));
				path.AddLineTo (new PointF (0f, segmentSize));
				path.AddLineTo (PointF.Empty);
				path.ClosePath ();

				path.ApplyTransform (CGAffineTransform.MakeTranslation (-segmentSize / 2f, -segmentSize / 1.2f));
				path.ApplyTransform (CGAffineTransform.MakeTranslation (radius * (1 - ratio), radius * (1 - ratio)));
				_iconLayer.Path = path.CGPath;
				_iconLayer.FillColor = null;
			}
		}

		void AnimateProgressBackgroundLayerFillColor ()
		{
			using (var colorAnimation = CABasicAnimation.FromKeyPath (@"fillColor"))
			{
				colorAnimation.Duration = .5;
				colorAnimation.RepeatCount = 1f;
				colorAnimation.RemovedOnCompletion = false;

				colorAnimation.From = new NSObject (_progressBackgroundLayer.BackgroundColor.Handle);
				colorAnimation.To = new NSObject (_progressLayer.StrokeColor.Handle);

				colorAnimation.TimingFunction = CAMediaTimingFunction.FromName (CAMediaTimingFunction.EaseIn);

				_progressBackgroundLayer.AddAnimation (colorAnimation, @"colorAnimation");
			}
		}

		#endregion

		static void Dispose (IDisposable disposable)
		{
			if (disposable != null)
			{
				disposable.Dispose ();
			}
		}

		protected override void Dispose (bool disposing)
		{
			Dispose (_progressBackgroundLayer);
			Dispose (_progressLayer);
			Dispose (_iconLayer);
			Dispose (_iconView);
			Dispose (IconPath);
			base.Dispose (disposing);
		}

	}
}

