using UnityEngine;

public class SwordHandler : MonoBehaviour
{
    public Transform rightHand;
    public Vector3 defaultPositionOffset;
    public Quaternion defaultRotationOffset;
    public Vector3 attack1PositionOffset;
    public Quaternion attack1RotationOffset;
    public Vector3 attack2PositionOffset;
    public Quaternion attack2RotationOffset;

    private Vector3 currentPositionOffset;
    private Quaternion currentRotationOffset;

    void Start()
    {
        if (rightHand == null)
        {
            Debug.LogError("RightHand is not assigned in SwordHandler.");
            return;
        }

        defaultPositionOffset = transform.localPosition;
        defaultRotationOffset = transform.localRotation;

        currentPositionOffset = defaultPositionOffset;
        currentRotationOffset = defaultRotationOffset;

        if (attack1PositionOffset == Vector3.zero) attack1PositionOffset = defaultPositionOffset;
        if (attack1RotationOffset == Quaternion.identity) attack1RotationOffset = defaultRotationOffset;
        if (attack2PositionOffset == Vector3.zero) attack2PositionOffset = defaultPositionOffset;
        if (attack2RotationOffset == Quaternion.identity) attack2RotationOffset = defaultRotationOffset;
    }

    void LateUpdate()
    {
        if (rightHand != null)
        {
            transform.position = rightHand.TransformPoint(currentPositionOffset);
            transform.rotation = rightHand.rotation * currentRotationOffset;
        }
    }

    public void SetDefault()
    {
        currentPositionOffset = defaultPositionOffset;
        currentRotationOffset = defaultRotationOffset;
    }

    public void SetAttack1()
    {
        currentPositionOffset = attack1PositionOffset;
        currentRotationOffset = attack1RotationOffset;
    }

    public void SetAttack2()
    {
        currentPositionOffset = attack2PositionOffset;
        currentRotationOffset = attack2RotationOffset;
    }
}