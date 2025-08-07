using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class AlignController : MonoBehaviour
{
    public Transform objectTransform;
    public Transform referenceObjectTransform;
    public TargetPositionUpdater updater;
    public Interactable alignButton;

    private void Start()
    {
        alignButton.OnClick.AddListener(AlignToReference);
    }

    private void AlignToReference()
    {
        Vector3 oldPos = objectTransform.position;
        Quaternion oldRot = objectTransform.rotation;

        Vector3 newPos = referenceObjectTransform.position;
        Quaternion newRot = referenceObjectTransform.rotation;
        
        updater.CalculateAlignmentOffsetTo(newPos, newRot);
        Debug.Log("AlignController: Alignment offset applied.");
        
        objectTransform.position = referenceObjectTransform.position;
        objectTransform.rotation = referenceObjectTransform.rotation;
        
        objectTransform.position = newPos;
        objectTransform.rotation = newRot;
        
        Vector3 posDelta = newPos - oldPos;
        float rotDelta = Quaternion.Angle(oldRot, newRot);

        Debug.Log("=== AlignController: Object Aligned ===");
        Debug.Log($"Old Pos: {oldPos:F3} | New Pos: {newPos:F3} | ΔPos: {posDelta.magnitude:F3} m");
        Debug.Log($"Old Rot: {oldRot.eulerAngles:F1} | New Rot: {newRot.eulerAngles:F1} | ΔRot: {rotDelta:F1}°");
    }
}