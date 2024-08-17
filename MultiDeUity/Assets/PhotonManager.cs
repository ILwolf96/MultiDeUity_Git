using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public InputField lobbyNameInput;
    public InputField roomNameInput;
    public InputField maxPlayersInput;
    public Text statusText;
    public Button connectButton;
    public Button joinLobbyButton;
    public Button createRoomButton;
    public Button leaveRoomButton;
    public Transform roomListContainer;
    public Transform playerListContainer;

    [Header("Prefabs")]
    public GameObject roomListItemPrefab;
    public GameObject playerListItemPrefab;

    private void Start()
    {
        UpdateUI();
    }

    public void ConnectToServer()
    {
        PhotonNetwork.ConnectUsingSettings();
        statusText.text = "Connecting to server...";
        UpdateUI();
    }

    public override void OnConnectedToMaster()
    {
        statusText.text = "Connected to server.";
        UpdateUI();
    }

    public void JoinLobby()
    {
        if (!string.IsNullOrEmpty(lobbyNameInput.text))
        {
            PhotonNetwork.JoinLobby(new TypedLobby(lobbyNameInput.text, LobbyType.Default));
            statusText.text = $"Joining lobby '{lobbyNameInput.text}'...";
        }
        else
        {
            statusText.text = "Lobby name cannot be empty.";
        }
        UpdateUI();
    }

    public override void OnJoinedLobby()
    {
        statusText.text = $"Joined lobby '{PhotonNetwork.CurrentLobby.Name}'.";
        UpdateUI();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdateRoomList(roomList);
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text) && int.TryParse(maxPlayersInput.text, out int maxPlayers))
        {
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = (byte)maxPlayers };
            PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
            statusText.text = $"Creating room '{roomNameInput.text}'...";
        }
        else
        {
            statusText.text = "Room name or max players is invalid.";
        }
        UpdateUI();
    }

    public override void OnCreatedRoom()
    {
        statusText.text = $"Room '{PhotonNetwork.CurrentRoom.Name}' created.";
        UpdateUI();
    }

    public override void OnJoinedRoom()
    {
        statusText.text = $"Joined room '{PhotonNetwork.CurrentRoom.Name}'.";
        UpdatePlayerList(); // Update player list using PhotonNetwork.CurrentRoom
        UpdateUI();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList(); // Refresh player list when a new player joins
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList(); // Refresh player list when a player leaves
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        statusText.text = "Leaving room...";
        UpdateUI();
    }

    public override void OnLeftRoom()
    {
        statusText.text = "Left room.";
        UpdateUI();
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        // Clear current room list UI
        foreach (Transform child in roomListContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate room list UI
        foreach (RoomInfo roomInfo in roomList)
        {
            GameObject roomItem = Instantiate(roomListItemPrefab, roomListContainer);
            roomItem.GetComponentInChildren<Text>().text = $"{roomInfo.Name} ({roomInfo.PlayerCount}/{roomInfo.MaxPlayers})";
            roomItem.GetComponent<Button>().onClick.AddListener(() => JoinRoom(roomInfo.Name));
        }
    }

    private void UpdatePlayerList()
    {
        // Clear current player list UI
        foreach (Transform child in playerListContainer)
        {
            Destroy(child.gameObject);
        }

        // Populate player list UI
        foreach (KeyValuePair<int, Player> playerInfo in PhotonNetwork.CurrentRoom.Players)
        {
            GameObject playerItem = Instantiate(playerListItemPrefab, playerListContainer);
            playerItem.GetComponentInChildren<Text>().text = playerInfo.Value.NickName;
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        statusText.text = $"Joining room '{roomName}'...";
        UpdateUI();
    }

    private void UpdateUI()
    {
        connectButton.interactable = !PhotonNetwork.IsConnected;
        joinLobbyButton.interactable = PhotonNetwork.IsConnected && PhotonNetwork.CurrentLobby == null;
        createRoomButton.interactable = PhotonNetwork.IsConnected && PhotonNetwork.InLobby;
        leaveRoomButton.interactable = PhotonNetwork.InRoom;
    }
}
