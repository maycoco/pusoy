﻿
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace SpringFramework.UI
{
    //Pie Base Data ,use this data to build Pie Graph
    [System.Serializable]
    public class PieData
    {
        public float Percent;
        public Color Color;
        public PieData(){}
        public PieData(float percent,Color color0)
        {
            Percent = percent / 100.0f;
            Color = color0;
        }

        public PieData(float percent)
        {
            Percent = percent / 100.0f;
            Color = Color.white * Percent;
        }
    };

    //Use this data struct to draw a Pie Graph;
    [System.Serializable]
    public class Pies
    {
        public List<PieData> PieDatas = null;
        public Pies(){}
        public Pies( List<PieData> pieDatas )
        {
            PieDatas = pieDatas;
        }
    };

    //GUI Text information for pie Graph
    [System.Serializable]
    public class PieText
    {
        public string Content = null;
        public Vector2 Position;
        public bool IsLeft = true;
        public PieText(){}
        public PieText( string content,Vector2 position ,bool isLeft )
        {
            Content = content;
            Position = position;
            IsLeft = isLeft;
        }
    }

    /// <summary>
    /// </summary>
    public class PieGraph : MaskableGraphic
    {
		//public float WinPercent		= 50.0f;
		//public float LostPercent	= 50.0f;


		public float PieRadius   	= 112.5f;
		public float HollowWidth 	= 0.0f;
		public float BoomDegree 	= 0.0f;
		public float Smooth 		= 200;
		public bool IsShowPercnet 	= false;
		public bool IsShowThumbnail = false;
		public float BrokenLineWidth = 2;

//        [Tooltip("圆环半径")]
//        [Range(5 , 150)]public float PieRadius   = 60.0f;
//        [Tooltip("空心半径")]
//        [Range(0 , 0)]public float HollowWidth = 0.0f;
//
//        [Tooltip("饼图炸裂程度")]
//        [Range(0, 15)] public float BoomDegree = 1.5f;
//
//        [Tooltip("饼图光滑程度")]
//        [Range(20, 200)] public float Smooth = 100;
//        [Tooltip("是否显示百分比")]
//        public bool IsShowPercnet = true;
//        [Tooltip("是否显示破折线")]
//        [Range(0.5f, 4)] public float BrokenLineWidth = 2;
//        [Tooltip("是否显示缩略图")]
//        public bool IsShowThumbnail = false;

        private Pies PieData = null;
        private List<PieText> _pieText = null;
        private Vector3 _realPosition;

//        protected override void Start()
//        {
//			PieData = new Pies(new List<PieData>()
//				{
//					new PieData(0 ,Color.cyan),
//					new PieData(100,Color.green),
//				});
//        }

		public void SetPie(float LostPercent, float WinPercent, Color Lost, Color Win){
			PieData = new Pies(new List<PieData>()
				{
					new PieData(LostPercent ,Lost),
					new PieData(WinPercent,Win),
				});
		}
			
        private void OnGUI()
        {
            if ( null == _pieText ) return;
            if (!IsShowPercnet) return;
            Vector3 result = transform.localPosition;
            _realPosition = getScreenPosition(transform , ref result);
            foreach ( PieText text in _pieText )
            {
                Vector2 position = local2Screen(_realPosition , text.Position);
                GUIStyle guiStyle = new GUIStyle();
                guiStyle.normal.textColor = Color.black;
                guiStyle.fontStyle = FontStyle.Bold;
                guiStyle.alignment = TextAnchor.MiddleLeft;
                if ( !text.IsLeft )
                    guiStyle.alignment = TextAnchor.MiddleRight;
				if(text.IsLeft)
					position += new Vector2(3,-10);
				else
					position += new Vector2(-23,-10);
                GUI.Label(new Rect(position , new Vector2(20 , 20)) , text.Content , guiStyle);
            }
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            if ( null == PieData ) return;
            vh.Clear();
            vh.AddUIVertexTriangleStream(DrawPie());
        }

        private List<UIVertex> DrawPie()
        {
            if (IsShowPercnet)
                _pieText = new List<PieText>();
            List<UIVertex> vertexs = new List<UIVertex>();

            float perRadian = Mathf.PI * 2 / Smooth;
            float totalRadian = 0;
            float boomRadian = BoomDegree / 180 * Mathf.PI;
            float pieRadianBase = Mathf.PI * 2 - boomRadian * PieData.PieDatas.Count;

            for ( int i = 0 ; i < PieData.PieDatas.Count ; i++ )
            {
                PieData data = PieData.PieDatas[i];
                float endRadian = boomRadian + data.Percent * pieRadianBase + totalRadian;
                for ( float r = boomRadian + totalRadian ; r < endRadian ; r += perRadian )
                {
                    Vector2 first = new Vector2(Mathf.Cos(r) , Mathf.Sin(r)) * HollowWidth;
                    Vector2 second = new Vector2(Mathf.Cos(r + perRadian) , Mathf.Sin(r + perRadian)) * HollowWidth;
                    Vector2 third = new Vector2(Mathf.Cos(r) , Mathf.Sin(r)) * PieRadius;
                    Vector2 four = new Vector2(Mathf.Cos(r + perRadian) , Mathf.Sin(r + perRadian)) * PieRadius;
                    vertexs.Add(GetUIVertex(first , data.Color));
                    vertexs.Add(GetUIVertex(third , data.Color));
                    vertexs.Add(GetUIVertex(second , data.Color));
                    vertexs.Add(GetUIVertex(second , data.Color));
                    vertexs.Add(GetUIVertex(third , data.Color));
                    vertexs.Add(GetUIVertex(four , data.Color));
                }

                if (IsShowPercnet)
                {
                    float middleRadian = boomRadian + data.Percent * pieRadianBase / 2 + totalRadian;
                    float brokenLineLength = PieRadius * 0.2f;
                    Vector2 middlePoint = new Vector2(Mathf.Cos(middleRadian) , Mathf.Sin(middleRadian)) * PieRadius;
                    Vector2 secondPoint = middlePoint + new Vector2(Mathf.Cos(middleRadian) , Mathf.Sin(middleRadian)) * brokenLineLength;

                    Vector2 first = middlePoint + new Vector2(Mathf.Sin(middleRadian) , -Mathf.Cos(middleRadian)) * BrokenLineWidth / 2;
                    Vector2 second = middlePoint + new Vector2(-Mathf.Sin(middleRadian) , Mathf.Cos(middleRadian)) * BrokenLineWidth / 2;
                    Vector2 third = secondPoint + new Vector2(Mathf.Sin(middleRadian) , -Mathf.Cos(middleRadian)) * BrokenLineWidth / 2;
                    Vector2 four = secondPoint + new Vector2(-Mathf.Sin(middleRadian) , Mathf.Cos(middleRadian)) * BrokenLineWidth / 2;

                    Vector2 five;
                    Vector2 six;
                    Vector2 seven;
                    Vector2 eight;

                    vertexs.Add(GetUIVertex(first , data.Color));
                    vertexs.Add(GetUIVertex(second , data.Color));
                    vertexs.Add(GetUIVertex(third , data.Color));

                    vertexs.Add(GetUIVertex(third , data.Color));
                    vertexs.Add(GetUIVertex(second , data.Color));
                    vertexs.Add(GetUIVertex(four , data.Color));

                    five = secondPoint + new Vector2(0 , BrokenLineWidth / 2.0f);
                    six = secondPoint + new Vector2(0 , -BrokenLineWidth / 2.0f);

                    if ( middleRadian > Mathf.PI / 2 && middleRadian < Mathf.PI * 3 / 2 )
                    {
                        seven = five + new Vector2(-brokenLineLength , 0);
                        eight = six + new Vector2(-brokenLineLength , 0);
                        _pieText.Add(new PieText(data.Percent * 100 + "%" , eight , false));
                    }
                    else
                    {
                        seven = five + new Vector2(brokenLineLength , 0);
                        eight = six + new Vector2(brokenLineLength , 0);
                        _pieText.Add(new PieText(data.Percent * 100 + "%" , eight , true));
                    }

                    vertexs.Add(GetUIVertex(third , data.Color));
                    vertexs.Add(GetUIVertex(four , data.Color));
                    vertexs.Add(GetUIVertex(five , data.Color));

                    vertexs.Add(GetUIVertex(five , data.Color));
                    vertexs.Add(GetUIVertex(six , data.Color));
                    vertexs.Add(GetUIVertex(third , data.Color));

                    vertexs.Add(GetUIVertex(five , data.Color));
                    vertexs.Add(GetUIVertex(six , data.Color));
                    vertexs.Add(GetUIVertex(seven , data.Color));

                    vertexs.Add(GetUIVertex(seven , data.Color));
                    vertexs.Add(GetUIVertex(eight , data.Color));
                    vertexs.Add(GetUIVertex(six , data.Color));
                }
                totalRadian = endRadian;
            }

            if ( IsShowThumbnail )
                vertexs.AddRange(DrawThumbnail());
            return vertexs;
        }

        private List<UIVertex> DrawThumbnail()
        {
            List<UIVertex> vertexs= new List<UIVertex>();
            Vector2 origin = new Vector2(PieRadius , -PieRadius) * 1.2f + new Vector2(0,PieRadius * 0.2f);
            float lenght = 12;
            float height = 6;
            float interval = 3;
            for (int i = 0; i < PieData.PieDatas.Count ; i++) 
            {
                PieData data = PieData.PieDatas[i];
                Vector2 first  = new Vector2(origin.x , origin.y + interval * i + height * ( i + 1 ));
                Vector2 second = new Vector2(origin.x , origin.y + height + interval * i + height * ( i + 1 ));
                Vector2 third  = new Vector2(origin.x + lenght , origin.y + interval * i + height * ( i + 1 ));
                Vector2 four   = new Vector2(origin.x + lenght , origin.y + height + interval * i + height * ( i + 1 ));
                vertexs.Add(GetUIVertex(first , data.Color));
                vertexs.Add(GetUIVertex(second , data.Color));
                vertexs.Add(GetUIVertex(third , data.Color));
                vertexs.Add(GetUIVertex(third , data.Color));
                vertexs.Add(GetUIVertex(second , data.Color));
                vertexs.Add(GetUIVertex(four , data.Color));
            }
            return vertexs;
        }

        private UIVertex GetUIVertex( Vector2 point , Color color0 )
        {
            UIVertex vertex = new UIVertex
            {
                position = point ,
                color = color0 ,
                uv0 = new Vector2(0 , 0)
            };
            return vertex;
        }
			
        private Vector2 local2Screen( Vector2 parentPos,Vector2 localPosition )
        {
            Vector2 pos = localPosition + parentPos;
            float xValue, yValue = 0;
            if ( pos.x > 0 )
                xValue = pos.x + Screen.width / 2.0f;
            else
                xValue = Screen.width / 2.0f - Mathf.Abs(pos.x);
            if ( pos.y > 0 )
                yValue = Screen.height / 2.0f - pos.y;
            else
                yValue = Screen.height / 2.0f + Mathf.Abs(pos.y);
            return new Vector2(xValue,yValue);
        }
			
        private Vector2 getScreenPosition( Transform trans, ref Vector3 result )
        {
            if ( null != trans.parent && null != trans.parent.parent )
            {
                result += trans.parent.localPosition;
                getScreenPosition(trans.parent , ref result);
            }
            if ( null != trans.parent && null == trans.parent.parent )
                return result;
            return result;
        }

    }
}