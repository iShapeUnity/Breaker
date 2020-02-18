using UnityEngine;

namespace Tests.Editor.Tests.Breaker {

    internal struct BreakerTestData {
        internal static Vector2[][] data = {
            new[] {
                new Vector2(-10, 0),
                new Vector2(0, 10),
                new Vector2(10, 0)
            },
            new[] {
                new Vector2(0, -10),
                new Vector2(-10, 0),
                new Vector2(-5, 10),
                new Vector2(5, 10),
                new Vector2(10, 0)
            }
        };
    }

}