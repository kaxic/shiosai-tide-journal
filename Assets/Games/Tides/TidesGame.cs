using System;
using UnityEngine;

[RequireComponent(typeof(TidesAmbience))]
[RequireComponent(typeof(TidesCatchPrint))]
public sealed class TidesGame : StudioGame
{
    public static readonly string[] SiteNames={"せせらぎの源流","銀鏡の湖","潮見の漁港","白帆の外礁"};
    public static readonly string[] SiteNotes={"石の間を縫う澄んだ流れ。まずは浅場の小魚から。","静かな水面とアシの岸。夜明けには沖の影も。","河口と海の境。岸壁の足元と流れの境が狙い目。","外洋の潮目に面した岩礁。強く走る魚が待つ。"};
    public static readonly string[] TimeNames={"朝","昼","夕","夜"},WeatherNames={"晴れ","曇り","雨"},TideNames={"下げ潮","上げ潮"};
    public static readonly string[] BaitNames={"川虫","練り餌","小魚","エビ"},GearNames={"竿","リール","道糸"};
    static readonly string[] painted={"oikawa","ayu","yamame","iwana","ugui","sakuramasu","wakasagi","funa","tanago","koi","nijimasu","biwamasu","haze","aji","mebaru","kurodai","suzuki","akame","saba","kasago","kawahagi","madai","kanpachi","hiramasa"};
    TidesCatalog catalog;TidesFishery fishery;TidesHud hud;TidesView view;
    bool mouseReel;float saveIn;int epoch;
    TidesRecord S=>fishery.S;
    public TidesMode Mode=>S.mode;
    public int Selection {get;private set;}
    public int Site=>S.site;
    public int Day=>S.day;
    public int TimeSlot=>S.slot;
    public int Weather=>fishery.Weather;
    public int Tide=>fishery.Tide;
    public Vector2 Player=>S.player;
    public Vector2 Facing=>S.facing;
    public bool Moving {get;private set;}
    public int Coins=>S.coins;
    public int TotalCaught=>fishery.Total;
    public int Discovered=>fishery.Discovered;
    public int ActiveBait=>S.bait;
    public TidesSpecies[] Fish=>catalog.species;
    public int FishIndex=>fishery.FishIndex;
    public int TrackedFish=>fishery.Tracked;
    public TidesOrder[] Orders=>catalog.orders;
    public TidesOrder ActiveOrder=>fishery.Order;
    public int RequestIndex=>fishery.RequestIndex;
    public int RepeatCount=>S.repeats;
    public int OrderCount=>fishery.OrderCount;
    public bool CanDeliver=>OrderCount>=ActiveOrder.quantity;
    public bool FinishedSurvey=>S.graduated;
    public int BagCount=>S.basket.Count;
    public int BagValue=>fishery.SaleValue;
    public int LandedValue=>S.landing?.value??0;
    public string LandingName=>S.landing?.name??"";
    public float LandingLength=>S.landing?.length??0;
    public float LandingWeight=>S.landing?.weight??0;
    public bool IsRecord=>S.isRecord;
    public string GradeName=>S.grade??"";
    public bool Practice=>S.practice;
    public float CastAim=>S.aim;
    public float CastPower=>S.power;
    public float PhaseTime=>S.phase;
    public float StrikeLeft=>Mathf.Max(0,S.biteFor-S.phase);
    public float Tension=>S.tension;
    public float Distance01=>S.distance;
    public float Progress01=>1-Distance01;
    public float Run01=>S.intervalFor<=0?0:Mathf.Clamp01(S.interval/S.intervalFor);
    public bool Running=>S.running;
    public bool Reeling=>S.reeling;
    public int Revision=>epoch+fishery.Revision;
    public string LastLog=>S.log??"水辺で Space を押して竿を構えましょう。";
    public float BitePulse=>Mode==TidesMode.Bite?.7f+.3f*Mathf.Sin(S.phase*15):Mode==TidesMode.Wait&&S.phase>S.waitFor*.52f?.10f+.1f*Mathf.Sin(S.phase*11):0;
    public Vector2 FloatPosition
    {
        get
        {
            if(Mode==TidesMode.Explore||Mode==TidesMode.Aim)
            {
                float a=CastAim*Mathf.Deg2Rad;return Player+new Vector2(Mathf.Sin(a),Mathf.Cos(a))*Mathf.Lerp(4,18,CastPower);
            }
            if(Mode==TidesMode.Fight)
            {
                var pos=Vector2.Lerp(Player+Vector2.up*1.5f,S.point,Distance01);
                pos.x+=Mathf.Sin(PhaseTime*(Running?3.2f:1.4f))*(Running?.6f:.17f)*Distance01;return pos;
            }
            return S.point;
        }
    }
    public string Nearby=>Player.y< -2&&Player.x>4.3f?"道具売店　E で開く":Player.y< -2&&Player.x< -4.3f?"渡し船　E で渡航先を選ぶ":"水辺　Space で竿を構える";
    public int GearLevel(int i)=>i>=0&&i<3?S.gear[i]:0;
    public int UpgradeCost(int i)=>fishery.Cost(i);
    public bool CanUpgrade(int i)=>UpgradeCost(i)>0&&Coins>=UpgradeCost(i);
    public bool CanTravel(int i)=>fishery.CanTravel(i);
    public string TravelRule(int i)=>CanTravel(i)?"渡航できます":$"調査依頼を {(i==1?1:i==2?3:5)} 件達成して開通";
    public int SpeciesCaught(int i)=>fishery.Finding(i)?.count??0;
    public float BestLength(int i)=>fishery.Finding(i)?.bestLength??0;
    public float BestWeight(int i)=>fishery.Finding(i)?.bestWeight??0;
    public bool IsFavorite(int i)=>fishery.Finding(i)?.favorite??false;
    public int SpecimenIndex(int i)=>i>=0&&i<Fish.Length?Array.IndexOf(painted,Fish[i].id):-1;
    public int SpeciesIndex(string id)=>catalog.Index(id);
    public bool OrderFinished(int i)=>i>=0&&i<Orders.Length&&S.delivered.Contains(Orders[i].id);
    public string FishHint(int i)
    {
        if(i<0||i>=Fish.Length)return "";
        var f=Fish[i];
        if(f.adaptable)return SiteNames[f.site]+"。どの時間・天気・餌・距離でも出会えます。";
        string time=Mask(TimeNames,f.timeMask),sky=Mask(WeatherNames,f.weatherMask),tide=Mask(TideNames,f.tideMask);
        return $"{SiteNames[f.site]}　{time}・{sky}・{tide}\n{BaitNames[f.bait]}　飛距離 {Mathf.RoundToInt(f.minCast*100)}～{Mathf.RoundToInt(f.maxCast*100)}％"+(f.legendary?"　竿 Lv.1 以上":"");
    }
    static string Mask(string[] labels,int mask)
    {
        if(mask==(1<<labels.Length)-1)return labels==TimeNames?"終日":labels==WeatherNames?"全天候":"潮不問";
        string s="";for(int i=0;i<labels.Length;i++)if((mask&(1<<i))!=0)s+=(s.Length>0?"／":"")+labels[i];return s;
    }
    public string Forecast(int slot)
    {
        if(slot<0||slot>3)return "";
        int day=slot<=TimeSlot?TidesFishery.NextDay(Day):Day;
        return (day==Day?"今日":"翌日")+"　"+WeatherNames[TidesFishery.Sky(day,slot,Site)]+"／"+TideNames[TidesFishery.Water(day,slot,Site)];
    }
    protected override void BeginGame()
    {
        catalog=TidesCatalog.Load();fishery=new TidesFishery(catalog,Load<TidesRecord>());
        hud=new TidesHud();view=gameObject.AddComponent<TidesView>();SetSelection();view.Render(this);
    }
    protected override void SaveCheckpoint(){if(fishery!=null)Save(S);saveIn=2;}
    protected override void LateGame(){if(fishery!=null){SetIntenseMusic(Mode==TidesMode.Fight);view.Render(this);}}
    bool Menu=>Mode==TidesMode.Travel||Mode==TidesMode.Clock||Mode==TidesMode.Shop||Mode==TidesMode.Journal||Mode==TidesMode.Requests;
    void Say(string s,int sound=-1){S.log=s;Notice(s);if(sound>=0)Tone(sound);}
    void SetSelection(){Selection=Mode==TidesMode.Travel?Site:Mode==TidesMode.Clock?TimeSlot:Mode==TidesMode.Journal?Mathf.Max(0,TrackedFish):Mode==TidesMode.Requests?RequestIndex:0;}
    public void Open(TidesMode mode)
    {
        if(!(Menu||Mode==TidesMode.Explore)||mode<TidesMode.Travel||mode>TidesMode.Clock)return;
        S.mode=mode;mouseReel=false;Moving=false;SetSelection();
    }
    public void Back()
    {
        if(Menu){S.mode=TidesMode.Explore;mouseReel=false;}
        else if(Mode==TidesMode.Aim)CancelCast();
        else if(Mode==TidesMode.Catch||Mode==TidesMode.Escape){fishery.ResultBack();SaveCheckpoint();}
    }
    protected override bool HandleEscape()
    {
        if(Menu||Mode==TidesMode.Aim||Mode==TidesMode.Catch||Mode==TidesMode.Escape){Back();return true;}
        mouseReel=false;return false;
    }
    int Options()=>Mode==TidesMode.Journal?Fish.Length:Mode==TidesMode.Requests?Orders.Length+(FinishedSurvey?1:0):Mode==TidesMode.Shop?8:4;
    public void Highlight(int i){Selection=Mathf.Clamp(i,0,Options()-1);}
    public void Travel(int i){if(fishery.Travel(i)){Say(SiteNames[i]+" に到着",1);SaveCheckpoint();}else Say(TravelRule(i));}
    public void SetTime(int i){if(fishery.Time(i)){Say($"第{Day}日　{TimeNames[TimeSlot]}・{WeatherNames[Weather]}・{TideNames[Tide]}",1);SaveCheckpoint();}}
    public void ShopAction(int i)
    {
        if(Mode!=TidesMode.Shop&&Mode!=TidesMode.Explore)return;
        if(i==7){SellAll();return;}
        if(i<0||i>6)return;
        if(i<3)
        {
            if(!fishery.Upgrade(i)){Say(UpgradeCost(i)==0?"この道具は最高段階です。":"魚の販売や依頼で資金を貯めましょう。");return;}
            Say(GearNames[i]+" Lv."+GearLevel(i)+" に強化",3);
        }
        else if(fishery.Bait(i-3))Say(BaitNames[i-3]+" に付け替えました。",1);
        SaveCheckpoint();
    }
    public void SellAll()
    {
        int sold=fishery.Sell();Say(sold>0?$"魚を販売　＋{sold} 円":"売却できる魚がありません。依頼分は魚籠に確保します。",sold>0?3:-1);SaveCheckpoint();
    }
    public void Deliver()
    {
        int reward=ActiveOrder.reward;string title=ActiveOrder.name;
        if(!fishery.Deliver()){Say("必要な魚を魚籠にそろえましょう。");return;}
        Selection=RequestIndex;Say(title+" を達成　＋"+reward+" 円",4);SaveCheckpoint();
    }
    public void Track(int i)
    {
        if(i<0||i>=Fish.Length)return;fishery.Track(i);Back();Say(Fish[i].name+" の生息条件を追跡します。",1);SaveCheckpoint();
    }
    public void ToggleFavorite(int i)
    {
        if(!fishery.Favorite(i)){Say("釣り上げた魚をお気に入りに登録できます。");return;}
        Say(IsFavorite(i)?"お気に入りに登録しました。":"お気に入りから外しました。");SaveCheckpoint();
    }
    public void TogglePractice(){S.practice=!S.practice;Say(S.practice?"ゆっくり釣り　次の投竿から合わせの猶予が延びます。":"通常の速さに戻しました。");SaveCheckpoint();}
    public void AdjustAim(float delta)=>fishery.SetAim(CastAim+delta,CastPower);
    public void AdjustPower(float delta)=>fishery.SetAim(CastAim,CastPower+delta);
    public void SetReel(bool held){mouseReel=held&&Mode==TidesMode.Fight;}
    public void CancelCast(){if(fishery.Cancel()){mouseReel=false;Say(S.log);SaveCheckpoint();}}
    public void Action()
    {
        switch(Mode)
        {
            case TidesMode.Explore:fishery.Aim();Moving=false;Tone(0);break;
            case TidesMode.Aim:fishery.Cast();Tone(1);break;
            case TidesMode.Wait:case TidesMode.Bite:fishery.Strike();Tone(Mode==TidesMode.Fight?2:0);break;
            case TidesMode.Catch:case TidesMode.Escape:fishery.ResultBack();break;
            default:return;
        }
        SaveCheckpoint();
    }
    static bool Press(KeyCode k)=>Input.GetKeyDown(k);
    static bool Held(KeyCode k)=>Input.GetKey(k);
    static bool Enter()=>Press(KeyCode.Return)||Press(KeyCode.KeypadEnter);
    void ToggleMenu(TidesMode next){if(Mode==next)Back();else Open(next);}
    protected override void TickGame()
    {
        float dt=Mathf.Min(Time.unscaledDeltaTime,.12f);Moving=false;
        if(Press(KeyCode.P)){TogglePractice();return;}
        if(Press(KeyCode.T)){ToggleMenu(TidesMode.Travel);return;}
        if(Press(KeyCode.R)){ToggleMenu(TidesMode.Clock);return;}
        if(Press(KeyCode.B)){ToggleMenu(TidesMode.Shop);return;}
        if(Press(KeyCode.H)){ToggleMenu(TidesMode.Journal);return;}
        if(Press(KeyCode.J)){ToggleMenu(TidesMode.Requests);return;}
        if(Menu)
        {
            int width=Mode==TidesMode.Journal?4:1;
            int d=(Press(KeyCode.RightArrow)?1:0)-(Press(KeyCode.LeftArrow)?1:0)+(Press(KeyCode.DownArrow)?width:0)-(Press(KeyCode.UpArrow)?width:0);
            if(d!=0)Highlight(Selection+d);
            if(Press(KeyCode.F)&&Mode==TidesMode.Journal)ToggleFavorite(Selection);
            if(Enter()||Press(KeyCode.Space))
            {
                if(Mode==TidesMode.Travel)Travel(Selection);else if(Mode==TidesMode.Clock)SetTime(Selection);
                else if(Mode==TidesMode.Shop)ShopAction(Selection);else if(Mode==TidesMode.Journal)Track(Selection);
                else if(Selection==RequestIndex)Deliver();else Say("手帖で対象魚の生息条件を確認できます。");
            }return;
        }
        if(Press(KeyCode.X)){CancelCast();return;}
        bool headingRight=Held(KeyCode.D)||Held(KeyCode.RightArrow),headingLeft=Held(KeyCode.A)||Held(KeyCode.LeftArrow);
        bool headingUp=Held(KeyCode.W)||Held(KeyCode.UpArrow),headingDown=Held(KeyCode.S)||Held(KeyCode.DownArrow);
        var input=new Vector2((headingRight?1:0)-(headingLeft?1:0),(headingUp?1:0)-(headingDown?1:0));
        if(Mode==TidesMode.Explore)
        {
            Moving=input.sqrMagnitude>.05f;fishery.Move(input.normalized*dt*3.8f);
            if(Press(KeyCode.E)&&Player.y< -2&&Mathf.Abs(Player.x)>4.3f){Open(Player.x>0?TidesMode.Shop:TidesMode.Travel);return;}
        }
        else if(Mode==TidesMode.Aim){AdjustAim(input.x*dt*52);AdjustPower(input.y*dt*.57f);}
        if(Press(KeyCode.Space)||Press(KeyCode.E)||Enter())
        {
            // A fresh strike must not also consume the following fight frame or dismiss its result.
            if(Mode!=TidesMode.Fight){Action();return;}
        }
        var prior=Mode;bool run=Running;
        if(Input.GetMouseButtonUp(0))mouseReel=false;
        fishery.Step(dt,mouseReel||Held(KeyCode.Space)||Held(KeyCode.E)||Held(KeyCode.Return));
        if(prior!=Mode)
        {
            Tone(Mode==TidesMode.Bite?2:Mode==TidesMode.Catch?4:0);mouseReel=false;SaveCheckpoint();
        }
        else if(Mode==TidesMode.Fight&&Running!=run)Tone(Running?0:1);
        saveIn-=dt;if(saveIn<=0&&(Mode==TidesMode.Wait||Mode==TidesMode.Bite||Mode==TidesMode.Fight))SaveCheckpoint();
    }
    protected override void DrawGame()
    {
        if(TidesCatchPrint.Framing){TidesCatchPrint.DrawCard(this);return;}
        hud.Draw(this);
    }
    public override void VerifyGame()
    {
        var w=TidesVerification.Run(catalog);
        if(Reviewing){Save(w.S);var copy=new TidesFishery(catalog,Load<TidesRecord>());if(copy.Total!=w.Total||copy.S.repeats!=w.S.repeats)throw new InvalidOperationException("釣り記録を再読込できません。");}
    }
    public override int ShowcaseViews=>3;
    public override void ShowcaseView(int index)
    {
        if(index==0){fishery=TidesVerification.Complete(catalog);epoch+=100000;TidesVerification.Prepare(fishery,catalog.Index("madai"));fishery.S.player=new Vector2(-1.8f,-.6f);fishery.Aim();fishery.SetAim(12,.70f);fishery.Cast();while(Mode==TidesMode.Wait)fishery.Step(.04f,false);fishery.Strike();fishery.Step(2.8f,true);}
        else if(index==1){fishery.Cancel();TidesVerification.Catch(fishery,catalog.Index("sakuramasu"),false);}
        else{fishery.ResultBack();Open(TidesMode.Journal);Highlight(catalog.Index("hiramasa"));}
        DismissNotice();view.Render(this);
        if(index==1)GetComponent<TidesCatchPrint>().Take();
    }
}
