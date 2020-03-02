
using iShape.Collections;
using iShape.Geometry;
using Unity.Collections;
using UnityEngine;

public static class TrianglesBuilder {
    
    public static NativeArray<Triangle> build(NativeArray<Vector2> points, NativeArray<int> indices, Allocator allocator) {

        int n = indices.Length / 3;
        var triangles = new NativeArray<Triangle>(n, allocator);
        for(int j = 0, i = 0; j < n; ++j, i += 3) {
            var a = points[indices[i]];
            var b = points[indices[i + 1]];
            var c = points[indices[i + 2]];
            triangles[j] = new Triangle(a, b, c);
        }

        return triangles;
    }
    
    public static NativeArray<Triangle> build(NativeSlice<Vector2> points, NativeArray<int> indices, Allocator allocator) {

        int n = indices.Length / 3;
        var triangles = new NativeArray<Triangle>(n, allocator);
        for(int j = 0, i = 0; j < n; ++j, i += 3) {
            var a = points[indices[i]];
            var b = points[indices[i + 1]];
            var c = points[indices[i + 2]];
            triangles[j] = new Triangle(a, b, c);
        }

        return triangles;
    }
}
