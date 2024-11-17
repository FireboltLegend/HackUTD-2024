using UnityEngine;

public class Movetofu : MonoBehaviour
{
    private float moveDistance = 0.15f;

    void Start()
    {
        // Invoke the MoveUpDown method repeatedly with a random interval
        InvokeRepeating("MoveUpDown", 0f, 1f); // Adjust time interval as needed
    }

    // Method to move the object up or down randomly
    void MoveUpDown()
    {
        // Get a random number between 0 and 1
        float randomValue = Random.Range(0f, 1f);

        // If the random value is less than 0.5, move the object up; otherwise, move it down.
        if (randomValue < 0.5f)
        {
            transform.position += new Vector3(0, moveDistance, 0);
        }
        else
        {
            transform.position -= new Vector3(0, moveDistance, 0);
        }
    }
}
