using UnityEngine;

public class doorOpen : MonoBehaviour
{
    public float openAngle = 90f;     // Hvor meget døren åbner
    public float speed = 2f;          // Hastighed på åbningen

    private bool isOpen = false;      // Track if door is open or closed
    private Quaternion targetRotation;
    private Quaternion startRotation;

    void Start()
    {
        // Gem startrotationen
        startRotation = transform.rotation;

        // Den rotation døren skal åbne til
        targetRotation = Quaternion.Euler(
            startRotation.eulerAngles.x,
            startRotation.eulerAngles.y + openAngle,
            startRotation.eulerAngles.z
        );
    }

    // Public method so other scripts (raycast) can toggle the door
    public void ToggleDoor()
    {
        isOpen = !isOpen;
    }

    void Update()
    {
        if (isOpen)
        {
            // Opening the door
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                Time.deltaTime * speed
            );
        }
        else
        {
            // Closing the door
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                startRotation,
                Time.deltaTime * speed
            );
        }
    }
}
