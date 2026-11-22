using System;
using System.Collections.Generic;
using UnityEngine;

public static class TidesVerification
{
    static void Check(bool yes,string action){if(!yes)throw new InvalidOperationException("Tides: "+action);}
    static TidesFishery Reload(TidesFishery w,TidesCatalog c=null)=>new TidesFishery(c??w.C,JsonUtility.FromJson<TidesRecord>(JsonUtility.ToJson(w.S)));
    static bool GoodTime(TidesFishery w,TidesSpecies f)=>f.adaptable||((f.timeMask&(1<<w.S.slot))!=0&&(f.weatherMask&(1<<w.Weather))!=0&&(f.tideMask&(1<<w.Tide))!=0);
    public static void Prepare(TidesFishery w,int i)
    {
        Check(i>=0&&i<w.C.species.Length,"調査対象の登録");var f=w.C.species[i];
        w.ResultBack();Check(w.Travel(f.site),"渡し舟 "+f.id);
        if(f.legendary&&w.S.gear[0]==0){w.Sell();Check(w.Upgrade(0),"大物の竿を通常の売上で購入");}
        w.Track(i);Check(w.Bait(f.bait),"餌を付け替える");
        int attempts=0;
        while(!GoodTime(w,f)&&attempts++<35)w.Time((w.S.slot+1)%4);
        Check(GoodTime(w,f),"到達可能な天気・潮・時刻 "+f.id);
    }
    static void Bite(TidesFishery w)
    {
        float time=0;
        while(w.S.mode==TidesMode.Wait&&time<9){w.Step(.05f,false);time+=.05f;}
        Check(w.S.mode==TidesMode.Bite,"魚信が出る");w.Step(.16f,false);
    }
    static void Land(TidesFishery w)
    {
        int n=0;
        while(w.S.mode==TidesMode.Fight&&n++<1200)w.Step(.05f,!w.S.running);
        Check(w.S.mode==TidesMode.Catch,"走るとき離し、休むとき巻いて取り込む "+w.S.log);
    }
    public static void Catch(TidesFishery w,int i,bool close=true)
    {
        var f=w.C.species[i];
        for(int attempt=0;attempt<2;attempt++)
        {
            Prepare(w,i);Check(w.Aim(),"岸で構える");
            w.SetAim(attempt==0?0:12,(f.minCast+f.maxCast)/2);
            Check(w.Eligible(f)&&w.Cast(),"生息条件で投竿 "+f.id);
            Bite(w);Check(w.Strike(),"適時に合わせる");Land(w);
            if(w.S.landing.fishId==f.id){if(close)w.ResultBack();return;}
            w.ResultBack();
        }
        throw new InvalidOperationException("追跡中の適合魚に2投で出会えません "+f.id);
    }
    public static TidesFishery Complete(TidesCatalog c)
    {
        var w=new TidesFishery(c);
        foreach(var order in c.orders)
        {
            Check(order.id==w.Order.id,"依頼順序");
            for(int i=0;i<order.quantity;i++)Catch(w,c.Index(order.fishId));
            int count=w.OrderCount,before=w.S.coins;int reserve=w.SaleValue;
            Check(w.Sell()==reserve&&w.OrderCount==Math.Min(count,order.quantity),"依頼魚を確保して余剰分を売る");
            Check(w.Deliver()&&w.S.coins==before+reserve+order.reward,"納品の原子性 "+order.id);
        }
        Check(w.S.graduated&&w.RequestIndex==c.orders.Length&&w.CanTravel(3),"四水域調査の完了");
        return w;
    }
    static void Timing(TidesCatalog c)
    {
        var w=new TidesFishery(c);Check(!w.Travel(1)&&!w.Deliver()&&!w.Upgrade(0),"始まりの予算と渡航");
        w.Move(new Vector2(50,-50));Check(w.S.player==new Vector2(8,-4.5f),"陸上の歩行境界");
        Check(w.Aim(),"初投");w.SetAim(80,2);Check(w.S.aim==45&&w.S.power==1&&w.Cast(),"投擲の強さ・方向");
        Check(!w.Strike()&&w.S.mode==TidesMode.Escape&&w.Total==0&&w.S.coins==28,"早合わせで逃げても道具・資金を失わない");
        w.ResultBack();w.Aim();w.Cast();Bite(w);w.Step(4,false);
        Check(w.S.mode==TidesMode.Escape&&w.S.basket.Count==0,"遅合わせ");
        foreach(bool hold in new[]{true,false})
        {
            w.ResultBack();w.Aim();w.Cast();Bite(w);w.Strike();
            for(int i=0;i<600&&w.S.mode==TidesMode.Fight;i++)w.Step(.1f,hold);
            Check(w.S.mode==TidesMode.Escape&&(hold?w.S.log.Contains("切れ"):w.S.log.Contains("緩ん")),"張りすぎと糸ふけを区別");
        }
        w.ResultBack();w.Aim();w.Cast();Check(!w.Travel(0)&&!w.Time(0)&&!w.Bait(3)&&w.Sell()==0,"釣り中の一貫性");
        w.Step(.7f,false);var copy=Reload(w);
        Check(copy.S.mode==TidesMode.Wait&&copy.S.hooked.id==w.S.hooked.id&&copy.S.phase==w.S.phase,"待ち時間から再開");
        Bite(copy);copy=Reload(copy);Check(copy.S.mode==TidesMode.Bite&&copy.Strike(),"合わせの猶予を保存");
        copy.Step(1.4f,true);var active=Reload(copy);
        Check(active.S.mode==TidesMode.Fight&&active.S.distance==copy.S.distance&&active.S.tension==copy.S.tension,"取り込みの途中再開");
        Land(active);Check(active.Total==1&&active.S.basket.Count==1,"一投一尾");
        var landed=Reload(active);landed.Step(10,true);Check(!landed.Strike()&&landed.Total==1&&landed.S.basket.Count==1,"釣果の二重付与を拒否");
        landed.ResultBack();landed.S.practice=true;landed.Aim();landed.Cast();Check(landed.S.biteFor>3,"ゆっくり釣りの猶予");
        Check(landed.Cancel()&&landed.S.mode==TidesMode.Explore,"仕掛けを回収");
    }
    static void Revision(TidesCatalog c,TidesFishery w)
    {
        var old=Reload(w);Prepare(old,c.Index("ayu"));var f=c.species[c.Index("ayu")];
        for(int i=0;i<2;i++)
        {
            old.Aim();old.SetAim(0,(f.minCast+f.maxCast)/2);old.Cast();
            if(old.S.hooked.id==f.id)break;
            old.Cancel();
        }
        Bite(old);old.Strike();old.Step(.45f,true);
        var revised=JsonUtility.FromJson<TidesCatalog>(JsonUtility.ToJson(c));var list=new List<TidesSpecies>(revised.species);
        list.RemoveAll(x=>x.id=="ayu");list.Reverse();
        var extra=JsonUtility.FromJson<TidesSpecies>(JsonUtility.ToJson(list[0]));extra.id="extra_fish";extra.name="追加入稿";list.Add(extra);
        revised.species=list.ToArray();foreach(var o in revised.orders)if(o.fishId=="ayu")o.fishId="oikawa";
        Array.Reverse(revised.orders);revised.Validate();var edited=Reload(old,revised);
        Check(edited.FishIndex<0&&edited.S.hooked.id=="ayu","削除した魚の進行中データ");
        int previous=edited.Total;Land(edited);Check(edited.Total==previous+1&&edited.S.landing.name==f.name,"未登録魚も一度だけ取り込む");
        var original=Reload(edited,c);Check(original.Finding(c.Index("ayu")).count>=1&&original.S.graduated,"元の図鑑へ手作りデータを復元");
    }
    public static TidesFishery Run(TidesCatalog c)
    {
        Timing(c);var w=Complete(c);
        foreach(var f in c.species)
        {
            int i=c.Index(f.id);Catch(w,i);
            var record=w.Finding(i);Check(record.count>0&&record.bestLength>=f.minCm-.06f&&record.bestLength<=f.maxCm+.06f&&record.bestWeight>0,"寸法台帳 "+f.id);
        }
        Check(w.Discovered==c.species.Length&&w.S.graduated,"全魚種と大物4種の図鑑");
        int favorite=c.Index("hiramasa");Check(w.Favorite(favorite)&&Reload(w).Finding(favorite).favorite,"お気に入りの保存");
        for(int type=0;type<3;type++)while(w.Cost(type)>0&&w.S.coins>=w.Cost(type))Check(w.Upgrade(type),"成長");
        for(int j=0;j<2;j++)
        {
            var o=w.Order;while(w.OrderCount<o.quantity)Catch(w,c.Index(o.fishId));
            Check(w.Deliver()&&w.S.repeats==j+1,"巡回納品と継続");
        }
        var restored=Reload(w);Check(restored.Total==w.Total&&restored.S.basket.Count==w.S.basket.Count&&restored.S.repeats==2&&restored.Order.fishId==w.Order.fishId,"魚籠・巡回の再開");
        Revision(c,w);
        Debug.Log("TIDES_RULES_OK all species "+c.species.Length+", story "+c.orders.Length+", 2 repeat surveys, 4 waters/trophies, line tension/slack, hook timing, save migration");
        return w;
    }
}
