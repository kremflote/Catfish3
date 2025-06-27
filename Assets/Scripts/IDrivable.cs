namespace StarterAssets {
    public interface IDrivable
    {
        FirstPersonController firstPersonController { get; set; }
        BoatController boatController { get; set; }
        void InsertKey();
        void EnterPilotMode();
        void ExitPilotMode();
        bool IsKeyInserted { get; }

    }
}