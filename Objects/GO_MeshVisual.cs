using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[][] GameObject - Mesh Visual
//[][] Provides functionality in rendering visual objects as meshes

public class GO_MeshVisual : MonoBehaviour
{
    [SerializeField] protected MeshRenderer   _meshRenderer;
    [SerializeField] protected MeshFilter     _meshFilter;
    [SerializeField] protected bool           _forceReportVerts;

    private Mesh _sharedMesh;

    private static readonly Vector3[] r_vertClear = new Vector3[] { };
    private static readonly Vector2[] r_UVClear = new Vector2[] { };
    private static readonly int[] r_triClear = new int[] { };

    private bool _doRandomColors = false;
    private bool _doBlackGradient = false;

    private bool _isSetUp = false;

    //[][] Auto Functions
    private void Awake()
    {
        Setup();
    }
    private void Update()
    {
        if (_forceReportVerts)
        {
            _forceReportVerts = false;

            string s = "";
            Vector3[] x = _sharedMesh.vertices;
            for (int i = 0; i < x.Length; i++)
            {
                if (i % 4 == 0) s += '\n';
                s += ((Vector2)x[i]).ToString("F4") + "; ";
            }
            Debug.Log("Vertex Report:" + s);
        }
    }

    //[][] Public Functions
    public void SetRandomColorMode(bool doRandomColors, bool doBlackGradient) { _doRandomColors = doRandomColors; _doBlackGradient = doBlackGradient; }
    public void SetMaterial(Material mat) { if (_meshRenderer != null) _meshRenderer.material = mat; }
    public void SetColor(Color32 color)
    {
        if (_sharedMesh == null) return;

        List<Color32> list = new List<Color32>();
        for (int i = 0; i < _sharedMesh.vertexCount; i++) list.Add(color);
        _sharedMesh.SetColors(list);
    }
    public void RenderMesh(List<Vector3> verts, List<Vector2> UVs, List<int> tris)
    {
        ClearSharedMesh();
        int vertCount = verts.Count;
        if (vertCount > 65535) vertCount = 65535;
        if (UVs.Count < vertCount)
        {
            for (int i = UVs.Count; i < vertCount; i++) UVs.Add(UVs[i - 1]);
        }
        _sharedMesh.vertices = verts.ToArray();
        _sharedMesh.uv = UVs.ToArray();
        _sharedMesh.triangles = tris.ToArray();
        if (_doRandomColors) _sharedMesh.SetColors(RandomColorList(vertCount));
        _sharedMesh.RecalculateNormals();
    }
    public void Clear() => ClearSharedMesh();

    //[][] Private Functions
    private void Setup()
    {
        if (_isSetUp) { SetColor(Color.white); return; }

        if (_meshRenderer == null)
        {
            _meshRenderer = GetComponent<MeshRenderer>();
            if (_meshRenderer == null) _meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }

        if (_meshFilter == null)
        {
            _meshFilter = GetComponent<MeshFilter>();
            if (_meshFilter == null) _meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        _sharedMesh = _meshFilter.sharedMesh;
        if (_sharedMesh == null)
        {
            _meshFilter.mesh = new Mesh();
            _sharedMesh = _meshFilter.sharedMesh;
        }

        _isSetUp = true;
    }
    private void ClearSharedMesh()
    {
        Setup();

        _sharedMesh.triangles = r_triClear;
        _sharedMesh.uv = r_UVClear;
        _sharedMesh.vertices = r_vertClear;
    }
    private List<Color32> RandomColorList(int len)
    {
        float perc = 1f / len;
        Color32 black = new Color32(0, 0, 0, 255);
        Color32 randCol = new Color32(
            (byte)(256 * Random.Range(0.3f, 1f)),
            (byte)(256 * Random.Range(0.3f, 1f)),
            (byte)(256 * Random.Range(0.3f, 1f)),
            255);
        List<Color32> retVal = new List<Color32>();
        for (int i = 0; i < len; i++)
        {
            retVal.Add(_doBlackGradient ? Color32.Lerp(randCol, black, perc * i) : randCol);
        }
        return retVal;
    }
}
