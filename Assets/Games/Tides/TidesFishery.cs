using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public sealed class TidesFinding
{
    public string id; public int count;public float bestLength,bestWeight;public bool favorite;
}
[Serializable] public sealed class TidesLanding
{
    public string fishId,name;public int serial,value;public float length,weight;
}
[Serializable] public sealed class TidesRecord
{
    public int version,site,day=1,slot,bait,coins=28,permits=1,repeats,serial,lastAward,castsInSlot,misses,cycles;
    public int[] gear=new int[3];
    public Vector2 player=new Vector2(0,-.8f),facing=Vector2.up,point;
    public string tracked,repeatFish,log,grade;
    public bool practice,running,reeling,isRecord,cleanStrike,graduated;
    public TidesMode mode;
    public float aim,power=.5f,phase,waitFor,biteFor,tension,distance,interval,intervalFor,danger,peakDanger;
    public TidesSpecies hooked; public TidesLanding landing;
    public List<TidesFinding> findings=new List<TidesFinding>();
    public List<TidesLanding> basket=new List<TidesLanding>();
    public List<string> delivered=new List<string>();
}
public sealed class TidesFishery
{
    public readonly TidesRecord S;
    public readonly TidesCatalog C;
    readonly Dictionary<string,TidesFinding> findings=new Dictionary<string,TidesFinding>(StringComparer.Ordinal);
    public int RequestIndex {get;private set;}
    public int Total {get;private set;}
    public int Discovered {get;private set;}
    public TidesOrder Order {get;private set;}
    public int Revision {get;private set;}
    public int Weather=>Sky(S.day,S.slot,S.site);
    public int Tide=>Water(S.day,S.slot,S.site);
    public static int Sky(int day,int time,int site)=>(day/2+time/2+site)%3;
    public static int Water(int day,int time,int site)=>(day+time+site)%2;
    public int FishIndex=>S.hooked==null?-1:C.Index(S.hooked.id);
    public int Tracked=>C.Index(S.tracked);
    public TidesFishery(TidesCatalog catalog,TidesRecord record=null)
    {
        C=catalog;S=record??new TidesRecord();
        S.version=1;S.day=Mathf.Clamp(S.day,1,999999);S.slot=Mathf.Clamp(S.slot,0,3);S.site=Mathf.Clamp(S.site,0,3);
        S.permits|=1|(1<<S.site);S.coins=Math.Max(0,S.coins);S.repeats=Math.Max(0,S.repeats);S.power=Mathf.Clamp01(S.power);
        S.aim=Mathf.Clamp(S.aim,-45,45);S.bait=Mathf.Clamp(S.bait,0,3);
        if(S.gear==null)S.gear=new int[3];if(S.gear.Length<3)Array.Resize(ref S.gear,3);
        for(int i=0;i<3;i++)S.gear[i]=Mathf.Clamp(S.gear[i],0,3);
        S.findings??=new List<TidesFinding>();S.delivered??=new List<string>();S.basket??=new List<TidesLanding>();
        S.findings.RemoveAll(f=>f==null||string.IsNullOrEmpty(f.id)||!findings.TryAdd(f.id,f));
        foreach(var f in S.findings){Total+=Math.Max(0,f.count);if(f.count>0&&C.Index(f.id)>=0)Discovered++;}
        S.basket.RemoveAll(f=>f==null||string.IsNullOrEmpty(f.fishId));
        if((S.mode==TidesMode.Wait||S.mode==TidesMode.Bite||S.mode==TidesMode.Fight||S.mode==TidesMode.Catch)&&
            (S.hooked==null||string.IsNullOrEmpty(S.hooked.id)||S.landing==null))S.mode=TidesMode.Explore;
        if(!Enum.IsDefined(typeof(TidesMode),S.mode))S.mode=TidesMode.Explore;
        if(S.repeats>0)S.graduated=true;
        S.reeling=false;RefreshOrders();
    }
    void RefreshOrders()
    {
        int complete=0;RequestIndex=C.orders.Length;
        for(int i=0;i<C.orders.Length;i++)
        {
            if(S.delivered.Contains(C.orders[i].id))complete++;
            else if(RequestIndex==C.orders.Length)RequestIndex=i;
        }
        if(complete>=1)S.permits|=2;if(complete>=3)S.permits|=4;if(complete>=5)S.permits|=8;
        if(RequestIndex<C.orders.Length){Order=C.orders[RequestIndex];return;}
        S.graduated=true;S.permits=15;
        int index=C.Index(S.repeatFish);
        if(index<0){index=(int)(((long)S.repeats*7+5)%C.species.Length);S.repeatFish=C.species[index].id;}
        var s=C.species[index];
        Order=new TidesOrder{id="tour-"+S.repeats,name="沿岸巡回・"+s.name,client="四水域の観測所",
            note=s.name+"を魚籠に確保し、観測記録を更新しよう。",fishId=s.id,quantity=s.legendary?1:1+(S.repeats%3==2?1:0),reward=s.price*2+80};
    }
    public TidesFinding Finding(int i)=>i>=0&&i<C.species.Length&&findings.TryGetValue(C.species[i].id,out var f)?f:null;
    TidesFinding Get(string id)
    {
        if(!findings.TryGetValue(id,out var f)){f=new TidesFinding{id=id};findings.Add(id,f);S.findings.Add(f);}
        return f;
    }
    public bool Favorite(int i)
    {
        var f=Finding(i);if(f==null||f.count<1)return false;f.favorite=!f.favorite;Revision++;return true;
    }
    public void Track(int i)
    {
        if(i<0||i>=C.species.Length)return;
        if(S.tracked!=C.species[i].id){S.tracked=C.species[i].id;S.misses=0;}
    }
    public bool CanTravel(int i)=>i>=0&&i<4&&(S.permits&(1<<i))!=0;
    static bool AtShore(TidesMode mode)=>mode==TidesMode.Explore||mode==TidesMode.Travel||mode==TidesMode.Shop||mode==TidesMode.Journal||mode==TidesMode.Requests||mode==TidesMode.Clock;
    public bool Travel(int site)
    {
        if(!AtShore(S.mode)||!CanTravel(site))return false;
        S.site=site;S.player=new Vector2(0,-.8f);S.facing=Vector2.up;S.mode=TidesMode.Explore;Revision++;return true;
    }
    public void Move(Vector2 delta)
    {
        if(S.mode!=TidesMode.Explore||delta.sqrMagnitude<.0000001f)return;
        var p=S.player+delta;S.player=new Vector2(Mathf.Clamp(p.x,-8,8),Mathf.Clamp(p.y,-4.5f,-.4f));S.facing=delta.normalized;
    }
    public bool Time(int slot)
    {
        if(!AtShore(S.mode)||slot<0||slot>3)return false;
        if(slot<=S.slot)S.day=NextDay(S.day);S.slot=slot;S.castsInSlot=0;S.mode=TidesMode.Explore;Revision++;return true;
    }
    public static int NextDay(int day)=>day==999999?1:day+1;
    public int Cost(int i)=>i>=0&&i<3&&S.gear[i]<3?new[]{46,55,50}[i]+S.gear[i]*80:0;
    public bool Upgrade(int i)
    {
        int price=Cost(i);if(!AtShore(S.mode)||price<=0||S.coins<price)return false;
        S.coins-=price;S.gear[i]++;Revision++;return true;
    }
    public bool Bait(int i){if(!AtShore(S.mode)||i<0||i>3)return false;S.bait=i;return true;}
    public int OrderCount
    {
        get{int n=0;foreach(var f in S.basket)if(f.fishId==Order.fishId)n++;return n;}
    }
    public int SaleValue
    {
        get
        {
            int keep=Order.quantity,value=0;
            foreach(var f in S.basket){if(f.fishId==Order.fishId&&keep>0){keep--;continue;}value+=f.value;}
            return value;
        }
    }
    public int Sell()
    {
        if(!AtShore(S.mode))return 0;
        int value=0,keep=Order.quantity;
        S.basket.RemoveAll(f=>{
            if(f.fishId==Order.fishId&&keep>0){keep--;return false;}
            value+=f.value;return true;
        });S.coins+=value;Revision++;return value;
    }
    public bool Deliver()
    {
        if(!AtShore(S.mode)||OrderCount<Order.quantity)return false;
        int need=Order.quantity;string id=Order.fishId;
        S.basket.RemoveAll(f=>{if(f.fishId!=id||need<=0)return false;need--;return true;});
        S.coins+=Order.reward;
        if(RequestIndex<C.orders.Length)S.delivered.Add(Order.id);
        else {S.repeats++;S.repeatFish=null;}
        RefreshOrders();Revision++;return true;
    }
    public bool Eligible(TidesSpecies f)
    {
        if(f.site!=S.site||f.legendary&&S.gear[0]<1)return false;
        return f.adaptable||f.bait==S.bait&&(f.timeMask&(1<<S.slot))!=0&&(f.tideMask&(1<<Tide))!=0&&
           (f.weatherMask&(1<<Weather))!=0&&S.power+.0001f>=f.minCast&&S.power-.0001f<=f.maxCast;
    }
    static uint Hash(uint value){unchecked{value^=value>>16;value*=0x7feb352d;value^=value>>15;value*=0x846ca68b;return value^(value>>16);}}
    float Roll(int salt)=>((Hash(unchecked((uint)S.serial*1664525u+(uint)salt*1013904223u+(uint)S.site*7u))>>8)&0xffffff)/16777216f;
    TidesSpecies Choose()
    {
        int target=Tracked;
        if(target>=0&&Eligible(C.species[target]))
        {
            if(S.misses>0||Roll(1)<.71f){S.misses=0;return C.species[target];}
            S.misses++;
        }
        float total=0;TidesSpecies chosen=null;
        for(int i=0;i<C.species.Length;i++)
        {
            var f=C.species[i];if(!Eligible(f))continue;
            float weight=f.legendary?.45f:f.adaptable?1:3.2f;total+=weight;
            if(Roll(30+i)*total<weight)chosen=f;
        }
        if(chosen!=null&&chosen.id==S.tracked)S.misses=0;
        return chosen;
    }
    public bool Aim()
    {
        if(S.mode!=TidesMode.Explore)return false;
        S.player=new Vector2(S.player.x,Mathf.Max(S.player.y,-1.15f));S.facing=Vector2.up;
        S.mode=TidesMode.Aim;S.hooked=null;S.landing=null;S.phase=0;S.log="左右で方向、上下で飛距離。Space で投げる。";Revision++;return true;
    }
    public void SetAim(float aim,float power)
    {
        if(S.mode!=TidesMode.Aim)return;S.aim=Mathf.Clamp(aim,-45,45);S.power=Mathf.Clamp01(power);
    }
    public bool Cast()
    {
        if(S.mode!=TidesMode.Aim)return false;
        S.serial++;var f=Choose();if(f==null){S.serial--;return false;}
        // The encounter snapshot keeps an in-flight cast consistent if its data is edited or removed.
        S.hooked=JsonUtility.FromJson<TidesSpecies>(JsonUtility.ToJson(f));
        float size=Mathf.Sqrt(Roll(3)*Roll(4));float length=Mathf.Lerp(f.minCm,f.maxCm,size);
        S.landing=new TidesLanding{fishId=f.id,name=f.name,serial=S.serial,length=Mathf.Round(length*10)/10,
            weight=Mathf.Max(.001f,Mathf.Round(length*length*length*f.bodyFactor)/1000),value=Math.Max(1,Mathf.RoundToInt(f.price*(.75f+size*.90f)))};
        float a=S.aim*Mathf.Deg2Rad;S.point=S.player+new Vector2(Mathf.Sin(a),Mathf.Cos(a))*Mathf.Lerp(4,18,S.power);
        S.waitFor=2.25f+Roll(5)*2.05f+Mathf.Abs(S.aim)/110;
        S.biteFor=(S.practice?3.25f:2.05f)+(S.gear[0]*.11f)-(f.legendary?.18f:0);
        S.phase=0;S.mode=TidesMode.Wait;S.cleanStrike=false;S.reeling=false;S.log="浮きを観察。沈んでから合わせる。";Revision++;return true;
    }
    public bool Strike()
    {
        if(S.mode==TidesMode.Wait){Escape("早合わせ。浮きが沈み、合図が変わるのを待とう。");return false;}
        if(S.mode!=TidesMode.Bite)return false;
        S.cleanStrike=S.phase>=.06f&&S.phase<=S.biteFor*.60f;
        S.mode=TidesMode.Fight;S.phase=0;S.distance=1;S.tension=.46f;S.interval=0;S.cycles=0;
        S.running=false;S.intervalFor=2.7f;S.peakDanger=0;S.danger=0;S.reeling=false;
        S.log="魚が休む間に巻き、走ったら離す。";Revision++;return true;
    }
    void Escape(string why)
    {
        S.mode=TidesMode.Escape;S.phase=0;S.reeling=false;S.log=why;Revision++;
    }
    public bool Cancel()
    {
        if(S.mode!=TidesMode.Wait&&S.mode!=TidesMode.Bite&&S.mode!=TidesMode.Fight&&S.mode!=TidesMode.Aim)return false;
        S.mode=TidesMode.Explore;S.reeling=false;S.log="仕掛けを回収しました。";Revision++;return true;
    }
    public void ResultBack()
    {
        if(S.mode!=TidesMode.Catch&&S.mode!=TidesMode.Escape)return;
        S.mode=TidesMode.Explore;
        if(++S.castsInSlot>=4){S.castsInSlot=0;S.slot=(S.slot+1)%4;if(S.slot==0)S.day=NextDay(S.day);}
        Revision++;
    }
    public void Step(float seconds,bool reel)
    {
        if(!(seconds>0)||float.IsInfinity(seconds))return;
        // Substeps preserve mechanical timing on slow frames and for deterministic replay.
        float remaining=Mathf.Min(seconds,10f);
        while(remaining>.00002f&&(S.mode==TidesMode.Wait||S.mode==TidesMode.Bite||S.mode==TidesMode.Fight))
        {
            float dt=Mathf.Min(.03333334f,remaining);remaining-=dt;S.phase+=dt;
            if(S.mode==TidesMode.Wait)
            {
                if(S.phase>=S.waitFor){S.phase-=S.waitFor;S.mode=TidesMode.Bite;S.log="浮きが沈んだ！ Space で合わせる。";Revision++;}continue;
            }
            if(S.mode==TidesMode.Bite){if(S.phase>=S.biteFor)Escape("合わせが遅れた。次は浮きが沈んだ瞬間に。");continue;}
            Fight(dt,reel);
        }
    }
    void Fight(float dt,bool reel)
    {
        S.reeling=reel;float strength=S.hooked.fight;
        if(S.running)
        {
            S.distance=Mathf.Min(1,S.distance+dt*.017f*strength);
            S.tension=reel?S.tension+dt*(.34f+.14f*strength)/(1+.13f*S.gear[2])*(S.practice?.72f:1):
                Mathf.MoveTowards(S.tension,.44f,dt*(.35f+.02f*S.gear[2]));
        }
        else if(reel)
        {
            S.distance-=dt*(.153f+.018f*S.gear[1]+.008f*S.gear[0])/(.72f+.39f*strength);
            S.tension+=dt*(.031f+.007f*strength)/(1+.13f*S.gear[2]);
        }
        else S.tension-=dt*(S.practice?.13f:.22f);
        S.tension=Mathf.Clamp01(S.tension);S.distance=Mathf.Clamp01(S.distance);
        bool limit=S.tension>=.999f||S.tension<=.001f;
        S.danger=limit?S.danger+dt:0;S.peakDanger=Mathf.Max(S.peakDanger,S.danger);
        if(S.danger>=(S.practice?1.5f:1f)){Escape(S.tension>.5f?"糸が切れた。走りの間は巻く手を離そう。":"糸が緩んで外れた。休みの合図で巻いてみよう。");return;}
        if(S.distance<=.00001f){Land();return;}
        S.interval+=dt;
        if(S.interval>=S.intervalFor)
        {
            S.interval-=S.intervalFor;S.running=!S.running;S.cycles++;
            S.intervalFor=S.running?1.38f+strength*.41f+.42f*Roll(70+S.cycles):2.42f+.66f*Roll(70+S.cycles);
        }
    }
    void Land()
    {
        S.mode=TidesMode.Catch;S.reeling=false;S.phase=0;
        if(S.lastAward==S.serial)return;
        var f=Get(S.landing.fishId);S.isRecord=S.landing.length>f.bestLength||S.landing.weight>f.bestWeight;
        if(f.count==0&&C.Index(f.id)>=0)Discovered++;
        f.count++;Total++;f.bestLength=Mathf.Max(f.bestLength,S.landing.length);f.bestWeight=Mathf.Max(f.bestWeight,S.landing.weight);
        S.grade=S.cleanStrike&&S.peakDanger<.15f?"澄んだ合わせ":S.peakDanger<.65f?"安定した取り込み":"粘りの一尾";
        if(S.cleanStrike&&S.peakDanger<.15f)S.landing.value=Mathf.RoundToInt(S.landing.value*1.15f);
        S.basket.Add(S.landing);S.lastAward=S.serial;S.log=S.landing.name+"を取り込んだ！";Revision++;
    }
}
