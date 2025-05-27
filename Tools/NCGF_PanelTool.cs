using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//[][] Panel Tool
//[][] Creates UI panel backgrounds as tiled or stretched meshes
[ExecuteAlways]
public class NCGF_PanelTool : MonoBehaviour
{
    [SerializeField] private NCGF_Pools                 _pools;
    [SerializeField] [Range(0f, 0.5f)] private float    _uvInsetVert    = 0.5f;
    [SerializeField] [Range(0f, 0.5f)] private float    _uvInsetHoriz   = 0.5f;

    [SerializeField] private float                      _height;
    [SerializeField] private float                      _width;

    [SerializeField] private Material                   _bgMaterial;
    [SerializeField] private Vector2                    _bgMatPixelsWH = Vector2.one;
    [SerializeField] private int                        _bgMatPixelsPerUnit = 1;

    [SerializeField] private bool _go       = false;
    [SerializeField] private bool _lock     = true;

    private List<Vector3>   _verts  = new List<Vector3>();
    private List<Vector2>   _uvs    = new List<Vector2>();
    private List<int>       _tris   = new List<int>();

    private void Update()
    {
        if (_go && !_lock)
        {
            _go = false;
            MakeMesh();
            gameObject.SetActive(false);
        }
    }
    private void MakeMesh()
    {
        if (_pools == null) return;
        if (_bgMaterial == null) return;
        if (_width <= 0 || _height <= 0) return;
        if (_bgMatPixelsPerUnit == 0) return;

        float matUnitPerPx = 1f / _bgMatPixelsPerUnit;

        Vector2 minSize = new Vector2(2 * _uvInsetHoriz * _bgMatPixelsWH.x, 2 * _uvInsetVert * _bgMatPixelsWH.y) * matUnitPerPx;
        if (minSize.x > _width || minSize.y > _height)
        {
            Debug.Log($"PanelTool: Cannot generate size {_width:0.000}, {_height:0.000} as minimum size is calc'd at {minSize.x:0.000}, {minSize.y:0.000}.");
            return;
        }

        Vector2 vertLLC     = new Vector2(_width, _height) * -0.5f;
        Vector2 vertLLCIns  = vertLLC + (minSize * 0.5f);
        Vector2 uvBias      = Vector2.one * 0.5f;
        Vector2 uvLLC       = -uvBias;
        Vector2 uvLLCIns    = new Vector2(uvLLC.x + _uvInsetHoriz, uvLLC.y + _uvInsetVert);

        Vector2 vertULC     = new Vector2(vertLLC.x, -vertLLC.y);
        Vector2 vertULCIns  = new Vector2(vertLLCIns.x, -vertLLCIns.y);
        Vector2 uvULC       = new Vector2(uvLLC.x, -uvLLC.y);
        Vector2 uvULCIns    = new Vector2(uvLLCIns.x, -uvLLCIns.y);

        AddParts(vertLLC, vertLLCIns, uvLLC + uvBias, uvLLCIns + uvBias, false);
        AddParts(-vertLLC, -vertLLCIns, -uvLLC + uvBias, -uvLLCIns + uvBias, false);
        AddParts(vertULC, vertULCIns, uvULC + uvBias, uvULCIns + uvBias, true);
        AddParts(-vertULC, -vertULCIns, -uvULC + uvBias, -uvULCIns + uvBias, true);

        AddParts(new Vector2(vertLLC.x, vertLLCIns.y), vertULCIns, new Vector2(uvLLC.x, uvLLCIns.y) + uvBias, uvULCIns + uvBias, false);
        AddParts(new Vector2(vertULCIns.x, vertULC.y), -vertLLCIns, new Vector2(uvULCIns.x, uvULC.y) + uvBias, -uvLLCIns + uvBias, true);
        AddParts(new Vector2(-vertLLC.x, -vertLLCIns.y), -vertULCIns, new Vector2(-uvLLC.x, -uvLLCIns.y) + uvBias, -uvULCIns + uvBias, false);
        AddParts(new Vector2(-vertULCIns.x, -vertULC.y), vertLLCIns, new Vector2(-uvULCIns.x, -uvULC.y) + uvBias, uvLLCIns + uvBias, true);

        AddParts(vertLLCIns, -vertLLCIns, uvLLCIns + uvBias, -uvLLCIns + uvBias, false);

        var bg = _pools.Obtain(typeof(GO_MeshVisual), false) as GO_MeshVisual;
        if (bg == null) return;
        bg.Clear();
        bg.name = "Panel Background";
        bg.transform.position = Vector3.zero;

        bg.SetMaterial(_bgMaterial);
        bg.RenderMesh(_verts, _uvs, _tris);

        _verts.Clear();
        _uvs.Clear();
        _tris.Clear();
    }
    private void AddParts(Vector2 vertC, Vector2 vertOpC, Vector2 uvC, Vector2 uvOpC, bool reverse)
    {
        int ct = _verts.Count;
        _verts.Add(vertC);
        _verts.Add(reverse ? new Vector2(vertOpC.x, vertC.y) : new Vector2(vertC.x, vertOpC.y));
        _verts.Add(vertOpC);
        _verts.Add(reverse ? new Vector2(vertC.x, vertOpC.y) : new Vector2(vertOpC.x, vertC.y));
        _uvs.Add(uvC);
        _uvs.Add(reverse ? new Vector2(uvOpC.x, uvC.y) : new Vector2(uvC.x, uvOpC.y));
        _uvs.Add(uvOpC);
        _uvs.Add(reverse ? new Vector2(uvC.x, uvOpC.y): new Vector2(uvOpC.x, uvC.y));
        _tris.AddRange(new List<int> { ct, ct + 1, ct + 2, ct, ct + 2, ct + 3 });
    }
}
