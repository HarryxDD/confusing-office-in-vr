using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace UnityJapanOffice
{
    public class ControllerSelectFrameImage : BaseMeshEffect
    {
        [SerializeField]
        private float frameSize = 5;
        [SerializeField]
        private Color frameColor = Color.red;
        [SerializeField]
        private Color bodyColor = new Color(1.0f, 0.0f, 0.0f, 0.06f);

        private List<UIVertex> vertexBuffer = new List<UIVertex>();
        private List<int> indexBuffer = new List<int>();
        private RectTransform rectTransformCache;
        private Image imageCache;

        public int paddingTop = 0;
        public int paddingBottom = 0;
        public int paddingLeft = 10;
        public int paddingRight = 0;

        private bool m_visible = true;

        public bool Visible
        {
            get
            {
                return m_visible;
            }
            set
            {
                m_visible = value;
                if (!imageCache)
                {
                    imageCache = GetComponent<Image>();
                }
                imageCache.enabled = value;
            }
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!rectTransformCache)
            {
                rectTransformCache = GetComponent<RectTransform>();
            }
            vertexBuffer.Clear();
            indexBuffer.Clear();

            if (Visible)
            {
                var rect = rectTransformCache.rect;
                rect.xMin -= paddingLeft;
                rect.xMax += paddingRight;
                rect.yMin -= paddingTop;
                rect.yMax += paddingBottom;

                AppendRect(rect, vertexBuffer, indexBuffer, bodyColor);
                var bottomRect = new Rect(rect.xMin - frameSize, rect.yMin - frameSize, rect.width + frameSize * 2, frameSize);
                var upRect = new Rect(rect.xMin - frameSize, rect.yMax, rect.width + frameSize * 2, frameSize);
                var leftRect = new Rect(rect.xMin - frameSize, rect.yMin, frameSize, rect.height);
                var rightRect = new Rect(rect.xMax, rect.yMin, frameSize, rect.height);
                AppendRect(bottomRect, vertexBuffer, indexBuffer, frameColor);
                AppendRect(upRect, vertexBuffer, indexBuffer, frameColor);
                AppendRect(leftRect, vertexBuffer, indexBuffer, frameColor);
                AppendRect(rightRect, vertexBuffer, indexBuffer, frameColor);
            }
            vh.Clear();
            vh.AddUIVertexStream(vertexBuffer, indexBuffer);
        }

        private static void AppendRect(Rect rect, List<UIVertex> verts, List<int> indecies, Color color)
        {
            int startIndex = verts.Count;
            var a1 = new UIVertex() { position = new Vector3(rect.xMin, rect.yMin, 0.0f), color = color };
            var a2 = new UIVertex() { position = new Vector3(rect.xMin, rect.yMax, 0.0f), color = color };
            var a3 = new UIVertex() { position = new Vector3(rect.xMax, rect.yMin, 0.0f), color = color };
            var a4 = new UIVertex() { position = new Vector3(rect.xMax, rect.yMax, 0.0f), color = color };
            verts.Add(a1);
            verts.Add(a2);
            verts.Add(a3);
            verts.Add(a4);

            indecies.Add(startIndex + 0);
            indecies.Add(startIndex + 1);
            indecies.Add(startIndex + 2);
            indecies.Add(startIndex + 1);
            indecies.Add(startIndex + 2);
            indecies.Add(startIndex + 3);


        }


        protected override void Awake()
        {
            base.Awake();
        }
    }
}