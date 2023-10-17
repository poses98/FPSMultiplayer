using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

namespace com.hli.fpsmultiplayer
{
    public class Launcher : MonoBehaviourPunCallbacks
    {
        public void Awake()
        {
            Debug.Log("Iniciando multiplayer");

            PhotonNetwork.AutomaticallySyncScene = true;
            Connect();
        }

        public override void OnConnectedToMaster()
        {
            Debug.Log("Conectado al servidor");

            Join();

            base.OnConnectedToMaster();
        }
        public override void OnJoinedRoom()
        {
            Debug.Log("Sala encontrada: " + PhotonNetwork.CurrentRoom.Name);
            Debug.Log("Iniciando juego en sala con numero de jugadores" + PhotonNetwork.CurrentRoom.PlayerCount);
            StartGame();

            base.OnJoinedRoom();
        }
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.Log("Sala no encontrada");
            Create();

            base.OnJoinRandomFailed(returnCode, message);
        }

        public void Connect()
        {
            Debug.Log("Trying to connect");
            PhotonNetwork.GameVersion = "0.0.4";
            PhotonNetwork.ConnectUsingSettings();

        }
        public void Join()
        {
            Debug.Log("Uniendo a sala");

            PhotonNetwork.JoinRandomRoom();
        }

        public void Create()
        {
            Debug.Log("Creando sala");
            PhotonNetwork.CreateRoom("");
        }
        public void StartGame()
        {
            Debug.Log("Iniciando juego en sala: " + PhotonNetwork.CurrentRoom.Name);
            if(PhotonNetwork.CurrentRoom.PlayerCount == 1)
            {
               
                PhotonNetwork.LoadLevel(1);
            }
        }
    }
}
