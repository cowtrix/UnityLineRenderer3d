using UnityEngine;

public class TrailRenderer3D : LineRenderer3D
{
    public int PointLimit = 100;
    public float UpdateThreshold = 1;

    private Vector3 _lastPosition;
    private Vector3 _lastNewPointPosition;

    protected void Awake()
    {
        _lastPosition = transform.position;
    }

    protected override int GetMaxPoints()
    {
        return PointLimit;
    }

    /*protected virtual PointState GetPointState()
    {
        var normal = (_lastNewPointPosition - transform.position).normalized;
        if (normal == Vector3.zero)
        {
            normal = transform.forward;
        }

        Vector3 tangent = Vector3.Cross(normal, Vector3.forward);
        if (tangent.magnitude == 0)
        {
            tangent = Vector3.Cross(normal, Vector3.up);
        }

        var position = transform.position;

        return new PointState
        {
            Position = position,
            Normal = normal,
            Tangent = tangent
        };
    }*/
    
    protected override void Update()
    {
        if ((_lastNewPointPosition - transform.position).magnitude > UpdateThreshold)
        {
            _lastNewPointPosition = transform.position;
            Points.Add(transform.position);
        }
        else if (Points.Count > 1)
        {
            Points[Points.Count - 1] = transform.position;
        }
        while (Points.Count > PointLimit)
        {
            Points.RemoveAt(0);
        }
        if (_lastPosition != transform.position)
        {
            _lastPosition = transform.position;
            RebakeMesh();
        }
        base.Update();
    }

    /*public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, _lastNewPointPosition);

        for (int i = 0; i < Points.Count; i++)
        {
            var variable = Points[i];
            Vector3 normal, tangent;
            GetNormalTangent(i, out normal, out tangent);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(variable, variable + normal);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(variable, variable + tangent);
            Gizmos.color = Color.white;
        }
    }*/
}