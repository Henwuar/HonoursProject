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
	    if(player)
        {
            //move the camera behind the player
            transform.position = player.transform.position - (player.transform.forward * 4) + (Vector3.up * 2);
            //look at the player
            transform.LookAt(player.transform.position + (player.transform.forward*3) + (player.GetComponent<Rigidbody>().velocity));

            if (Input.GetKeyDown(KeyCode.Space))
            {
                player.GetComponent<Car>().ToggleControlled();
                player.GetComponent<Car>().Init();
            } 
        }
        else
        {
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

            if(Input.GetKeyDown(KeyCode.Space))
            {
                GameObject.FindGameObjectWithTag("Car").GetComponent<Car>().ToggleControlled();
            }
        }
	}
}
