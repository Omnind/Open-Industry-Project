using Godot;
using System;

[Tool]
public partial class LimitSwitch : Node3D
{
	[Export]
	Vector3 centerPosition;
	
	Joint3D center;
	RigidBody3D rigidBody;
	Vector3 startPosition;
	
	Root main;
	
	public override void _Ready()
	{
		center = GetNode<Joint3D>("Generic6DOFJoint3D");
		rigidBody = GetNode<RigidBody3D>("RigidBody3D");
		
		if (GetNode("..") != null)
		{
			main = GetNode("..") as Root;
			
			if (main != null)
			{
				main.SimulationStarted += OnSimulationStarted;
				main.SimulationEnded += OnSimulationEnded;
			}
		}
	}
	
	public override void _PhysicsProcess(double delta)
	{
		rigidBody.Position = centerPosition;
		rigidBody.Scale = Vector3.One;
	}

	void OnSimulationStarted()
	{
		center.Position = centerPosition;
	}

	void OnSimulationEnded()
	{
		center.Position = centerPosition;
		rigidBody.Rotation = Vector3.Zero;
	}
}
