Shader "Tides/Moving shore" {
 Properties{_Print("Painted caustics",2D)="gray"{} _Mirror("Reflected banks",2D)="gray"{} _Shallow("Shallows",Color)=(.33,.69,.62,1) _Deep("Deep reach",Color)=(.13,.45,.55,1) _Foam("Sea glass",Color)=(.84,.92,.81,1) _Air("Sea haze",Color)=(.9,.88,.8,1) _Mood("Habitat, rain, time, tide",Vector)=(0,0,0,0)}
 SubShader{Tags{"RenderType"="Opaque" "Queue"="Geometry+2"} Cull Off Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.0
 #include "UnityCG.cginc"
 sampler2D _Print,_Mirror;half4 _Shallow,_Deep,_Foam,_Air;float4 _Mood;
 struct p{float4 pos:SV_POSITION;float3 world:TEXCOORD0;float4 screen:TEXCOORD1;};
 p vert(float4 vertex:POSITION){p o;float3 w=mul(unity_ObjectToWorld,vertex).xyz;float t=_Time.y;
 w.y+=sin(w.x*.82+w.z*.41+t*.9)*(.012+.014*_Mood.x)+cos(w.z*1.7-w.x*.38+t*1.14)*(.008+.009*_Mood.y);
 o.world=w;o.pos=mul(UNITY_MATRIX_VP,float4(w,1));o.screen=ComputeScreenPos(o.pos);return o;}
 float noise2(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
 half luminance(half3 c){
 #ifndef UNITY_COLORSPACE_GAMMA
  c=LinearToGammaSpace(c);
 #endif
 return dot(c,half3(.27,.58,.15));}
 half4 frag(p i):SV_Target{
 float t=_Time.y;float2 world=i.world.xz;float flow=t*(.11+_Mood.x*.015+_Mood.y*.07);
 float2 wave=float2(sin(world.y*1.63+flow*3.3+world.x*.23),cos(world.x*.83+world.y*.63-flow*2.6));
 float2 uv=world*float2(.124,.157)+wave*.034+float2(flow*.054,-flow*.071);
 half w1=luminance(tex2D(_Print,uv).rgb);half w2=luminance(tex2D(_Print,uv*1.49+float2(.28,flow*.15)).rgb);
 float shore=abs(world.y-.19*sin(world.x*.72)-.075*cos(world.x*1.19));
 float depth=smoothstep(.28,8.8+.90*sin(world.x*.32),shore);
 half3 col=lerp(_Shallow.rgb,_Deep.rgb,depth);float caustic=smoothstep(.56,.77,w1*.79+w2*.21);
 col=lerp(col,_Foam.rgb,caustic*(.29-.21*depth));col*=.92+.15*w2;
 float2 screenUV=i.screen.xy/i.screen.w;float2 bend=wave*.0032+float2(w1-.52,w2-.52)*.023;
 half3 mirror=tex2D(_Mirror,saturate(screenUV+bend*(.8+_Mood.y*.28))).rgb;
 float incidence=saturate(1-normalize(_WorldSpaceCameraPos-i.world).y);float f=.13+.28*incidence*incidence;
 col=lerp(col,mirror,f+.065*depth);
 float shoreBand=.23+.10*wave.x+_Mood.w*.06;
 float foam=1-smoothstep(.028,.10,abs(shore-shoreBand));
 col=lerp(col,_Foam.rgb,foam*(.32+.45*w1)*(1-depth));
 float chop=smoothstep(.84,.995,sin(world.y*3.4+world.x*.17+wave.x*.70+flow*4))*
    smoothstep(.53,.76,w1)*(.040+.026*_Mood.x+.029*_Mood.y);
 col=lerp(col,_Foam.rgb,chop);
 float2 cell=world*1.15;float2 grid=floor(cell);
 float age=frac(t*.81+noise2(grid)*3.7);float2 offset=(float2(noise2(grid+.51),noise2(grid+17.13))-.5)*.47;
 float ring=1-smoothstep(.013,.039,abs(length(frac(cell)-.5-offset)-age*.32));
 col=lerp(col,_Foam.rgb,ring*(1-age)*.40*step(.72,_Mood.y));
 float glint=smoothstep(.88,.99,sin(world.y*(4.1+.30*wave.x)+wave.y*.61+flow*4.4))*smoothstep(.57,.82,w1);
 float stripe=exp(-abs(world.x+world.y*.29+1.8)*.20);
 col=lerp(col,_Foam.rgb,glint*stripe*(.22+.18*step(1.5,_Mood.z))*(1-_Mood.y*.58));
 col=lerp(col,_Air.rgb,smoothstep(29,83,distance(_WorldSpaceCameraPos,i.world)));
 return half4(col,1);}
 ENDCG
 }}
}
