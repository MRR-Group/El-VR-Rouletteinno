using UnityEngine;
using UnityEditor;
using Unity.Netcode;
using System.Collections.Generic;
using System.Reflection;

public class NetworkPrefabDebugger : EditorWindow
{
    private List<NetworkPrefab> _networkPrefabs = new List<NetworkPrefab>();
    private List<GameObject> _projectPrefabs = new List<GameObject>();
    private List<GameObject> _sceneNetworkObjects = new List<GameObject>();

    private string _searchHash = "";

    private bool _loadedNetworkPrefabsState = false;
    private bool _loadedProjectPrefabsState = false;
    private bool _loadedSceneObjectsState = false;

    private Dictionary<NetworkPrefab, bool> _networkPrefabFoldoutStates = new Dictionary<NetworkPrefab, bool>();
    private Dictionary<GameObject, bool> _projectPrefabFoldoutStates = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, bool> _sceneObjectFoldoutStates = new Dictionary<GameObject, bool>();

    private Vector2 _scrollPosition;

    private FieldInfo _globalObjectIdHashField;

    [MenuItem("Tools/Network Prefab Editor")]
    public static void ShowWindow()
    {
        GetWindow<NetworkPrefabDebugger>("Network Prefab Debugger");
    }

    private void OnEnable()
    {
        _globalObjectIdHashField = typeof(NetworkObject).GetField("GlobalObjectIdHash", BindingFlags.NonPublic | BindingFlags.Instance);
    }

    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        GUILayout.Space(10);
        _searchHash = EditorGUILayout.TextField("Hash to Find:", _searchHash);

        GUILayout.Space(10);

        if (GUILayout.Button("Reload Network Prefabs"))
        {
            LoadNetworkPrefabs();
        }

        _loadedNetworkPrefabsState = EditorGUILayout.Foldout(_loadedNetworkPrefabsState, "Loaded Prefabs from NetworkManager:");
        if (_loadedNetworkPrefabsState)
        {
            EditorGUI.indentLevel++;
            foreach (var prefab in _networkPrefabs)
            {
                if (!string.IsNullOrEmpty(_searchHash))
                {
                    bool containsSearchedHash = prefab.SourcePrefabGlobalObjectIdHash.ToString().Contains(_searchHash) ||
                                                prefab.TargetPrefabGlobalObjectIdHash.ToString().Contains(_searchHash);

                    if (!containsSearchedHash) continue;
                }

                _networkPrefabFoldoutStates.TryAdd(prefab, false);

                _networkPrefabFoldoutStates[prefab] = EditorGUILayout.Foldout(_networkPrefabFoldoutStates[prefab],
                    $"{prefab.Prefab.name}, Hash: {prefab.SourcePrefabGlobalObjectIdHash}");

                if (_networkPrefabFoldoutStates[prefab])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.LabelField($"Source Hash: {prefab.SourcePrefabGlobalObjectIdHash}");
                    EditorGUILayout.LabelField($"Target Hash: {prefab.TargetPrefabGlobalObjectIdHash}");
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Reload All NetworkObject Prefabs from Project"))
        {
            LoadProjectPrefabs();
        }

        _loadedProjectPrefabsState = EditorGUILayout.Foldout(_loadedProjectPrefabsState, "All NetworkObject Prefabs in Project:");
        if (_loadedProjectPrefabsState)
        {
            EditorGUI.indentLevel++;
            foreach (var prefab in _projectPrefabs)
            {
                var network = prefab.GetComponent<NetworkObject>();
                var prefabHash = network.PrefabIdHash;
                var globalHash = GetGlobalObjectIdHash(network);

                if (!string.IsNullOrEmpty(_searchHash))
                {
                    if (!prefabHash.ToString().Contains(_searchHash) && !globalHash.ToString().Contains(_searchHash)) continue;
                }

                _projectPrefabFoldoutStates.TryAdd(prefab, false);

                _projectPrefabFoldoutStates[prefab] = EditorGUILayout.Foldout(_projectPrefabFoldoutStates[prefab],
                    $"{prefab.name}, Hash: {prefabHash}");

                if (_projectPrefabFoldoutStates[prefab])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.ObjectField("Prefab:", prefab, typeof(GameObject), false);
                    EditorGUILayout.LabelField($"PrefabIdHash: {prefabHash}");
                    EditorGUILayout.LabelField($"GlobalObjectIdHash: {globalHash}");
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }

        GUILayout.Space(20);

        if (GUILayout.Button("Reload In-Scene NetworkObjects"))
        {
            LoadSceneNetworkObjects();
        }

        _loadedSceneObjectsState = EditorGUILayout.Foldout(_loadedSceneObjectsState, "All NetworkObjects in Scene:");
        if (_loadedSceneObjectsState)
        {
            EditorGUI.indentLevel++;
            foreach (var obj in _sceneNetworkObjects)
            {
                var network = obj.GetComponent<NetworkObject>();
                var objHash = network.PrefabIdHash;
                var globalHash = GetGlobalObjectIdHash(network);

                if (!string.IsNullOrEmpty(_searchHash))
                {
                    if (!objHash.ToString().Contains(_searchHash) && !globalHash.ToString().Contains(_searchHash)) continue;
                }

                _sceneObjectFoldoutStates.TryAdd(obj, false);

                _sceneObjectFoldoutStates[obj] = EditorGUILayout.Foldout(_sceneObjectFoldoutStates[obj],
                    $"{obj.name}, Hash: {objHash}");

                if (_sceneObjectFoldoutStates[obj])
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.ObjectField("Scene Object:", obj, typeof(GameObject), true);
                    EditorGUILayout.LabelField($"PrefabIdHash: {objHash}");
                    EditorGUILayout.LabelField($"GlobalObjectIdHash: {globalHash}");
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.EndScrollView();
    }

    private void LoadNetworkPrefabs()
    {
        _networkPrefabs.Clear();
        _networkPrefabFoldoutStates.Clear();

        if (NetworkManager.Singleton is not null)
        {
            _networkPrefabs.AddRange(NetworkManager.Singleton.NetworkConfig.Prefabs.Prefabs);
        }
        else
        {
            Debug.LogError("NetworkManager.Singleton is null. Make sure the NetworkManager is in the scene.");
        }
    }

    private void LoadProjectPrefabs()
    {
        _projectPrefabs.Clear();
        _projectPrefabFoldoutStates.Clear();

        string[] guids = AssetDatabase.FindAssets("t:Prefab");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab != null && prefab.GetComponent<NetworkObject>() != null)
            {
                _projectPrefabs.Add(prefab);
            }
        }
    }

    private void LoadSceneNetworkObjects()
    {
        _sceneNetworkObjects.Clear();
        _sceneObjectFoldoutStates.Clear();

        foreach (var obj in GameObject.FindObjectsByType<NetworkObject>(FindObjectsSortMode.None))
        {
            _sceneNetworkObjects.Add(obj.gameObject);
        }
    }

    private uint GetGlobalObjectIdHash(NetworkObject networkObject)
    {
        return _globalObjectIdHashField != null ? (uint)_globalObjectIdHashField.GetValue(networkObject) : 0;
    }
}
