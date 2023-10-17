using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace com.hli.fpsmultiplayer
{
    public class Player : MonoBehaviourPunCallbacks
    {
        #region Variables
        public float speed;
        public float sprintModifier;
        public float jumpForce;
        public int maxHealth;
        public Camera normalCam;
        public GameObject camParent;
        public Transform weaponParent;
        public Transform groundDetector;
        public Transform[] spawn_points;
        public LayerMask ground;

        private Transform uiHealthBar;

        private Vector3 targetWeaponBobPosition;
        private Vector3 weaponParentOrigin;

        private bool dead;
        private int currentHealth;
        private float movementCounter;
        private float idleCounter;

        private Manager manager;

        //  Starting Field Of View
        private float baseFOV;
        //  Modified FOV for sprint
        private float sprintFOVModifier = 1.25f;

        private Rigidbody rb;
        #endregion

        #region MonoBehaviour Callbacks
        private void Start()
        {
            dead = false;
            spawn_points = GameObject.Find("Spawns").GetComponentsInChildren<Transform>();
            manager = GameObject.Find("Manager").GetComponent<Manager>();
            currentHealth = maxHealth;
            camParent.SetActive(photonView.IsMine);
            if (!photonView.IsMine)
            {
                SetLayerRecursively(gameObject, 11); //  We set the layer to 11 as it is the normal player layer
                return;
            }
            
            baseFOV = normalCam.fieldOfView;
            if (Camera.main)
            {
                
                Camera.main.enabled = false;    //  We turn off the main camera
            }
            weaponParentOrigin = weaponParent.localPosition;
            rb = GetComponent<Rigidbody>();
            if (photonView.IsMine)
            {
                uiHealthBar = GameObject.Find("HUD/Health/Bar").transform;
                RefreshHealthBar();
            }
        }
        private void Update()
        {
            if (!photonView.IsMine)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                TakeDamage(10);
            }

            // Axles
            /*
            // Mobile
            float t_hmove = CrossPlatformInputManager.GetAxis("Horizontal");
            float t_vmove = CrossPlatformInputManager.GetAxis("Vertical");
            */
            
            //  PC
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");
            

            //  Controls

            bool t_sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); // We check if player is pressing left or right shift
            bool jump = Input.GetKeyDown(KeyCode.Space);// We check if player is pressing space
            
            //  States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = t_sprint && t_vmove > 0 && !isJumping && isGrounded;

            //Jumping
            if (isJumping)
            {
                rb.AddForce(Vector3.up * jumpForce);
            }

            //  Head Bob
            if(t_hmove == 0 && t_vmove == 0)
            {
                HeadBob(idleCounter,0.01f,0.01f);
                idleCounter += Time.deltaTime;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 2f);
            }
            else if(!isSprinting)
            {
                HeadBob(movementCounter,0.035f,0.035f);
                movementCounter += Time.deltaTime * 3f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 6f);
            }
            else 
            {
                HeadBob(movementCounter, 0.15f, 0.035f);
                movementCounter += Time.deltaTime * 7f;
                weaponParent.localPosition = Vector3.Lerp(weaponParent.localPosition, targetWeaponBobPosition, Time.deltaTime * 10f);
            }
            
        }
        void FixedUpdate()
        {
            if (!photonView.IsMine)
            {
                return;
            }

            // Axles
            /*
            // Mobile
            float t_hmove = CrossPlatformInputManager.GetAxis("Horizontal");
            float t_vmove = CrossPlatformInputManager.GetAxis("Vertical");
            */
            //  PC
            float t_hmove = Input.GetAxisRaw("Horizontal");
            float t_vmove = Input.GetAxisRaw("Vertical");
            
            //  Controls

            bool t_sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift); // We check if player is pressing left or right shift
            bool jump = Input.GetKeyDown(KeyCode.Space);// We check if player is pressing space

            //  States
            bool isGrounded = Physics.Raycast(groundDetector.position, Vector3.down, 0.1f, ground);
            bool isJumping = jump && isGrounded;
            bool isSprinting = t_sprint && t_vmove > 0 && !isJumping && isGrounded;


            //Jumping
            if (isJumping)
            {
                rb.AddForce(Vector3.up * jumpForce);
            }

            //  Movement
            Vector3 t_direction = new Vector3(t_hmove, 0, t_vmove);
            t_direction.Normalize(); //  We normalize so that the player moves with same speed no matter where he moves

            float t_adjustedSpeed = speed;
            if (isSprinting)
            {
                t_adjustedSpeed *= sprintModifier;
            }

            Vector3 t_targetVelocity = transform.TransformDirection(t_direction) * t_adjustedSpeed * Time.deltaTime; //  Delta time is put there so everyplayer has the same time of movement (calculated with fps)
            t_targetVelocity.y = rb.velocity.y; //In order to have a good jump we have to set the Y axis to our actual velocity in Y as we set it to zero.
            rb.velocity = t_targetVelocity;
            
            //  Field of view
            if (isSprinting)
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV * sprintFOVModifier, Time.deltaTime * 8f);
            }
            else
            {
                normalCam.fieldOfView = Mathf.Lerp(normalCam.fieldOfView, baseFOV, Time.deltaTime * 8f); ;
            }

            //  Ui Refreshes
            RefreshHealthBar();

            //  Death checker

            
        }
        #endregion

        #region Private methods

        

        private void HeadBob (float p_z, float p_x_intensity, float p_y_intensity)
        {
            targetWeaponBobPosition = weaponParentOrigin + new Vector3(Mathf.Cos(p_z) * p_x_intensity, Mathf.Sin(p_z*2) * p_y_intensity, 0);
        }

        public static void SetLayerRecursively(GameObject go, int layerNumber)
        {
            foreach (Transform trans in go.GetComponentsInChildren<Transform>(true))
            {
                trans.gameObject.layer = layerNumber;
            }
        }
        #endregion

        #region Public methods

        //  We want to send this value through the network
        [PunRPC]
        public void TakeDamage(int p_damage)
        {
            if (photonView.IsMine)
            {
                
                currentHealth -= p_damage;
                
                RefreshHealthBar();
                if (currentHealth <= 0)
                {
                    manager.RetardSpawn();
                    Die();
                }
            }
        }
        
        [PunRPC]
        private void Die()
        {
            if (photonView.IsMine)
            {
                GameObject.Find("HUD/HitMark/Hit").GetComponent<Image>().enabled = false;
                PhotonNetwork.Destroy(gameObject);
                camParent.SetActive(false);
                Destroy(gameObject);
            }
        }

        private void RefreshHealthBar()
        {
            //  We get the ratio of health we have
            float t_health_ratio = (float)currentHealth / (float)maxHealth;
            //  Smoothly change our actual healthbar to the desired healthbar scale
            uiHealthBar.localScale = Vector3.Lerp(uiHealthBar.localScale, new Vector3(t_health_ratio, 1, 1), Time.deltaTime * 4f);
        }
        public bool IsDead()
        {
            return dead;
        }
        
        #endregion
    }
}