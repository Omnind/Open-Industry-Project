using Godot;
using System;
using System.Threading.Tasks;

[Tool]
public partial class ImfDiffuserSensor : Node3D
{
	[Export]
	bool enableComms = false;
	[Export]
	string tagName;
	[Export]
	float distance = 6.0f;

    readonly Guid id = Guid.NewGuid();
    double scan_interval = 0;
	bool readSuccessful = false;
	bool running = false;

	sbyte blocked = 0;

    bool debugBeam = true;
	[Export]
	bool DebugBeam
	{
		get
		{
			return debugBeam;
		}
		set
		{
			debugBeam = value;
			if (rayMarker != null)
				rayMarker.Visible = value;
		}
	}
	
	[Export]
	Color collisionColor;
	[Export]
	Color scanColor;
	
	Marker3D rayMarker;
	MeshInstance3D rayMesh;
	CylinderMesh cylinderMesh;
	StandardMaterial3D rayMaterial;
	
	Root Main;
	public override void _Ready()
	{
		rayMarker = GetNode<Marker3D>("RayMarker");
		rayMesh = GetNode<MeshInstance3D>("RayMarker/MeshInstance3D");
		cylinderMesh = rayMesh.Mesh.Duplicate() as CylinderMesh;
		rayMesh.Mesh = cylinderMesh;
		rayMaterial = cylinderMesh.Material.Duplicate() as StandardMaterial3D;
		cylinderMesh.Material = rayMaterial;

        Main = GetParent().GetTree().EditedSceneRoot as Root;

        if (Main != null)
        {
            Main.SimulationStarted += OnSimulationStarted;
            Main.SimulationEnded += OnSimulationEnded;
        }

        rayMarker.Visible = debugBeam;
	}
	
	public override void _PhysicsProcess(double delta)
	{
		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(rayMarker.GlobalPosition, rayMarker.GlobalPosition + GlobalTransform.Basis.Z * distance);
		query.CollisionMask = 8;
		var result = spaceState.IntersectRay(query);
		
		if (result.Count > 0)
		{
			blocked = 1;
			float resultDistance = rayMarker.GlobalPosition.DistanceTo((Vector3)result["position"]);
			if (cylinderMesh.Height != resultDistance)
				cylinderMesh.Height = resultDistance;
			if (rayMaterial.AlbedoColor != collisionColor)
				rayMaterial.AlbedoColor = collisionColor;
		}
		else
		{
			blocked = 0;
            if (cylinderMesh.Height != distance)
				cylinderMesh.Height = distance;
			if (rayMaterial.AlbedoColor != scanColor)
				rayMaterial.AlbedoColor = scanColor;
		}

		if (enableComms && running && readSuccessful)
		{
            scan_interval += delta;
            if (scan_interval > 0.1 && readSuccessful)
            {
                scan_interval = 0;
                Task.Run(ScanTag);
            }
        }


        rayMesh.Position = new Vector3(0, 0, cylinderMesh.Height * 0.5f);
	}
	
	void OnSimulationStarted()
	{
        running = true;
		if (enableComms)
		{
            Main.Connect(id, Root.DataType.Bool, tagName);
        }
        readSuccessful = true;
	}
	
	void OnSimulationEnded()
	{
		running = false;
		cylinderMesh.Height = distance;
		rayMaterial.AlbedoColor = scanColor;
		rayMesh.Position = new Vector3(0, 0, cylinderMesh.Height * 0.5f);
	}

    async Task ScanTag()
    {
        try
        {
			await Main.Write(id, blocked);
        }
        catch
        {
            GD.PrintErr("Failure to write: " + tagName + " in Node: " + Name);
        }
    }
}
