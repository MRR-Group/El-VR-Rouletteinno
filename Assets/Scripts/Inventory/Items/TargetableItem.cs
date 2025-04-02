using System;
using UnityEngine;

public abstract class TargetableItem<T>: NetworkItem where T : MonoBehaviour
{
    [SerializeField]
    protected Transform m_raycastStart;
    
    [SerializeField] 
    protected LayerMask m_layermask;

    [SerializeField]
    protected GameObject m_laser;

    private Renderer m_laserRenderer;
    
    [SerializeField]
    protected Material m_successLaserMaterial;
    protected Material _defaultLaserMaterial;

    protected override void Start()
    {
        base.Start();
        
        m_laser.SetActive(false);
        m_laserRenderer = m_laser.GetComponent<Renderer>();
        _defaultLaserMaterial = m_laserRenderer.material;
    }

    public virtual void Update()
    {
        base.Update();
        
        if (!_isGrabbed)
        {
            m_laser.SetActive(false);
            
            return;
        }
        
        m_laser.SetActive(true);
        
        if (StartRayCast(out _) != null)
        {
            m_laserRenderer.material = m_successLaserMaterial;
        }
        else
        {
            m_laserRenderer.material = _defaultLaserMaterial;
        }
    }

    public override bool Use()
    {
        var target = StartRayCast(out _);

        if (target == null)
        {
            return false;
        }

        return Use(target);
    }
    
    public abstract bool Use(T target);
    
    protected T StartRayCast(out RaycastHit hit)
    {
        var success= Physics.Raycast(m_raycastStart.position, transform.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, m_layermask);
        
        return success ? hit.transform?.GetComponentInParent<T>() : null;
    }
}