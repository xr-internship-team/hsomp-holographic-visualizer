using UnityEngine;

public class TargetPositionUpdater : MonoBehaviour
{
    public Transform markerTransform;
    public Transform objectTransform;

	private Vector3 _smoothedPosition;
	private Quaternion _smoothedRotation;

	private bool _firstUpdate = true;
	public float smoothFactor;
    
    public void CubePositionSetter(Vector3 positionDif, Quaternion rotationDif)
    {
        var originalQuaternion = rotationDif;
        var invertedQuaternion = new Quaternion(
            -originalQuaternion.x,
            -originalQuaternion.y,
            -originalQuaternion.z,
            originalQuaternion.w
        );
        
        var originalVector = positionDif;
        var invertedVector = new Vector3(
            -originalVector.x,
            -originalVector.y,
            -originalVector.z);

        var position = markerTransform.position - objectTransform.rotation * invertedVector;
        var rotaion = markerTransform.rotation * Quaternion.Inverse(invertedQuaternion);
        
        if (_firstUpdate)
    	{
        	_smoothedPosition = position;
        	_smoothedRotation = rotaion;
        	_firstUpdate = false;
    	}
    	else
    	{
        	// Pozisyon ve rotasyonu yumuşat
        	_smoothedPosition = Vector3.Lerp(_smoothedPosition, position, smoothFactor);
        	_smoothedRotation = Quaternion.Slerp(_smoothedRotation, rotaion, smoothFactor);
    	}
        
	    // if ((position - _smoothedPosition).magnitude > 1.0f)
	    // {
		   //  _smoothedPosition = position; // Ani sıçrama varsa doğrudan uygula
	    // }
	    
    	// Objeye uygula
    	objectTransform.position = _smoothedPosition;
    	objectTransform.rotation = _smoothedRotation;

		//Debug.Log("STAJ: Smooth update applied.");
        // Debug.Log("STAJ: Object transformation position and rotation setted.");
    }
    
    public void SetSmoothFactor(float value)
    {
	    smoothFactor = value;
	    Debug.Log($"STAJ: Smooth factor set to {smoothFactor}");
    }

}
