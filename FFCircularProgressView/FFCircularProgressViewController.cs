using System;
using System.Drawing;
using System.Threading;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace FFCircularProgressView
{
	// The UIApplicationDelegate for the application. This class is responsible for launching the
	// User Interface of the application, as well as listening (and optionally responding) to
	// application events from iOS.
	public class FFCircularProgressViewController : UIViewController
	{
		const long NSEC_PER_SEC = 1000000000L;

		FFCircularProgressView _circularPV;

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();

			_circularPV = new FFCircularProgressView (new RectangleF(40f, 40f, 80f, 80f));
			_circularPV.Center = View.Center;

			View.AddSubview (_circularPV);

			StartAutoDemo ();
		}

		void StartAutoDemo ()
		{
			_demoing = true;
			_circularPV.StartInfiniteSpin ();

			double delayInSeconds = 2.5;
			var popTime = new DispatchTime (DispatchTime.Now, (Int64)delayInSeconds * NSEC_PER_SEC);
			DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default).DispatchAfter (popTime, StartProgressing);

			delayInSeconds = 2;
			popTime = new DispatchTime (DispatchTime.Now, (Int64)delayInSeconds * NSEC_PER_SEC);
			DispatchQueue.MainQueue.DispatchAfter (popTime, StopInfiniteSpin);
		}





		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			base.TouchesEnded (touches, evt);
			StartDemo ();
		}

		bool _demoing = false;
		bool _infiniteSpinWasLastUsed = false;

		void StartDemo ()
		{
			if(_demoing)
			{
				return;
			}

			if(_infiniteSpinWasLastUsed)
			{
				DispatchQueue.GetGlobalQueue (DispatchQueuePriority.Default).DispatchAsync (StartProgressing);
			}
			else
			{
				_demoing = true;
				_circularPV.StartInfiniteSpin ();
				const double delayInSeconds = 2.0;
				var popTime = new DispatchTime (DispatchTime.Now, (Int64)delayInSeconds * NSEC_PER_SEC);
				DispatchQueue.MainQueue.DispatchAfter (popTime, StopInfiniteSpin);
			}
		}

		void StartProgressing ()
		{
			_demoing = true;
			for (float i = 0; i < 1.1; i += .01f)
			{
				var i1 = i;
				DispatchQueue.MainQueue.DispatchAsync (() => _circularPV.Progress = i1);
				Thread.Sleep (100);
			}

			const double delayInSeconds = 2.0;
			DispatchTime popTime = new DispatchTime (DispatchTime.Now, (Int64)delayInSeconds * NSEC_PER_SEC);
			DispatchQueue.MainQueue.DispatchAfter (popTime, ClearProgress);
		}

		void ClearProgress ()
		{
			_circularPV.Progress = 0f;
			_infiniteSpinWasLastUsed = false;
			_demoing = false;
		}

		void StopInfiniteSpin ()
		{
			_circularPV.StopInfiniteSpin ();
			_infiniteSpinWasLastUsed = true;
			_demoing = false;
		}
	}
}

