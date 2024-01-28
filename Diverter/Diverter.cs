using Godot;
using System;
using System.Threading.Tasks;

[Tool]
public partial class Diverter : Node3D
{
    [Export]
	bool enableComms = false;
    [Export]
    public string tagName;
	[Export]
	bool FireDivert = false;
	[Export]
	float divertTime = 0.5f;
	[Export]
	float divertDistance = 1.0f;

    bool readSuccessful = false;
    bool running = false;
    double scan_interval = 0;

    bool cycled = false;
    bool divert = false;
    private bool previousFireDivertState = false;

    readonly Guid id = Guid.NewGuid();
    DiverterAnimator diverterAnimator;
	Root Main;
	
	public override void _Ready()
	{
		diverterAnimator = GetNode<DiverterAnimator>("DiverterAnimator");

        Main = GetParent().GetTree().EditedSceneRoot as Root;

        if (Main != null)
        {
            Main.SimulationStarted += OnSimulationStarted;
            Main.SimulationEnded += OnSimulationEnded;
        }
    }
    public override void _PhysicsProcess(double delta)
    {
        if (FireDivert && !previousFireDivertState)
        {
            divert = true;
            cycled = false;  
        }

        if (divert && !cycled)
        {
            diverterAnimator.Fire(divertTime, divertDistance);
            divert = false;
            cycled = true;  
        }

        previousFireDivertState = FireDivert;

        if (enableComms && running && readSuccessful)
        {
            scan_interval += delta;
            if (scan_interval > 0.1 && readSuccessful)
            {
                scan_interval = 0;
                Task.Run(ScanTag);
            }
        }
    }
    void OnSimulationStarted()
    {
        if (Main == null) return;

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
		diverterAnimator.Disable();
	}

    async Task ScanTag()
    {
        try
        {
            FireDivert = await Main.ReadBool(id);
        }
        catch
        {
            GD.PrintErr("Failure to read: " + tagName + " in Node: " + Name);
            readSuccessful = false;
        }
    }
}
