Shader "Tides/Field watercolour" {
 Properties{_Grass("Sedge",2D)="gray"{} _Sand("Shell sand",2D)="gray"{} _Rock("Granite",2D)="gray"{} _Wood("Cedar",2D)="gray"{} _Leaves("Canopy",2D)="gray"{} _LightInk("Sun",Color)=(1,.92,.78,1) _SkyInk("Atmosphere",Color)=(.8,.87,.84,1) _Conditions("Rain, night, dusk",Vector)=(0,0,0,0)}
 SubShader{Tags{"RenderType"="Opaque"} Pass{Tags{"LightMode"="ForwardBase"}
 CGPROGRAM
 #pragma target 3.0
 #pragma vertex vert
 #pragma fragment frag
 #pragma multi_compile_fwdbase
 #include "UnityCG.cginc"
 #include "AutoLight.cginc"
 sampler2D _Grass,_Sand,_Rock,_Wood,_Leaves;half4 _LightInk,_SkyInk,_Conditions;
 struct a{float4 vertex:POSITION;float3 normal:NORMAL;float4 uv:TEXCOORD0;half4 color:COLOR;};
 struct p{float4 pos:SV_POSITION;float4 uv:TEXCOORD0;float3 world:TEXCOORD1;half3 n:TEXCOORD2;half4 color:COLOR;SHADOW_COORDS(3)};
 p vert(a v){p o;o.pos=UnityObjectToClipPos(v.vertex);o.world=mul(unity_ObjectToWorld,v.vertex).xyz;o.n=UnityObjectToWorldNormal(v.normal);o.uv=v.uv;o.color=v.color;TRANSFER_SHADOW(o);return o;}
 half3 sampleInk(sampler2D s,float2 uv){half3 c=tex2D(s,uv).rgb;
 #ifndef UNITY_COLORSPACE_GAMMA
  c=LinearToGammaSpace(c);
 #endif
 return c;}
 half4 frag(p i):SV_Target{
 half t=floor(i.uv.z+.5);float2 uv=i.uv.xy;half3 tex=0;
 if(t==1)tex=(sampleInk(_Grass,uv/5.3)-half3(.55,.58,.27))*.52;
 else if(t==2)tex=(sampleInk(_Sand,uv/3.8)-half3(.86,.70,.62))*.56;
 else if(t==3)tex=(sampleInk(_Wood,uv/2.2)-half3(.62,.52,.45))*.61;
 else if(t==4)tex=(sampleInk(_Rock,uv/3.1)-half3(.69,.65,.59))*.66;
 else if(t==5||t==6)tex=(sampleInk(_Wood,uv/3.2)-half3(.62,.52,.45))*.13;
 else if(t==7)tex=(sampleInk(_Leaves,uv/2.5)-half3(.44,.57,.25))*.25;
 else if(t==8)tex=(sampleInk(_Rock,uv/.45)-half3(.69,.65,.59))*.092;
 else if(t==10)tex=(sampleInk(_Leaves,uv/10)-half3(.44,.57,.25))*.12;
 half3 n=normalize(i.n);float sun=saturate(dot(n,normalize(half3(-.54,.85,-.42))));
 half3 light=lerp(half3(.51,.60,.65),half3(.80,.84,.83),n.y*.5+.5)+sun*lerp(.10,.31,SHADOW_ATTENUATION(i))*half3(1,.94,.81);
 half3 c=(i.color.rgb+tex)*lerp(light,half3(1.12,1.09,1.03),saturate(i.uv.w));
 c*=lerp(half3(1,1,1),half3(.81,.87,.94),_Conditions.x*.75);
 c*=lerp(half3(1,1,1),half3(.48,.60,.77),_Conditions.y*(1-i.uv.w));
 c*=lerp(half3(1,1,1),half3(1.06,.88,.81),_Conditions.z*(1-i.uv.w));
 #ifndef UNITY_COLORSPACE_GAMMA
  c=GammaToLinearSpace(saturate(c));
 #endif
 float d=distance(_WorldSpaceCameraPos,i.world);c=lerp(c,_SkyInk.rgb,smoothstep(31,100,d)*.67);
 return half4(c,1);}
 ENDCG
 } UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
 }Fallback "Diffuse"
}
