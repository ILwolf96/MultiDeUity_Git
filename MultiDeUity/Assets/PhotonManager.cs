using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    public Button connectButton;
    public Button joinLobbyButton;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button leaveRoomButton;

    public InputField lobbyNameInput;
    public InputField roomNameInput;
    public InputField maxPlayersInput;

    public Text statusText;
    public Transform roomListContainer;
    public Transform playerListContainer;
    public GameObject roomListItemPrefab;
    public GameObject playerListItemPrefab;

    private Dictionary<string, RoomInfo> cachedRoomList = new Dictionary<string, RoomInfo>();

    void Start()
    {
        // Initial UI setup
        UpdateUI();
    }

    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        statusText.text = "Connecting to server...";
        UpdateUI();
    }

    public void JoinLobby()
    {
        string lobbyName = lobbyNameInput.text;
        PhotonNetwork.JoinLobby(new TypedLobby(lobbyName, LobbyType.Default));
        statusText.text = $"Joining lobby '{lobbyName}'...";
        UpdateUI();
    }

    public void CreateRoom()
    {
        string roomName = roomNameInput.text;
        byte maxPlayers = byte.Parse(maxPlayersInput.text);
        RoomOptions options = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.CreateRoom(roomName, options);
        statusText.text = $"Creating room '{roomName}'...";
        UpdateUI();
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        statusText.text = $"Joining room '{roomName}'...";
        UpdateUI();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        statusText.text = "Leaving room...";
        UpdateUI();
    }

    private void UpdateUI()
    {
        connectButton.interactable = !PhotonNetwork.IsConnected;
        joinLobbyButton.interactable = PhotonNetwork.IsConnected && !PhotonNetwork.InLobby;
        createRoomButton.interactable = PhotonNetwork.InLobby;
        joinRoomButton.interactable = PhotonNetwork.InLobby && cachedRoomList.Count > 0;
        leaveRoomButton.interactable = PhotonNetwork.InRoom;
    }

    private void UpdateRoomList()
    {
        foreach (Transform child in roomListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (RoomInfo roomInfo in cachedRoomList.Values)
        {
            GameObject roomItem = Instantiate(roomListItemPrefab, roomListContainer);
            roomItem.GetComponentInChildren<Text>().text = $"{roomInfo.Name} ({roomInfo.PlayerCount}/{roomInfo.MaxPlayers})";
            roomItem.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomInfo.Name));
        }
    }

    private void UpdatePlayerList()
    {
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            GameObject playerItem = Instantiate(playerListItemPrefab, playerListContainer);
            playerItem.GetComponentInChildren<Text>().text = player.NickName;
        }
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected to server.";
        UpdateUI();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = "Joined lobby.";
        cachedRoomList.Clear();
        UpdateUI();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (RoomInfo room in roomList)
        {
            if (room.RemovedFromList)
                cachedRoomList.Remove(room.Name);
            else
                cachedRoomList[room.Name] = room;
        }
        UpdateRoomList();
    }

    public override void OnJoinedRoom()
    {
        statusText.text = $"Joined room '{PhotonNetwork.CurrentRoom.Name}'.";
        UpdatePlayerList();
        UpdateUI();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnLeftRoom()
    {
        statusText.text = "Left room.";
        UpdateUI();
    }
}
