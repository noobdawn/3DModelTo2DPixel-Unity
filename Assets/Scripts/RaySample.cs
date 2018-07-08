using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System.IO;

public class SampleInfo{
    public Vector3 Position;
    public Vector3 Normal;
    public Color Color;
}

public class RaySample : MonoBehaviour {
    [Header("图片设定")]
    public int SampleHeight = 50;
    public int SampleWidth = 200;
    public float PixelWidth = 0.1f;
    public float RayLength = 10f;
    public int SampleTime = 1;
    [Header("调试设定")]
    public bool ShowSampleRay;
    public bool ShowSampleArea;
    public RawImage PreviewImg;
    [Header("色调设定")]
    public bool ReplaceColor = false;
    public Color c001;
    public Color c010;
    public Color c100;
    public Color c011;
    public Color c110;
    public Color c101;
    public Color c000;
    public Color c111;
    [Header("渲染设定")]
    public bool ExportNormal = false;
    #region Mono
    void OnDrawGizmos()
    {
        float halfWidth = SampleWidth * 0.5f;
        if (ShowSampleRay)
        {
            for (int y = 0; y < SampleHeight; y++)
            for (int x = 0; x < SampleWidth; x++)
            {
                Gizmos.DrawLine(transform.position + new Vector3((x - halfWidth) * PixelWidth, y * PixelWidth, 0), transform.position + new Vector3((x - halfWidth) * PixelWidth, y * PixelWidth, RayLength));
            }
        }
        if (ShowSampleArea)
        {
            Gizmos.color = Color.red;
            Vector3 v000 = transform.position + new Vector3(-halfWidth * PixelWidth, 0, 0);
            Vector3 v001 = transform.position + new Vector3(-halfWidth * PixelWidth, 0, RayLength);
            Vector3 v010 = transform.position + new Vector3(-halfWidth * PixelWidth, SampleHeight * PixelWidth, 0);
            Vector3 v011 = transform.position + new Vector3(-halfWidth * PixelWidth, SampleHeight * PixelWidth, RayLength);
            Vector3 v100 = transform.position + new Vector3(halfWidth * PixelWidth, 0, 0);
            Vector3 v101 = transform.position + new Vector3(halfWidth * PixelWidth, 0, RayLength);
            Vector3 v110 = transform.position + new Vector3(halfWidth * PixelWidth, SampleHeight * PixelWidth, 0);
            Vector3 v111 = transform.position + new Vector3(halfWidth * PixelWidth, SampleHeight * PixelWidth, RayLength);
            Gizmos.DrawLine(v000, v001);
            Gizmos.DrawLine(v100, v101);
            Gizmos.DrawLine(v010, v011);
            Gizmos.DrawLine(v110, v111);
            Gizmos.DrawLine(v000, v010);
            Gizmos.DrawLine(v100, v110);
            Gizmos.DrawLine(v001, v011);
            Gizmos.DrawLine(v101, v111);
            Gizmos.DrawLine(v000, v100);
            Gizmos.DrawLine(v010, v110);
            Gizmos.DrawLine(v001, v101);
            Gizmos.DrawLine(v011, v111);
        }
    }

    int interval = 0;
    int refreshRate = 10;
    void FixedUpdate()
    {
        if (interval >= 10)
        {
            Texture2D previewTex = CreatePreview(CreateFrame(StartSample()));
            if (PreviewImg != null && previewTex != null)
            {
                previewTex.filterMode = FilterMode.Point;
                PreviewImg.rectTransform.sizeDelta = new Vector2(SampleWidth, SampleHeight) * 5;
                PreviewImg.texture = previewTex;
            }
            interval = 0;
        }
        interval++;
    }
    #endregion
    #region 采样
    SampleInfo[] StartSample()
    {
        SampleInfo[] res = new SampleInfo[SampleWidth * SampleHeight];
        for (int y = 0; y < SampleHeight; y++)
            for (int x = 0; x < SampleWidth; x++)
            {
                //如果只采样一次，就采样最中间的
                if (SampleTime == 1)
                    res[y * SampleWidth + x] = SampleColor(new Ray(transform.position + new Vector3((x - SampleWidth * 0.5f) * PixelWidth, y * PixelWidth, 0), Vector3.forward));
                else
                {
                    //法线、位置等信息采样最中间，颜色信息随机
                    res[y * SampleWidth + x] = SampleColor(new Ray(transform.position + new Vector3((x - SampleWidth * 0.5f) * PixelWidth, y * PixelWidth, 0), Vector3.forward));
                    if (res[y * SampleWidth + x] == null) continue;
                    for (int i = 0; i < SampleTime; i++ )
                    {
                        var t = SampleColor(new Ray(transform.position + new Vector3((x - SampleWidth * 0.5f + Random.Range(-0.5f, 0.5f)) * PixelWidth, (y + Random.Range(-0.5f, 0.5f)) * PixelWidth, 0), Vector3.forward));
                        if (t != null)
                            res[y * SampleWidth + x].Color += t.Color;
                    }
                    res[y * SampleWidth + x].Color /= (float)SampleTime;
                }
            }
        return res;
    }

    SampleInfo SampleColor(Ray ray)
    {
        var hits = Physics.RaycastAll(ray, RayLength);
        if (hits.Length == 0)
            return null;
        RaycastHit firstHit = hits[0];
        for (int i = 1; i < hits.Length; i++)
        {
            if (firstHit.point.z > hits[i].point.z)
                firstHit = hits[i];
        }
        if (firstHit.collider.gameObject.layer != 8)
            return null;
        SampleInfo resInfo = new SampleInfo();
        resInfo.Normal = firstHit.normal;
        resInfo.Position = firstHit.point;
        resInfo.Color = firstHit.collider.GetComponent<BodyPart>().GetColor(firstHit.textureCoord);
        #region 色调替换
        if (ReplaceColor)
        {
            if (resInfo.Color.r == 0)
            {
                if (resInfo.Color.g == 0)
                {
                    if (resInfo.Color.b == 0)
                    {
                        resInfo.Color = c000;
                    }
                    else if (resInfo.Color.b == 1)
                    {
                        resInfo.Color = c001;
                    }
                }
                else if (resInfo.Color.g == 1)
                {
                    if (resInfo.Color.b == 0)
                    {
                        resInfo.Color = c010;
                    }
                    else if (resInfo.Color.b == 1)
                    {
                        resInfo.Color = c011;
                    }
                }
            }
            else if (resInfo.Color.r == 1)
            {
                if (resInfo.Color.g == 0)
                {
                    if (resInfo.Color.b == 0)
                    {
                        resInfo.Color = c100;
                    }
                    else if (resInfo.Color.b == 1)
                    {
                        resInfo.Color = c101;
                    }
                }
                else if (resInfo.Color.g == 1)
                {
                    if (resInfo.Color.b == 0)
                    {
                        resInfo.Color = c110;
                    }
                    else if (resInfo.Color.b == 1)
                    {
                        resInfo.Color = c111;
                    }
                }
            }
        }
        #endregion
        return resInfo;
    }

    Color[] CreateNormal(SampleInfo[] infos)
    {
        Color[] colors = new Color[SampleWidth * SampleHeight];
        for (int i = 0; i < colors.Length; i++)
        {
            if (infos[i] != null)
            {
                colors[i] = new Color(
                    infos[i].Normal.x,
                    infos[i].Normal.y,
                    infos[i].Normal.z,
                    1);
            }
            else
                colors[i] = new Color(0, 0, 0, 0);
        }
        return colors;
    }

    Color[] CreateFrame(SampleInfo[] infos)
    {
        Color[] colors = new Color[SampleWidth * SampleHeight];
        for (int i = 0; i < colors.Length; i++)
        {
            if (infos[i] != null)
                colors[i] = infos[i].Color;
            else
                colors[i] = new Color(0, 0, 0, 0);
        }
        return colors;
    }
    #endregion
    #region 输出

    ///输出到预览
    Texture2D CreatePreview(Color[] colors)
    {
        if (SampleWidth * SampleHeight != colors.Length)
        {
            Debug.LogError("长宽与数组长度无法对应！");
            return null;
        }
        Texture2D tex = new Texture2D(SampleWidth, SampleHeight, TextureFormat.ARGB32, false);
        tex.SetPixels(colors);
        tex.Apply();
        return tex;
    }

    /// 输出到文件
    void CreatePng(Color[] colors, string path)
    {
        if (SampleWidth * SampleHeight != colors.Length)
        {
            Debug.LogError("长宽与数组长度无法对应！");
            return;
        }
        Texture2D tex = new Texture2D(SampleWidth, SampleHeight, TextureFormat.ARGB32, false);
        tex.filterMode = FilterMode.Point;
        tex.alphaIsTransparency = true;
        tex.SetPixels(colors);
        tex.Apply();
        byte[] bytes = tex.EncodeToPNG();
        FileStream fs = new FileStream(path, FileMode.Create);
        BinaryWriter bw = new BinaryWriter(fs);
        bw.Write(bytes);
        fs.Close();
        bw.Close();
    }
    #endregion
    #region 开放给UI的操作接口
    public void ExportCurrent(string roleName, string name)
    {
        string dir = Application.dataPath + "/Export/" + roleName;
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        var t = StartSample();
        CreatePng(CreateFrame(t), dir + "/" + name + ".png");
        if (ExportNormal)
        {
            CreatePng(CreateNormal(t), dir + "/" + name + "_n.png");
        }
    }
    #endregion
}
