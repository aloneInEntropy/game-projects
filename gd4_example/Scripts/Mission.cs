/// <summary>
/// The Mission class. Contains information about missions to be completed by the Player.
/// </summary>
public partial class Mission
{
    /// <summary>
    /// The name of the mission.
    /// </summary>
    public string Name { set; get; }

    /// <summary>
    /// The description of the mission.
    /// </summary>
    public string Description  { set; get; }

    /// <summary>
    /// The description of the win condition of the mission.
    /// </summary>
    public string WinConditionDescription  { set; get; }

    /// <summary>
    /// The active state of the mission; whether or not the mission is able to be completed.
    /// </summary>
    public bool Active  { set; get; }

    /// <summary>
    /// The completion state of the mission; whether or not the mission is completed.
    /// </summary>
    public bool Completed  { set; get; }

}
