using System;
using UnityEngine;
using static TidesMesh;
using static TidesMesh.Finish;

internal static class TidesFigures
{
    static readonly string[] Names={"oikawa","ayu","yamame","iwana","ugui","sakuramasu","wakasagi","funa","tanago","koi","nijimasu","biwamasu",
        "haze","aji","mebaru","kurodai","suzuki","akame","saba","kasago","kawahagi","madai","kanpachi","hiramasa",
        "kawamutsu","numamutsu","mugitsuku","shimadojo","kajika","ito","shirauo","sayori","bora","amimehagi","konoshiro","hoshigarei","chouchouuo","budai","oyabiccha","nizadai","akahata","rouninaji","kamasu","tobiuo","shiira","katsuo","binnaga","kurokajiki"};
    static readonly string[] Backs={"73949B","84937D","87876E","788376","758781","939889","9DA8A5","85856C","8BA8A2","9A876B","8E9581","83918D",
        "8C816B","729D9E","9A7764","797F7B","829792","7A8C83","658994","AB745A","9E967A","BE8F84","7E9D95","7D9D90",
        "648F89","848785","AA9158","93794B","887D67","88786B","C5D3CB","8CAEB0","779A8B","8A9A64","6F99A9","978B71","CDAC47","3B8A86","4D859E","638878","AA5D4D","557F93","678D8D","577D9F","3A9A95","4E7294","536B91","38567F"};
    static readonly string[] Flanks={"B9AAA8","CCD3B3","B7B59C","B8B998","C7BBB0","C7B4AC","D0D6C7","B9AA87","B5C2C7","C2AA82","C3ADA5","C1B59E",
        "B8AC8F","BDCEB5","B39778","B2B5AB","C2C6AF","B2B7A2","ACBBC1","C89379","C6B798","DCB6A4","B8C7A0","BECAA5",
        "BFB795","BEC9AF","CDB986","DED2A5","B0A58A","BBAD90","E0E6D7","CED9D2","C0CABC","BAC994","BCD7D2","BDAD8F","ECDC6B","75B9AA","C8BF81","9CB89D","DF9472","9CBAC2","CFD5BD","ACC5C6","D7C55F","A2B5B8","ACC1C3","7496AF"};
    public static void Angler(TidesMesh m,int part)
    {
        void o(Vector3 p,Vector3 r,string c,Finish f=Cloth,int seed=4){m.Ink(c,f);m.Blob(p,r,seed,10);}
        void box(Vector3 p,Vector3 size,string c,Finish f=Cloth){m.Ink(c,f);m.Box(p,size,Quaternion.identity);}
        void tube(Vector3 a,Vector3 b,float r,float s,string c,Finish f=Cloth){m.Ink(c,f);m.Tube(a,b,r,s,10);}
        const string skin="DFB895",jacket="6C9290",vest="819071",trousers="6C7772",boots="716859";
        if(part==1||part==2){
            float sign=part==1?1:-1;Vector3 elbow=At(sign*.052f,-.230f,.051f),wrist=At(sign*.012f,-.287f,.237f);
            tube(Vector3.zero,elbow,.108f,.081f,jacket);tube(elbow,wrist,.081f,.066f,"739C98");
            tube(wrist,wrist+At(0,-.012f,.045f),.070f,.070f,"A0B0A0");o(wrist+At(0,0,.082f),At(.063f,.058f,.079f),skin,Plain);return;
        }
        if(part==3||part==4){
            float sign=part==3?1:-1;tube(Vector3.zero,At(sign*.021f,-.37f,0),.113f,.084f,trousers);
            tube(At(sign*.021f,-.328f,0),At(sign*.024f,-.554f,.013f),.095f,.081f,boots);o(At(sign*.024f,-.555f,.066f),At(.101f,.069f,.158f),boots);return;
        }
        o(At(0,.78f,0),At(.223f,.21f,.148f),trousers);
        tube(At(0,.75f,0),At(0,1.12f,.015f),.188f,.233f,jacket);
        o(At(0,.96f,.002f),At(.239f,.235f,.173f),vest);
        box(At(0,1.025f,.169f),At(.055f,.28f,.015f),"5A776D");box(At(0,.873f,-.158f),At(.287f,.18f,.055f),"98A283");
        box(At(-.157f,.905f,.141f),At(.10f,.12f,.065f),"C4B18D");box(At(.161f,.91f,.142f),At(.105f,.115f,.069f),"B2AA87");
        tube(At(0,1.12f,0),At(0,1.23f,0),.094f,.101f,skin,Plain);
        o(At(0,1.36f,.012f),At(.183f,.213f,.160f),skin,Plain);
        o(At(0,1.42f,-.042f),At(.182f,.157f,.146f),"655E54");
        foreach(float x in new[]{-.065f,.065f})o(At(x,1.39f,.153f),At(.015f,.018f,.011f),"555653",Plain);
        m.Ink("C98157",Cloth);m.Drum(0,1.467f,0,.263f,.290f,.042f,16);
        m.Ink("DC9B65",Cloth);m.Drum(0,1.499f,0,.211f,.171f,.124f,14);
        m.Ink("AC795B",Cloth);m.Drum(0,1.504f,0,.215f,.203f,.043f,14);
        m.Ink("DEB578",Cloth);m.Drum(0,1.626f,0,.171f,.159f,.021f,14);
        tube(At(-.187f,1.10f,-.106f),At(.168f,.771f,-.135f),.037f,.036f,"D2B492");
        box(At(.202f,.72f,-.057f),At(.17f,.22f,.21f),"AE8C68",Wood);
        box(At(.220f,.79f,-.058f),At(.17f,.058f,.224f),"C09D76",Wood);
        m.Ink("BE7D60",Cloth);m.Drum(0,1.164f,0,.116f,.112f,.064f,10);
    }
    public static int Kind(string key){int k=Array.IndexOf(Names,key);return k<0?0:k;}
    public static void Fish(TidesMesh m,int k)
    {
        bool deep=k==35||k==36||k==37||k==38||k==39||k==40||k==41||k==7||k==8||k==14||k==15||k==19||k==20||k==21;
        bool salmon=k==29||k==2||k==3||k==5||k==10||k==11;
        bool fork=k==13||k==18||k>=21;
        bool bottom=k==27||k==28||k==35||k==12||k==19;float h=deep?.304f:bottom?.173f:.205f;float w=deep?.128f:.149f;
        if(k==27||k==30||k==31||k==42) {h*=.59f;w*=.67f;}
        if(k==35){h*=.35f;w*=2.10f;}
        if(k==44){h*=1.33f;}
        float[] x={-.56f,-.42f,-.23f,.035f,.28f,.456f,.54f};
        float[] section={.19f,.46f,.85f,1f,.87f,.52f,.13f};const int n=12;
        var rings=new Vector3[x.Length,n];
        for(int r=0;r<x.Length;r++)for(int j=0;j<n;j++){
            float a=j*Mathf.PI*2/n;float sr=section[r];
            rings[r,j]=At(x[r],Mathf.Sin(a)*sr*h+(bottom?-.02f:.01f)*Mathf.Cos(r*.95f),Mathf.Cos(a)*sr*w);}
        for(int r=0;r<x.Length-1;r++)for(int j=0;j<n;j++){
            int b=(j+1)%n;float height=Mathf.Sin((j+.5f)*Mathf.PI*2/n);
            string color=height>.47f?Backs[k]:height<-.46f?"DBD6BD":Flanks[k];
            m.Ink(color,Finish.Fish);m.Quad(rings[r,j],rings[r+1,j],rings[r+1,b],rings[r,b],At(0,height,Mathf.Cos((j+.5f)*Mathf.PI*2/n)));}
        m.Ink(Flanks[k],Finish.Fish);for(int j=0;j<n;j++){int b=(j+1)%n;m.Tri(At(.551f,-.006f,0),rings[6,j],rings[6,b],Vector3.right);}
        void Fin(Vector3 a,Vector3 b,Vector3 c,string shade){
            m.Ink(shade,Finish.Fish);var norm=Vector3.Cross(b-a,c-a).normalized;
            m.Tri(a,b,c,norm);m.Tri(a,c,b,-norm);}
        if(k==31||k==47){
            m.Ink(Backs[k],Finish.Fish);
            m.Tube(At(.50f,-.023f,0),At(k==47?.97f:.78f,-.020f,0),.042f,.003f,8);
        }
        if(k==43||k==46)for(int side=-1;side<=1;side+=2)
            Fin(At(.19f,0,side*w),At(-.45f,-.08f,side*(k==43?.64f:.34f)),At(-.28f,-.105f,side*w),Flanks[k]);
        if(k==35)for(int i=0;i<14;i++){
            float xPos=-.45f+i*.063f,spread=Mathf.Sin(i*Mathf.PI/13);
            for(int side=-1;side<=1;side+=2) Fin(At(xPos,.015f,side*w*spread*.8f),At(xPos-.03f,0,side*w*spread*1.65f),At(xPos+.07f,.02f,side*w*spread*.8f),Backs[k]);
        }
        if(k>=24&&k%3!=0)for(int i=0;i<5;i++){
            float xPos=-.27f+i*.11f;float spread=Mathf.Sqrt(1-Mathf.Pow(xPos/.55f,2));
            for(int side=-1;side<=1;side+=2){
                m.Ink(k==36||k==38?"5E686E":Backs[k],Finish.Fish);
                m.Blob(At(xPos,.01f,side*w*spread),At(.018f,k%2==0?h*.65f:.035f,.009f),i,6);
            }
        }
        string fin=k==21||k==19?"C78F79":k==13||k==22||k==23?"B9AF70":Backs[k];
        float tail=fork?.24f:.172f;Fin(At(-.52f,.044f,0),At(-.81f,tail,.02f),At(-.71f,.012f,0),fin);
        Fin(At(-.52f,.044f,0),At(-.71f,.012f,0),At(-.52f,-.045f,0),fin);
        Fin(At(-.52f,-.045f,0),At(-.71f,.012f,0),At(-.80f,-tail*.86f,-.017f),fin);
        if(!fork)Fin(At(-.79f,tail,0),At(-.77f,-tail*.85f,0),At(-.69f,0,0),fin);
        if(deep||bottom){for(int i=0;i<6;i++){float f=i/5f;Fin(At(-.25f+f*.49f,h*.77f,0),
            At(-.28f+f*.49f,h*(1.14f+f*.53f),0),At(-.15f+f*.49f,h*.83f,0),fin);}}
        else Fin(At(-.18f,h*.8f,0),At(-.14f,h*1.65f,.012f),At(.14f,h*.90f,0),fin);
        if(salmon)Fin(At(-.34f,.09f,0),At(-.32f,.156f,0),At(-.26f,.108f,0),fin);
        for(int side=-1;side<=1;side+=2){
            float s=side;Fin(At(.16f,-h*.05f,w*.98f*s),At(-.032f,-h*.65f,w*1.57f*s),At(-.062f,-.041f,w*1.09f*s),fin);
            Vector3 eye=At(.388f,h*.24f,w*.71f*s);
            m.Ink("DFCF93");m.Blob(eye,At(.044f,.046f,.017f),3,8);
            m.Ink("414F53");m.Blob(eye+At(.004f,.003f,.012f*s),At(.029f,.033f,.011f),8,8);
            m.Ink("EBE5CA");m.Blob(eye+At(.01f,.013f,.020f*s),At(.009f,.011f,.006f),4,6);
            if(salmon||k==18||k==19||k==2||k==15){
                for(int d=0;d<10;d++){float xx=-.35f+d*.065f,rr=Mathf.Sqrt(Mathf.Max(.08f,1f-Mathf.Pow((xx-.017f)/.57f,2)));
                    float yy=(.14f+.35f*Mathf.Abs(Mathf.Sin(d*3.31f)))*h;
                    m.Ink(k==3?"D7C8A1":k==19?"95795F":"718486",Finish.Fish);
                    m.Blob(At(xx,yy,s*w*rr*Mathf.Sqrt(1f-yy*yy/(h*h))*.99f),At(k==18?.033f:.019f,k==15?.049f:.025f,.012f),d,6);}
            }else if(k==13||k==22||k==23||k==0||k==8){
                for(int j=1;j<5;j++)m.Beam(At(x[j],.015f,s*section[j]*w),At(x[j+1],.020f,s*section[j+1]*w),.022f,.019f);
            }
        }
    }
}
