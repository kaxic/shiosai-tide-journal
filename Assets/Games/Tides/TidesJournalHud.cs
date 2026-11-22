using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class TidesHud
{
    static readonly Dictionary<string,Texture2D> fishArt=new Dictionary<string,Texture2D>();
    static Texture2D silhouette;

    void Journal()
    {
        Rect p=Book(961,546,"THE ANGLER'S FIELD NOTES","水辺の魚類手帖");
        int count=game.Fish.Length,selected=Mathf.Clamp(game.Selection,0,count-1);
        bool compact=Screen.width<800;
        int pageSize=compact?8:12;
        int first=selected/pageSize*pageSize;
        float x=p.x+20,y=p.y+111,left=500,rx=x+left+19,rw=p.xMax-rx-20;
        string collected=game.Discovered==count?"全種採集":$"採集 {game.Discovered} / {count} 種";
        Text(collected+$"　　釣果 {game.TotalCaught} 尾",x,p.y+74,left,28,15,Reed);
        float width=(left-18)/4,height=compact?158:104;
        for(int i=first;i<Mathf.Min(first+pageSize,count);i++)
        {
            int cell=i-first;
            Rect r=new Rect(x+cell%4*(width+6),y+cell/4*(height+6),width,height);
            bool found=game.SpeciesCaught(i)>0;
            Row(r,i==selected);
            float pictureHeight=compact?92:57;
            FishPicture(new Rect(r.x+5,r.y+4,r.width-10,pictureHeight),i,found);
            if(game.IsFavorite(i))Text("★",r.xMax-22,r.y+2,17,18,12,Ember,align:TextAnchor.MiddleCenter);
            Text(game.Fish[i].name,r.x+4,r.y+pictureHeight+6,r.width-8,compact?36:23,compact?16:14,Ink,align:TextAnchor.MiddleCenter);
            string mark=i==game.TrackedFish?"追跡中":found?$"{game.SpeciesCaught(i)} 尾":"未採集";
            Text(mark,r.x+4,r.yMax-20,r.width-8,17,12,i==game.TrackedFish?Ember:Reed,align:TextAnchor.MiddleCenter);
            if(Hit(r))game.Highlight(i);
        }
        selected=Mathf.Clamp(game.Selection,0,count-1);
        var species=game.Fish[selected];
        bool recorded=game.SpeciesCaught(selected)>0;
        FishPicture(new Rect(rx,p.y+83,rw,115),selected,recorded);
        Text(species.name,rx+1,p.y+204,rw-(species.legendary?87:2),34,23,Ink,true);
        if(species.legendary)Text("水域の大物",rx+rw-82,p.y+204,81,34,14,Ember,align:TextAnchor.MiddleRight);
        Text(recorded?species.note:"この魚はまだ記録されていません。条件を手がかりに水辺を調べましょう。",rx+1,p.y+240,rw-2,49,14,Reed);
        Line(rx,p.y+294,rw);
        Text("釣れる条件",rx+1,p.y+298,rw-2,24,15,Water);
        Text(game.FishHint(selected),rx+1,p.y+323,rw-2,65,14,Ink);
        Line(rx,p.y+394,rw);
        string record=recorded?$"最大 {game.BestLength(selected):0.0} cm　／　{FormatWeight(game.BestWeight(selected))}":"最大記録　未採集";
        Text(record,rx+1,p.y+398,rw-2,33,15,Ink);
        float favWidth=98;
        if(Button(new Rect(rx,p.yMax-59,favWidth,38),game.IsFavorite(selected)?"★ お気入り":"☆ お気入り",recorded,false))game.ToggleFavorite(selected);
        if(Button(new Rect(rx+favWidth+6,p.yMax-59,rw-favWidth-6,38),selected==game.TrackedFish?"追跡中の魚":"この魚を追跡",true))game.Track(selected);
        if(Button(new Rect(x,p.yMax-59,89,36),"前の頁",first>0,false))game.Highlight(first-pageSize);
        if(Button(new Rect(x+95,p.yMax-59,89,36),"次の頁",first+pageSize<count,false))game.Highlight(first+pageSize);
        Text($"{first/pageSize+1} / {(count+pageSize-1)/pageSize} 頁",x+195,p.yMax-58,101,35,14,Reed);
    }

    void Landing()
    {
        bool landed=game.Mode==TidesMode.Catch;
        Rect p=Book(landed?733:556,landed?353:248,landed?"CATCH RECORD":"WATER NOTES",landed?"釣果の記録":"魚が離れました",false);
        float x=p.x+20,y=p.y+84;
        if(landed)
        {
            int i=game.FishIndex;
            if(i<0||i>=game.Fish.Length)i=Array.FindIndex(game.Fish,f=>f.name==game.LandingName);
            FishPicture(new Rect(x,y,263,194),i,true);
            float rx=x+280,rw=p.xMax-rx-20;
            Text(game.IsRecord?"自己記録を更新":"取り込み成功",rx,y,rw,25,15,Ember);
            Text(game.LandingName,rx,y+28,rw,39,26,Ink,true);
            Text($"{game.LandingLength:0.0} cm　／　{FormatWeight(game.LandingWeight)}",rx,y+77,rw,34,19,Ink);
            Text(game.GradeName+"　　査定 "+game.LandedValue+" 円",rx,y+119,rw,30,15,Reed);
            Text("釣った魚は魚籠と手帖に記録されます。",rx,y+156,rw,34,14,Reed);
            if(Button(new Rect(x,p.yMax-57,194,38),"F8　釣果カードを保存",true,false))game.GetComponent<TidesCatchPrint>().Take();
            Text("F9　アルバムを開く",x+201,p.yMax-57,165,38,14,Reed);
        }
        else
        {
            Text(game.LastLog,x,y,p.width-40,51,18,Ink,true);
            Text("魚が走る間は糸を緩め、休む間に巻き寄せましょう。",x,y+57,p.width-40,40,15,Reed);
        }
        if(Button(new Rect(p.xMax-242,p.yMax-57,222,38),"Space　水辺に戻る",true))game.Action();
    }

    public static string FormatWeight(float kg)=>kg<1 ? $"{Mathf.RoundToInt(kg*1000)} g" : $"{kg:0.00} kg";

    void FishPicture(Rect frame,int index,bool revealed)=>DrawFish(game,frame,index,revealed);

    public static void DrawFish(TidesGame game,Rect frame,int index,bool revealed=true)
    {
        game.Box(frame,Page);
        if(Event.current.type!=EventType.Repaint)return;
        if(revealed && index>=0 && index<game.Fish.Length)
        {
            string id=game.Fish[index].id;
            if(!fishArt.TryGetValue(id,out var image))
            {
                image=Resources.Load<Texture2D>("Tides/Fish/"+id);
                fishArt.Add(id,image);
            }
            if(image){GUI.DrawTexture(frame,image,ScaleMode.ScaleToFit);return;}
            int cell=game.SpecimenIndex(index);
            var atlas=game.skin.specimenPlate;
            if(cell>=0 && atlas)
            {
                float aspect=atlas.width/(float)atlas.height*4f/6f;
                float w=Mathf.Min(frame.width,frame.height*aspect),h=w/aspect;
                game.Plate(new Rect(frame.center.x-w/2,frame.center.y-h/2,w,h),cell,6,4);
                return;
            }
        }
        if(!silhouette)silhouette=MakeSilhouette();
        Color old=GUI.color;
        GUI.color=revealed?Water:new Color32(148,157,135,255);
        float sw=Mathf.Min(frame.width*.78f,frame.height*1.62f),sh=sw/2;
        GUI.DrawTexture(new Rect(frame.center.x-sw/2,frame.center.y-sh/2,sw,sh),silhouette);
        GUI.color=old;
    }
    static Texture2D MakeSilhouette()
    {
        const int w=96,h=48;
        var pixels=new Color32[w*h];
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)
        {
            float dx=(x-55)/36f,dy=(y-23.5f)/10.7f;
            bool body=dx*dx+dy*dy<1;
            float tail=Mathf.Abs(y-23.5f);
            bool fork=x>=3&&x<=26 && tail<(27-x)*.68f+2 && x>12-tail*.73f;
            bool fin=x>=35&&x<=62&&y>25&&y<40-Mathf.Abs(x-46)*.6f;
            bool lower=x>=39&&x<53&&y<19&&y>11+Mathf.Abs(x-43)*.48f;
            if(body||fork||fin||lower)pixels[y*w+x]=new Color32(255,255,255,255);
        }
        var texture=new Texture2D(w,h,TextureFormat.RGBA32,false){name="Fish field journal silhouette",hideFlags=HideFlags.HideAndDontSave,wrapMode=TextureWrapMode.Clamp};
        texture.SetPixels32(pixels);texture.Apply(false,true);return texture;
    }
}
