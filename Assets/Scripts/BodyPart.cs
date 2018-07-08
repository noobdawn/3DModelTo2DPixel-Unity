using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 用于统一纯色和使用纹理的颜色采样用的
/// </summary>
[RequireComponent(typeof(Renderer))]
public class BodyPart : MonoBehaviour {
    public Color Color;
    public Material Mat;
    public Texture2D Texture;       
    public bool isPureColor;        //是否纯色

    private Renderer mr;
    private Material myMat;
    private void Awake()
    {
        myMat = Instantiate(Mat) as Material;
        mr = GetComponent<Renderer>();
        mr.material = myMat;
    }

    private void Update()
    {
        mr.material.SetColor("_Color", Color);
        mr.material.mainTexture = Texture;
    }

    internal UnityEngine.Color GetColor(Vector2 texcoord)
    {
        if (isPureColor)
            return Color;
        else
            return ((Texture2D)(mr.material.mainTexture)).GetPixelBilinear(texcoord.x, texcoord.y);
    }
}
