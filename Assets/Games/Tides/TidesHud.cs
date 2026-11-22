using UnityEngine;

public sealed partial class TidesHud
{
    static readonly Color Ink=new Color32(34,54,54,255);
    static readonly Color Deep=new Color32(26,56,62,255);
    static readonly Color Water=new Color32(60,99,102,255);
    static readonly Color Reed=new Color32(89,100,79,255);
    static readonly Color Paper=new Color32(247,238,212,255);
    static readonly Color Page=new Color32(231,221,193,255);
    static readonly Color RuleColor=new Color32(188,179,149,255);
    static readonly Color Light=new Color32(176,220,205,255);
    static readonly Color Ember=new Color32(151,66,45,255);
    static readonly Color Apricot=new Color32(243,174,108,255);
    static readonly Color Select=new Color32(205,221,202,255);

    TidesGame game;
    bool reelButtonHeld;
    int reelControl;

    public void Draw(TidesGame g)
    {
        game=g;
        if(reelButtonHeld && (game.Mode!=TidesMode.Fight || !Input.GetMouseButton(0)))ReleaseReel();
        switch(g.Mode)
        {
            case TidesMode.Explore:
            case TidesMode.Aim:
            case TidesMode.Wait:
            case TidesMode.Bite:
            case TidesMode.Fight: WaterSide();break;
            case TidesMode.Catch:
            case TidesMode.Escape: Landing();break;
            case TidesMode.Travel: Travel();break;
            case TidesMode.Clock: Clock();break;
            case TidesMode.Shop: Shop();break;
            case TidesMode.Journal: Journal();break;
            case TidesMode.Requests: Requests();break;
        }
    }

    void WaterSide()
    {
        Rect site=new Rect(15,14,242,81);
        Card(site,Paper);
        Fill(new Rect(site.x,site.y,4,site.height),Water);
        Text("潮 路 の 釣 り 手 帖",site.x+12,site.y+3,site.width-75,21,13,Water);
        Text($"第 {game.Day} 日",site.x+140,site.y+3,site.width-151,21,13,Reed,align:TextAnchor.MiddleRight);
        Text(TidesGame.SiteNames[game.Site],site.x+11,site.y+23,site.width-22,31,20,Ink,true);
        string sky=TidesGame.TimeNames[game.TimeSlot]+"　"+TidesGame.WeatherNames[game.Weather]+"　"+TidesGame.TideNames[game.Tide];
        Text(sky,site.x+12,site.y+54,site.width-24,21,14,Reed);
        if(game.Mode==TidesMode.Explore&&Hit(site))game.Open(TidesMode.Travel);

        var journal=new Rect(game.W-188,14,173,61);
        Card(journal,Paper);
        Text("魚 の 手 帖",journal.x+11,journal.y+4,120,21,13,Water);
        Text($"記録　{game.Discovered} / {game.Fish.Length}",journal.x+10,journal.y+25,journal.width-20,29,17,Ink,true);
        if(game.Mode==TidesMode.Explore)
        {
            Key(journal.xMax-34,journal.y+6,"H");
            if(Hit(journal))game.Open(TidesMode.Journal);
        }
        if(game.Mode==TidesMode.Fight){Fight();return;}
        if(game.Mode==TidesMode.Bite){Bite();return;}
        if(game.Mode==TidesMode.Aim){Aim();return;}
        if(game.Mode==TidesMode.Wait){Wait();return;}
        Explore();
    }

    void Explore()
    {
        float y=game.H-50;
        Rect action=new Rect((game.W-232)/2,y-48,232,38);
        string nearby=game.Nearby;
        bool byWater=nearby.StartsWith("水辺");
        if(Button(action,byWater?"E ／ Space　竿を構える":"Space　竿を構える",true))game.Action();
        if(byWater)
        {
            int target=game.TrackedFish>=0?game.TrackedFish:game.SpeciesIndex(game.ActiveOrder.fishId);
            nearby=target<0?"":(game.TrackedFish>=0?"追跡中　":"依頼の魚　")+game.Fish[target].name;
        }
        if(!string.IsNullOrEmpty(nearby))
        {
            var place=new Rect(action.x-65,y-77,action.width+130,24);
            Fill(place,Deep);
            Text(nearby,place.x+5,place.y,place.width-10,place.height,13,Paper,align:TextAnchor.MiddleCenter);
        }
        float x=15;
        if(Shortcut(x,y,"T","釣り場"))game.Open(TidesMode.Travel);x+=103;
        if(Shortcut(x,y,"R","時刻"))game.Open(TidesMode.Clock);x+=103;
        if(Shortcut(x,y,"B","釣具店"))game.Open(TidesMode.Shop);x+=103;
        if(Shortcut(x,y,"H","手帖"))game.Open(TidesMode.Journal);x+=103;
        if(Shortcut(x,y,"J","依頼"))game.Open(TidesMode.Requests);
        Rect practice=new Rect(x+103,y,148,36);
        if(Button(practice,game.Practice?"P　ゆっくり釣り":"P　通常の釣り",true,false))game.TogglePractice();
        var bait=new Rect(game.W-226,y,211,36);
        var move=new Rect(practice.xMax+5,y,bait.x-practice.xMax-10,36);
        if(move.width>=155)
        {
            Fill(move,Deep);
            Text("WASD ／ 矢印　移動",move.x+3,move.y,move.width-6,move.height,13,Paper,align:TextAnchor.MiddleCenter);
        }
        Card(bait,Paper);
        Text("餌　"+TidesGame.BaitNames[game.ActiveBait],bait.x+9,bait.y,bait.width-18,bait.height,15,Ink);
        if(Hit(bait)){game.Open(TidesMode.Shop);game.Highlight(3+game.ActiveBait);}
    }

    void Aim()
    {
        Rect p=new Rect((game.W-586)/2,game.H-111,586,96);
        Card(p,Paper);
        Text("投げる向き",p.x+9,p.y+6,166,22,14,Reed);
        Text("飛ばす強さ",p.x+199,p.y+6,170,22,14,Reed);
        if(Button(new Rect(p.x+9,p.y+34,37,32),"←",true,false))game.AdjustAim(-5);
        Text($"{game.CastAim:+0;-0;0}°",p.x+48,p.y+32,69,35,17,Ink,align:TextAnchor.MiddleCenter);
        if(Button(new Rect(p.x+121,p.y+34,37,32),"→",true,false))game.AdjustAim(5);
        if(Button(new Rect(p.x+199,p.y+34,37,32),"−",true,false))game.AdjustPower(-.1f);
        Text($"{Mathf.RoundToInt(game.CastPower*100)}%",p.x+239,p.y+32,69,35,17,Ink,align:TextAnchor.MiddleCenter);
        if(Button(new Rect(p.x+309,p.y+34,37,32),"+",true,false))game.AdjustPower(.1f);
        Text("← → 方向　　↑ ↓ 強さ",p.x+9,p.y+67,349,23,13,Reed);
        if(Button(new Rect(p.xMax-207,p.y+17,194,41),"Space　投げる",true))game.Action();
        if(Button(new Rect(p.xMax-207,p.y+64,194,23),"X　仕掛けを戻す",true,false))game.CancelCast();
    }

    void Wait()
    {
        Rect p=new Rect((game.W-388)/2,game.H-73,388,58);
        Card(p,Paper);
        Text("ウキを見て待つ",p.x+11,p.y+3,248,26,18,Ink,true);
        Text("沈んだら Space で合わせる",p.x+12,p.y+30,253,23,14,Reed);
        if(Button(new Rect(p.xMax-105,p.y+12,94,33),"X　回収",true,false))game.CancelCast();
    }

    void Bite()
    {
        Rect signal=new Rect((game.W-360)/2,19,360,63);
        Card(signal,Ember);
        Text("アタリ！　合わせる",signal.x+4,signal.y+3,signal.width-8,35,23,Paper,true,TextAnchor.MiddleCenter);
        Text($"Space またはクリック　　残り {Mathf.Max(0,game.StrikeLeft):0.0} 秒",signal.x+5,signal.y+38,signal.width-10,21,14,Paper,align:TextAnchor.MiddleCenter);
        if(Hit(signal))game.Action();
        if(Button(new Rect((game.W-228)/2,game.H-63,228,44),"Space　合わせる",true))game.Action();
    }

    void Fight()
    {
        Rect p=new Rect((game.W-538)/2,game.H-121,538,106);
        Card(p,Deep);
        bool ease=game.Running||game.Tension>.84f;
        string cue=game.Tension>.84f?"張りすぎ　巻く手を放す":game.Running?"魚が走る　糸を緩める":
            game.Tension<.14f?"糸ふけ　短く巻く":"魚が休む　巻き寄せる";
        Color signal=ease?Apricot:Light;
        Text(cue,p.x+11,p.y+4,323,28,18,signal,true);
        float meterWidth=199;
        Text("糸の張り",p.x+12,p.y+36,84,22,14,Paper);
        Rect meter=new Rect(p.x+95,p.y+43,meterWidth,9);
        Fill(meter,Water);
        Fill(new Rect(meter.x,meter.y,meter.width*.14f,meter.height),Reed);
        Fill(new Rect(meter.x+meter.width*.84f,meter.y,meter.width*.16f,meter.height),Ember);
        float needle=meter.x+Mathf.Clamp01(game.Tension)*meter.width;
        Fill(new Rect(needle-2,meter.y-4,4,17),Paper);
        Text("取り込み",p.x+12,p.y+61,84,22,14,Paper);
        Fill(new Rect(meter.x,p.y+69,meter.width,7),Water);
        Fill(new Rect(meter.x,p.y+69,meter.width*Mathf.Clamp01(game.Progress01),7),Light);
        string warning=game.Tension>.84f?"張りすぎ。少し放す":game.Tension<.14f?"糸がたるんでいます。短く巻く":$"岸まで残り {Mathf.CeilToInt(game.Distance01*100)}%";
        Text(warning,p.x+12,p.y+82,310,21,13,game.Tension>.84f||game.Tension<.14f?Apricot:Light);
        Rect hold=new Rect(p.xMax-204,p.y+15,191,49);
        ReelButton(hold);
        Text("Space 長押し ／ 放して緩める",hold.x,p.y+68,hold.width,28,13,Paper,align:TextAnchor.MiddleCenter);
    }

    void ReelButton(Rect r)
    {
        Fill(r,reelButtonHeld||game.Reeling?Apricot:Paper);
        Text(game.Reeling?"巻いています":"押し続けて巻く",r.x+4,r.y,r.width-8,r.height,17,Ink,align:TextAnchor.MiddleCenter);
        int id=GUIUtility.GetControlID(FocusType.Passive,r);
        Event e=Event.current;
        if(e.GetTypeForControl(id)==EventType.MouseDown&&e.button==0&&r.Contains(e.mousePosition))
        {
            GUIUtility.hotControl=id;reelControl=id;reelButtonHeld=true;game.SetReel(true);e.Use();
        }
        else if(e.GetTypeForControl(id)==EventType.MouseUp&&GUIUtility.hotControl==id)
        {
            GUIUtility.hotControl=0;ReleaseReel();e.Use();
        }
    }
    void ReleaseReel()
    {
        if(GUIUtility.hotControl==reelControl)GUIUtility.hotControl=0;
        reelButtonHeld=false;reelControl=0;game.SetReel(false);
    }

    bool Shortcut(float x,float y,string key,string label)
    {
        Rect r=new Rect(x,y,97,36);
        Card(r,Hover(r)?Select:Paper);
        Key(x+5,y+7,key);
        Text(label,x+33,y,r.width-36,r.height,15,Ink,align:TextAnchor.MiddleCenter);
        return Hit(r);
    }
    void Key(float x,float y,string key)
    {
        Rect r=new Rect(x,y,24,22);Fill(r,Deep);
        Text(key,x,y,24,22,13,Paper,align:TextAnchor.MiddleCenter);
    }
    Rect Book(float w,float h,string english,string title,bool back=true)
    {
        Fill(new Rect(0,0,game.W,game.H),new Color(.06f,.12f,.13f,.61f));
        float y=Mathf.Min((game.H-h)/2,game.H-h-60);
        Rect p=new Rect((game.W-w)/2,Mathf.Max(12,y),w,h);Card(p,Paper);
        Fill(new Rect(p.x+13,p.y+13,3,45),Ember);
        Text(english,p.x+24,p.y+8,w-175,22,12,Reed);
        Text(title,p.x+22,p.y+28,w-175,36,26,Ink,true);
        if(back&&Button(new Rect(p.xMax-106,p.y+20,87,33),"戻る",true,false))game.Back();
        Line(p.x+18,p.y+70,w-36);
        return p;
    }
    void Card(Rect r,Color color)
    {
        Fill(new Rect(r.x+2,r.y+3,r.width,r.height),new Color(.08f,.1f,.08f,.27f));Fill(r,color);
        Fill(new Rect(r.x,r.y,r.width,1),RuleColor);
    }
    void Row(Rect r,bool selected)
    {
        Fill(r,selected||Hover(r)?Select:Page);
        if(selected)Fill(new Rect(r.x,r.y,3,r.height),Water);
    }
    bool Button(Rect r,string word,bool enabled,bool primary=true)
    {
        Fill(r,enabled?(primary?(Hover(r)?Water:Deep):(Hover(r)?Select:Page)):Page);
        Text(word,r.x+3,r.y,r.width-6,r.height,15,enabled&&primary?Paper:enabled?Ink:Reed,align:TextAnchor.MiddleCenter);
        return enabled&&Hit(r);
    }
    static bool Hover(Rect r)=>r.Contains(Event.current.mousePosition);
    static bool Hit(Rect r)=>GUI.Button(r,GUIContent.none,GUIStyle.none);
    void Text(string value,float x,float y,float w,float h,int size,Color color,bool serif=false,TextAnchor align=TextAnchor.MiddleLeft)
        =>game.Label(new Rect(x,y,w,h),value,size,color,serif,align);
    void Fill(Rect r,Color color)=>game.Box(r,color);
    void Line(float x,float y,float w)=>Fill(new Rect(x,y,w,1),RuleColor);
}
