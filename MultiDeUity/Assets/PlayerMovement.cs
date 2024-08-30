using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun, IPunObservable
{
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    void Start()
    {
        // Ensure the transform component is not null
        if (transform == null)
        {
            Debug.LogError("Transform component is missing!");
            return;
        }

        // Ensure the PhotonView component is not null
        if (photonView == null)
        {
            Debug.LogError("PhotonView component is missing!");
            return;
        }

        // Initialize network position and rotation
        networkPosition = transform.position;
        networkRotation = transform.rotation;

        Debug.Log("PlayerMovement initialized. Position: " + networkPosition + ", Rotation: " + networkRotation);
    }

    void Update()
    {
        // Check if photonView is assigned
        if (photonView == null)
        {
            Debug.LogError("PhotonView component is missing during Update!");
            return;
        }

        // Check if transform is assigned
        if (transform == null)
        {
            Debug.LogError("Transform component is missing during Update!");
            return;
        }

        if (photonView.IsMine)
        {
            // Handle local player movement here
            // Example: Move the player using input
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            Vector3 move = new Vector3(horizontal, 0, vertical) * Time.deltaTime * 5f;
            transform.Translate(move);

            // Update network position
            networkPosition = transform.position;
            networkRotation = transform.rotation;
        }
        else
        {
            // Smoothly interpolate position and rotation
            transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 5f);
            transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 5f);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // Send position and rotation to other players
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            // Receive position and rotation from other players
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}
