using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class HumanoidController : MonoBehaviour
{

    public abstract void JumpEvent();
    public abstract void ColliderEvent(Collision coll);

}
