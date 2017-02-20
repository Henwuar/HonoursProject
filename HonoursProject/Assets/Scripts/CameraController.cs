using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed_;
    [SerializeField]
    private float rotateSpeed_;
    [SerializeField]
    private float zoomSpeed_;
    [SerializeField]
    private GameObject playerCanvas_;
    [SerializeField]
    private Transform arrow_;

    private Vector3 eulerRotation;

    // Use this for initialization
    void Start ()
    {
	    
	}
	
	// Update is called once per frame
	void FixedUpdate ()
    {
        //see if a car is being controlled
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject checkpoint = GameObject.FindGameObjectWithTag("CheckpointManager");
	    if(player)
        {
            playerCanvas_.SetActive(true);
            float angle = Vector3.Angle(checkpoint.transform.position, player.transform.forward);
            angle *= Mathf.Sign(Vector3.Cross(player.transform.forward, checkpoint.transform.position - player.transform.position).y);
            arrow_.eulerAngles = new Vector3(0, 0, -angle);
            //move the camera behind the player
            transform.position = player.transform.position - (player.transform.forward * 4) + (Vector3.up * 2);
            //look at the player
            transform.LookAt(player.transform.position + (new Vector3(player.transform.forward.x, 0, player.transform.forward.z) * 3) + (new Vector3(player.GetComponent<Rigidbody>().velocity.x, 0, player.GetComponent<Rigidbody>().velocity.z)));

            if (Input.GetButtonDown("Toggle"))
            {
                player.GetComponent<Car>().ToggleControlled();
                player.GetComponent<Car>().Init();
            }
        }
        else
        {
            playerCanvas_.SetActive(false);
            //right mouse button
            if (Input.GetMouseButton(1))
            {
                eulerRotation.y += Input.GetAxis("Mouse X") * rotateSpeed_;
                eulerRotation.x -= Input.GetAxis("Mouse Y") * rotateSpeed_;
            }

            transform.rotation = Quaternion.Euler(new Vector3(eulerRotation.x, eulerRotation.y, 0));

            Vector3 moveDirection;
            moveDirection.x = Input.GetAxis("Vertical") * moveSpeed_;
            moveDirection.y = Input.GetAxis("Horizontal") * moveSpeed_;
            moveDirection.z = Input.GetAxis("Up") * moveSpeed_;
            moveDirection = Vector3.ClampMagnitude(moveDirection, moveSpeed_);

            Vector3 moveAmount = (moveDirection.x * transform.forward) + (moveDirection.y * transform.right) + (moveDirection.z * transform.up);
            transform.position += moveAmount * Time.deltaTime;

            float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed_;
            transform.position += transform.forward * scrollAmount;

            if(Input.GetButton("Toggle"))
            {
                print("space");
                GameObject.FindGameObjectWithTag("Car").GetComponent<Car>().ToggleControlled();
            }
        }
	}
}
