using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class RoomListItem : MonoBehaviour
{
    public Text roomNameText;
    private string roomName;

    public void Setup(string name, int playerCount, int maxPlayers)
    {
        roomName = name;
        roomNameText.text = $"{name} ({playerCount}/{maxPlayers})";
    }

    public void OnClick()
    {
        PhotonNetwork.JoinRoom(roomName);
    }
}
