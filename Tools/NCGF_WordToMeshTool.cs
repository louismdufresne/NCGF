using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class NCGF_WordToMeshTool : MonoBehaviour
{
    [SerializeField] private GO_Word        _wordReference;
    [SerializeField] private GO_MeshVisual  _meshVisPrefab;
    [SerializeField] private Material       _lettersMaterial;
    [SerializeField] private Material       _outlineMaterial;

    public bool _makeMeshWord = false;

    private void Update()
    {
        if (!_makeMeshWord) return;
        _makeMeshWord = false;

        if (_wordReference == null) { Debug.Log("Word reference is null!"); return; }
        if (_meshVisPrefab == null) { Debug.Log("Mesh visual prefab is null!"); return; }
        if (_lettersMaterial == null) { Debug.Log("Material is null!"); return; }
        if (_wordReference.GetWord() == "") { Debug.Log("Word is zero characters!"); return; }

        var verts = new List<Vector3>();
        var uvs = new List<Vector2>();
        var tris = new List<int>();

        GO_MeshVisual meshWord, meshOutline;
        meshWord = Instantiate(_meshVisPrefab);
        var letterSprites = _wordReference.GetVisuals();

        if (letterSprites[1].Count != 0)    meshOutline = Instantiate(_meshVisPrefab);
        else                                meshOutline = null;

        Vector2[] curVerts, curUVs; ushort[] curTris;

        GO_Visual x;
        Vector3 loc;
        for (int g = 0; g < letterSprites.Length; g++)
        {
            for (int h = 0; h < letterSprites[g].Count; h++)
            {
                x = letterSprites[g][h];
                loc = x.transform.position - _wordReference.transform.position;
                curVerts = x._spriteRenderer.sprite.vertices;
                curUVs = x._spriteRenderer.sprite.uv;
                curTris = x._spriteRenderer.sprite.triangles;
                for (int i = 0; i < curVerts.Length; i++)
                    verts.Add(new Vector3(curVerts[i].x + loc.x, curVerts[i].y + loc.y, x.transform.position.z));
                for (int i = 0; i < curUVs.Length; i++) uvs.Add(curUVs[i]);
                for (int i = 0; i < curTris.Length; i++) tris.Add(curTris[i] + (4 * h));
            }
        }

        int amt = letterSprites[0].Count;
        int amt4 = amt * 4;
        int amt6 = amt * 6;
        meshWord.SetMaterial(_lettersMaterial);
        meshWord.RenderMesh(
            verts.GetRange(0, amt4),
            uvs.GetRange(0, amt4),
            tris.GetRange(0, amt6));
        meshWord.transform.position = Vector3.zero;
        meshWord.name = _wordReference.GetWord() + " Mesh";

        if (meshOutline != null)
        {
            meshOutline.SetMaterial(_outlineMaterial);

            meshOutline.RenderMesh(
                verts.GetRange(amt4, verts.Count - amt4),
                uvs.GetRange(amt4, uvs.Count - amt4),
                tris.GetRange(amt6, tris.Count - amt6));

            meshOutline.transform.position = meshWord.transform.position + new Vector3(0, 0, NCGF_UI_R_Parameters._textOutlineZOffset);
            meshOutline.transform.parent = meshWord.transform;
            meshOutline.name = meshWord.name + " Outline";
        }
    }
}
