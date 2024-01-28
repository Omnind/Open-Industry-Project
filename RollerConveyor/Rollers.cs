using Godot;
using System;

[Tool]
public partial class Rollers : Node3D
{
	[Export]
	PackedScene rollerScene;
	
	float parentScale = 1.0f;
	
	[Export]
	public float ParentScale
	{
		get
		{
			return parentScale;
		}
		set
		{
			int roundedScale = Mathf.RoundToInt(value / rollersDistance) + 1;
			int rollerCount = GetChildCount();
			
			if (roundedScale - 2 > rollerCount && roundedScale != 0)
			{
				SpawnRoller();
			}
			else if (rollerCount > roundedScale - 2)
			{
				if (rollerCount > 2)
				{
					RemoveRoller();
				}
			}
			
			parentScale = value;
		}
	}
	
	float rollersDistance = 0.33f;
		
	RollerConveyor owner;
	
	public override void _Ready()
	{
		owner = Owner as RollerConveyor;
		FixRollers();
	}
	
	public override void _PhysicsProcess(double delta)
	{
		if (owner != null)
		{
			Scale = new Vector3(1 / owner.Scale.X, 1, 1);
		}
	}
	
	void SpawnRoller()
	{
		if (GetParent() == null || owner == null) return;
		Roller roller = rollerScene.Instantiate() as Roller;
		AddChild(roller, forceReadableName: true);
		roller.Owner = GetParent();
		roller.Position = new Vector3(rollersDistance * GetChildCount(), 0, 0);
		roller.speed = owner.Speed;
		roller.RotationDegrees = new Vector3(roller.RotationDegrees.X, owner.SkewAngle, roller.RotationDegrees.Z);
		FixRollers();
	}
	
	void RemoveRoller()
	{
		GetChild(GetChildCount() - 1).QueueFree();
	}

	void FixRollers()
	{
		((Roller)GetChild(0)).Position = new Vector3(rollersDistance, 0, 0);
	}
}
