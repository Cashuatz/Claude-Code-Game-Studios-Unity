using UnityEngine;

namespace Proto.Rendering
{
    public static class FullscreenTriangle
    {
        private static Mesh _cached;

        public static Mesh Get()
        {
            if (_cached != null) return _cached;
            _cached = Create();
            return _cached;
        }

        private static Mesh Create()
        {
            var mesh = new Mesh { name = "Proto_FullscreenTriangle" };
            mesh.hideFlags = HideFlags.HideAndDontSave;

            mesh.SetVertices(new[]
            {
                new Vector3(-1f, -1f, 0f),
                new Vector3(-1f,  3f, 0f),
                new Vector3( 3f, -1f, 0f),
            });

            mesh.SetUVs(0, new[]
            {
                new Vector2(0f, 0f),
                new Vector2(0f, 2f),
                new Vector2(2f, 0f),
            });

            mesh.SetIndices(new[] { 0, 1, 2 }, MeshTopology.Triangles, 0, false);
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(1e6f, 1e6f, 0f));
            mesh.UploadMeshData(true);
            return mesh;
        }
    }
}