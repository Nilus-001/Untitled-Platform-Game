using System.Collections.Generic;
using UnityEngine;

namespace Global {
    public static class Tools {
        public static float GetAngleByVector(Vector2 vector) {
            return Vector2.Angle( Vector2.up , vector ) * -Mathf.Sign( vector.x );
        }
        
        public static Vector2 BezierQuadratique(Vector2 P0, Vector2 P1, Vector2 P2 , float t) {
            Vector2 a = Vector2.Lerp(P0,P1,t);
            Vector2 b = Vector2.Lerp(P1,P2,t);
            return Vector2.Lerp(a,b,t);
        }
        
        public static List<Vector2> GetPointsCircle(int n, float angle,float rayon) {
            List<Vector2> list = new();
            for (int i = 0; i < n ; i++) {
                float calc = (2 * Mathf.PI * i / n) + angle;
                float x = rayon * Mathf.Cos(calc);
                float y = rayon * Mathf.Sin(calc);
                list.Add(new Vector2(x,y));
            }
            return list;
        }
    }
}


// Sur ton Body, un script BodyAnimator.cs séparé


