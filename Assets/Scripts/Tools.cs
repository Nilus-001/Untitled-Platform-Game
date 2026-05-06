using System;
using UnityEngine;

namespace Global {
    public static class Tools {
        public static float GetAngleByVector(Vector2 vector) {
            return Vector2.Angle( Vector2.up , vector ) * -Mathf.Sign( vector.x );
        }
    }
}
