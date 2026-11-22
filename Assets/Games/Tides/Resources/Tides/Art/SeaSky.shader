Shader "Tides/Naturalist sky" {
 Properties{_Crown("Zenith",Color)=(.43,.69,.83,1) _Air("Horizon",Color)=(.91,.87,.72,1) _Cloud("Cloud",Color)=(.97,.92,.78,1) _Night("Night",Float)=0}
 SubShader{Tags{"Queue"="Background" "PreviewType"="Skybox" "RenderType"="Background"} Cull Off ZWrite Off Pass{
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 float4 _Crown,_Air,_Cloud;float _Night;
 struct p{float4 pos:SV_POSITION;float3 dir:TEXCOORD0;};p vert(float4 vertex:POSITION){p o;o.pos=UnityObjectToClipPos(vertex);o.dir=vertex.xyz;return o;}
 float hash(float2 q){return frac(sin(dot(q,float2(171.1,251.3)))*43758.5453);}
 float noise(float2 q){float2 p=floor(q),t=frac(q);t=t*t*(3-2*t);return lerp(lerp(hash(p),hash(p+float2(1,0)),t.x),lerp(hash(p+float2(0,1)),hash(p+1),t.x),t.y);}
 half4 frag(p i):SV_Target{float3 d=normalize(i.dir);float y=d.y;float2 uv=d.xz/max(.095,y+.13);
 float vapor=noise(uv*1.34+float2(_Time.y*.006,0))*.60+noise(uv*3.61)*.28+noise(uv*8.13)*.12;
 half3 c=lerp(_Air.rgb,_Crown.rgb,smoothstep(-.02,.63,y));c=lerp(c,_Cloud.rgb,smoothstep(.57,.78,vapor)*.71*smoothstep(-.02,.12,y));
 float sun=smoothstep(.9981,.9993,dot(d,normalize(float3(-.48,.24,.84))));c=lerp(c,_Cloud.rgb,sun*.92);
 float2 star=uv*97;float sparkle=step(.993,hash(floor(star)))*(1-smoothstep(.035,.19,length(frac(star)-.50)));c+=sparkle*_Night*.50;
 return half4(c,1);}
 ENDCG
 } }}
