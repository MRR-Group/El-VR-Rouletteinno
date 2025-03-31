using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Dissolve : NetworkBehaviour
{
    private static readonly int Value = Shader.PropertyToID("_Value");
    private static readonly int BaseMap = Shader.PropertyToID("_BaseMap");
    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int Smoothness = Shader.PropertyToID("_Smoothness");
    private static readonly int Metallic = Shader.PropertyToID("_Metallic");

    [SerializeField] 
    private Material m_dissolveMaterial;

    [SerializeField] 
    private float m_dissolvingTime = 1f;

    public float DissolvingTime => m_dissolvingTime;
    
    [SerializeField] 
    private Renderer m_renderer;
    
    private NetworkVariable<bool> net_dissolve = new (false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private float _dissolvingProgress;
    private List<Material> _materials = new ();

    private void Start()
    {
        if (m_renderer == null)
        {
            m_renderer = GetComponent<Renderer>();
        }

        _dissolvingProgress = m_dissolvingTime;
    }

    [Rpc(SendTo.Server)]
    public void DissolveRpc()
    {
        if (!IsServer) return;

        net_dissolve.Value = true;
        ChangeMaterialRpc();
    }

    [Rpc(SendTo.Everyone)]
    private void ChangeMaterialRpc()
    {
        for (int i = 0; i < m_renderer.materials.Length; i++)
        {
            Material original = m_renderer.materials[i];
            Material dissolveMat = new Material(m_dissolveMaterial);

            if (original.HasProperty(BaseMap) && original.GetTexture(BaseMap) != null)
            {
                dissolveMat.SetTexture(BaseMap, original.GetTexture(BaseMap));
            }
            
            dissolveMat.SetColor(BaseColor, original.HasProperty(BaseColor) ? original.GetColor(BaseColor) : Color.white);

            if (original.HasProperty(Smoothness))
            {
                dissolveMat.SetFloat(Smoothness, original.GetFloat(Smoothness));
            }

            if (original.HasProperty(Metallic))
            {
                dissolveMat.SetFloat(Metallic, original.GetFloat(Metallic));
            }
            
            _materials.Add(dissolveMat);
        }

        m_renderer.materials = _materials.ToArray();
    }

    private void Update()
    {
        if (!net_dissolve.Value)
        {
            return;
        }

        _dissolvingProgress -= Time.deltaTime;
        float progress = Mathf.Clamp01(_dissolvingProgress / m_dissolvingTime);
        
        foreach (var mat in _materials)
        {
            mat.SetFloat(Value, progress);
        }
    }
}
