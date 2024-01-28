using Godot;
using System;
using System.Threading.Tasks;

[Tool]
public partial class TripleStackLight : Node3D
{
    MainInstances mainInstances;

    [Export]
    bool enableComms = false;

    [Export]
	string tagSegment1;
	[Export]
	string tagSegment2;
	[Export]
	string tagSegment3;
	
	bool segment1 = false;
	[Export]
	bool Segment1
	{
		get
		{
			return segment1;
		}
		set
		{
			segment1 = value;
			
			if (Segment1)
			{
                SetMaterialColor(materialSeg1, SegmentColor1);
            }
			else
                SetMaterialToWhite(materialSeg1, SegmentColor1);
		}
	}
	
	bool segment2 = false;
	[Export]
	bool Segment2
	{
		get
		{
			return segment2;
		}
		set
		{
			segment2 = value;
			
			if (Segment2)
                SetMaterialColor(materialSeg2, SegmentColor2);
			else
                SetMaterialToWhite(materialSeg2, SegmentColor2);
		}
	}
	
	bool segment3 = false;
	[Export]
	bool Segment3
	{
		get
		{
			return segment3;
		}
		set
		{
			segment3 = value;
			
			if (Segment3)
                SetMaterialColor(materialSeg3, SegmentColor3);
			else
                SetMaterialToWhite(materialSeg3, SegmentColor3);
		}
	}
	
	Color segmentColor1 = new(0.0f, 1.0f, 0.0f, 0.5f);
	[Export]
	Color SegmentColor1
	{
		get
		{
			return segmentColor1;
		}
		set
		{
			segmentColor1 = value;
			if (Segment1)
                SetMaterialColor(materialSeg1, SegmentColor1);
		}
	}
	
	Color segmentColor2 = new(1.0f, 0.56f, 0.0f, 0.5f);
	[Export]
	Color SegmentColor2
	{
		get
		{
			return segmentColor2;
		}
		set
		{
			segmentColor2 = value;
			if (Segment2)
                SetMaterialColor(materialSeg2, SegmentColor2);
		}
	}
	
	Color segmentColor3 = new(1.0f, 0.0f, 0.0f, 0.5f);
	[Export]
	Color SegmentColor3
	{
		get
		{
			return segmentColor3;
		}
		set
		{
			segmentColor3 = value;
			if (Segment3)
                SetMaterialColor(materialSeg3, SegmentColor3);
		}
	}
	
	MeshInstance3D segment1MeshInstance;
	MeshInstance3D segment2MeshInstance;
	MeshInstance3D segment3MeshInstance;
	
	StandardMaterial3D materialSeg1;
	StandardMaterial3D materialSeg2;
	StandardMaterial3D materialSeg3;

    readonly Guid segment1Id = Guid.NewGuid();
    readonly Guid segment2Id = Guid.NewGuid();
    readonly Guid segment3Id = Guid.NewGuid();

	bool readSuccessful = false;
    bool running = false;
    double scan_interval = 0;

    Root main;
    public Root Main
    {
        get
        {
            return main;
        }
        set
        {
            main = value;
        }
    }

    public override void _Ready()
	{
		segment1MeshInstance = GetNode<MeshInstance3D>("color-seg-1");
		segment2MeshInstance = GetNode<MeshInstance3D>("color-seg-2");
		segment3MeshInstance = GetNode<MeshInstance3D>("color-seg-3");
		
		segment1MeshInstance.Mesh = segment1MeshInstance.Mesh.Duplicate() as Mesh;
		segment2MeshInstance.Mesh = segment1MeshInstance.Mesh.Duplicate() as Mesh;
		segment3MeshInstance.Mesh = segment1MeshInstance.Mesh.Duplicate() as Mesh;
		
		materialSeg1 = segment1MeshInstance.Mesh.SurfaceGetMaterial(0).Duplicate() as StandardMaterial3D;
		materialSeg2 = segment2MeshInstance.Mesh.SurfaceGetMaterial(0).Duplicate() as StandardMaterial3D;
		materialSeg3 = segment3MeshInstance.Mesh.SurfaceGetMaterial(0).Duplicate() as StandardMaterial3D;
		
		segment1MeshInstance.Mesh.SurfaceSetMaterial(0, materialSeg1);
		segment2MeshInstance.Mesh.SurfaceSetMaterial(0, materialSeg2);
		segment3MeshInstance.Mesh.SurfaceSetMaterial(0, materialSeg3);

        if (Owner != null)
        {
            Main = Owner as Root;

            if (Main != null)
            {
                Main.SimulationStarted += OnSimulationStarted;
                Main.SimulationEnded += OnSimulationEnded;
            }
        }
        else if (mainInstances != null)
        {
            Main = mainInstances.main;

            if (Main != null)
            {
                Main.SimulationStarted += OnSimulationStarted;
                Main.SimulationEnded += OnSimulationEnded;

                if (Main.Start)
                    OnSimulationStarted();
            }
        }
    }

    public override void _PhysicsProcess(double delta)
    {
		Segment1 = segment1;
		Segment2 = segment2;
		Segment3 = segment3;

        if (enableComms && running && readSuccessful)
        {
            scan_interval += delta;
            if (scan_interval > 0.05 && readSuccessful)
            {
                scan_interval = 0;
                Task.Run(ScanTag);
            }
        }
    }

    static void SetMaterialColor(StandardMaterial3D material, Color color)
	{
		material.AlbedoColor = color;
		material.Emission = color;
		material.EmissionEnergyMultiplier = 1.0f;

	}

    static void SetMaterialToWhite(StandardMaterial3D material, Color color)
	{
		if (material != null)
		{
			material.AlbedoColor = color;
            material.Emission = color;
            material.EmissionEnergyMultiplier = 0;
		}
	}

    void OnSimulationStarted()
    {
        if (Main == null) return;

		if (enableComms)
		{
            if (tagSegment1 != string.Empty)
                Main.Connect(segment1Id, Root.DataType.Bool, tagSegment1);
            if (tagSegment1 != string.Empty)
                Main.Connect(segment2Id, Root.DataType.Bool, tagSegment2);
            if (tagSegment1 != string.Empty)
                Main.Connect(segment3Id, Root.DataType.Bool, tagSegment3);
        }

        running = true;
        readSuccessful = true;
    }

    void OnSimulationEnded()
    {
        running = false;
    }

    async Task ScanTag()
    {
        if (tagSegment1 != string.Empty)
            Segment1 = await Main.ReadBool(segment1Id);
        if (tagSegment1 != string.Empty)
            Segment2 = await Main.ReadBool(segment2Id);
        if (tagSegment1 != string.Empty)
            Segment3 = await Main.ReadBool(segment3Id);
    }
}
