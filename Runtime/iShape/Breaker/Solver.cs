using System;
using iShape.Collections;
using iShape.Geometry;
using iShape.Geometry.Container;
using Unity.Collections;
using UnityEngine;

namespace iShape.Breaker {

    public struct BreakSolver {

        private const Allocator tempAllocator = Allocator.Temp;

        public enum FadeSpawnStrategy {
            no,
            random,
            randomFade
        }

        public struct Hexagon {
            public Vector2 p0;
            public Vector2 p1;
            public Vector2 p2;
            public Vector2 p3;
            public Vector2 p4;
            public Vector2 p5;
            public Vector2 center;

            public float area {
                get {
                    float s = 0;
                    s += p0.x * (p5.y - p1.y);
                    s += p1.x * (p0.y - p2.y);
                    s += p2.x * (p1.y - p3.y);
                    s += p3.x * (p2.y - p4.y);
                    s += p4.x * (p3.y - p5.y);
                    s += p5.x * (p4.y - p0.y);
                    return 0.5f * s;
                }
            }
        }

        private readonly float minArea;
        private readonly int maxCount;
        private readonly FadeSpawnStrategy fadeSpawnStrategy;

        public BreakSolver(float minArea, int maxCount, FadeSpawnStrategy fadeSpawnStrategy) {
            this.minArea = minArea;
            this.maxCount = maxCount;
            this.fadeSpawnStrategy = fadeSpawnStrategy;
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
                float value = random(s);
                switch (this.fadeSpawnStrategy) {
                    case FadeSpawnStrategy.no:
                        return false;
                    case FadeSpawnStrategy.random:
                    if (value < 0.5f) {
                        return false;
                    }
                    break;
                    case FadeSpawnStrategy.randomFade:
                    if (value > s / this.minArea) {
                        return false;
                    }
                    break;
                    default:
                        throw new ArgumentOutOfRangeException();
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
        
        public NativeArray<Hexagon> HexagonDivide(NativeArray<Triangle> triangles, Allocator allocator) {
            var result = new DynamicArray<Hexagon>(maxCount, allocator);

            var bufferA = new DynamicArray<Triangle>(maxCount / 3, tempAllocator);
            var bufferB = new DynamicArray<Triangle>(maxCount / 3, tempAllocator);

        
            bufferA.Add(triangles);
            do {
                for(int i = 0; i < bufferA.Count; ++i) {
                    var triangle = bufferA[i];
                    if (hexagonDivide(triangle, out var hexagon, ref bufferB)) {
                        result.Add(hexagon);
                        if (result.Count >= maxCount) {
                            goto onFinish;
                        }
                    }
                }

                bufferA.RemoveAll();

                if (bufferB.Count > 0) {
                    for(int i = 0; i < bufferB.Count; ++i) {
                        var triangle = bufferB[i];
                        if (hexagonDivide(triangle, out var hexagon, ref bufferA)) {
                            result.Add(hexagon);
                            if (result.Count >= maxCount) {
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
        
        private bool hexagonDivide(Triangle triangle, out Hexagon hexagon, ref DynamicArray<Triangle> triangles) {
            float s = triangle.Area;
            hexagon = new Hexagon();
            if (s < this.minArea) {
                float value = random(s);
                switch (this.fadeSpawnStrategy) {
                    case FadeSpawnStrategy.no:
                        return false;
                    case FadeSpawnStrategy.random:
                    if (value < 0.5f) {
                        return false;
                    }
                    break;
                    case FadeSpawnStrategy.randomFade:
                    if (value > s / this.minArea) {
                        return false;
                    }
                    break;
                    default:
                        throw new ArgumentOutOfRangeException();
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

            hexagon.p0 = CA0 - O;
            hexagon.p1 = CA1 - O;
            hexagon.p2 = AB0 - O;
            hexagon.p3 = AB1 - O;
            hexagon.p4 = BC0 - O;
            hexagon.p5 = BC1 - O;
            hexagon.center = O;

            return true;
        }

        private static float random(double value) {
            long bits = BitConverter.DoubleToInt64Bits(value);
            long mantissa = bits & 0xfffffffffffffL;
            long tail = mantissa % 1000;
            float random = 0.001f * tail;
            return random;
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
