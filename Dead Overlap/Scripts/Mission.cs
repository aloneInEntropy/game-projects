using System.Text.Json;
using System.Text.Json.Serialization;
using System;


/// <summary>
/// The Mission class. Contains information about missions to be completed by the Player.
/// </summary>
public partial class Mission
{
    /// <summary>
    /// The type of the mission.
    /// </summary>
    public MissionType MType { set; get; }

    /// <summary>
    /// The name of the mission.
    /// </summary>
    public string Name { set; get; }

    /// <summary>
    /// The description of the mission.
    /// </summary>
    public string Description { set; get; }

    /// <summary>
    /// The description of the win condition of the mission.
    /// </summary>
    public string WinConditionDescription { set; get; }

    /// <summary>
    /// The active state of the mission; whether or not the mission is able to be completed.
    /// </summary>
    public bool Active { set; get; }

    /// <summary>
    /// The completion state of the mission; whether or not the mission is completed.
    /// </summary>
    public bool Completed { set; get; }
    
    /// <summary>
    /// Complete this mission, setting the Completed state and Active state of this object to true.
    /// </summary>
    public void Complete() {
        if (Active) {
            Completed = true;
        }
    }
    
    /// <summary>
    /// Activate this mission, setting the Active state of this object to true and allowing it to be completed.
    /// </summary>
    public void Activate() {
        Active = true;
    }
    
    /// <summary>
    /// Deactivate this mission, setting the Active state of this object to false.
    /// </summary>
    public void Deactivate() {
        Active = false;
    }

    public bool Equals(Mission m) {
        return MType == m.MType &&
                Name == m.Name &&
                Description == m.Description && 
                WinConditionDescription == m.WinConditionDescription && 
                Active == m.Active &&
                Completed == m.Completed;
    }

    public override int GetHashCode() {
		return HashCode.Combine(Name, Description, WinConditionDescription, Active, Completed);
	}
}

/// <summary>
/// The MissionType class. Defines information about the type of mission.
/// There are four types of missions:<br/><br/>
/// <b>Deliver</b>: The Player is tasked with delivering an item to a location or NPC. Has (Item)<c>ItemType</c> and <c>MissionLocation</c> fields.<br/>
/// <b>Talk</b>: The Player must speak to an NPC. Has an (NPC) <c>TargetNPC</c> field.<br/>
/// <b>KillN</b>: The Player must kill N of a certain enemy or creature. Has (Creature)<c>TargetCreature</c> and (float)<c>Amount</c> fields.<br/>
/// <b>Find</b>: The Player must find and pick up an item. Has an (Item)<c>ItemType</c> field.<br/>
/// </summary>
public partial class MissionType
{
    [JsonPropertyName("Name")]
    public string Name { set; get; }

    [JsonPropertyName("TargetNPCName")]
    public string TargetNPCName { set; get; }
    
    [JsonPropertyName("Location")]
    public string Location { set; get; }

    public NPC TargetNPC = new();

    /// <summary>
    /// Initialise the mission type and assign values to all variables if necessary, and null to all unneeded variables.
    /// </summary>
    public void Init() {
        TargetNPC = Globals.GetNPC(TargetNPCName);
        // GD.Print(TargetNPCName);
        // GD.Print(TargetNPC.trueName);
    }

    public bool Equals(MissionType m) {
        return Name == m.Name &&
                TargetNPCName == m.TargetNPCName && 
                Location == m.Location && 
                TargetNPC == m.TargetNPC;
    }

    public override int GetHashCode() {
		return HashCode.Combine(Name, TargetNPCName, Location, TargetNPC);
	}
}

public struct MissionLocation
{
    public object Value;
    public MissionLocation(NPC n)
    {
        Value = n;
    }

    public MissionLocation(string loc)
    {
        Value = loc;
    }

}
