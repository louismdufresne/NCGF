using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] Resource GameObject - Pools
//[][] Provides an implementation of a basic, generic object pooling and retrieval system

public class NCGF_Pools : MonoBehaviour
{
    private readonly Dictionary<System.Type, List<object>> _pools = new Dictionary<System.Type, List<object>>();
    [SerializeField] private List<MonoBehaviour> _prefabs;

    // Keeping
    private static System.Type  s_type;
    private static List<object> s_list;
    private static bool         s_isMono;

    // Singleton
    public static NCGF_Pools Pools;

    // Etc
    private bool                _ISU = false;

    //[][] Auto and Setup
    private void Awake()
    {
        SetUp();
    }
    public void SetUp()
    {
        if (_ISU) return;

        if (Pools != null) Destroy(Pools);
        Pools = this;

        // NCGF ONLY!
        NCGF_Res._pools = this;

        foreach (var x in _prefabs)
        {
            LabelAsPrefab(x);
        }

        _ISU = true;
    }

    //[][] Public Functions
    public void Pool(object toPool)
    {
        if (toPool == null) { Debug.Log("NCGF_Pools.Pool: passed null reference!"); return; }
        s_type = toPool.GetType();
        s_isMono = s_type.IsSubclassOf(typeof(MonoBehaviour)) || s_type == typeof(MonoBehaviour);

        if (!_pools.TryGetValue(s_type, out s_list))
        {
            s_list = new List<object>();
            _pools.Add(s_type, s_list);
            if (s_isMono) CreateExample((MonoBehaviour)toPool);
        }
        s_list.Add(toPool);
        if (s_isMono) CleanGameObject(((MonoBehaviour)toPool).gameObject);
    }
    public bool Has(System.Type type)
    {
        return (_pools.TryGetValue(type, out s_list) ? (s_list.Count != 0) : false);
    }
    public object Obtain(System.Type type, bool suppressCreateNew)
    {
        s_isMono = type.IsSubclassOf(typeof(MonoBehaviour)) || type == typeof(MonoBehaviour);
        object retVal = null;

        // Try to find existing
        if (_pools.TryGetValue(type, out s_list))
        {
            for (int i = s_list.Count - 1; i >= 0; i--) if (s_list[i] == null) s_list.RemoveAt(i);  // Cleaning
            if (s_list.Count != 0)
            {
                retVal = s_list[s_list.Count - 1];
                s_list.RemoveAt(s_list.Count - 1);
                if (s_isMono) StartGameObject(((MonoBehaviour)retVal).gameObject);
                return retVal;
            }
        }

        // Else, make new, if applicable
        if (suppressCreateNew) return retVal;

        if (s_isMono)
        {
            retVal = MakeIfHasPrefab(type);
            ((MonoBehaviour)retVal).name = type.ToString();
        }
        else if (!type.IsValueType)
        {
            if (type.GetConstructor(System.Type.EmptyTypes) != null) retVal = System.Activator.CreateInstance(type);
        }
        return retVal;
    }
    public bool PopulatePool(System.Type type, int amount)
    {
        if (amount <= 0) return false;
        if (_pools.ContainsKey(type)) if (_pools[type].Count >= amount) return false;

        s_isMono = type.IsSubclassOf(typeof(MonoBehaviour)) || type == typeof(MonoBehaviour);
        if (!s_isMono)
        {
            if (type.IsValueType) return false;
            if (type.GetConstructor(System.Type.EmptyTypes) == null) return false;

            for (int i = 0; i < amount; i++) Pool(System.Activator.CreateInstance(type));
            return true;
        }

        MonoBehaviour template = null;
        for (int i = 0; i < _prefabs.Count; i++)
        {
            if (_prefabs[i].GetType() == type) template = _prefabs[i];
        }
        if (template == null) return false;

        for (int i = 0; i < amount; i++) Pool(Instantiate(template));
        return true;
    }

    //[][] Private Functions
    private object MakeIfHasPrefab(System.Type type)
    {
        for (int i = 0; i < _prefabs.Count; i++)
        {
            if (_prefabs[i].GetType() == type) return Instantiate(_prefabs[i]);
        }
        return null;
    }
    private void CleanGameObject(GameObject toClean)
    {
        if (toClean == null) return;
        toClean.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        toClean.transform.parent = transform;
        toClean.transform.localScale = Vector3.one;
        toClean.SetActive(false);
    }
    private void StartGameObject(GameObject toStart)
    {
        toStart.SetActive(true);
    }
    private void CreateExample(MonoBehaviour like)
    {
        var type = like.GetType();
        for (int i = 0; i < _prefabs.Count; i++)
        {
            if (_prefabs[i].GetType() == type) return;
        }

        var ex1 = Instantiate(like);
        ex1.gameObject.SetActive(false);
        LabelAsPrefab(ex1);
        _prefabs.Add(ex1);     
    }
    private void LabelAsPrefab(MonoBehaviour x)
    {
        x.name = x.GetType().Name + " Prefab";
    }
    private void OnDestroy()
    {
        foreach (var x in _pools)
        {
            x.Value.Clear();
        }
        _pools.Clear();
        _prefabs.Clear();
    }
}