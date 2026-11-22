Shader "Tides/Fishing silk" {
 Properties{_Color("Ink",Color)=(1,1,1,1)}
 SubShader{Tags{"Queue"="Transparent+20" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 half4 _Color;struct a{float4 vertex:POSITION;half4 color:COLOR;};struct p{float4 pos:SV_POSITION;half4 c:COLOR;};
 p vert(a v){p o;o.pos=UnityObjectToClipPos(v.vertex);o.c=v.color;return o;}
 half4 frag(p i):SV_Target{return i.c*_Color;}
 ENDCG}
 }}
