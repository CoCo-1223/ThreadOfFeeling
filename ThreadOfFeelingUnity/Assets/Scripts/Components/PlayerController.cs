using UnityEngine;


public class NewMonoBehaviourScript : MonoBehaviour
{
    public float speed = 3;
    Vector3 move;

    void Start()
    {
        
    }

    void Update()
    {
        move = Vector3.zero;

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) {
            move += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) {
            move += new Vector3(1, 0, 0);
        }
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) {
            move += new Vector3(0, 1, 0);
        }
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) {
            move += new Vector3(0, -1, 0);
        }
        move = move.normalized;
    }

    private void FixedUpdate()
    {
        transform.Translate(move * speed * Time.deltaTime);
    }
}
