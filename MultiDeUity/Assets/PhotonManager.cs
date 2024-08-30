using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private InputField lobbyNameInput;
    [SerializeField] private InputField chatInputField;
    [SerializeField] private Button connectToPhotonButton;
    [SerializeField] private Button connectToLobbyButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private Button leaveRoomButton;
    [SerializeField] private Button showNextRoomButton;
    [SerializeField] private Button selectCharacterButton;
    [SerializeField] private Button sendChatMessageButton;
    [SerializeField] private Button syncPositionButton;
    [SerializeField] private Text roomListText;
    [SerializeField] private Text playerListText;
    [SerializeField] private Text statusText;
    [SerializeField] private Text chatDisplayText;

    [Header("Character Settings")]
    [SerializeField] private GameObject[] characters;
    private int selectedCharacterIndex = -1;

    void Start()
    {
        if (connectToPhotonButton == null || connectToLobbyButton == null || createRoomButton == null || joinRoomButton == null || leaveRoomButton == null || showNextRoomButton == null || roomListText == null || playerListText == null || statusText == null || chatInputField == null || selectCharacterButton == null || sendChatMessageButton == null || syncPositionButton == null || chatDisplayText == null)
        {
            Debug.LogError("One or more UI references are missing. Please assign all UI elements in the Inspector.");
            return;
        }

        connectToPhotonButton.onClick.AddListener(ConnectToPhoton);
        connectToLobbyButton.onClick.AddListener(ConnectToLobby);
        createRoomButton.onClick.AddListener(CreateRoom);
        joinRoomButton.onClick.AddListener(JoinRoom);
        leaveRoomButton.onClick.AddListener(LeaveRoom);
        showNextRoomButton.onClick.AddListener(ShowNextRoom);
        selectCharacterButton.onClick.AddListener(SelectCharacter);
        sendChatMessageButton.onClick.AddListener(SendChatMessage);
        syncPositionButton.onClick.AddListener(SynchronizePosition);

        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        leaveRoomButton.interactable = false;
        showNextRoomButton.interactable = false;
        connectToLobbyButton.interactable = false;
        selectCharacterButton.interactable = false;
        sendChatMessageButton.interactable = false;
        syncPositionButton.interactable = false;
    }

    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    public void ConnectToPhoton()
    {
        if (!PhotonNetwork.IsConnected)
        {
            UpdateStatus("Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        Debug.Log("Connected to Photon Master Server.");
        UpdateStatus("Connected to Photon Master Server.");
        PhotonNetwork.AutomaticallySyncScene = true;
        connectToPhotonButton.interactable = false;
        connectToLobbyButton.interactable = true;
    }

    public void ConnectToLobby()
    {
        string lobbyName = lobbyNameInput.text;
        if (!string.IsNullOrEmpty(lobbyName))
        {
            UpdateStatus("Joining lobby...");
            PhotonNetwork.JoinLobby(new TypedLobby(lobbyName, LobbyType.Default));
        }
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.Log("Joined Lobby.");
        UpdateStatus("Joined Lobby.");
        createRoomButton.interactable = true;
        joinRoomButton.interactable = true;
        showNextRoomButton.interactable = true;
        selectCharacterButton.interactable = true;
        sendChatMessageButton.interactable = true;
        syncPositionButton.interactable = true;
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        Debug.Log("Left Lobby.");
        UpdateStatus("Left Lobby.");
        createRoomButton.interactable = false;
        joinRoomButton.interactable = false;
        showNextRoomButton.interactable = false;
        selectCharacterButton.interactable = false;
        sendChatMessageButton.interactable = false;
        syncPositionButton.interactable = false;
    }

    public void CreateRoom()
    {
        string roomName = lobbyNameInput.text;
        RoomOptions roomOptions = new RoomOptions { MaxPlayers = 4 };
        UpdateStatus("Creating room...");
        PhotonNetwork.CreateRoom(roomName, roomOptions);
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        Debug.Log("Room created.");
        UpdateStatus("Room created.");
        leaveRoomButton.interactable = true;
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.LogError($"Failed to create room: {message}");
        UpdateStatus($"Failed to create room: {message}");
    }

    public void JoinRoom()
    {
        string roomName = lobbyNameInput.text;
        UpdateStatus("Joining room...");
        PhotonNetwork.JoinRoom(roomName);
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined Room.");
        UpdateStatus("Joined Room.");
        leaveRoomButton.interactable = true;
        joinRoomButton.interactable = false;
        UpdatePlayerList();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.LogError($"Failed to join room: {message}");
        UpdateStatus($"Failed to join room: {message}");
    }

    public void LeaveRoom()
    {
        UpdateStatus("Leaving room...");
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.Log("Left Room.");
        UpdateStatus("Left Room.");
        leaveRoomButton.interactable = false;
        joinRoomButton.interactable = true;
        createRoomButton.interactable = true;
        showNextRoomButton.interactable = true;
        connectToLobbyButton.interactable = true;
        playerListText.text = "";
    }

    public void ShowNextRoom()
    {
        // Placeholder for showing the next available room
        // This functionality is managed by OnRoomListUpdate
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        Debug.Log("Room list updated.");

        roomListText.text = "";
        foreach (RoomInfo room in roomList)
        {
            Debug.Log($"Room: {room.Name} - Players: {room.PlayerCount}/{room.MaxPlayers}");
            roomListText.text += $"Room: {room.Name} - Players: {room.PlayerCount}/{room.MaxPlayers}\n";
        }
    }

    public void SelectCharacter()
    {
        if (selectedCharacterIndex >= 0 && selectedCharacterIndex < characters.Length)
        {
            photonView.RPC("RPC_SelectCharacter", RpcTarget.MasterClient, selectedCharacterIndex);
        }
    }

    [PunRPC]
    private void RPC_SelectCharacter(int characterIndex, PhotonMessageInfo info)
    {
        Debug.Log($"Character selected: {characterIndex} by player {info.Sender.NickName}");
        // Handle character selection logic here
        // Instantiate or activate the selected character for this player
    }

    public void SendChatMessage()
    {
        string message = chatInputField.text;
        if (!string.IsNullOrEmpty(message))
        {
            photonView.RPC("RPC_ReceiveChatMessage", RpcTarget.All, PhotonNetwork.NickName, message);
            chatInputField.text = "";
        }
    }

    [PunRPC]
    private void RPC_ReceiveChatMessage(string senderName, string message)
    {
        Debug.Log($"{senderName}: {message}");
        chatDisplayText.text += $"{senderName}: {message}\n";
    }

    public void SynchronizePosition()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.InRoom)
        {
            PlayerMovement playerMovement = GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                Vector3 position = playerMovement.transform.position;
                Quaternion rotation = playerMovement.transform.rotation;
                photonView.RPC("RPC_UpdatePlayerPosition", RpcTarget.All, position, rotation);
            }
        }
    }

    [PunRPC]
    private void RPC_UpdatePlayerPosition(Vector3 position, Quaternion rotation)
    {
        PlayerMovement playerMovement = GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.transform.position = position;
            playerMovement.transform.rotation = rotation;
        }
    }

    private void UpdatePlayerList()
    {
        playerListText.text = "";
        foreach (Photon.Realtime.Player player in PhotonNetwork.PlayerList)
        {
            playerListText.text += $"{player.NickName}\n";
        }
    }
}
