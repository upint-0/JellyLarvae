using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    // Used to determine who can write to this transform. Owner client only 
    // This imposes state to the server. This is putting trust on your clients. Make sure no
    // security sensitive features use this transform
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}