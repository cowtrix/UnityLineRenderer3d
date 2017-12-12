using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LineRenderer3D : MonoBehaviour
{
    public int CircularResolution = 5;
    public Material Material;
    public bool Loop;
    public float Radius = .2f;
    public AnimationCurve RadiusOverLifetime = AnimationCurve.Linear(0, 1, 1, 1);
    public List<Vector3> Points = new List<Vector3>();
    
    private Mesh _mesh;
    private List<int> _triangleBuffer = new List<int>();
    private List<Vector4> _uvBuffer = new List<Vector4>();
    private List<Vector3> _vertexBuffer = new List<Vector3>();
    private Vector3 _lastTangent;

    protected virtual void Update()
    {
        if (_mesh == null)
        {
            return;
        }
        Graphics.DrawMesh(_mesh, transform.localToWorldMatrix, Material, gameObject.layer);
    }

    protected void GetNormalTangent(int i, out Vector3 normal, out Vector3 tangent)
    {
        if (Points.Count < 2)
        {
            normal = Vector3.zero;
            tangent = Vector3.zero;
            return;
        }

        var position = Points[i];
        Vector3 lastPoint, nextPoint;
        if (i == 0)
        {
            lastPoint = Loop ? Points[Points.Count - 1] : position;
            nextPoint = Points[1];
        }
        else if (i == Points.Count - 1)
        {
            lastPoint = Points[Points.Count - 2];
            nextPoint = Loop ? Points[0] : position;
        }
        else
        {
            lastPoint = Points[i - 1];
            nextPoint = Points[i + 1];
        }

        normal = ((lastPoint - position) + (position - nextPoint)) / 2;
        tangent = Vector3.zero;

        var tangent1 = Vector3.Cross(normal, Vector3.forward);
        var tangent2 = Vector3.Cross(normal, Vector3.up);
        var tangent3 = Vector3.Cross(normal, Vector3.right);

        var a1 = Vector3.Angle(_lastTangent, tangent1);
        var a2 = Vector3.Angle(_lastTangent, tangent2);
        var a3 = Vector3.Angle(_lastTangent, tangent3);

        if (tangent1 != Vector3.zero && a1 < a2 && a1 < a3)
        {
            tangent = tangent1;
        }
        if (tangent2 != Vector3.zero && a2 < a1 && a2 < a3)
        {
            tangent = tangent2;
        }
        if (tangent3 != Vector3.zero && tangent == Vector3.zero)
        {
            tangent = tangent3;
        }

        if (tangent == Vector3.zero)
        {
            Debug.LogError("");
        }

        normal.Normalize();
        tangent.Normalize();
        _lastTangent = tangent;
    }

    [ContextMenu("Rebake")]
    public void RebakeMesh()
    {
        if (_mesh == null)
        {
            _mesh = new Mesh();
        }

        _vertexBuffer.Clear();
        _triangleBuffer.Clear();
        _uvBuffer.Clear();
        _mesh.Clear();

        if (Points.Count < 2)
        {
            return;
        }

        if (Loop)
        {
            Points.Add(Points[0]);
        }

        for (var i = Points.Count - 1; i >= 0; i--)
        {
            var position = Points[i];
            var maxPoints = (float) GetMaxPoints();
            var lifetime = ((Points.Count - i - 1)/maxPoints);

            Vector3 normal, tangent;
            GetNormalTangent(i, out normal, out tangent);

            var anglestep = 360/CircularResolution;
            for (var step = 0; step < CircularResolution; step++)
            {
                var angle = step*anglestep;
                var circlePosition = position + Quaternion.AngleAxis(angle, normal)
                                     * tangent * Radius * RadiusOverLifetime.Evaluate(lifetime);
                circlePosition = transform.InverseTransformPoint(circlePosition);

                // Add vertex
                _vertexBuffer.Add(circlePosition);
                _uvBuffer.Add(new Vector4((step/(float) (CircularResolution - 1)), lifetime));
                if (i == Points.Count - 1)
                {
                    continue;
                }

                // Add tris
                var p1 = _vertexBuffer.Count - 1;
                var p2 = p1 - CircularResolution;
                var p3 = p1 + 1;
                var p4 = p2 + 1;
                if (step == CircularResolution - 1)
                {
                    p3 -= CircularResolution;
                    p4 -= CircularResolution;
                }
                _triangleBuffer.Add(p1);
                _triangleBuffer.Add(p2);
                _triangleBuffer.Add(p3);

                _triangleBuffer.Add(p3);
                _triangleBuffer.Add(p2);
                _triangleBuffer.Add(p4);
            }
        }

        if (Loop)
        {
            Points.RemoveAt(Points.Count-1);
        }

        _mesh.SetVertices(_vertexBuffer);
        _mesh.SetTriangles(_triangleBuffer, 0);
        _mesh.SetUVs(0, _uvBuffer);
        _mesh.RecalculateNormals();
    }

    protected virtual int GetMaxPoints()
    {
        return Points.Count;
    }

    public void Simplify(float threshold)
    {
        for (int i = Points.Count - 2; i > 0; i--)
        {
            var vector3 = Points[i];
            var next = Points[i - 1];
            if (Vector3.Distance(vector3, next) < threshold)
            {
                Points.RemoveAt(i);
            }
        }
    }
}