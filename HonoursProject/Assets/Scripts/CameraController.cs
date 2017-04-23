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
    [SerializeField]
    private GameObject debugDisplay_;

    private Vector3 eulerRotation_;
    private GameObject following_ = null;

    private bool toggled_ = false;
	
    public void Init()
    {
        eulerRotation_ = transform.eulerAngles;
    }

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

            if (arrow_.gameObject.activeSelf)
            {
                arrow_.eulerAngles = new Vector3(0, 0, angle * Mathf.Rad2Deg);
            }

            //move the camera behind the player
            transform.position = player.transform.position - (player.transform.forward * 4) + (Vector3.up * 2);
            //look at the player
            transform.LookAt(player.transform.position +  (new Vector3(player.transform.forward.x, 0, player.transform.forward.z) * 3));
        }
        else if(following_)
        {
            transform.position = following_.GetComponent<Car>().GetCurRoad().GetComponent<Road>().GetJunction().gameObject.transform.position + Vector3.up * 5;
            transform.LookAt(following_.transform);

            if (toggled_)
            {
                following_ = null;
                toggled_ = false;
                eulerRotation_ = transform.eulerAngles;
            }
        }
        else
        {

            GameObject.FindGameObjectWithTag("CheckpointManager").GetComponent<CheckpointManager>().StopTimer();
            playerCanvas_.SetActive(false);
            //right mouse button
            if (Input.GetMouseButton(1))
            {
                eulerRotation_.y += Input.GetAxis("Mouse X") * rotateSpeed_;
                eulerRotation_.x -= Input.GetAxis("Mouse Y") * rotateSpeed_;
            }

            eulerRotation_.y += Input.GetAxis("RightStick X") * rotateSpeed_;
            eulerRotation_.x += Input.GetAxis("RightStick Y") * rotateSpeed_;

            transform.rotation = Quaternion.Euler(new Vector3(eulerRotation_.x, eulerRotation_.y, 0));

            Vector3 moveDirection;
            moveDirection.x = Input.GetAxis("Vertical") * moveSpeed_;
            moveDirection.y = Input.GetAxis("Horizontal") * moveSpeed_;
            moveDirection.z = Input.GetAxis("Up") * moveSpeed_;
            moveDirection = Vector3.ClampMagnitude(moveDirection, moveSpeed_);

            Vector3 moveAmount = (moveDirection.x * transform.forward) + (moveDirection.y * transform.right) + (moveDirection.z * transform.up);
            transform.position += moveAmount * Time.deltaTime;

            float scrollAmount = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed_;
            transform.position += transform.forward * scrollAmount;

            if (toggled_)
            { 
                following_ = GameObject.FindGameObjectWithTag("Car");
                toggled_ = false;
            }
        }

        if (Input.GetMouseButtonDown(0) && !player)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            Physics.Raycast(ray, out hit);

            if (hit.collider && hit.collider.tag == "Car")
            {
                following_ = hit.collider.gameObject;
            }
        }

        if(!player)
        {
            debugDisplay_.GetComponent<DebugDisplayManager>().SetSelectedCar(following_);   
        }
    }

    public void ToggleCompass(bool value)
    {
        arrow_.gameObject.SetActive(value);
    }
}
