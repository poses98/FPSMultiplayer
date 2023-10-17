using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;

namespace com.hli.fpsmultiplayer
{
    public class Weapon : MonoBehaviourPunCallbacks
    {
        #region Variables

        public Gun[] loadout;
        public Transform weaponParent;
        public GameObject bulletHolePrefab;
        public LayerMask canBeShot;

        private float coolDown;
        private int currentIndex;
        private GameObject currentWeapon;

        #endregion

        #region MonoBehaviour Callbacks
        void Start()
        {

        }

        void Update()
        {
            

            DetectInputEquip();
            if (currentWeapon != null)
            {
                if (photonView.IsMine)
                {
                    //  Cooldown
                    if (coolDown > 0)
                    {
                        coolDown -= Time.deltaTime;
                    }
                }

            }

        }

        private void FixedUpdate()
        {
            if (currentWeapon != null)
            {
                if (photonView.IsMine)
                {
                    RpcAim(Input.GetMouseButton(1));
                    if (Input.GetMouseButton(0) && coolDown <= 0)
                    {
                        photonView.RPC("RpcShoot", RpcTarget.All);
                    }
                }
                //  Weapon elasticity
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, Vector3.zero, Time.deltaTime * 4f);
            }
        }
        #endregion

        #region Private methods

        private void DetectInputEquip()
        {
            if (photonView.IsMine)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    //  We send the server an RPC of the RpcEquip method
                    photonView.RPC("RpcEquip", RpcTarget.All, 0);
                }
                if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    //  We send the server an RPC of the RpcEquip method
                    photonView.RPC("RpcEquip", RpcTarget.All, 1);
                }
                if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    //  We send the server an RPC of the RpcEquip method
                    photonView.RPC("RpcEquip", RpcTarget.All, 2);
                }
                if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    //  We send the server an RPC of the RpcEquip method
                    photonView.RPC("RpcEquip", RpcTarget.All, 3);
                }
            }
        }

        //  We need to send the information that we are equipping a weapon to the server through an RPC

        [PunRPC]
        void RpcEquip(int p_ind)
        {
            if(p_ind !=3 && p_ind != 2)
            {
                coolDown = 0;
            }
            if(currentWeapon != null)
            {
                Destroy(currentWeapon);
            }
            
            currentIndex = p_ind;
            GameObject t_newEquipment = (GameObject)Instantiate(loadout[p_ind].prefab, weaponParent.position, weaponParent.rotation, weaponParent);
            t_newEquipment.transform.localPosition = Vector3.zero;
            t_newEquipment.transform.localEulerAngles = Vector3.zero;
            t_newEquipment.GetComponent<Sway>().isMine = photonView.IsMine;
            currentWeapon = t_newEquipment;
        }
        //  As for style we need to see other players aiming so we'll make it an RPC
        
        private void RpcAim(bool p_isAiming)
        {
            Transform t_anchor = currentWeapon.transform.Find("Anchor");
            Transform t_state_ads = currentWeapon.transform.Find("States/ADS");
            Transform t_state_hip = currentWeapon.transform.Find("States/Hip");

            if (p_isAiming)
            {
                //  Aim
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_ads.position, Time.deltaTime * loadout[currentIndex].aimSpeed);
            }
            else
            {
                //  Hip
                t_anchor.position = Vector3.Lerp(t_anchor.position, t_state_hip.position, Time.deltaTime * loadout[currentIndex].aimSpeed);

            }
        }

        //  We need to send the information that we are shooting a weapon to the server through an RPC
        [PunRPC]
        private void RpcShoot()
        {
            int t_damage = loadout[currentIndex].damage;
            Transform t_spawn = transform.Find("Cameras/NormalCamera");

            //  Bloom

            Vector3 t_bloom = t_spawn.position + t_spawn.forward * 1000f;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.up;
            t_bloom += Random.Range(-loadout[currentIndex].bloom, loadout[currentIndex].bloom) * t_spawn.right;
            t_bloom -= t_spawn.position;
            t_bloom.Normalize();

            //  Raycast

            RaycastHit t_hit = new RaycastHit();
            //  If collision succeded we create a new bullet hole in the position where it hit
            if(Physics.Raycast(t_spawn.position, t_bloom, out t_hit, 1000f, canBeShot))
            {
                
                GameObject t_newHole = Instantiate(bulletHolePrefab, t_hit.point + t_hit.normal * 0.001f, Quaternion.identity) as GameObject;
                t_newHole.transform.LookAt(t_hit.point + t_hit.normal);
                Destroy(t_newHole, 0.5f);

                //  Damage
                if (photonView.IsMine)
                {
                    //  If the bullet collides with a player
                    if(t_hit.collider.gameObject.layer == 11)
                    {
                        ShowHitmark();
                        //  RPC call to Damage Player
                        t_hit.collider.gameObject.GetPhotonView().RPC("SendDamage", RpcTarget.All,t_damage);
                        
                    }
                }
            }
            if (photonView.IsMine)
            {
                //  Gun FX
                currentWeapon.transform.Rotate(-loadout[currentIndex].recoil, 0, 0);
                currentWeapon.transform.position -= currentWeapon.transform.forward * loadout[currentIndex].kickback;
            }
            

            //  CoolDown

            coolDown = loadout[currentIndex].firerate;

        }
        private void ShowHitmark()
        {
            GameObject.Find("HUD/HitMark/Hit").GetComponent<Image>().enabled = true;
            Invoke("HideHitMark", 0.1f);
        }
        private void HideHitMark()
        {
            GameObject.Find("HUD/HitMark/Hit").GetComponent<Image>().enabled = false;
        }
        //  We make damage to other player
        [PunRPC]
        private void SendDamage(int p_damage)
        {
            
            GetComponent<Player>().TakeDamage(p_damage);
        }
        #endregion
    }
}