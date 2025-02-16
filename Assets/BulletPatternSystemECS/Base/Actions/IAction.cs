using Unity.Transforms;

public interface IAction
{
    /// <summary>
    /// How Long To Run the Action
    /// </summary>
    public float Duration { get; }

    /// <summary>
    /// Base Speed Agument To Tell Action how fast to run
    /// </summary>
    public float ActionSpeed { get; }

    /// <summary>
    /// Start the timer with a value. Used for certain Action
    /// </summary>
    public float StartTimer { get; }
}

public enum ActionTypes
{
    TransformAction,
    TransformWithEntities,
    //DelayAction,
    //SplitAction
}