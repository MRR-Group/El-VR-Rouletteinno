using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Marker : TargetableItem<Inventory>
{
    protected List<LineRenderer> _lineRenderer = new ();

    [SerializeField]
    protected Color m_color = Color.red;

    [SerializeField]
    protected Material m_drawingMaterial;

    [SerializeField]
    protected Transform m_tip;

    [SerializeField]
    [Range(0.001f, 0.1f)]
    protected float m_tipWidth = 0.005f;

    [SerializeField]
    protected float m_minDistanceBetweenPoints = 0.01f;

    private int _index;

    protected override bool CanUse()
    {
        return true;
    }

    public override bool Use(Inventory inventory)
    {
        StartLine();

        return true;
    }
    
    protected void StartLine()
    {
        var line = new GameObject().AddComponent<LineRenderer>();
        _index = 0;
        line.material = m_drawingMaterial;
        line.startColor = m_color;
        line.endWidth = m_tipWidth;
        _lineRenderer.Add(line);
    }

    public override void Update()
    {
        base.Update();
        
        if (CanUse() && _isInUse && !StartRayCast(out var hit))
        {
            Draw(hit.point);
        }
    }

    private void Draw(Vector3 position)
    {
        var line = _lineRenderer.FirstOrDefault();

        if (!line)
        {
            return;
        }

        var currentPosition = line.GetPosition(_index);

        if (!(Vector3.Distance(currentPosition, position) > m_minDistanceBetweenPoints))
        {
            return;
        }
        
        _index += 1;
        line.positionCount = _index + 1;
        line.SetPosition(_index, m_tip.position);
    }
}
