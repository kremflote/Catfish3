namespace StarterAssets {
    public interface IDrivable
    {
        BoatController boatController { get; set; }
        void InsertKey();
        void EnterPilotMode();
        void ExitPilotMode();
        bool IsKeyInserted { get; }

    }
}