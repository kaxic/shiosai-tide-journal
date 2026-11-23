using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static TidesMesh;

public sealed class TidesView : MonoBehaviour
{
    const int BankLayer=29;
    readonly Dictionary<string,Mesh> library=new Dictionary<string,Mesh>();
    readonly Transform[] limbs=new Transform[5];
    readonly LineRenderer[] rings=new LineRenderer[3];
    readonly Vector3[] rodPoints=new Vector3[13],stringPoints=new Vector3[22];
    readonly Vector3[] circle=new Vector3[69];
    Transform set,actor,body,bob,aimMark,swimmer,reel,crank,cascade;
    MeshFilter bank,fishShape;
    Material paint,water,sky,formerSky,silk,spray;
    Camera cameraView,mirror;RenderTexture surface;
    LineRenderer pole,line,arc;
    Vector3 castFrom,floatWorld;
    Vector2 previousPlayer;float stride,walk,time,phaseAge,nextMirror,pivotX;
    int habitat=-1,weather=-1,slot=-1,tide=-1,kind=-1;
    TidesMode lastMode;bool started;
    public void Render(TidesGame g)
    {
        if(!set)Setup();
        float dt=Mathf.Min(Time.unscaledDeltaTime,.075f);time+=dt;phaseAge+=dt;
        bool changed=habitat!=g.Site;
        if(changed){
            habitat=g.Site;bank.sharedMesh=Cache("Habitat "+habitat,m=>new TidesScenery(m,habitat).Build());
            cascade.gameObject.SetActive(habitat==0);
            previousPlayer=g.Player;pivotX=g.Player.x*.82f;nextMirror=0;}
        if(changed||slot!=g.TimeSlot||weather!=g.Weather||tide!=g.Tide)Conditions(g);
        if(!started||lastMode!=g.Mode){
            if(g.Mode==TidesMode.Wait)castFrom=Grip();
            phaseAge=0;lastMode=g.Mode;started=true;}
        Frame(g,dt,changed);Animate(g,dt);Fishing(g);
        if(surface&&(changed||nextMirror<=time)){Reflect();nextMirror=time+.10f;}
    }
    static Color C(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var c);return c;}
    static void Dispose(UnityEngine.Object o){if(!o)return;if(Application.isPlaying)Destroy(o);else DestroyImmediate(o);}
    Transform Group(string label,Transform p){var t=new GameObject(label).transform;t.SetParent(p,false);return t;}
    Mesh Cache(string id,Action<TidesMesh> make){if(library.TryGetValue(id,out var mesh))return mesh;var m=new TidesMesh();make(m);mesh=m.Build(id);library[id]=mesh;return mesh;}
    MeshFilter Draw(string label,Transform p,Mesh shape,Material ink,bool reflect=false)
    {
        var t=Group(label,p);if(reflect)t.gameObject.layer=BankLayer;var mf=t.gameObject.AddComponent<MeshFilter>();mf.sharedMesh=shape;
        var mr=t.gameObject.AddComponent<MeshRenderer>();mr.sharedMaterial=ink;mr.lightProbeUsage=LightProbeUsage.Off;mr.reflectionProbeUsage=ReflectionProbeUsage.Off;
        if(ink!=paint){mr.shadowCastingMode=ShadowCastingMode.Off;mr.receiveShadows=false;}return mf;
    }
    LineRenderer Stroke(string id,float w,Color a,Color b)
    {
        var s=Group(id,set).gameObject.AddComponent<LineRenderer>();s.sharedMaterial=silk;s.useWorldSpace=true;s.widthMultiplier=w;s.startColor=a;s.endColor=b;
        s.numCapVertices=3;s.numCornerVertices=2;s.shadowCastingMode=ShadowCastingMode.Off;s.receiveShadows=false;
        s.textureMode=LineTextureMode.Stretch;s.alignment=LineAlignment.View;return s;
    }
    void Setup()
    {
        set=Group("Tidal field journal",transform);
        const string path="Tides/Art/";Material New(string key)=>new Material(Resources.Load<Shader>(path+key)){name=key};
        paint=New("Bank");water=New("LivingWater");sky=New("SeaSky");silk=New("FishingSilk");spray=New("RiverSpray");
        string[] props={"_Grass","_Sand","_Wood","_Rock","_Leaves","_Grain"},maps={"Sedge","TidalSand","Cedar","Strata","Canopy","ShoreGrain"};
        for(int i=0;i<maps.Length;i++)paint.SetTexture(props[i],Resources.Load<Texture2D>(path+maps[i]));
        water.SetTexture("_Print",Resources.Load<Texture2D>(path+"Caustics"));formerSky=RenderSettings.skybox;RenderSettings.skybox=sky;
        cameraView=Camera.main;if(!cameraView){cameraView=Group("Waterside lens",set).gameObject.AddComponent<Camera>();cameraView.tag="MainCamera";}
        cameraView.orthographic=false;cameraView.fieldOfView=45;cameraView.nearClipPlane=.12f;cameraView.farClipPlane=140;
        cameraView.clearFlags=CameraClearFlags.Skybox;cameraView.allowHDR=false;if(!FindAnyObjectByType<AudioListener>())cameraView.gameObject.AddComponent<AudioListener>();
        var sun=Group("Coastal sunlight",set).gameObject.AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.12f;
        sun.transform.rotation=Quaternion.LookRotation(At(.54f,-.85f,.42f));sun.shadows=LightShadows.Soft;sun.shadowStrength=.77f;sun.shadowNormalBias=.20f;
        bank=Draw("Waterside habitat",set,null,paint,true);var sheet=WaterMesh();library.Add("Moving water",sheet);Draw("Water",set,sheet,water);
        var cascadeMesh=new Mesh{name="Brook cascade",vertices=new[]{At(7.29f,.054f,12.42f),At(8.70f,.054f,12.47f),At(8.46f,1.36f,13.66f),At(7.64f,1.36f,13.68f)},
            uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(1,1),new Vector2(0,1)},triangles=new[]{0,1,2,0,2,3}};
        library["Brook cascade"]=cascadeMesh;cascade=Draw("Brook cascade",set,cascadeMesh,spray,true).transform;
        if(SystemInfo.graphicsDeviceType!=GraphicsDeviceType.Null){
            mirror=Group("Planar water reflection",set).gameObject.AddComponent<Camera>();mirror.enabled=false;
            surface=new RenderTexture(512,288,16,RenderTextureFormat.ARGB32){name="Mirrored waterside",filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
            surface.Create();water.SetTexture("_Mirror",surface);}
        actor=Group("Angler",set);body=Group("Facing",actor);
        Draw("Contact shadow",actor,Cache("Footfall",m=>{
            for(int i=0;i<22;i++){
                float a=i*Mathf.PI/11,b=(i+1)*Mathf.PI/11;Vector3 P(float ang,float f)=>At(Mathf.Cos(ang)*.295f*f,.022f-Mathf.Sin(ang)*.012f*f,Mathf.Sin(ang)*.182f*f);
                m.GradientTri(P(0,0),P(a,.60f),P(b,.60f),"515E5B69","515E5B45","515E5B45");
                m.GradientTri(P(a,.60f),P(a,1),P(b,1),"515E5B45","515E5B00","515E5B00");
                m.GradientTri(P(a,.60f),P(b,1),P(b,.60f),"515E5B45","515E5B00","515E5B45");}
        }),silk);
        for(int i=0;i<limbs.Length;i++){int n=i;limbs[i]=Draw("Field angler "+n,body,Cache("Angler "+n,m=>TidesFigures.Angler(m,n)),paint,true).transform;
            if(i>0){int sign=i==1||i==3?1:-1;limbs[i].localPosition=i<=2?At(sign*.214f,1.094f,.018f):At(sign*.105f,.619f,0);}}
        pole=Stroke("Tapered split-cane rod",1,C("896944"),C("4C6968"));pole.widthCurve=new AnimationCurve(new Keyframe(0,.064f),new Keyframe(.22f,.044f),new Keyframe(1,.014f));
        reel=Draw("Cork grip and brass reel",set,Cache("Fishing reel",m=>{
            m.Ink("BAA07A",Finish.Wood);m.Tube(At(0,0,-.24f),At(0,0,.17f),.052f,.045f,10);
            m.Ink("667B7C");m.Tube(At(0,-.015f,.063f),At(0,-.105f,.087f),.020f,.024f,8);
            m.Ink("AE9872");m.Tube(At(-.060f,-.117f,.080f),At(.059f,-.117f,.080f),.070f,.071f,12);
            m.Ink("E0CC97");m.Tube(At(-.071f,-.117f,.080f),At(-.052f,-.117f,.080f),.083f,.082f,12);
        }),paint).transform;
        crank=Draw("Reeling handle",reel,Cache("Reel crank",m=>{
            m.Ink("738680");m.Tube(Vector3.zero,At(.034f,-.073f,.016f),.015f,.014f,7);
            m.Ink("A38A62",Finish.Wood);m.Tube(At(.034f,-.073f,.016f),At(.083f,-.073f,.016f),.022f,.023f,8);
        }),paint).transform;crank.localPosition=At(.061f,-.117f,.080f);
        line=Stroke("Fishing line",1,C("E4DEBB"),C("BED6C6"));line.startWidth=.018f;line.endWidth=.014f;
        arc=Stroke("Cast trajectory",1,C("DFD2A0"),C("BDE7D5"));arc.startWidth=.022f;arc.endWidth=.012f;
        for(int i=0;i<rings.Length;i++){rings[i]=Stroke("Surface disturbance "+i,1,C("CEE9DC"),C("CEE9DC"));rings[i].loop=true;}
        bob=Draw("Lacquered float",set,Cache("Float",m=>{
            m.Ink("577E7F",Finish.Painted);m.Tube(At(0,-.24f,0),At(0,.04f,0),.024f,.030f,8);
            m.Ink("F1E5B9",Finish.Painted);m.Blob(At(0,.072f,0),At(.096f,.126f,.094f),5,12);
            m.Ink("BB5D48",Finish.Painted);m.Drum(0,.095f,0,.095f,.076f,.065f,12);
            m.Ink("C56C50",Finish.Painted);m.Drum(0,.147f,0,.059f,.028f,.139f,10);
            m.Ink("EBC369",Finish.Glow,.46f);m.Blob(At(0,.292f,0),At(.035f,.042f,.035f),13,8);
        }),paint).transform;
        aimMark=Draw("Aim ring",set,Cache("Aim ring",m=>{m.Ink("D2EAD4",Finish.Plain,.74f);
            for(int i=0;i<6;i++)m.Ring(Vector3.zero,.282f,.329f,7,i*60+5,i*60+46);}),paint).transform;
        swimmer=Group("Fish on the line",set);fishShape=Draw("Fish species",swimmer,null,paint);
    }
    void Conditions(TidesGame g)
    {
        slot=g.TimeSlot;weather=g.Weather;tide=g.Tide;float rain=weather==2?1:weather==1?.45f:0;
        Color skyHigh=C(new[]{"9DC2D1","78B1CA","BD98A3","45596F"}[slot]),horizon=C(new[]{"EDDEBD","DBE9DB","ECD1A8","8597A6"}[slot]);
        skyHigh=Color.Lerp(skyHigh,C("94AAB3"),rain*.60f);horizon=Color.Lerp(horizon,C("B8C6C4"),rain*.61f);
        sky.SetColor("_Crown",skyHigh);sky.SetColor("_Air",horizon);sky.SetColor("_Cloud",Color.Lerp(horizon,Color.white,slot==3?.20f:.41f));sky.SetFloat("_Night",slot==3?1:0);
        paint.SetColor("_SkyInk",horizon);paint.SetVector("_Conditions",new Vector4(rain,slot==3?1:0,slot==2?1:0,0));
        Color near=C(new[]{"7DB3A0","8EBAB0","8CAAA6","7ABEB7","69AD93","AEB89C","77D4CA","538CAB"}[habitat]),far=C(new[]{"5F9590","6F9DA4","668B96","548F9B","438D81","7F9F99","38A9B3","2D678D"}[habitat]);
        if(slot==3){near=Color.Lerp(near,C("466C80"),.64f);far=C("45637A");}
        else if(slot==2){near=Color.Lerp(near,C("C9987E"),.40f);far=Color.Lerp(far,C("998A9D"),.38f);}
        water.SetColor("_Shallow",near);water.SetColor("_Deep",far);water.SetColor("_Foam",Color.Lerp(horizon,C("D2EBDF"),.52f));
        water.SetColor("_Air",horizon);
        water.SetVector("_Mood",new Vector4(habitat<4?habitat:new[]{.65f,.38f,1.8f,3.2f}[habitat-4],rain,slot,tide));nextMirror=0;
    }
    void Frame(TidesGame g,float dt,bool snap)
    {
        bool casting=g.Mode==TidesMode.Wait||g.Mode==TidesMode.Bite||g.Mode==TidesMode.Fight;
        float targetX=g.Player.x*.79f+(casting?(g.FloatPosition.x-g.Player.x)*.24f:0);
        pivotX=snap?targetX:Mathf.Lerp(pivotX,targetX,1-Mathf.Exp(-dt*4.4f));
        Vector3 eye=At(pivotX-2.04f,6.14f,-11.8f+g.Player.y*.25f);
        Vector3 target=At(pivotX+.35f,.94f,5.60f);
        cameraView.transform.SetPositionAndRotation(eye,Quaternion.LookRotation(target-eye));
        cameraView.fieldOfView=Mathf.Max(43,46*1.50f/Mathf.Max(1.05f,cameraView.aspect));
    }
    void Animate(TidesGame g,float dt)
    {
        var p=g.Player;float dist=Vector2.Distance(p,previousPlayer);previousPlayer=p;
        walk=Mathf.MoveTowards(walk,dist>.001f?1:0,dt*10);stride+=Mathf.Min(dist,.30f)*8;
        actor.position=At(p.x,TidesScenery.Ground(p.y)+.008f,p.y);var facing=g.Facing;
        if(g.Mode==TidesMode.Aim||g.Mode==TidesMode.Wait||g.Mode==TidesMode.Bite||g.Mode==TidesMode.Fight)
            facing=new Vector2(Mathf.Sin(g.CastAim*Mathf.Deg2Rad),Mathf.Cos(g.CastAim*Mathf.Deg2Rad));
        if(facing.sqrMagnitude>.001f){var q=Quaternion.LookRotation(At(facing.x,0,facing.y));
            body.rotation=Application.isPlaying?Quaternion.RotateTowards(body.rotation,q,dt*640):q;}
        body.localPosition=Vector3.up*(Mathf.Abs(Mathf.Sin(stride))*.044f*walk);
        for(int i=1;i<5;i++){
            float arm=0;if(i<=2){arm=g.Mode==TidesMode.Aim?-36:g.Mode==TidesMode.Fight?-13-g.Tension*25:2;
                if(g.Mode==TidesMode.Wait&&phaseAge<.46f)arm-=Mathf.Sin(phaseAge/.46f*Mathf.PI)*23;}
            limbs[i].localRotation=Quaternion.Euler(arm+Mathf.Sin(stride)*walk*(i==1||i==4?1:-1)*(i<3?13:24),i==2?25:0,0);}
    }
    Vector3 Grip()=>limbs[1]?limbs[1].TransformPoint(At(.014f,-.28f,.30f)):Vector3.zero;
    void Fishing(TidesGame g)
    {
        bool aiming=g.Mode==TidesMode.Aim,waiting=g.Mode==TidesMode.Wait,bite=g.Mode==TidesMode.Bite,fight=g.Mode==TidesMode.Fight,caught=g.Mode==TidesMode.Catch;
        bool afloat=waiting||bite||fight,air=waiting&&phaseAge<.60f;
        Vector3 grip=Grip(),forward=body.forward;float angle=aiming?54:afloat?27:64;
        if(fight)angle=38+g.Tension*26;if(air)angle+=52*(1-Mathf.Clamp01(phaseAge/.45f));
        float rad=angle*Mathf.Deg2Rad;
        Vector3 dir=(forward+body.right*(fight?.13f:.055f)).normalized*Mathf.Cos(rad)+Vector3.up*Mathf.Sin(rad);
        reel.SetPositionAndRotation(grip,Quaternion.LookRotation(dir,Vector3.up));
        if(fight&&g.Reeling)crank.Rotate(Vector3.right,Time.unscaledDeltaTime*430,Space.Self);
        float flex=fight?.22f+g.Tension*.69f:aiming?.21f:air?.40f:.11f;
        for(int i=0;i<rodPoints.Length;i++){float f=(float)i/(rodPoints.Length-1);rodPoints[i]=grip+dir*(f*2.47f)-Vector3.up*(flex*f*f)+(forward*.39f+body.right*.28f)*(f*f*flex);}
        pole.positionCount=rodPoints.Length;pole.SetPositions(rodPoints);Vector3 tip=rodPoints[rodPoints.Length-1];
        var destination=At(g.FloatPosition.x,.09f,g.FloatPosition.y);
        floatWorld=destination;if(air){float f=Mathf.Clamp01(phaseAge/.60f);
            var from=castFrom.sqrMagnitude>.001f?castFrom:tip;floatWorld=Vector3.Lerp(from,destination,f)+Vector3.up*Mathf.Sin(f*Mathf.PI)*2.63f;}
        floatWorld.y+=.046f*Mathf.Sin(time*3.1f+destination.x*.3f);
        if(bite)floatWorld.y-=.12f+.082f*g.BitePulse;
        else if(waiting&&!air)floatWorld.y-=g.BitePulse*.24f;
        bob.gameObject.SetActive(afloat);bob.position=floatWorld;
        float size=Mathf.Clamp(Vector3.Distance(cameraView.transform.position,floatWorld)*.056f,.91f,1.82f);bob.localScale=Vector3.one*size;
        bob.rotation=Quaternion.Euler(Mathf.Sin(time*3.2f)*(bite?13:3),0,Mathf.Sin(time*2.5f)*(fight?17:4));
        line.enabled=afloat||aiming;arc.enabled=aiming;aimMark.gameObject.SetActive(aiming);
        if(aiming){aimMark.position=destination+Vector3.up*.035f;aimMark.localRotation=Quaternion.Euler(0,time*24,0);}
        if(afloat||aiming){
            Vector3 end=aiming?destination:floatWorld+Vector3.up*.135f*size;
            float sag=fight?.06f+(1-g.Tension)*.24f:Mathf.Min(.83f,Vector3.Distance(tip,end)*.057f);
            for(int i=0;i<stringPoints.Length;i++){float f=(float)i/(stringPoints.Length-1);
                stringPoints[i]=Vector3.Lerp(tip,end,f)+Vector3.up*Mathf.Sin(f*Mathf.PI)*(aiming?1.5f:-sag);}
            if(aiming){line.enabled=false;arc.positionCount=stringPoints.Length;arc.SetPositions(stringPoints);}
            else{line.positionCount=stringPoints.Length;line.SetPositions(stringPoints);}
        }
        for(int j=0;j<rings.Length;j++){
            var ring=rings[j];ring.enabled=afloat&&!air&&(j==0||bite||fight);if(!ring.enabled)continue;
            float age=Mathf.Repeat(time*(bite?1.15f:fight?.91f:.59f)+(float)j/3,1);
            float r=(.13f+age*(bite?1.02f:fight?.78f:.55f))*size;
            for(int k=0;k<circle.Length;k++){float a=k*Mathf.PI*2/circle.Length;circle[k]=At(floatWorld.x+Mathf.Cos(a)*r,.069f+.009f*j,floatWorld.z+Mathf.Sin(a)*r);}
            float a2=(1-age)*(bite?.86f:.56f);Color ink=C(bite?"F1D294":"D2E5CE");ink.a=a2;
            ring.startColor=ring.endColor=ink;ring.startWidth=ring.endWidth=.021f*(1-age*.56f)*size;ring.positionCount=circle.Length;ring.SetPositions(circle);}
        bool show=(fight||caught)&&g.FishIndex>=0&&g.FishIndex<g.Fish.Length;swimmer.gameObject.SetActive(show);
        if(show){
            int id=TidesFigures.Kind(g.Fish[g.FishIndex].id);if(id!=kind){kind=id;fishShape.sharedMesh=Cache("Fish "+id,m=>TidesFigures.Fish(m,id));}
            float fs=Mathf.Clamp(.55f+g.LandingLength*.009f,.65f,1.97f);
            swimmer.localScale=Vector3.one*fs;
            if(caught){swimmer.position=grip+body.right*.51f+Vector3.up*.24f;swimmer.rotation=Quaternion.Euler(12,22,65+Mathf.Sin(time*4)*5);}
            else{float leap=g.Running?Mathf.Pow(Mathf.Max(0,Mathf.Sin(time*3.7f)),3)*.38f:.04f;
                swimmer.position=floatWorld+At(-.20f,.004f+leap,0);
                swimmer.rotation=Quaternion.Euler(0,25+Mathf.Sin(time*(g.Running?7:3))*26,Mathf.Sin(time*5)*9+leap*28);}
        }
    }
    void Reflect()
    {
        var source=cameraView;mirror.CopyFrom(source);mirror.enabled=false;mirror.targetTexture=surface;mirror.rect=new Rect(0,0,1,1);
        mirror.aspect=source.aspect;mirror.allowHDR=false;mirror.allowMSAA=false;mirror.cullingMask=1<<BankLayer;
        Vector3 pos=source.transform.position;pos.y=-pos.y;mirror.transform.position=pos;
        Matrix4x4 view=source.worldToCameraMatrix*Matrix4x4.Scale(At(1,-1,1));mirror.worldToCameraMatrix=view;
        var n=view.MultiplyVector(Vector3.up).normalized;var p=view.MultiplyPoint(At(0,.045f,0));var plane=new Vector4(n.x,n.y,n.z,-Vector3.Dot(p,n));
        mirror.projectionMatrix=source.CalculateObliqueMatrix(plane);bool initial=GL.invertCulling;
        try{GL.invertCulling=!initial;mirror.Render();}finally{GL.invertCulling=initial;}
    }
    static Mesh WaterMesh(){
        const int nx=83,nz=53;var v=new Vector3[nx*nz];var t=new int[(nx-1)*(nz-1)*6];int k=0;
        for(int z=0;z<nz;z++)for(int x=0;x<nx;x++){int i=z*nx+x;v[i]=At(-57+x*114f/(nx-1),0,-2.1f+z*79f/(nz-1));
            if(x==nx-1||z==nz-1)continue;t[k++]=i;t[k++]=i+nx;t[k++]=i+nx+1;t[k++]=i;t[k++]=i+nx+1;t[k++]=i+1;}
        return new Mesh{name="Water plane",vertices=v,triangles=t,bounds=new Bounds(At(0,0,37),At(116,2,83))};}
    void OnDestroy()
    {
        if(RenderSettings.skybox==sky)RenderSettings.skybox=formerSky;if(surface){surface.Release();Dispose(surface);}
        foreach(var mesh in library.Values)Dispose(mesh);Dispose(paint);Dispose(water);Dispose(sky);Dispose(silk);Dispose(spray);
    }
}
