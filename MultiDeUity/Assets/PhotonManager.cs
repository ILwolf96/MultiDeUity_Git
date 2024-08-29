using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    public Button connectButton;
    public Button joinLobbyButton;
    public Button createRoomButton;
    public Button joinRoomButton;
    public Button leaveRoomButton;
    public Button nextRoomButton;
    public InputField roomNameInput;
    public InputField maxPlayersInput;
    public Text statusText;
    public Text roomListText;
    public Text playerListText;

    private List<RoomInfo> availableRooms = new List<RoomInfo>();
    private int currentRoomIndex = 0;

    void Start()
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
        Debug.Log("Connected to Photon Master Server");
        statusText.text = "Connected to server.";
        UpdateUI();
    }

    public void JoinLobby()
    {
        PhotonNetwork.JoinLobby();
        statusText.text = "Joining lobby...";
        UpdateUI();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        statusText.text = "Joined lobby.";
        UpdateUI();
    }

    public void CreateRoom()
    {
        if (!string.IsNullOrEmpty(roomNameInput.text) && int.TryParse(maxPlayersInput.text, out int maxPlayers))
        {
            RoomOptions roomOptions = new RoomOptions { MaxPlayers = (byte)maxPlayers };
            Debug.Log($"Creating room with name: {roomNameInput.text} and max players: {maxPlayers}");
            PhotonNetwork.CreateRoom(roomNameInput.text, roomOptions);
            statusText.text = $"Creating room '{roomNameInput.text}'...";
            UpdateUI();
        }
        else
        {
            statusText.text = "Room name or max players is invalid.";
        }
    }

    public void JoinRoom()
    {
        if (availableRooms.Count > 0)
        {
            RoomInfo roomInfo = availableRooms[currentRoomIndex];
            Debug.Log($"Attempting to join room: {roomInfo.Name}");
            PhotonNetwork.JoinRoom(roomInfo.Name);
            statusText.text = $"Joining room '{roomInfo.Name}'...";
            UpdateUI();
        }
        else
        {
            Debug.Log("No available rooms to join.");
            statusText.text = "No available rooms to join.";
        }
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        statusText.text = "Leaving room...";
        UpdateUI();
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left room");
        statusText.text = "Left room.";
        UpdateUI();
    }

    public void ShowNextRoom()
    {
        if (availableRooms.Count > 0)
        {
            currentRoomIndex = (currentRoomIndex + 1) % availableRooms.Count;
            RoomInfo roomInfo = availableRooms[currentRoomIndex];
            Debug.Log($"Displaying room: {roomInfo.Name}");
            SetupRoomListItem(roomInfo);
            UpdateUI();
        }
        else
        {
            Debug.Log("No rooms available to display.");
            statusText.text = "No rooms available.";
        }
    }

    private void SetupRoomListItem(RoomInfo roomInfo)
    {
        roomListText.text = $"Room: {roomInfo.Name} | Players: {roomInfo.PlayerCount}/{roomInfo.MaxPlayers}";
    }

    private void UpdatePlayerList()
    {
        playerListText.text = "";
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerListText.text += player.NickName + "\n";
        }
    }

    private void UpdateUI()
    {
        connectButton.interactable = !PhotonNetwork.IsConnected;
        joinLobbyButton.interactable = PhotonNetwork.IsConnected && PhotonNetwork.CurrentLobby == null;
        createRoomButton.interactable = PhotonNetwork.IsConnected && PhotonNetwork.InLobby;
        leaveRoomButton.interactable = PhotonNetwork.InRoom;
        nextRoomButton.interactable = availableRooms.Count > 0 && PhotonNetwork.InLobby;
        joinRoomButton.interactable = availableRooms.Count > 0 && PhotonNetwork.InLobby;
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"Successfully joined room: {PhotonNetwork.CurrentRoom.Name}");
        statusText.text = $"Joined room '{PhotonNetwork.CurrentRoom.Name}'.";
        UpdatePlayerList();
        UpdateUI();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Failed to join room: {message}");
        statusText.text = $"Failed to join room: {message}";
        UpdateUI();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        availableRooms = roomList;
        currentRoomIndex = 0;
        if (availableRooms.Count > 0)
        {
            ShowNextRoom();  // Update the displayed room
        }
        else
        {
            Debug.Log("No rooms available in the lobby.");
            statusText.text = "No rooms available.";
        }
        UpdateUI();
    }
}
