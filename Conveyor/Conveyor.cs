using Godot;
using System;
using System.Threading.Tasks;

[Tool]
public partial class Conveyor : Node3D, IConveyor
{
	public enum ConvTexture 
	{
		Black,
		Arrow
	}
	
	ConvTexture beltTexture = ConvTexture.Black;
	[Export]
	public ConvTexture BeltTexture
	{
		get
		{
			return beltTexture;
		}
		set
		{
			beltTexture = value;
			if (beltMaterial != null)
				((ShaderMaterial)beltMaterial).SetShaderParameter("BlackTextureOn", beltTexture == ConvTexture.Black);
			if (conveyorEnd1 != null)
				((ShaderMaterial)conveyorEnd1.beltMaterial).SetShaderParameter("BlackTextureOn", beltTexture == ConvTexture.Black);
			if (conveyorEnd2 != null)
				((ShaderMaterial)conveyorEnd2.beltMaterial).SetShaderParameter("BlackTextureOn", beltTexture == ConvTexture.Black);
		}
	}
	
	[Export]
	private bool enableComms = false;
	[Export]
	public string tagName;
	[Export]
    public float Speed { get; set; }

    private Vector3 origin;
	readonly Guid id = Guid.NewGuid();
	double scan_interval = 0;
	bool readSuccessful = false;

	RigidBody3D rb;
	MeshInstance3D mesh;
	Material beltMaterial;
	Material metalMaterial;
	
	bool running = false;
	public double beltPosition = 0.0;
	Vector3 boxSize;
	
	ConveyorEnd conveyorEnd1;
	ConveyorEnd conveyorEnd2;

    public Root Main { get; set; }

    public override void _Ready()
	{
		rb = GetNode<RigidBody3D>("RigidBody3D");
		
		mesh = GetNode<MeshInstance3D>("RigidBody3D/MeshInstance3D");
		mesh.Mesh = mesh.Mesh.Duplicate() as Mesh;
		beltMaterial = mesh.Mesh.SurfaceGetMaterial(0).Duplicate() as Material;
		metalMaterial = mesh.Mesh.SurfaceGetMaterial(1).Duplicate() as Material;
		mesh.Mesh.SurfaceSetMaterial(0, beltMaterial);
		mesh.Mesh.SurfaceSetMaterial(1, metalMaterial);
		mesh.Mesh.SurfaceSetMaterial(2, metalMaterial);
		
		conveyorEnd1 = GetNode<ConveyorEnd>("RigidBody3D/Ends/ConveyorEnd");
		conveyorEnd2 = GetNode<ConveyorEnd>("RigidBody3D/Ends/ConveyorEnd2");
		
		origin = rb.Position;

        ((ShaderMaterial)beltMaterial).SetShaderParameter("BlackTextureOn", beltTexture == ConvTexture.Black);
        conveyorEnd1.beltMaterial.SetShaderParameter("BlackTextureOn", beltTexture == ConvTexture.Black);
        conveyorEnd2.beltMaterial.SetShaderParameter("BlackTextureOn", beltTexture == ConvTexture.Black);

        Main = GetParent().GetTree().EditedSceneRoot as Root;

        if (Main != null)
        {
            Main.SimulationStarted += OnSimulationStarted;
            Main.SimulationEnded += OnSimulationEnded;
        }
    }

	public override void _PhysicsProcess(double delta)
	{
        if (Main == null) return;

		if (running)
		{
			var localLeft = rb.GlobalTransform.Basis.X.Normalized();
			var velocity = localLeft * Speed;
			rb.LinearVelocity = velocity;
			rb.Position = origin;

			beltPosition += Speed * delta;
			if (Speed != 0)
				((ShaderMaterial)beltMaterial).SetShaderParameter("BeltPosition", beltPosition * Mathf.Sign(Speed));
			if (beltPosition >= 1.0)
				beltPosition = 0.0;

			rb.Rotation = Vector3.Zero;
			rb.Scale = new Vector3(1, 1, 1);

            if (enableComms && readSuccessful)
            {
                scan_interval += delta;
                if (scan_interval > 0.1 && readSuccessful)
                {
                    scan_interval = 0;
                    Task.Run(ScanTag);
                }
            }
        }

		Scale = new Vector3(Scale.X, 1, Scale.Z);
		
		if (beltMaterial != null && Speed != 0)
			((ShaderMaterial)beltMaterial).SetShaderParameter("Scale", Scale.X * Mathf.Sign(Speed));
		
		if (metalMaterial != null && Speed != 0)
			((ShaderMaterial)metalMaterial).SetShaderParameter("Scale", Scale.X);
	}
	
	void OnSimulationStarted()
	{
		if (Main == null) return;
		if (enableComms)
		{
            Main.Connect(id, Root.DataType.Float, tagName);
        }
		running = true;
		readSuccessful = true;
	}
	
	void OnSimulationEnded()
	{
		running = false;
		
		beltPosition = 0;
		((ShaderMaterial)beltMaterial).SetShaderParameter("BeltPosition", beltPosition);
		
		rb.Position = Vector3.Zero;
		rb.Rotation = Vector3.Zero;
		rb.LinearVelocity = Vector3.Zero;
		
		foreach (Node3D child in rb.GetChildren())
		{
			child.Position = Vector3.Zero;
			child.Rotation = Vector3.Zero;
		}
	}
	
	async Task ScanTag()
	{
		try
		{
			Speed = await Main.ReadFloat(id);
		}
		catch
        {
            GD.PrintErr("Failure to read: " + tagName + " in Node: " + Name);
			readSuccessful = false;
        }
	}
}
