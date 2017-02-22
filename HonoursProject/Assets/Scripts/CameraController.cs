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

    private bool toggled_ = false;
	
    void Update()
    {
        if (!toggled_ && Input.GetButtonDown("Toggle"))
        {
            toggled_ = true;
        }
    }

	// Update is called once per frame
	void FixedUpdate ()
    {
        //see if a car is being controlled
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject checkpoint = GameObject.FindGameObjectWithTag("CheckpointManager");

	    if(player)
        {
            //udpdate the player's compass
            playerCanvas_.SetActive(true);
            Vector3 north = checkpoint.transform.position - player.transform.position;
            float northHeading = Mathf.Atan2(north.z, north.x);
            float angle = northHeading - Mathf.Atan2(player.transform.forward.z, player.transform.forward.x);
            arrow_.eulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);

            //move the camera behind the player
            transform.position = player.transform.position - (player.transform.forward * 4) + (Vector3.up * 2);
            //look at the player
            float playerDir = player.GetComponentInChildren<WheelCollider>().steerAngle;
            transform.LookAt(player.transform.position +  (new Vector3(player.transform.forward.x, 0, player.transform.forward.z) * 3));// + (new Vector3(player.GetComponent<Rigidbody>().velocity.x, 0, player.GetComponent<Rigidbody>().velocity.z)));

            if (toggled_)
            {
                player.GetComponent<Car>().ToggleControlled();
                player.GetComponent<Car>().Init();
                toggled_ = false;
            }

            GameObject.FindGameObjectWithTag("CheckpointManager").GetComponent<CheckpointManager>().StartTimer();
        }
        else
        {

            GameObject.FindGameObjectWithTag("CheckpointManager").GetComponent<CheckpointManager>().StopTimer();
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

            if(toggled_)
            {
                GameObject.FindGameObjectWithTag("Car").GetComponent<Car>().ToggleControlled();
                toggled_ = false;
            }

            if(Input.GetMouseButtonDown(0))
            {
            
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                Physics.Raycast(ray, out hit);
                
                if(hit.collider && hit.collider.tag == "Car")
                {
                    hit.collider.gameObject.GetComponent<Car>().ToggleControlled();
                }
            }
        }
	}
}
