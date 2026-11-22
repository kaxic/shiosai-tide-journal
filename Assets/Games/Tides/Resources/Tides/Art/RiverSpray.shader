Shader "Tides/River spray" {
 SubShader{Tags{"Queue"="Transparent+2" "RenderType"="Transparent"} Cull Off ZWrite Off Blend SrcAlpha OneMinusSrcAlpha
 Pass{CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct p{float4 pos:SV_POSITION;float2 uv:TEXCOORD0;};p vert(float4 v:POSITION,float2 uv:TEXCOORD0){p o;o.pos=UnityObjectToClipPos(v);o.uv=uv;return o;}
 half4 frag(p i):SV_Target{float2 uv=i.uv;float x=floor(uv.x*19);float w=sin(uv.y*10+_Time.y*5+frac(x*.62)*8);
 float shine=smoothstep(.58,.95,w);float edge=smoothstep(0,.15,min(uv.x,1-uv.x));
 return half4(lerp(half3(.55,.76,.76),half3(.89,.94,.87),shine),edge*(.51+.38*shine));}
 ENDCG}
 }}
