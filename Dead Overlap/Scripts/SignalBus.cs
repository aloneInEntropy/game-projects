using Godot;
using System;

public partial class SignalBus : Node
{
    [Signal]
	public delegate void DataLoadedEventHandler();
	[Signal]
	public delegate void DataSavedEventHandler();

}