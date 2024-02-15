using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace Player
{
    public class PlayerController : MonoBehaviour
    {
        [Expandable]
        internal ScriptableObject playerScriptable;

        private Rigidbody _rb;
    }
}

