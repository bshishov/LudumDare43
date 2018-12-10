using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class MapDrawerWindow : EditorWindow
{
    public static Type HandleUtilityType;

    public static MethodInfo IntersectRayMeshMethodInfo
    {
        get
        {
            if (_intersectRayMeshMethodInfo == null)
            {
                var editorTypes = typeof(Editor).Assembly.GetTypes();

                HandleUtilityType = editorTypes.FirstOrDefault(t => t.Name == "HandleUtility");
                if (HandleUtilityType != null)
                    _intersectRayMeshMethodInfo = HandleUtilityType.GetMethod("IntersectRayMesh", (BindingFlags.Static | BindingFlags.NonPublic));
            }

            return _intersectRayMeshMethodInfo;
        }
    }

    private static MethodInfo _intersectRayMeshMethodInfo;

    public static bool IntersectRayMesh(Ray ray, MeshFilter meshFilter, out RaycastHit hit)
    {
        return IntersectRayMesh(ray, meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, out hit);
    }

    public static bool IntersectRayMesh(Ray ray, Mesh mesh, Matrix4x4 matrix, out RaycastHit hit)
    {
        var parameters = new object[] { ray, mesh, matrix, null };
        bool result = (bool)IntersectRayMeshMethodInfo.Invoke(null, parameters);
        hit = (RaycastHit)parameters[3];
        return result;
    }

    // Add menu named "My Window" to the Window menu
    [MenuItem("Window/Map drawer")]
    static void Init()
    {
        var window = (MapDrawerWindow)EditorWindow.GetWindow(typeof(MapDrawerWindow));
        window.Show();
        window.wantsMouseMove = true;
        window.autoRepaintOnSceneChange = true;

        window._drawerMaterial = new Material(Shader.Find("Hidden/TextureDrawer"));
    }
    
    
    private Vector2 _uv;
    private bool _isDirty = false;
    private RenderTexture _renderTexture;
    private RenderTexture _renderTexture2;

    private MeshFilter _fieldMeshFilter;
    private MeshRenderer _fieldMeshRenderer;
    private Texture2D _fieldTexture2D;
    private int _fieldWidth = 512;
    private int _fieldHeight = 512;
    private RenderTextureFormat _fieldFmt = RenderTextureFormat.ARGB32;

    private float _paintBrushSize = 1f;
    private float _paintBrushStrength = 1f;
    private Color _paintBrushColor = Color.white;
    private Material _drawerMaterial;


    void OnGUI()
    {
        _fieldMeshFilter =
            EditorGUILayout.ObjectField("Mesh", _fieldMeshFilter, typeof(MeshFilter), allowSceneObjects: true) as MeshFilter;
        
        _fieldMeshRenderer =
            EditorGUILayout.ObjectField("Mesh Renderer", _fieldMeshRenderer, typeof(MeshRenderer), allowSceneObjects: true) as MeshRenderer;

        EditorGUILayout.Vector2Field("UV coordinates", _uv);

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        if (GUILayout.Button("Load"))
        {
            CreateRenderTextureFromExisting(_fieldTexture2D);
        }
        _fieldTexture2D =
            EditorGUILayout.ObjectField(_fieldTexture2D, typeof(Texture2D), allowSceneObjects: false) as Texture2D;
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        if (GUILayout.Button("New"))
        {
            CreateRenderTexture(_fieldWidth, _fieldHeight);
        }
        _fieldWidth = Mathf.Max(16, EditorGUILayout.IntField("Width", _fieldWidth));
        _fieldHeight = Mathf.Max(16, EditorGUILayout.IntField("Height", _fieldHeight));
        _fieldFmt = (RenderTextureFormat)EditorGUILayout.EnumFlagsField("Format", _fieldFmt);

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();



        GUILayout.BeginVertical();
        _paintBrushSize = EditorGUILayout.Slider("Brush size", _paintBrushSize, 0f, 1f);
        _paintBrushStrength = EditorGUILayout.Slider("Brush strength", _paintBrushStrength, 0f, 2f);
        _paintBrushColor = EditorGUILayout.ColorField("Color", _paintBrushColor);
        GUILayout.EndVertical();



        if (_renderTexture != null)
        {
            var bounds = new Rect(10, 300, 512, 512);
            EditorGUI.DrawPreviewTexture(bounds, _renderTexture, null, ScaleMode.ScaleToFit);

            var centerX = bounds.xMin + bounds.size.x * _uv.x;
            var centerY = bounds.yMin + bounds.size.y * (1 - _uv.y);

            EditorGUI.DrawRect(new Rect(centerX - 4f, centerY - 4f, 8f, 8f), Color.black);
            EditorGUI.DrawRect(new Rect(centerX - 2f, centerY - 2f, 4f, 4f), Color.white);
        }

        if (_fieldTexture2D != null)
        {
            if (Event.current.control)
            {
                Draw();
                _isDirty = true;
            }
            else
            {
                if (_isDirty)
                {
                    //_fieldTexture2D.Apply(true);
                    _isDirty = false;
                }
            }
        }

        if (_fieldMeshRenderer != null)
        {
            //_fieldMeshRenderer
        }
    }

    void Draw()
    {
        if(_drawerMaterial == null)
            return;

        if(_renderTexture == null || _renderTexture2 == null)
            return;

        _drawerMaterial.mainTexture = _renderTexture;
        _drawerMaterial.SetFloat("_X", _uv.x);
        _drawerMaterial.SetFloat("_Y", _uv.y);
        _drawerMaterial.SetColor("_Color", _paintBrushColor);
        _drawerMaterial.SetFloat("_Radius", _paintBrushSize);
        _drawerMaterial.SetFloat("_Strength", _paintBrushStrength);

        Graphics.SetRenderTarget(_renderTexture2);
        Graphics.Blit(_renderTexture, _renderTexture2, _drawerMaterial, 0);
        Graphics.Blit(_renderTexture2, _renderTexture);
        Graphics.SetRenderTarget(null);
    }


    void CreateRenderTextureFromExisting(Texture2D tex)
    {
        if (_renderTexture != null)
            _renderTexture.Release();

        if (_renderTexture2 != null)
            _renderTexture2.Release();

        var descriptor =
            new RenderTextureDescriptor(tex.width, tex.height)
            {
                enableRandomWrite = true,
                useMipMap = true
            };

        _renderTexture = new RenderTexture(descriptor)
        {
            wrapMode = tex.wrapMode,
            anisoLevel = tex.anisoLevel,
            filterMode = tex.filterMode
        };
        _renderTexture.Create();

        _renderTexture2 = new RenderTexture(descriptor)
        {
            wrapMode = tex.wrapMode,
            anisoLevel = tex.anisoLevel,
            filterMode = tex.filterMode
        };
        _renderTexture2.Create();

        // Copy contents to RT
        Graphics.Blit(tex, _renderTexture);
        Graphics.Blit(tex, _renderTexture2);
    }

    void CreateRenderTexture(int width=512, int height=512, RenderTextureFormat fmt=RenderTextureFormat.ARGB32)
    {
        if (_renderTexture != null)
            _renderTexture.Release();

        if (_renderTexture2 != null)
            _renderTexture2.Release();

        var descriptor =
            new RenderTextureDescriptor(Mathf.ClosestPowerOfTwo(width), Mathf.ClosestPowerOfTwo(height), fmt)
            {
                enableRandomWrite = true,
                useMipMap = true
            };

        _renderTexture = new RenderTexture(descriptor) { wrapMode = TextureWrapMode.Clamp };
        _renderTexture.Create();

        _renderTexture2 = new RenderTexture(descriptor) { wrapMode = TextureWrapMode.Clamp };
        _renderTexture2.Create();
    }

    void OnEnable()
    {
        SceneView.onSceneGUIDelegate += this.OnSceneGUI;
    }

    void OnDisable()
    {
        SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
        if (_renderTexture != null)
        {
            _renderTexture.Release();
            _renderTexture = null;
        }

        if (_renderTexture2 != null)
        {
            _renderTexture2.Release();
            _renderTexture2 = null;
        }
    }

    void OnSceneGUI(SceneView sceneView)
    {
        var triangleIndex = -1;
        var baricentricCoords = Vector3.zero;

        if (_fieldMeshFilter != null)
        {
            var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
            RaycastHit hit;
            if (IntersectRayMesh(ray, _fieldMeshFilter, out hit))
            {
                Handles.DrawWireDisc(hit.point, hit.normal, _paintBrushSize * 5f);
                triangleIndex = hit.triangleIndex;
                baricentricCoords = hit.barycentricCoordinate;

                var mesh = _fieldMeshFilter.sharedMesh;
                var triangles = mesh.triangles;
                var uvs = mesh.uv;

                // Indices of triangle points
                var p0i = triangles[hit.triangleIndex * 3 + 0];
                var p1i = triangles[hit.triangleIndex * 3 + 1];
                var p2i = triangles[hit.triangleIndex * 3 + 2];

                // uv coordinates in points
                var uv0 = uvs[p0i];
                var uv1 = uvs[p1i];
                var uv2 = uvs[p2i];

                _uv = uv2 * baricentricCoords.x + uv0 * baricentricCoords.y + uv1 * baricentricCoords.z;

                /*
                var vertices = mesh.vertices;
                var p0 = vertices[p0i];
                var p1 = vertices[p1i];
                var p2 = vertices[p2i];

                Handles.DrawWireDisc(_fieldMeshFilter.transform.TransformPoint(p0), Vector3.up, _paintBrushSize * 0.2f);
                Handles.DrawWireDisc(_fieldMeshFilter.transform.TransformPoint(p1), Vector3.up, _paintBrushSize * 0.2f);
                Handles.DrawWireDisc(_fieldMeshFilter.transform.TransformPoint(p2), Vector3.up, _paintBrushSize * 0.2f);
                */

                Repaint();
            }
        }

        

        // Do your drawing here using Handles.
        Handles.BeginGUI();
        GUI.Label(new Rect(10, 10, 200, 20), string.Format("Triangle index: {0}", triangleIndex));
        GUI.Label(new Rect(10, 30, 200, 20), string.Format("UV X: {0}", _uv.x));
        GUI.Label(new Rect(10, 50, 200, 20), string.Format("UV Y: {0}", _uv.y));
        GUI.Label(new Rect(10, 70, 200, 20), string.Format("Baricentric: {0}", baricentricCoords));
        Handles.EndGUI();
    }


    void Update()
    {
        
    }
}
