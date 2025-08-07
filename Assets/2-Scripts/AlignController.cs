using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class AlignController : MonoBehaviour
{
    public TargetPositionUpdater updater;
    
    public Transform objectTransform;
    public Transform referenceObjectTransform;
    public Interactable alignButton;
    
    private void Start()
    {
        alignButton.OnClick.AddListener(AlignObjects);
    }

    private void AlignObjects()
    {
        // Önceki konum/rotasyonu al
        Vector3 oldPosition = objectTransform.position;
        Quaternion oldRotation = objectTransform.transform.rotation;

        // Yeni konuma hizala
        objectTransform.position = referenceObjectTransform.position;
        objectTransform.rotation = referenceObjectTransform.rotation;

        // Yeni konum/rotasyon
        Vector3 newPosition = objectTransform.position;
        Quaternion newRotation = objectTransform.rotation;

        // Değişimi hesapla
        Vector3 positionDelta = newPosition - oldPosition;
        float rotationDelta = Quaternion.Angle(oldRotation, newRotation);

        // Konsola yazdır
        Debug.Log("=== Object Aligned ===");
        Debug.Log($"Old Position: {oldPosition:F3} | New Position: {newPosition:F3} | ΔPos: {positionDelta.magnitude:F3} m");
        Debug.Log($"Old Rotation: {oldRotation.eulerAngles:F1} | New Rotation: {newRotation.eulerAngles:F1} | ΔRot: {rotationDelta:F1}°");
        
        updater.SetReferenceTransform(referenceObjectTransform.transform);
        Debug.Log("AlignController: markerTransform updated in TargetPositionUpdater.");
    }
}