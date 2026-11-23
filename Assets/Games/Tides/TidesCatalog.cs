using System;
using System.Collections.Generic;
using UnityEngine;

public enum TidesMode { Explore,Aim,Wait,Bite,Fight,Catch,Escape,Travel,Shop,Journal,Requests,Clock }
[Serializable] public sealed class TidesSpecies
{
    public string id,name,note;
    public int site,bait,timeMask,tideMask,weatherMask,price;
    public float minCast,maxCast,minCm,maxCm,fight,bodyFactor;
    public bool legendary,adaptable;
}
[Serializable] public sealed class TidesOrder {public string id,name,client,note,fishId;public int quantity,reward;}
[Serializable] public sealed class TidesCatalog
{
    public TidesSpecies[] species;
    public TidesOrder[] orders;
    [NonSerialized] Dictionary<string,int> speciesIds;
    public static TidesCatalog Load()
    {
        var json=Resources.Load<TextAsset>("Tides/Fishery");
        if(!json)throw new InvalidOperationException("Resources/Tides/Fishery.json がありません。");
        var c=JsonUtility.FromJson<TidesCatalog>(json.text);c.Validate();return c;
    }
    public int Index(string id)=>id!=null&&speciesIds.TryGetValue(id,out int i)?i:-1;
    public void Validate()
    {
        if(species==null||species.Length<1||orders==null||orders.Length<1)throw new InvalidOperationException("魚と依頼を各1件以上定義してください。");
        speciesIds=new Dictionary<string,int>(StringComparer.Ordinal);
        int flexibleSites=0;
        for(int i=0;i<species.Length;i++)
        {
            var s=species[i];
            if(s==null||string.IsNullOrWhiteSpace(s.id)||string.IsNullOrWhiteSpace(s.name)||!speciesIds.TryAdd(s.id,i)||
              s.site<0||s.site>=TidesFishery.PermitOrders.Length||s.bait<0||s.bait>=TidesGame.BaitNames.Length||s.timeMask<=0||s.timeMask>15||s.tideMask<=0||s.tideMask>3||
              s.weatherMask<=0||s.weatherMask>7||!(s.minCast>=0&&s.maxCast<=1&&s.maxCast>=s.minCast)||
              !(s.fight>=.35f&&s.fight<=2.5f&&s.minCm>0&&s.maxCm>=s.minCm&&s.bodyFactor>0)||s.price<1)
                throw new InvalidOperationException("魚 "+(i+1)+" のID・場所・季節条件・価格・寸法を確認してください。");
            if(s.adaptable&&!s.legendary)flexibleSites|=1<<s.site;
        }
        if(flexibleSites!=(1<<TidesFishery.PermitOrders.Length)-1)throw new InvalidOperationException("各水域にどの仕掛けでも釣れる普通種 adaptable を1種登録してください。");
        var ids=new HashSet<string>();
        for(int i=0;i<orders.Length;i++)
        {
            var o=orders[i];if(o==null||string.IsNullOrWhiteSpace(o.id)||!ids.Add(o.id)||string.IsNullOrWhiteSpace(o.name)||
                Index(o.fishId)<0||o.quantity<1||o.reward<1)throw new InvalidOperationException("依頼 "+(i+1)+" のID・対象魚・数量・報酬を確認してください。");
        }
    }
}
