using System;
using iShape.Breaker;
using iShape.Collections;
using iShape.Extension.Shape.Delaunay;
using iShape.Geometry;
using iShape.Geometry.Container;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;


namespace Tests.Editor.Tests.Breaker {

    public class BreakerTests {
        private const Allocator allocator = Allocator.Temp;
        private IntGeom iGeom = IntGeom.DefGeom;

        [Test]
        public void Test_00() {
            var data = BreakerTestData.data[0];
            
            var nPoints = new NativeArray<Vector2>(data, allocator);
            var iPoints = iGeom.Int(nPoints, allocator);
            
            var pShape = new PlainShape(iPoints, true, allocator);
            iPoints.Dispose();


            var indices = pShape.DelaunayTriangulate(allocator);
            pShape.Dispose();
            
            var triangles = TrianglesBuilder.build(nPoints, indices, allocator);

            indices.Dispose();
            nPoints.Dispose();

            var breaker0 = new BreakSolver(5, 300, BreakSolver.SmallSpawnStrategy.no);
            var polygons0 = breaker0.Divide(triangles, allocator);

            Assert.AreEqual(polygons0.Count, 3);

            Assert.AreEqual(polygons0.Get(0, allocator).Convert().isEqual(new[] {
                new Vector2(2.0710678f, 0.0f),
                new Vector2(-2.0710678f, 0.0f),
                new Vector2(-5.0f, 5.0f),
                new Vector2(-2.0710678f, 7.928932f),
                new Vector2(2.0710678f, 7.928932f),
                new Vector2(5.0f, 5.0f)
            }, 0.001f), true);
        
            Assert.AreEqual(polygons0.Get(1, allocator).Convert().isEqual(new[] {
                new Vector2(-4.1628227f, 3.570849f),
                new Vector2(-3.1991942f, 1.9258323f),
                new Vector2(-4.597563f, 0.0f),
                new Vector2(-6.504041f, 0.0f),
                new Vector2(-7.6229224f, 2.3770776f),
                new Vector2(-6.2748384f, 3.7251613f)
            }, 0.001f), true);

            Assert.AreEqual(polygons0.Get(2, allocator).Convert().isEqual(new[] {
                new Vector2(3.199194f, 1.925832f),
                new Vector2(4.1628222f, 3.5708487f),
                new Vector2(6.2748384f, 3.7251616f),
                new Vector2(7.622922f, 2.3770778f),
                new Vector2(6.504041f, 0.0f),
                new Vector2(4.597563f, 0.0f)
            }, 0.001f), true);
            
            polygons0.Dispose();
            
            var breaker1 = new BreakSolver(1.5f, 10, BreakSolver.SmallSpawnStrategy.no);
            var polygons1 = breaker1.Divide(triangles, allocator);

            Assert.AreEqual(polygons1.Count > 3, true);

            triangles.Dispose();
        }
        
        
        [Test]
        public void Test_01() {
            var data = BreakerTestData.data[1];
            
            var nPoints = new NativeArray<Vector2>(data, allocator);
            var iPoints = iGeom.Int(nPoints, allocator);
            
            var pShape = new PlainShape(iPoints, true, allocator);
            iPoints.Dispose();


            var indices = pShape.DelaunayTriangulate(allocator);
            pShape.Dispose();
            
            var triangles = TrianglesBuilder.build(nPoints, indices, allocator);

            indices.Dispose();
            nPoints.Dispose();

            var breaker = new BreakSolver(20, 20, BreakSolver.SmallSpawnStrategy.no);
            var polygons = breaker.Divide(triangles, allocator);
            triangles.Dispose();

            Assert.AreEqual(polygons.Count, 4);

            Assert.AreEqual(polygons.Get(0, allocator).Convert().isEqual(new[] {
                new Vector2(-1.0173526f, 5.988432f),
                new Vector2(-3.1394918f, 4.5736723f),
                new Vector2(-6.684741f, 6.6305175f),
                new Vector2(-5.5441256f, 8.911749f),
                new Vector2(-3.8257403f, 10.0f),
                new Vector2(-1.2752466f, 10.0f)
            }, 0.001f), true);
        
            Assert.AreEqual(polygons.Get(1, allocator).Convert().isEqual(new[] {
                new Vector2(6.31049f, 7.3790197f),
                new Vector2(8.128133f, 3.7437348f),
                new Vector2(4.760133f, 0.0f),
                new Vector2(0.6957607f, 0.0f),
                new Vector2(-1.3489046f, 5.767397f),
                new Vector2(2.0328574f, 8.021905f)
            }, 0.001f), true);

            Assert.AreEqual(polygons.Get(2, allocator).Convert().isEqual(new[] {
                new Vector2(-2.0710678f, 0.0f),
                new Vector2(2.0710678f, 0.0f),
                new Vector2(5.0f, -5.0f),
                new Vector2(2.0710678f, -7.928932f),
                new Vector2(-2.0710678f, -7.928932f),
                new Vector2(-5.0f, -5.0f)
            }, 0.001f), true);
            
            Assert.AreEqual(polygons.Get(3, allocator).Convert().isEqual(new[] {
                new Vector2(-0.7366932f, 4.0405293f),
                new Vector2(0.02077043f, 1.9039481f),
                new Vector2(-1.8329408f, 0.0f),
                new Vector2(-4.0998178f, 0.0f),
                new Vector2(-5.1286488f, 3.2475674f),
                new Vector2(-3.2424932f, 4.5050044f)
            }, 0.001f), true);
            
            polygons.Dispose();
            
        }
    }


    internal static class ArrayEqualExtension {
        internal static bool isEqual(this Vector2[] self, Vector2[] array, float precision) {
            if (self.Length != array.Length) {
                return false;
            }
            
            for(int i = 0; i < array.Length; ++i) {
                var a = self[i];
                var b = array[i];
                if (Math.Abs(a.x - b.x) > precision || Math.Abs(a.y - b.y) > precision) {
                    return false;
                }
            }

            return true;
        }

    
    }
    
}

