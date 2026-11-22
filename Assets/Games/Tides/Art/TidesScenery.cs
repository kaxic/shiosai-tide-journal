using UnityEngine;
using static TidesMesh;
using static TidesMesh.Finish;

internal sealed class TidesScenery
{
    readonly TidesMesh m;readonly int site;
    public TidesScenery(TidesMesh mesh,int habitat){m=mesh;site=habitat;}
    void Ink(string c,Finish t=Plain,float e=0)=>m.Ink(c,t,e);
    void Box(Vector3 p,Vector3 size,string c,Finish t=Plain,float yaw=0,float tilt=0,float roll=0){
        Ink(c,t);m.Box(p,size,Quaternion.Euler(tilt,yaw,roll));}
    void B(float x,float y,float z,float w,float h,float d,string c,Finish t=Plain)=>Box(At(x,y,z),At(w,h,d),c,t);
    void Blob(Vector3 p,Vector3 size,string c,Finish t,int seed=0,int sides=9){Ink(c,t);m.Blob(p,size,seed,sides);}
    void Pole(Vector3 a,Vector3 b,float w,float d,string c,Finish t=Wood,int sides=8){Ink(c,t);m.Tube(a,b,w,d,sides);}
    void Rail(Vector3 a,Vector3 b,float w,float d,string c,Finish t=Wood){Ink(c,t);m.Beam(a,b,w,d);}
    public static float Shoreline(float x)=>.135f+.19f*Mathf.Sin(x*.72f)+.075f*Mathf.Cos(x*1.19f);
    public static float Ground(float z)=>.19f+.068f*Mathf.Clamp(-z,0,6);
    public void Build()
    {
        GroundBank();Dock();SupplyTable();TravelSign();Coastline();
        switch(site){case 0:Headwaters();break;case 1:Lake();break;case 2:Harbour();break;default:Reef();break;}
    }
    void GroundBank()
    {
        string top=site==0?"9BAB7D":site==1?"A5B086":site==2?"B0A392":"D7BB9F";
        Finish texture=site<2?Grass:site==2?Rock:Sand;
        for(int x=-38;x<38;x++)
        {
            float edge0=Shoreline(x),edge1=Shoreline(x+1);
            for(int band=0;band<3;band++){
                float z0=band==0?-18:band==1?-3.1f:-.48f;
                float l=band==2?edge0:band==0?-3.1f:-.48f,r=band==2?edge1:l;
                Ink(band==2?(site==2?"9F998B":site==3?"DEBEA0":"B9AD86"):top,band==2&&site!=2?Sand:texture);
                m.Quad(At(x,Ground(z0),z0),At(x,Ground(l),l),At(x+1,Ground(r),r),At(x+1,Ground(z0),z0),Vector3.up);
            }
            Ink(site==2?"8E9389":"B6A084",site==2?Rock:Sand);
            m.Quad(At(x,Ground(edge0),edge0),At(x,-.42f,edge0+.32f),At(x+1,-.42f,edge1+.32f),At(x+1,Ground(edge1),edge1),Vector3.forward);
        }
        if(site==2)for(int j=-11;j<=11;j++){
            float x=j*1.03f,z=Shoreline(x);
            B(x,.09f,z,.99f,.47f,.38f,j%3==0?"8B9189":"A7A59A",Rock);}
    }
    void Dock()
    {
        float x=6.30f,z=-.40f;string timber=site==2?"9E8C71":"B89A73";
        for(int j=0;j<11;j++){
            B(x,.289f,z+j*.282f,2.02f,.15f,.271f,j%3==0?"AB9578":timber,Wood);
            foreach(float side in new[]{-.83f,.83f}){Ink("746F62");m.Drum(x+side,.366f,z+j*.282f,.025f,.026f,.011f,6);}
        }
        foreach(float dx in new[]{-.92f,.92f})foreach(float dz in new[]{-.27f,1.14f,2.68f}){
            Pole(At(x+dx,-1.4f,z+dz),At(x+dx,.72f,z+dz),.101f,.115f,"887963");
            Pole(At(x+dx,.64f,z+dz),At(x+dx,.72f,z+dz),.128f,.126f,"B4A285");
        }
        Ink("ABAA8A");m.Ring(At(x+.61f,.371f,z+.31f),.141f,.172f,15);m.Ring(At(x+.61f,.382f,z+.31f),.143f,.175f,15);
        if(site==2||site==3){LifeRing(At(x+.98f,.54f,z+1.1f),.22f);Barrel(At(x+1.28f,Ground(-.83f),-.83f),.49f);}
    }
    void LifeRing(Vector3 p,float r)
    {
        for(int j=0;j<18;j++){
            float a=j*Mathf.PI/9,b=(j+1)*Mathf.PI/9;
            Pole(p+At(Mathf.Cos(a)*r,Mathf.Sin(a)*r,0),p+At(Mathf.Cos(b)*r,Mathf.Sin(b)*r,0),r*.25f,r*.25f,j%5<2?"F4E0AF":"C67B5E",Cloth,7);}
    }
    void Barrel(Vector3 p,float s){
        Ink("A28D6C",Wood);m.Drum(p.x,p.y,p.z,s*.42f,s*.47f,s*.85f,12);
        for(int j=0;j<2;j++){Ink("7E8881");m.Drum(p.x,p.y+s*(.16f+j*.47f),p.z,s*.45f,s*.47f,s*.056f,12);}
        Ink("BFAC87",Wood);m.Drum(p.x,p.y+s*.85f,p.z,s*.43f,s*.43f,s*.07f,12);
    }
    void TravelSign()
    {
        float x=-6.05f,z=-3.06f,y=Ground(z);Pole(At(x,y,z),At(x,y+1.29f,z),.080f,.076f,"8A765E");
        Box(At(x-.02f,y+1.07f,z),At(.88f,.32f,.10f),"B69B72",Wood,8,0,3);
        Box(At(x-.02f,y+1.065f,z-.061f),At(.73f,.21f,.019f),"507D7A",Painted,8,0,3);
        Rail(At(x-.21f,y+1.04f,z-.088f),At(x+.19f,y+1.085f,z-.138f),.037f,.024f,"E9D5A5",Painted);
        Rail(At(x-.21f,y+1.05f,z-.089f),At(x-.11f,y+1.13f,z-.101f),.044f,.024f,"E9D5A5",Painted);
    }
    void SupplyTable()
    {
        float x=6.04f,z=-3.17f,y=Ground(z);B(x,y+.53f,z,1.01f,.12f,.62f,"AF956F",Wood);
        foreach(float a in new[]{-.38f,.38f})foreach(float b in new[]{-.22f,.22f})B(x+a,y+.26f,z+b,.085f,.53f,.090f,"84755F",Wood);
        B(x-.14f,y+.71f,z,.46f,.26f,.36f,"658989",Painted);B(x-.14f,y+.86f,z,.50f,.070f,.41f,"ABC4AC",Painted);
        B(x+.23f,y+.67f,z+.10f,.18f,.16f,.16f,"BC8E73",Painted);Barrel(At(x+1.0f,y,z+.16f),.70f);
    }
    void Coastline()
    {
        string[] stone=site==3?new[]{"9E8F84","B3A497","968D84"}:new[]{"A7A99B","969D94","B3B3A4"};
        for(int j=0;j<68;j++){
            float x=-15f+30f*Mathf.Repeat(j*.618034f,1),z=Shoreline(x)+.30f*Mathf.Sin(j*3.17f)-.17f;
            if(Mathf.Abs(x-6.30f)<1.22f||site==2&&j%3!=0)continue;
            float s=.075f+.17f*Mathf.Repeat(j*.427f,1);Blob(At(x,.145f,z),At(s*1.33f,s*.65f,s),stone[j%3],Rock,j,7);
        }
        for(int j=0;j<14;j++)
        {
            float x=-10f+j*1.54f,z= -.50f-.43f*Mathf.Abs(Mathf.Sin(j*1.92f));
            if(Mathf.Abs(x)<2.6f||Mathf.Abs(x-6.3f)<1.22f)continue;
            if(site<2)Reeds(x,z,site==0?.47f:.72f,j,9);
            else if(site==3){Blob(At(x,.18f,z+.88f),At(.58f,.36f,.48f),"A79E92",Rock,j,8);Reeds(x-.13f,z,.31f,j,5);}
        }
        if(site<2){
            for(int j=0;j<41;j++){
                float x=(j%2==0?-1:1)*(3.7f+4.7f*Mathf.Repeat(j*.618f,1));
                float z=-.91f-1.92f*Mathf.Repeat(j*.382f+.12f*Mathf.Sin(j),1);
                if(Mathf.Abs(x-6)<1.03f)continue;float y=Ground(z),h=.16f+j%5*.037f;
                Pole(At(x,y,z),At(x+.030f,y+h,z),.013f,.009f,"83956B",Foliage,5);
                for(int k=0;k<4;k++){float a=k*Mathf.PI*.5f;
                    Blob(At(x+.03f+Mathf.Cos(a)*.039f,y+h,z+Mathf.Sin(a)*.039f),At(.049f,.025f,.045f),
                        site==0?(j%3==0?"D3B9CE":"F4D69E"):"EAE6C6",Painted,k+j,5);}
                Blob(At(x+.03f,y+h+.025f,z),At(.031f,.025f,.030f),"D5AE6C",Painted,j,6);}
        }else if(site==3){
            for(int j=0;j<22;j++){float x=-7.5f+14*Mathf.Repeat(j*.618f,1),z=-.24f-Mathf.Repeat(j*.418f,1)*1.13f;
                if(Mathf.Abs(x)<2.1f||Mathf.Abs(x-6.4f)<1.1f)continue;Ink(j%3==0?"D7C4AB":"A8AB80",Foliage);
                for(int k=0;k<4;k++)m.Leaf(At(x,Ground(z)+.02f,z),.15f+j%3*.042f,.030f,j*27+k*73,.24f);}
        }
    }
    void Reeds(float x,float z,float scale,int seed,int count)
    {
        for(int j=0;j<count;j++){
            float a=(seed+j)*2.399f,rad=.11f+.30f*j/count;
            Vector3 p=At(x+Mathf.Cos(a)*rad,z<0?Ground(z):-.055f,z+Mathf.Sin(a)*rad);
            float h=scale*(.63f+.36f*Mathf.Abs(Mathf.Sin(j*2.31f+seed)));
            Ink(j%2==0?"829A69":"97A977",Foliage);m.Leaf(p,h*.80f,h*.057f,a*57,h> .4f?.96f:.77f);
            if(j%3==0){var end=p+At(h*.15f,h,0);Pole(p,end,.013f,.010f,"929660",Foliage,5);
                Blob(end,At(.026f,.089f,.028f),site==1?"A89268":"A9AE81",Foliage,j,6);}
        }
    }
    void Hill(float x,float z,float w,float h,float d,string c,int seed){
        Blob(At(x,-.74f,z),At(w,h,d),c,Distant,seed,13);}
    void Ridge(float z,float height,float seed,string color)
    {
        Vector3 P(int i,int row){
            float x=-72+i*2.4f;float valley=.24f+.70f*Mathf.PerlinNoise(x*.055f+seed,seed*.17f)+.16f*Mathf.Sin(x*.17f+seed);
            float shoulder=row==0?-.4f:row==1?.40f:row==2?1f:.26f;
            return At(x,(valley*height)*shoulder-.41f,z+row*3.2f+1.17f*Mathf.Sin(x*.15f+seed+row));}
        Ink(color,Distant);for(int i=0;i<60;i++)for(int row=0;row<3;row++)m.Quad(P(i,row),P(i,row+1),P(i+1,row+1),P(i+1,row),Vector3.up);
    }
    void Tree(float x,float y,float z,float s,int seed,bool pine=false)
    {
        Vector3 P(float a,float b,float c)=>At(x+a*s,y+b*s,z+c*s);
        Pole(P(0,0,0),P(.08f,2.50f,.06f),s*.15f,s*.074f,"847964");
        if(pine){
            for(int j=0;j<4;j++){Ink(j%2==0?"638875":"759680",Foliage);
                m.Tube(P(0,1.06f+j*.46f,0),P(.028f,2.35f+j*.40f,0),s*(.85f-j*.15f),s*.043f,9);}return;}
        for(int j=0;j<5;j++){
            float a=(j+seed)*2.399f;var p=P(Mathf.Cos(a)*.69f,1.74f+j*.24f,Mathf.Sin(a)*.60f);
            if(j<3)Pole(P(0,.93f,0),p,s*.067f,s*.038f,"8E826B");
            Blob(p,At(.97f,.65f,.91f)*s,j%3==0?"79967B":j%3==1?"8AA07F":"A6B18B",Foliage,seed+j,10);}
    }
    void Headwaters()
    {
        Ridge(44,14.4f,32.4f,"92A9AA");Ridge(32.7f,9.1f,19.2f,"89A39A");Ridge(23.6f,5.40f,14.37f,"759383");
        for(int j=0;j<10;j++){float x=-28f+j*6f;Hill(x,23.7f+Mathf.Cos(j)*1.38f,5.4f,2.05f+j%3*.38f,3.71f,"91A58C",j+24);}
        for(int j=0;j<46;j++){float x=-30f+j*1.35f+.28f*Mathf.Sin(j*2.34f);
            Tree(x,.47f,24.4f+Mathf.Sin(j*2)*.84f,.77f+.67f*Mathf.PerlinNoise(j*.43f,2.68f),j,j%5!=1);}
        Hill(-17f,20.9f,5.1f,3.05f,5.63f,"829984",15);Hill(-4.9f,23.6f,3.6f,3.12f,4.98f,"8BA18A",6);
        for(int j=0;j<9;j++){
            float a=j*2.4f;float x=j<5?-14.72f:-5.66f,z=19.60f;
            Blob(At(x+Mathf.Cos(a)*1.20f, .34f+.21f*(j%3),z+Mathf.Sin(a)*.79f),At(1.33f,.91f,1.08f),"A0A595",Rock,j+20);}
        Tree(-16.27f,1.29f,19.81f,1.24f,31);Tree(-4.49f,.92f,21.16f,.97f,13);
        for(int j=0;j<3;j++){
            float x=-15+j*3.19f,z=19.23f+j*.14f;
            for(int i=0;i<2;i++)Pole(At(x+i*.35f,-.07f,z),At(x+i*.35f,2.0f,z),.15f,.13f,"927B65");
        }
        for(int j=0;j<25;j++){
            float f=j/24f,x=-15.3f+9.3f*f,y=1.48f+Mathf.Sin(f*Mathf.PI)*.56f;
            B(x,y,19.50f,.386f,.14f,1.26f,"B39E77",Wood);
            if(j%4==0){foreach(float z in new[]{18.93f,20.04f})B(x,y+.37f,z,.070f,.73f,.073f,"A46F58",Wood);}
            if(j<24)foreach(float z in new[]{18.93f,20.04f}){
                float n=(j+1)/24f;Rail(At(x,y+.7f,z),At(-15.3f+9.3f*n,2.18f+Mathf.Sin(n*Mathf.PI)*.56f,z),.072f,.078f,"BD8162");}
        }
        for(int i=0;i<20;i++){float a=i*2.40f,x=8.63f+Mathf.Cos(a)*(1.50f+i%3*.73f),z=15.6f+Mathf.Sin(a)*1.78f;
            Blob(At(x,.10f+i%4*.31f,z),At(1.06f+i%3*.15f,.75f+i%3*.40f,1.11f),"96A397",Rock,i+15);}
        Tree(-10f,.23f,-1.24f,1.38f,6);Tree(11.8f,.27f,2.0f,1.19f,7,true);
    }
    void Lake()
    {
        Ridge(44,11.3f,5.65f,"95ACB9");Ridge(36.5f,5.8f,14.94f,"A2B6B1");
        Hill(-13,18.4f,5.55f,1.76f,3.36f,"9EAE87",34);Hill(11.2f,21.2f,6.23f,2.12f,3.7f,"A6B18B",47);
        for(int j=0;j<11;j++){
            bool left=j<5;float a=(j+3)*2.399f;
            Tree((left?-13f:11.2f)+Mathf.Cos(a)*2.51f,-.06f,(left?19.31f:21.2f)+Mathf.Sin(a)*.94f,.73f+j%3*.17f,j,j%3==0);}
        float x=7.2f,z=13.75f;
        foreach(float a in new[]{-1.12f,1.12f}){
            Pole(At(x+a,-.38f,z),At(x+a,2.68f,z),.16f,.12f,"B67561",Painted,12);
            Pole(At(x+a,-.08f,z),At(x+a,.45f,z),.174f,.154f,"687D78",Rock,12);}
        B(x,1.86f,z,3.04f,.144f,.25f,"BA7B62",Painted);B(x,2.54f,z,3.47f,.20f,.38f,"B97B62",Painted);
        B(x,2.70f,z,3.72f,.13f,.47f,"687A73",Painted);B(x,2.18f,z,.32f,.56f,.096f,"716E57",Wood);
        Boat(-7.4f,11.5f,1.00f,24,false);Boathouse(-13.1f,.6f,16.8f,1.20f,"839FA0");
        for(int i=0;i<9;i++)Reeds(-8.4f+i*.32f,4.81f+Mathf.Sin(i)*.53f,.54f,i,6);
        for(int j=0;j<18;j++){
            float a=j*2.39f,xp=-6.82f+Mathf.Cos(a)*(.32f+j*.066f),zp=4.40f+Mathf.Sin(a)*(.22f+j*.037f);
            Ink(j%2==0?"99AB80":"869F80",Foliage);m.Ring(At(xp,.068f,zp),.001f,.15f+.05f*(j%3),14,22+j*19,352+j*19);
            if(j%5==0)for(int i=0;i<5;i++){Ink("E7DCC5",Painted);m.Leaf(At(xp,.087f,zp),.120f,.033f,i*72,.52f);}
        }
        Tree(-9.3f,.26f,-1.70f,1.18f,17);Tree(10.09f,.20f,-1.86f,1.31f,5);
    }
    void Harbour()
    {
        Ridge(44,8.8f,44.7f,"99AEAD");Ridge(34,3.71f,57.2f,"A8B5A8");
        B(-2.3f,.28f,22.2f,45,1.3f,4.0f,"9A9F94",Rock);
        for(int j=0;j<8;j++){
            float x=-18f+j*4.11f,s=.79f+(j%3)*.18f;
            if(j==3){
                B(x,2.31f,23.5f,3.26f,2.75f,2.50f,"9BA5A0",Painted);
                B(x,3.76f,23.5f,3.55f,.17f,2.76f,"71888B",Painted);
                B(x+.84f,4.26f,23.61f,.32f,1.03f,.37f,"A78D73",Rock);
                for(int w=0;w<4;w++)B(x-1.12f+w*.73f,2.85f,22.21f,.50f,.50f,.06f,"627F88",Painted);
                B(x,1.57f,22.18f,.9f,1.25f,.12f,"8D826C",Wood);
            }else Boathouse(x,.92f,23.2f+j%2*1.31f,s,j%3==0?"B88371":j%3==1?"879BA0":"A4A987");
        }
        Boat(7.44f,11.62f,1.43f,-23,true);Boat(-8.58f,15.2f,1.13f,32,true);
        for(int j=0;j<7;j++){float x=-9.5f+j*2.82f;Pole(At(x,-.5f,18.7f),At(x,1.24f,18.7f),.16f,.13f,"827967");
            B(x+.49f,.65f,19.20f,2.8f,.18f,.8f,"988870",Wood);}
        for(int j=0;j<3;j++){float x=-5.78f+j*4.30f,z=10.3f+j%2*2.52f;Buoy(x,z,j);}
        Boathouse(-12.7f,.35f,-.85f,1.01f,"7E9690");Barrel(At(-8.2f,.32f,-2.11f),.84f);
        for(int j=0;j<5;j++){
            float x=-6.91f+(j%3)*.55f,z=-1.22f+(j/3)*.48f,y=Ground(z);
            B(x,y+.19f,z,.49f,.36f,.42f,j%2==0?"AA9172":"91A6A0",Wood);
            foreach(float side in new[]{-.18f,.18f})B(x+side,y+.20f,z-.22f,.036f,.36f,.033f,"BDAC8A",Wood);}
        for(int p=0;p<2;p++){
            float x=p==0?-9.7f:10.4f,z=20.84f;Pole(At(x,.74f,z),At(x,4.81f,z),.060f,.045f,"8A8B79");
            Vector3 last=At(x,4.6f,z);
            for(int j=1;j<=9;j++){float f=j/9f;var next=At(x+4.3f*f,4.61f-.88f*Mathf.Sin(f*Mathf.PI)-f*.3f,z+f*.31f);
                Rail(last,next,.018f,.021f,"8D9078",Cloth);if(j%2==1){Ink(j%3==0?"DBAD77":"A1BDC0",Cloth);
                    m.Tri(last,next,(last+next)*.5f+At(.054f,-.30f,0),Vector3.back);}
                last=next;}}
    }
    void Reef()
    {
        for(int j=0;j<8;j++){float a=j*2.43f,x=-10.8f+Mathf.Cos(a)*3.04f,z=14.1f+Mathf.Sin(a)*2.39f;
            Blob(At(x,.06f+j%2*.37f,z),At(1.55f+(j%3)*.43f,.82f+j%3*1.00f,1.56f),"8D9395",Rock,28+j,8);}
        Hill(8.87f,20.2f,4.66f,1.71f,3.5f,"929D91",16);
        float x0=8.40f,z0=19.2f;
        for(int j=0;j<5;j++){float r=.65f-j*.056f;Pole(At(x0,1.0f+j*.80f,z0),At(x0,1.8f+j*.80f,z0),r,r-.053f,j==2?"C8806D":"E8DFBF",Painted,13);}
        Pole(At(x0,5.01f,z0),At(x0,5.20f,z0),.63f,.66f,"6C8586",Painted,12);
        Pole(At(x0,5.19f,z0),At(x0,5.8f,z0),.353f,.37f,"D8CC93",Glow,12);
        for(int j=0;j<8;j++){float a=j*Mathf.PI*.25f;Pole(At(x0+Mathf.Cos(a)*.39f,5.17f,z0+Mathf.Sin(a)*.39f),
            At(x0+Mathf.Cos(a)*.39f,5.87f,z0+Mathf.Sin(a)*.39f),.034f,.034f,"688486",Painted);}
        Pole(At(x0,5.83f,z0),At(x0,6.28f,z0),.59f,.073f,"9E7160",Painted,12);
        Boat(-7.1f,8.50f,.89f,-36,false);Tree(11.33f,.03f,20.4f,.91f,22,true);Tree(10.8f,.10f,19.6f,.75f,11,true);
        for(int j=0;j<12;j++){float a=j*2.399f;float x= -7.9f+Mathf.Cos(a)*1.2f,z=.88f+Mathf.Sin(a)*1.17f;
            Blob(At(x,-.02f,z),At(.54f,.44f+j%3*.11f,.41f),"A69C89",Rock,j,7);}
        Buoy(5.71f,8.91f,2);
    }
    void Buoy(float x,float z,int k){
        Ink(k%2==0?"CE8F71":"C9B16E",Painted);m.Drum(x,-.08f,z,.24f,.23f,.26f,10);m.Drum(x,.18f,z,.23f,.075f,.22f,10);
        Pole(At(x,.36f,z),At(x,.85f,z),.022f,.021f,"798981");Blob(At(x,.85f,z),At(.067f,.067f,.067f),"DABD81",Glow,k,7);}
    void Boathouse(float x,float y,float z,float s,string c)
    {
        B(x,y+.77f*s,z,2.87f*s,1.54f*s,2.13f*s,"C3B99C",Wood);
        B(x,y+.75f*s,z-1.072f*s,.91f*s,1.46f*s,.07f*s,"879994",Wood);
        foreach(float a in new[]{-.90f,.9f}){B(x+a*s,y+.88f*s,z-1.084f*s,.50f*s,.53f*s,.059f*s,"68868A",Painted);
            B(x+a*s,y+.88f*s,z-1.124f*s,.040f*s,.53f*s,.032f*s,"CAAE83",Wood);}
        foreach(int sign in new[]{-1,1}){
            Box(At(x,y+1.80f*s,z+sign*.60f*s),At(3.19f*s,.13f*s,1.42f*s),c,Painted,0,sign*24);
        }
        B(x,y+2.09f*s,z,3.18f*s,.13f*s,.20f*s,c,Painted);
        B(x,y+1.56f*s,z-1.09f*s,2.96f*s,.105f*s,.14f*s,"93806A",Wood);
    }
    void Boat(float x,float z,float s,float yaw,bool cabin)
    {
        Quaternion q=Quaternion.Euler(0,yaw,0);Vector3 P(float a,float y,float b)=>At(x,0,z)+q*At(a,y,b)*s;
        float[] length={-1.87f,-1.46f,-.72f,.62f,1.46f,1.98f};float[] width={.25f,.65f,.74f,.70f,.43f,.036f};
        for(int j=0;j<length.Length-1;j++){
            for(int k=-1;k<=1;k+=2){
                Ink(cabin?"71979D":"9B9F88",Painted);m.Quad(P(width[j]*k*.71f,-.16f,length[j]),P(width[j+1]*k*.71f,-.16f,length[j+1]),
                    P(width[j+1]*k,.39f,length[j+1]),P(width[j]*k,.39f,length[j]),q*Vector3.right*k);
                Rail(P(width[j]*k,.39f,length[j]),P(width[j+1]*k,.39f,length[j+1]),.069f*s,.063f*s,"D1BA92");}
            Ink("AE9777",Wood);m.Quad(P(-width[j],.32f,length[j]),P(-width[j+1],.32f,length[j+1]),P(width[j+1],.32f,length[j+1]),P(width[j],.32f,length[j]),Vector3.up);}
        if(cabin){
            m.Ink("D7CFB5",Painted);m.Box(P(0,.70f,-.40f),At(.93f,.68f,1.14f)*s,q);
            m.Ink("687F83",Painted);m.Box(P(0,.84f,-1.0f),At(.68f,.32f,.044f)*s,q);
            m.Ink("C38D71",Painted);m.Box(P(0,1.09f,-.42f),At(1.10f,.13f,1.34f)*s,q);
            Pole(P(.28f,1.1f,-.48f),P(.28f,2.40f,-.49f),.028f*s,.019f*s,"A89A7C");}
        else for(int j=0;j<3;j++){m.Ink("BCA57F",Wood);m.Box(P(0,.41f,-.96f+j*.79f),At(1.03f,.066f,.20f)*s,q);}
    }
}
