using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Create_Fires : MonoBehaviour
{
    public Target target;

    private void OnDisable() => target.Fire_Particles();
}
