using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
namespace com.hli.fpsmultiplayer
{
    public class Look : MonoBehaviourPunCallbacks
    {
        #region Variables
        public static bool cursorLocked = true;

        //  Player's transform and cams' transform
        public Transform player;
        public Transform cams;
        public Transform weapon;

        //  X and Y sensitivity for the camera movement
        public float xSens;
        public float ySens;
        //  Max angle which the camera can rotate in the Y axis
        public float maxAngle;

        //  Origin of the cameras
        private Quaternion camCenter;
        #endregion

        #region Monobehaviour Callbacks
        void Start()
        {
            camCenter = cams.localRotation; //  Set rotation origin for cameras to camCenter;
        }

        void Update()
        {
            if (!photonView.IsMine)
            {
                return;
            }
            //  Set the X and Y values of the view
            SetY();
            SetX();
            //UpdateCursorLock();
        }

        #endregion

        #region Private methods
        void SetY()
        {
            //  We get the mouse input
            //  Mobile

            
            //  PC
             float t_input = Input.GetAxis("Mouse Y") * ySens * Time.deltaTime;

            // Quaternion is a 4 dimension vector...
            Quaternion t_adjustment = Quaternion.AngleAxis(t_input, -Vector3.right); // We put -Vector3.right as we are rotating around horizontal axis
                                                                                     //  Herewe are rotating the camera, as we don't want the player to be rotating up and down. Jus t the camera
            Quaternion t_delta = cams.localRotation * t_adjustment;

            //  Checks if the camera rotation is higher than the maxAngle we want
            if (Quaternion.Angle(camCenter, t_delta) < maxAngle)
            {
                cams.localRotation = t_delta;
                weapon.localRotation = t_delta;
            }
        }
        void SetX()
        {
            //  We get the mouse input
            //  Mobile

            
            //  PC
            float t_input = Input.GetAxis("Mouse X") * ySens * Time.deltaTime;

            // Quaternion is a 4 dimension vector...
            Quaternion t_adjustment = Quaternion.AngleAxis(t_input, Vector3.up); // We put -Vector3.right as we are rotating around vertical axis
            //  Here instead of rotating the camera, we are rotating the player as the camera moves with him
            Quaternion t_delta = player.localRotation * t_adjustment;
            //  Here we don't have a maxAngle as we can rotate 360º if we want. 
            player.localRotation = t_delta;
        }
        //  States if the cursor is locked and hidden or not
        //  User can lock&unlock it by pressing escape
        void UpdateCursorLock()
        {
            if (cursorLocked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cursorLocked = false;
                }
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    cursorLocked = false;
                }
            }
        }
        #endregion

    }
}

