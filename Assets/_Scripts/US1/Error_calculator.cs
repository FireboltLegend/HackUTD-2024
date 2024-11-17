using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Error_calculator : MonoBehaviour
{
	[SerializeField]
	List<GameObject> objects_original;
	[SerializeField]
	List<GameObject> objects_move;

	[SerializeField]
	float positionError = 0;

	[SerializeField]
	float rotationError = 0;
	// Start is called before the first frame update
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}

	[Button]
	public void calc_error()
	{
		positionError = 0;
		rotationError = 0;
		
		for (int i = 0; i < objects_original.Count; i++)
		{
			// Position Error Calculation
			positionError += Vector3.Distance(objects_move[i].transform.position, objects_original[i].transform.position);
            rotationError += Quaternion.Angle(objects_move[i].transform.rotation, objects_original[i].transform.rotation);

            // Rotation Error Calculation (Excluding Y-axis)
            //Vector3 moveRotationEuler = objects_move[i].transform.rotation.eulerAngles;
            //Vector3 originalRotationEuler = objects_original[i].transform.rotation.eulerAngles;

            //// Zero out the Y component
            //moveRotationEuler.y = 0;
            //originalRotationEuler.y = 0;

            //// Convert back to Quaternion and calculate angle difference
            //Quaternion adjustedMoveRotation = Quaternion.Euler(moveRotationEuler);
            //Quaternion adjustedOriginalRotation = Quaternion.Euler(originalRotationEuler);

        }

		// Calculate average errors
		if (objects_original.Count > 0)
		{
			positionError /= objects_original.Count;
			rotationError /= objects_original.Count;
		}
		
	}

	public (float,float) GetError()
	{
		calc_error();
		return (positionError, rotationError);
	}

	public void UpdateOriginalList(List<GameObject> listRef)
	{
		objects_original = listRef;
	}

}

