using UnityEngine;
using Microsoft.MixedReality.Toolkit.UI;

public class AlignController : MonoBehaviour
{
    public GameObject objectToAlign;           // Main cube
    public GameObject referenceObject;         // Hand-placed reference cube
    public TargetPositionUpdater updater;      // TargetPositionUpdater
    public Interactable alignButton;           // MRTK button

    private void Start()
    {
        alignButton.OnClick.AddListener(AlignToReference);
    }

    private void AlignToReference()
    {
        var oldPos = objectToAlign.transform.position;
        var oldRot = objectToAlign.transform.rotation;

        var targetPos = referenceObject.transform.position;
        var targetRot = referenceObject.transform.rotation;

        // 1) Compute offsets from the latest RAW pose (idempotent)
        updater.CalculateAlignmentOffsetTo(targetPos, targetRot);

        // Logs
        var posDelta = targetPos - oldPos;
        var rotDelta = Quaternion.Angle(oldRot, targetRot);
        Debug.Log("=== AlignController: Align triggered ===");
        Debug.Log($"Old Pos: {oldPos:F3} | Target Pos: {targetPos:F3} | ΔPos: {posDelta.magnitude:F3} m");
        Debug.Log($"Old Rot: {oldRot.eulerAngles:F1} | Target Rot: {targetRot.eulerAngles:F1} | ΔRot: {rotDelta:F1}°");
    }
}
