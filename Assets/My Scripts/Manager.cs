using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.hli.fpsmultiplayer
{
    public class Manager : MonoBehaviourPunCallbacks
    {
        public string player_prefab;
        public Transform[] spawn_points;

        private void Start()
        {
            Spawn();
        }
        public void RetardSpawn()
        {
            
            Invoke("Spawn", 0.5f);
        }

        public void Spawn()
        {
            Transform t_spawn = spawn_points[Random.Range(0, spawn_points.Length)];
            PhotonNetwork.Instantiate(player_prefab, t_spawn.position, t_spawn.rotation);
        }
    }
}