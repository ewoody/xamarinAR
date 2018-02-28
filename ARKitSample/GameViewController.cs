using System;
using SceneKit;
using UIKit;
using ARKit;
using CoreGraphics;
using System.Threading.Tasks;

namespace ARKitSample
{
	public partial class GameViewController : UIViewController, IARSCNViewDelegate
	{
        private SCNNode ArrowUp;
        private SCNNode ArrowLeft;
        private SCNNode ArrowRight;

        enum Direction
        {
            Left,
            Right,
            LeftUp,
            RightUp
        }
        private Direction direction = Direction.LeftUp;

		#region Computed Properties
		public ARSCNView SceneView {
			get { return View as ARSCNView; }
		}

		public float AmbientIntensity {
			get {
				// Get the current frame
				var frame = SceneView.Session.CurrentFrame;
				if (frame == null) return 1000;

				// Return ambient intensity
				if (frame.LightEstimate == null) {
					return 1000;
				} else {
					return (float)frame.LightEstimate.AmbientIntensity;
				}
			}
		}
		#endregion

		#region Constructors
		protected GameViewController(IntPtr handle) : base(handle)
		{
			// Note: this .ctor should not contain any initialization logic.
		}
		#endregion

		#region Override Methods
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Set self as the Scene View's delegate
			SceneView.Delegate = this;

			// Track changes to the session
			SceneView.Session.Delegate = new SessionDelegate();

			// Create a new scene
            var arrowRight = SCNScene.FromFile("art.scnassets/arrow-right");
            ArrowRight = arrowRight.RootNode.FindChildNode("ArrowRight", true);


            var arrowLeft = SCNScene.FromFile("art.scnassets/arrow-left");
            ArrowLeft = arrowLeft.RootNode.FindChildNode("ArrowLeft", true);


            var arrowUp = SCNScene.FromFile("art.scnassets/arrow-up");
            ArrowUp = arrowUp.RootNode.FindChildNode("ArrowUp", true);


			// Set the scene to the view
            SceneView.Scene = arrowRight;


			// Add a tap gesture recognizer
			var tapGesture = new UITapGestureRecognizer(HandleTap);
			View.AddGestureRecognizer(tapGesture);

		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			// Create a session configuration
			var configuration = new ARWorldTrackingConfiguration {
				PlaneDetection = ARPlaneDetection.Horizontal,
				LightEstimationEnabled = true
			};

			// Run the view's session
			SceneView.Session.Run(configuration, ARSessionRunOptions.ResetTracking);
			
            StartNavigating(); 
            ChangeDirection();
			
		}

		public override void ViewWillDisappear(bool animated)
		{
			base.ViewWillDisappear(animated);

			// Pause the view's session
			SceneView.Session.Pause();
		}

        public void StartNavigating()
        {

            // Create a session configuration
            var configuration = new ARWorldTrackingConfiguration
            {
                PlaneDetection = ARPlaneDetection.Horizontal,
                LightEstimationEnabled = true
            };

            // Run the view's session
            SceneView.Session.Run(configuration, ARSessionRunOptions.ResetTracking);

            // Find the ship and position it just in front of the camera

            //var ship = SceneView.Scene.RootNode.ChildNodes[0];
            //ship.Position = new SCNVector3(0f, 0f, -0.5f);
            //SceneView.PointOfView.AddChildNode(ship);
            //ship.Scale = new SCNVector3(0.1f, 0.1f, 0.1f);





            //ship.RunAction(SCNAction.RepeatActionForever(SCNAction.MoveBy(new SCNVector3(1f, 0f, 0f), 1)));
            //HACK: to see the jet move (circle around the viewer in a roll), comment out the ship.Position line above
            // and uncomment the code below (courtesy @lobrien)

            //var animation = SCNAction.RepeatActionForever(SCNAction.RotateBy(0, (float)Math.PI, (float)Math.PI, (float)1));
            //var pivotNode = new SCNNode { Position = new SCNVector3(0.0f, 2.0f, 0.0f) };
            //pivotNode.RunAction(SCNAction.RepeatActionForever(SCNAction.RotateBy(0, -2, 0, 10)));
            //ship.RemoveFromParentNode();
            //pivotNode.AddChildNode(ship);
            //SceneView.Scene.RootNode.AddChildNode(pivotNode);
            //ship.Position = new SCNVector3(2f, -2f, -3f);
            //ship.RunAction(SCNAction.RepeatActionForever(SCNAction.RotateBy(0, 0, 2, 1)));

            //ENDHACK



            StartOrResetTimer();

        }

        public void DirectionTimer()
        {
            Task.Run(async () =>
            {
                if (direction == Direction.LeftUp)
                {
                    direction = Direction.Left;
                }
                else if (direction == Direction.RightUp)
                {
                    direction = Direction.Right;
                }
                else if (direction == Direction.Left)
                {
                    direction = Direction.RightUp;
                }
                else if (direction == Direction.Right)
                {
                    direction = Direction.LeftUp;
                }

                int period = 1500;
                if (direction == Direction.Left || direction == Direction.Right)
                {
                    period = 4000;
                    
                }
                await Task.Delay(period);

                InvokeOnMainThread(() =>
                {
                    ChangeDirection();


                });
            });
        }

        private void ChangeDirection()
        {
            //var node = SceneView.Scene.RootNode.FindChildNode("ArrowRight", true);
            //node.RemoveFromParentNode();
            //ArrowUp.Position = new SCNVector3(0f, 0f, -0.5f);

            //SceneView.Scene.RootNode.AddChildNode(ArrowUp);
            //SceneView.PointOfView.AddChildNode(ArrowUp);


            SCNScene arrow = null; 
            if (direction == Direction.Right)
            {
                arrow = SCNScene.FromFile("art.scnassets/arrow-right");
            }
            else if(direction == Direction.Left)
            {
                arrow = SCNScene.FromFile("art.scnassets/arrow-left");
            }
            else
            {
                arrow = SCNScene.FromFile("art.scnassets/arrow-up");
            }
            //ArrowLeft = arrowLeft.RootNode.FindChildNode("ArrowLeft", true);
            SceneView.Scene = arrow;
            var ship = SceneView.Scene.RootNode.ChildNodes[0];


            ship.Position = new SCNVector3(0f, 0f, -0.5f);
            if (SceneView.PointOfView.ChildNodes.Length > 0)
            {
                var node = SceneView.PointOfView.ChildNodes[0];
                node.RemoveFromParentNode();   
            }

            SceneView.PointOfView.AddChildNode(ship);
            ship.Scale = new SCNVector3(0.1f, 0.1f, 0.1f);

            DirectionTimer();
        }

        public void StartOrResetTimer()
        {
            Task.Run(async () =>
            {
                await Task.Delay(15000);
                InvokeOnMainThread(() =>
                {
                    
                    ShowDestinationReached();

                });
            });
        }

        public void ShowDestinationReached()
        {
            //Create Alert
            var okCancelAlertController = UIAlertController.Create("Congratulations", "Destination reached !!!", UIAlertControllerStyle.Alert);

            //Add Actions
            okCancelAlertController.AddAction(UIAlertAction.Create("Exit", UIAlertActionStyle.Default, alert => StartNavigating() /* System.Diagnostics.Process.GetCurrentProcess().CloseMainWindow()*/));

            okCancelAlertController.AddAction(UIAlertAction.Create("Start", UIAlertActionStyle.Cancel, alert => StartNavigating()));
            //Present Alert
            PresentViewController(okCancelAlertController, true, null);
            //direction = 0;
            SceneView.Session.Pause();
        }

		public override bool ShouldAutorotate()
		{
			return true;
		}

		public override UIInterfaceOrientationMask GetSupportedInterfaceOrientations()
		{
			return UIInterfaceOrientationMask.All;
		}
		#endregion

		#region Private Methods
		private void HandleTap(UIGestureRecognizer gestureRecognize)
		{
			// Get current frame
			var currentFrame = SceneView.Session.CurrentFrame;
			if (currentFrame == null) return;

			// Create an image plane using a snapshot of the view
			var imagePlane = SCNPlane.Create(SceneView.Bounds.Width / 6000, SceneView.Bounds.Height / 6000);
			imagePlane.FirstMaterial.Diffuse.Contents = SceneView.Snapshot();
			imagePlane.FirstMaterial.LightingModelName = SCNLightingModel.Constant;

			// Create a plane node and add it to the scene
			var planeNode = SCNNode.FromGeometry(imagePlane);
			SceneView.Scene.RootNode.AddChildNode(planeNode);

			// Set transform of node to be 10cm in front of the camera
			var translation = SCNMatrix4.CreateTranslation(0, 0, 0.1f);
			var cameraTranslation = currentFrame.Camera.Transform.ToSCNMatrix4();
			planeNode.Transform = SCNMatrix4.Mult(cameraTranslation, translation);
		}

		private void AddAnchorToScene() {

			// Get the current frame
			var frame = SceneView.Session.CurrentFrame;
			if (frame == null) return;

			// Create a ray to test from
			var point = new CGPoint(0.5, 0.5);

			// Preform hit testing on frame
			var results = frame.HitTest(point, ARHitTestResultType.ExistingPlane | ARHitTestResultType.EstimatedHorizontalPlane);

			// Use the first result
			if (results.Length >0) {
				var result = results[0];

				// Create an anchor for it
				var anchor = new ARAnchor(result.WorldTransform);

				// Add anchor to session
				SceneView.Session.AddAnchor(anchor);
			}

		}
		#endregion
	}
}
