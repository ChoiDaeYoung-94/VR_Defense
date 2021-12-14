using UnityEngine;
using Valve.VR;

/// <summary>
/// 각 컨트롤러의 키값을 받아오기 위함
/// </summary>
public class InputManager : MonoBehaviour
{
    public SteamVR_Input_Sources handType;
    public SteamVR_Action_Boolean grabAction;

    public bool gunRight = false;
    public bool gunLeft = false;

    void Update()
    {
        if (grabAction.GetState(handType) && handType == SteamVR_Input_Sources.RightHand)
            gunRight = true;

        if (grabAction.GetStateUp(handType) && handType == SteamVR_Input_Sources.RightHand)
            gunRight = false;

        if (grabAction.GetState(handType) && handType == SteamVR_Input_Sources.LeftHand)
            gunLeft = true;

        if (grabAction.GetStateUp(handType) && handType == SteamVR_Input_Sources.LeftHand)
            gunLeft = false;
    }
}