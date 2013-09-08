using System;
using System.Windows.Forms;
using System.Windows.Input;
using SlimDX;

namespace Mojo.Breadcrumber3D
{
    internal class UserInputHandler : IUserInputHandler
    {
        const float MINIMUM_DISTANCE_TO_TARGET = 0.1f;

        const float MOUSE_SENSITIVITY_ROTATE_X = 1.0f;
        const float MOUSE_SENSITIVITY_ROTATE_Y = 1.5f;
        const float MOUSE_SENSITIVITY_PAN = 0.001f;
        const float MOUSE_SENSITIVITY_ZOOM = 0.001f;

        private readonly Breadcrumber mBreadcrumber;

        private int mMousePreviousX;
        private int mMousePreviousY;

        private static readonly Vector3 CAMERA_GROUND_NORMAL = new Vector3( 0f, 1f, 0f );
        private const int CAMERA_GROUND_NORMAL_NON_ZERO_COORDINATE = 1;
        private const int CAMERA_GROUND_NORMAL_SIGN = 1;

        public UserInputHandler( Breadcrumber breadcrumber )
        {
            mBreadcrumber = breadcrumber;
        }

        public void OnKeyDown( System.Windows.Input.KeyEventArgs keyEventArgs )
        {
            switch( keyEventArgs.Key )
            {
                case Key.Left:
                    mBreadcrumber.CurrentSlice = Math.Max( 0, mBreadcrumber.CurrentSlice - 1 );
                    break;

                case Key.Right:
                    mBreadcrumber.CurrentSlice = Math.Min( mBreadcrumber.VolumeDescription.NumVoxelsZ - 1, mBreadcrumber.CurrentSlice + 1 );
                    break;

                case Key.Up:
                    mBreadcrumber.CurrentEdge = Math.Min( mBreadcrumber.ShortestPathDescriptions.Get( "Trail 1" )[ 0 ].SmoothPath.Count - 1, mBreadcrumber.CurrentEdge + 1 );
                    break;

                case Key.Down:
                    mBreadcrumber.CurrentEdge = Math.Max( 0, mBreadcrumber.CurrentEdge - 1 );
                    break;
            }
        }

        public void OnMouseDown( System.Windows.Forms.MouseEventArgs mouseEventArgs, int width, int height )
        {
            mMousePreviousX = mouseEventArgs.X;
            mMousePreviousY = mouseEventArgs.Y;
        }

        public void OnMouseUp( System.Windows.Forms.MouseEventArgs mouseEventArgs, int width, int height )
        {
        }

        public void OnMouseMove( System.Windows.Forms.MouseEventArgs mouseEventArgs, int width, int height )
        {
            if ( mouseEventArgs.Button == MouseButtons.Left )
            {
                var mouseDeltaX = mouseEventArgs.X - mMousePreviousX;
                var mouseDeltaY = mouseEventArgs.Y - mMousePreviousY;

                mMousePreviousX = mouseEventArgs.X;
                mMousePreviousY = mouseEventArgs.Y;

                var verticalTrackBallRadius = (float)Math.Min( width, height ) / 2;
                var verticalAngleRadians = Math.Atan( mouseDeltaY * MOUSE_SENSITIVITY_ROTATE_Y / verticalTrackBallRadius );
                var horizontalAngleRadians = Math.Atan( mouseDeltaX * MOUSE_SENSITIVITY_ROTATE_X / verticalTrackBallRadius );

                RotateCamera( - horizontalAngleRadians, verticalAngleRadians );                
            }

            if ( mouseEventArgs.Button == MouseButtons.Right )
            {
                var mouseDeltaX = mouseEventArgs.X - mMousePreviousX;
                var mouseDeltaY = mouseEventArgs.Y - mMousePreviousY;

                mMousePreviousX = mouseEventArgs.X;
                mMousePreviousY = mouseEventArgs.Y;

                MoveCameraAlongRightVector( mouseDeltaX * MOUSE_SENSITIVITY_PAN );
                MoveCameraAlongUpVector( mouseDeltaY * MOUSE_SENSITIVITY_PAN );
            }
        }
        
        public void OnMouseWheel( System.Windows.Forms.MouseEventArgs mouseEventArgs )
        {
            MoveCameraAlongViewVector( mouseEventArgs.Delta * MOUSE_SENSITIVITY_ZOOM );
        }

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression
        private void RotateCamera( double horizontalAngleRadians, double verticalAngleRadians )
        {
            Vector3 oldPosition, oldTarget, oldUp, oldTargetToOldPositionUnit;
            Vector3 planarOldPosition, planarOldTarget, planarOldTargetToOldPosition, planarOldRight, planarNewRight;
            Vector3 newPositionParallel, newPositionPerpendicular, newPosition, newUp, newPositionToOldTarget;

            Matrix rotationMatrix;

            //
            // compute the vertical component of the rotation first
            //
            mBreadcrumber.Camera.GetLookAtVectors( out oldPosition, out oldTarget, out oldUp );

            newPositionParallel      = (float)Math.Cos( verticalAngleRadians ) * ( oldPosition - oldTarget );
            newPositionPerpendicular = (float)Math.Sin( verticalAngleRadians ) * ( oldPosition - oldTarget ).Length() * oldUp;
            newPosition              = oldTarget + newPositionParallel + newPositionPerpendicular;

            mBreadcrumber.Camera.SetLookAtVectors( newPosition, oldTarget, oldUp );


            //
            // horizontally rotate the camera's position by temporarily discarding the y coordinates
            // of the old position and target, compute the rotation in 2D, and set the new position
            // y coordinate to equal the old y coordinate.  in other words, do a 2D rotation on the
            // horizontal plane.
            //
            mBreadcrumber.Camera.GetLookAtVectors( out oldPosition, out oldTarget, out oldUp );

            if ( CAMERA_GROUND_NORMAL_SIGN * oldUp[ CAMERA_GROUND_NORMAL_NON_ZERO_COORDINATE ] < 0 )
            {
                horizontalAngleRadians *= -1;
            }

            planarOldPosition                                             = oldPosition;
            planarOldTarget                                               = oldTarget;
            planarOldTarget[ CAMERA_GROUND_NORMAL_NON_ZERO_COORDINATE ]   = 0;
            planarOldPosition[ CAMERA_GROUND_NORMAL_NON_ZERO_COORDINATE ] = 0;    
            planarOldTargetToOldPosition                                  = planarOldPosition - planarOldTarget;
            oldTargetToOldPositionUnit                                    = oldPosition - oldTarget;
            oldTargetToOldPositionUnit.Normalize();

            newPositionParallel = (float)Math.Cos( horizontalAngleRadians ) * planarOldTargetToOldPosition;

            var oldRightUnit = Vector3.Cross( oldTargetToOldPositionUnit, oldUp );
            oldRightUnit.Normalize();

            newPositionPerpendicular = (float)Math.Sin( horizontalAngleRadians ) * planarOldTargetToOldPosition.Length() * oldRightUnit;

            newPosition                                             = planarOldTarget + newPositionParallel + newPositionPerpendicular;
            newPosition[ CAMERA_GROUND_NORMAL_NON_ZERO_COORDINATE ] = oldPosition[ CAMERA_GROUND_NORMAL_NON_ZERO_COORDINATE ];

            //
            // now rotate the camera's orientation.  assume that the camera's right vector always lies
            // in the horizontal plane.  compute the new right vector by rotating the old right vector
            // in 2D on the horizontal plane.
            //
            Release.Assert( MathUtil.ApproxEqual( Vector3.Dot( oldTargetToOldPositionUnit, oldUp ), 0.0f, 0.001f ) );

            planarOldRight = Vector3.Cross( oldTargetToOldPositionUnit, oldUp );
            planarOldRight.Normalize();

            Release.Assert( MathUtil.ApproxEqual( planarOldRight[ CAMERA_GROUND_NORMAL_NON_ZERO_COORDINATE ], 0, 0.001f ) );

            if ( CAMERA_GROUND_NORMAL_SIGN * oldUp[ CAMERA_GROUND_NORMAL_NON_ZERO_COORDINATE ] >= 0 )
            {
                rotationMatrix = Matrix.RotationAxis( CAMERA_GROUND_NORMAL, - (float)horizontalAngleRadians );
            }
            else
            {
                rotationMatrix = Matrix.RotationAxis( - CAMERA_GROUND_NORMAL, - (float)horizontalAngleRadians );
            }

            planarNewRight = MathUtil.TransformAndHomogeneousDivide( planarOldRight, rotationMatrix );
            planarNewRight.Normalize();

            Release.Assert( Vector3.Dot( planarNewRight, planarOldRight ) > 0 );

            newPositionToOldTarget = oldTarget - newPosition;

            newUp = Vector3.Cross( newPositionToOldTarget, planarNewRight );
            newUp.Normalize();

            Release.Assert( Vector3.Dot( newUp, oldUp ) > 0 );

            mBreadcrumber.Camera.SetLookAtVectors( newPosition, oldTarget, newUp );
        }
// ReSharper restore ConvertIfStatementToConditionalTernaryExpression

        private void MoveCameraAlongRightVector( float delta )
        {
            Vector3 oldPosition, oldTarget, oldUp;
            mBreadcrumber.Camera.GetLookAtVectors( out oldPosition, out oldTarget, out oldUp );

            var right = Vector3.Cross( oldTarget - oldPosition, oldUp );
            right.Normalize();

            mBreadcrumber.Camera.SetLookAtVectors( oldPosition + ( right * delta ), oldTarget + ( right * delta ), oldUp );            
        }

        private void MoveCameraAlongUpVector( float delta )
        {
            Vector3 oldPosition, oldTarget, oldUp;
            mBreadcrumber.Camera.GetLookAtVectors( out oldPosition, out oldTarget, out oldUp );

            oldUp.Normalize();

            mBreadcrumber.Camera.SetLookAtVectors( oldPosition + ( oldUp * delta ), oldTarget + ( oldUp * delta ), oldUp );
        }

        private void MoveCameraAlongViewVector( float delta )
        {
            Vector3 oldPosition, oldTarget, oldUp;
            mBreadcrumber.Camera.GetLookAtVectors( out oldPosition, out oldTarget, out oldUp );

            var view = oldTarget - oldPosition;

            if ( view.Length() > delta + MINIMUM_DISTANCE_TO_TARGET )
            {
                view.Normalize();
                mBreadcrumber.Camera.SetLookAtVectors( oldPosition + ( delta * view ), oldTarget, oldUp );            
            }            
        }
    }
}
