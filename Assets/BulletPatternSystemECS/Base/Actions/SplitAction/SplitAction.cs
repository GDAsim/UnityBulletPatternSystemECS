using UnityEngine;

public struct SplitAction : IAction
{
    public bool DestroyOnEnd;
    public bool IsCopy; // Copy Current State/Vars Over

    public Ammo splitPrefab;
    public IAction[] splitActions;
    public TransformData splitDelta;

    Ammo main;

    #region Interface
    float IAction.Duration { get => 0; }
    float IAction.ActionSpeed { get => 1; }
    float IAction.StartTimer { get => 0; }
    #endregion

    public void ReadyAction(Ammo main)
    {
        this.main = main;

        if (splitPrefab == null) splitPrefab = main;
        if (splitActions == null) splitActions = main.patterns;
        if (splitDelta.Rotation.Equals(default)) splitDelta.Rotation = Quaternion.identity;
    }

    public void DoAction()
    {
        var newAmmo = GameObject.Instantiate(splitPrefab);

        var ammoTransform = new TransformData(main.transform) + splitDelta;
        ammoTransform.ApplyTo(newAmmo.transform);

        if (!IsCopy)
        {
            newAmmo.Setup(splitActions);
        }
        else
        {
            newAmmo.Setup(splitPrefab);
        }
    }

    public void EndAction()
    {
        if (DestroyOnEnd)
        {
            GameObject.Destroy(main.gameObject);
        }
    }
}
