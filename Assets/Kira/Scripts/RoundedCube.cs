﻿using UnityEngine;

namespace Kira
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoundedCube : MonoBehaviour
    {
        [SerializeField, Header("Mesh")]
        private int xSize = 4, zSize = 4, ySize = 2;

        [SerializeField]
        private int roundness = 2;

        [Header("Display"), SerializeField, Range(0.01f, 0.5f)]
        private float gizmoRadius = 0.1f;
        [SerializeField, Range(0.05f, 2f)]
        private float centerRadius = 1f;

        [SerializeField]
        private Color centerColor = Color.magenta;

        [SerializeField]
        private Color gizmoColor = Color.white;

        [SerializeField]
        private Color rayColor = Color.green;

        [SerializeField]
        private bool displayCenterDebug = true;

        [SerializeField]
        private bool dislayGizmosDebug = true;

        [SerializeField]
        private bool displayNormalsDebug;

        private Mesh mesh;
        private Vector3[] vertices;
        private Vector3[] normals;

        private void Awake()
        {
            Generate();
        }

        private void Generate()
        {
            GetComponent<MeshFilter>().mesh = mesh = new Mesh();
            mesh.name = "Procedural Cube";
            CreateVertices();
            CreateTriangles();
        }

        private void CreateVertices()
        {
            int cornerVertices = 8;
            int edgeVertices = (xSize + ySize + zSize - 3) * 4;
            int faceVertices = (
                (xSize - 1) * (ySize - 1) +
                (xSize - 1) * (zSize - 1) +
                (ySize - 1) * (zSize - 1)
            ) * 2;

            vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
            normals = new Vector3[vertices.Length];


            int v = 0;

            for (int y = 0; y <= ySize; y++)
            {
                for (int x = 0; x <= xSize; x++)
                {
                    SetVertex(v++, x, y, 0);
                }

                for (int z = 1; z <= zSize; z++)
                {
                    SetVertex(v++, xSize, y, z);
                }

                for (int x = xSize - 1; x >= 0; x--)
                {
                    SetVertex(v++, x, y, zSize);
                }

                for (int z = zSize - 1; z > 0; z--)
                {
                    SetVertex(v++, 0, y, z);
                }
            }

            for (int z = 1; z < zSize; z++)
            {
                for (int x = 1; x < xSize; x++)
                {
                    SetVertex(v++, x, ySize, z);
                }
            }

            for (int z = 1; z < zSize; z++)
            {
                for (int x = 1; x < xSize; x++)
                {
                    SetVertex(v++, x, 0, z);
                }
            }


            mesh.vertices = vertices;
            mesh.normals = normals;
        }


        private void SetVertex(int i, int x, int y, int z)
        {
            Vector3 inner = vertices[i] = new Vector3(x, y, z);

            if (x < roundness)
            {
                inner.x = roundness;
            }
            else if (x > xSize - roundness)
            {
                inner.x = xSize - roundness;
            }

            if (y < roundness)
            {
                inner.y = roundness;
            }
            else if (y > ySize - roundness)
            {
                inner.y = ySize - roundness;
            }

            if (z < roundness)
            {
                inner.z = roundness;
            }
            else if (z > zSize - roundness)
            {
                inner.z = zSize - roundness;
            }

            normals[i] = (vertices[i] - inner).normalized;
            vertices[i] = inner + normals[i] * roundness;

            // vertices[i] = new Vector3(x, y, z);
        }

        private void CreateTriangles()
        {
            int[] trianglesZ = new int[(xSize * ySize) * 12];
            int[] trianglesX = new int[(ySize * zSize) * 12];
            int[] trianglesY = new int[(xSize * zSize) * 12];

            int ring = (xSize + zSize) * 2;
            int tZ = 0, tX = 0, tY = 0, v = 0;


            for (int y = 0; y < ySize; y++, v++)
            {
                for (int q = 0; q < xSize; q++, v++)
                {
                    tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                }

                for (int q = 0; q < zSize; q++, v++)
                {
                    tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                }

                for (int q = 0; q < xSize; q++, v++)
                {
                    tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
                }

                for (int q = 0; q < zSize - 1; q++, v++)
                {
                    tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
                }

                tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
            }

            tY = CreateTopFace(trianglesY, tY, ring);
            tY = CreateBottomFace(trianglesY, tY, ring);

            mesh.subMeshCount = 3;
            mesh.SetTriangles(trianglesZ, 0);
            mesh.SetTriangles(trianglesX, 1);
            mesh.SetTriangles(trianglesY, 2);
        }

        private int CreateTopFace(int[] triangles, int t, int ring)
        {
            int v = ring * ySize;
            for (int x = 0; x < xSize - 1; x++, v++)
            {
                t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + ring);
            }

            t = SetQuad(triangles, t, v, v + 1, v + ring - 1, v + 2);

            int vMin = ring * (ySize + 1) - 1;
            int vMid = vMin + 1;
            int vMax = v + 2;

            for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
            {
                t = SetQuad(triangles, t, vMin, vMid, vMin - 1, vMid + xSize - 1);
                for (int x = 1; x < xSize - 1; x++, vMid++)
                {
                    t = SetQuad(
                        triangles, t,
                        vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
                }

                t = SetQuad(triangles, t, vMid, vMax, vMid + xSize - 1, vMax + 1);
            }

            int vTop = vMin - 2;
            t = SetQuad(triangles, t, vMin, vMid, vTop + 1, vTop);
            for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, vTop, vTop - 1);
            }

            t = SetQuad(triangles, t, vMid, vTop - 2, vTop, vTop - 1);
            return t;
        }

        private int CreateBottomFace(int[] triangles, int t, int ring)
        {
            int v = 1;
            int vMid = vertices.Length - (xSize - 1) * (zSize - 1);
            t = SetQuad(triangles, t, ring - 1, vMid, 0, 1);
            for (int x = 1; x < xSize - 1; x++, v++, vMid++)
            {
                t = SetQuad(triangles, t, vMid, vMid + 1, v, v + 1);
            }

            t = SetQuad(triangles, t, vMid, v + 2, v, v + 1);

            int vMin = ring - 2;
            vMid -= xSize - 2;
            int vMax = v + 2;

            for (int z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
            {
                t = SetQuad(triangles, t, vMin, vMid + xSize - 1, vMin + 1, vMid);
                for (int x = 1; x < xSize - 1; x++, vMid++)
                {
                    t = SetQuad(
                        triangles, t,
                        vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
                }

                t = SetQuad(triangles, t, vMid + xSize - 1, vMax + 1, vMid, vMax);
            }

            int vTop = vMin - 1;
            t = SetQuad(triangles, t, vTop + 1, vTop, vTop + 2, vMid);
            for (int x = 1; x < xSize - 1; x++, vTop--, vMid++)
            {
                t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vMid + 1);
            }

            t = SetQuad(triangles, t, vTop, vTop - 1, vMid, vTop - 2);

            return t;
        }

        private static int SetQuad(int[] triangles, int i, int v00, int v10, int v01, int v11)
        {
            triangles[i] = v00;
            triangles[i + 1] = triangles[i + 4] = v01;
            triangles[i + 2] = triangles[i + 3] = v10;
            triangles[i + 5] = v11;
            return i + 6;
        }

        private void OnDrawGizmos()
        {
            if (displayCenterDebug)
            {
                Gizmos.color = centerColor;
                Vector3 center = transform.position;
                center.x += xSize / 2f;
                center.y += ySize / 2f;
                center.z += zSize / 2f;
                Gizmos.DrawSphere(center, centerRadius);
            }

            if (!displayNormalsDebug && !dislayGizmosDebug) return;

            if (vertices == null)
            {
                return;
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                if (dislayGizmosDebug)
                {
                    Gizmos.color = gizmoColor;
                    Gizmos.DrawSphere(transform.position + vertices[i], gizmoRadius);
                }

                if (!displayNormalsDebug)
                    continue;

                Gizmos.color = rayColor;
                Gizmos.DrawRay(transform.position + vertices[i], normals[i]);
            }
        }
    }
}