using FishNet.Object;
using UnityEngine;

public class AudioManagerSetup : NetworkBehaviour
{
    public AudioListener audioListener;

    public override void OnStartClient()
    {
        base.OnStartClient();

        // Only enable the AudioListener for the local player
        audioListener.enabled = IsOwner;
    }
}
