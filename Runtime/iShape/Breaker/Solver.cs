using iShape.Collections;
using iShape.Geometry;
using iShape.Geometry.Container;
using Unity.Collections;
using UnityEngine;

namespace iShape.Breaker {

    public struct BreakSolver {

        private static readonly Allocator tempAllocator = Allocator.Temp;
        
        public enum SmallSpawnStrategy {
            no,
            random,
            randomFade
        }

        private float minArea;
        private int maxCount;
        private SmallSpawnStrategy smallSpawnStrategy;

        public BreakSolver(float minArea, int maxCount, SmallSpawnStrategy smallSpawnStrategy) {
            this.minArea = minArea;
            this.maxCount = maxCount;
            this.smallSpawnStrategy = smallSpawnStrategy;
        }

        public PathList Divide(NativeArray<Triangle> triangles, Allocator allocator) {
            var result = new DynamicPathList(6 * maxCount, maxCount, allocator);

            var bufferA = new DynamicArray<Triangle>(maxCount / 3, tempAllocator);
            var bufferB = new DynamicArray<Triangle>(maxCount / 3, tempAllocator);

        
            bufferA.Add(triangles);

            do {
                for(int i = 0; i < bufferA.Count; ++i) {
                    var triangle = bufferA[i];
                    if (Divide(triangle, ref bufferB, ref result)) {
                        if (result.layouts.Count >= maxCount) {
                            goto onFinish;
                        }
                    }
                }

                bufferA.RemoveAll();

                if (bufferB.Count > 0) {
                    for(int i = 0; i < bufferB.Count; ++i) {
                        var triangle = bufferB[i];
                        if (Divide(triangle, ref bufferA, ref result)) {
                            if (result.layouts.Count >= maxCount) {
                                goto onFinish;
                            }
                        }
                    }
                }
                
                bufferB.RemoveAll();

            } while (bufferA.Count > 0);
        
            onFinish:
            
            bufferA.Dispose();
            bufferB.Dispose();

            return result.Convert();
        }
        
        private bool Divide(Triangle triangle, ref DynamicArray<Triangle> triangles, ref DynamicPathList result) {
            float s = triangle.Area;
            if (s < this.minArea) {
                switch (this.smallSpawnStrategy) {
                    case SmallSpawnStrategy.no:
                        return false;
                    case SmallSpawnStrategy.random:
                    if (Random.value < 0.5f) {
                        return false;
                    }
                    break;
                    case SmallSpawnStrategy.randomFade:
                    if (Random.value > s / this.minArea) {
                        return false;
                    }
                    break;
                }
            }

            var A = triangle.a;
            var B = triangle.b;
            var C = triangle.c;

            var AB = Vector2.Distance(A, B);
            var BC = Vector2.Distance(B, C);
            var CA = Vector2.Distance(C, A);

            float p = AB + BC + CA;

            float r = 2 * s / p;

            float Ox = (BC * A.x + CA * B.x + AB * C.x) / p;
            float Oy = (BC * A.y + CA * B.y + AB * C.y) / p;
            var O = new Vector2(Ox, Oy);
            
            
            float Ac = Vector2.Dot(O - A, B - A) / AB;
            float Ba = Vector2.Dot(O - B, C - B) / BC;
            float Cb = Vector2.Dot(O - C, A - C) / CA;

            var nAB = A.Unit(B, AB);
            var nBC = B.Unit(C, BC);
            var nCA = C.Unit(A, CA);

            var CA0 = C + nCA * (Cb * (CA - r) / CA);
            var CA1 = CA0 + nCA * r;


            var AB0 = A + nAB * (Ac * (AB - r) / AB);
            var AB1 = AB0 + nAB * r;

            var BC0 = B + nBC * (Ba * (BC - r) / BC);
            var BC1 = BC0 + nBC * r;
        
            triangles.Add(new Triangle(CA1, triangle.a, AB0));
            triangles.Add(new Triangle(AB1, triangle.b, BC0));
            triangles.Add(new Triangle(BC1, triangle.c, CA0));
            
            var path = new NativeArray<Vector2>(6, tempAllocator);
            path[0] = CA0;
            path[1] = CA1;
            path[2] = AB0;
            path[3] = AB1;
            path[4] = BC0;
            path[5] = BC1;
            
            result.Add(path, true);
            path.Dispose();

            return true;
        }
    }

    internal static class Vector2Extension {

        internal static Vector2 Unit(this Vector2 self, Vector2 vector, float length) {
            float dx = vector.x - self.x;
            float dy = vector.y - self.y;
            return new Vector2(dx / length, dy / length);
        }

    }

}
