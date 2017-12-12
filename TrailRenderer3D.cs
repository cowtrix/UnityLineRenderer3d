using UnityEngine;

public class TrailRenderer3D : LineRenderer3D
{
    public int PointLimit = 100;
    public float UpdateThreshold = 1;

    private Vector3 _lastNewPointPosition;

    protected override void Awake()
    {
        base.Awake();
        OnPositionChanged.AddListener(OnPositionChangedCallbck);
    }

    private void OnPositionChangedCallbck()
    {
        RebakeMesh();
    }

    protected override int GetMaxPoints()
    {
        return PointLimit;
    }

    protected override void Update()
    {
        if ((_lastNewPointPosition - CurrentTransform.position).magnitude > UpdateThreshold)
        {
            _lastNewPointPosition = CurrentTransform.position;
            Points.Add(GetPointState());
        }
        else if (Points.Count > 1)
        {
            Points[Points.Count - 1] = GetPointState();
        }
        while (Points.Count > PointLimit)
        {
            Points.RemoveAt(0);
        }
        base.Update();
    }
}