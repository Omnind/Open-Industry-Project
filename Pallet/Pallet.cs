using Godot;
using System;

[Tool]
public partial class Pallet : Node3D
{	
	RigidBody3D rigidBody;
	Vector3 initialPos;
	bool selected = false;
	bool frozen = false;
	bool keyHeld = false;
	Root Main;
	
	public override void _Ready()
	{
        Main = GetParent().GetTree().EditedSceneRoot as Root;

        if (Main == null)
        {
            return;
        }
        else
        {
            Main.SimulationStarted += Set;
            Main.SimulationEnded += Reset;
            Main.SimulationSetPaused += OnSetPaused;
        }

        rigidBody = GetNode<RigidBody3D>("RigidBody3D");

        SetPhysicsProcess(false);
    }
	
	public override void _PhysicsProcess(double delta)
	{
		if (Main == null) return;

        selected = Main.selectedNodes.Contains(this);

        if (selected && Input.IsPhysicalKeyPressed(Key.G) && !Main.paused)
        {
            if (!keyHeld)
            {
                keyHeld = true;
                frozen = !frozen;
            }
        }

        if (!Input.IsPhysicalKeyPressed(Key.G))
        {
            keyHeld = false;
        }

        if (frozen)
		{
			rigidBody.Freeze = true;
			rigidBody.TopLevel = false;
			rigidBody.Position = Vector3.Zero;
			rigidBody.Rotation = Vector3.Zero;
			rigidBody.Scale = Vector3.One;
		}
		else
		{
			rigidBody.TopLevel = true;
			rigidBody.Freeze = false;
			GlobalPosition = rigidBody.GlobalPosition;
			GlobalRotation = rigidBody.GlobalRotation;
			Scale = rigidBody.Scale;
		}
	}
	
	void Set()
	{
		if (Owner == null) return;
		
		initialPos = GlobalPosition;
		rigidBody.TopLevel = true;
		rigidBody.Freeze = false;
		SetPhysicsProcess(true);
	}
	
	void Reset()
	{
		SetPhysicsProcess(false);
		rigidBody.TopLevel = false;
		
		rigidBody.Position = Vector3.Zero;
		rigidBody.Rotation = Vector3.Zero;
		rigidBody.Scale = Vector3.One;
		
		rigidBody.LinearVelocity = Vector3.Zero;
		rigidBody.AngularVelocity = Vector3.Zero;
		
		GlobalPosition = initialPos;
		Rotation = Vector3.Zero;
	}
	
	void OnSetPaused(bool paused)
	{
		frozen = paused;
	}
}