using System;
using UnityEngine;

public sealed partial class TidesHud
{
    void Travel()
    {
        Rect p=Book(922,414,"WATERSIDE ATLAS","四つの釣り場");
        float x=p.x+20,y=p.y+86,width=(p.width-61)/4;
        for(int i=0;i<TidesGame.SiteNames.Length;i++)
        {
            Rect r=new Rect(x+i*(width+7),y,width,232);
            Row(r,game.Selection==i);
            WaterPicture(new Rect(r.x+5,r.y+5,width-10,101),i);
            Text(TidesGame.SiteNames[i],r.x+8,r.y+111,width-16,32,17,Ink,true);
            Text(TidesGame.SiteNotes[i],r.x+9,r.y+147,width-18,49,14,Reed);
            Text(i==game.Site?"現在地":game.CanTravel(i)?"渡航できます":"未開放",r.x+9,r.y+201,width-18,24,14,game.CanTravel(i)?Water:Ember);
            if(Hit(r))game.Highlight(i);
        }
        int selected=Mathf.Clamp(game.Selection,0,TidesGame.SiteNames.Length-1);
        Text(game.CanTravel(selected)?"この水辺に着いてから岸辺で竿を構えます。":game.TravelRule(selected),x,p.yMax-62,p.width-290,43,15,Reed);
        if(Button(new Rect(p.xMax-222,p.yMax-58,202,38),selected==game.Site?"この水辺に戻る":"この水辺へ",game.CanTravel(selected)))game.Travel(selected);
    }

    void Clock()
    {
        Rect p=Book(766,345,"TIDE AND WEATHER","時刻と釣り予報");
        float x=p.x+19,y=p.y+89,w=(p.width-59)/4;
        for(int i=0;i<4;i++)
        {
            Rect r=new Rect(x+i*(w+7),y,w,133);
            Row(r,game.Selection==i);
            Text(TidesGame.TimeNames[i],r.x+9,r.y+5,w-18,33,21,Ink,true);
            Text(game.Forecast(i),r.x+10,r.y+42,w-20,79,14,Reed);
            if(Hit(r))game.Highlight(i);
        }
        int index=Mathf.Clamp(game.Selection,0,3);
        Text("今と同じ時刻か、それ以前を選ぶと翌日になります。",x,p.yMax-80,p.width-261,61,15,Reed);
        if(Button(new Rect(p.xMax-211,p.yMax-59,191,38),TidesGame.TimeNames[index]+"まで待つ",true))game.SetTime(index);
    }

    void Shop()
    {
        Rect p=Book(833,467,"THE TACKLE SHOP","水辺の釣具店");
        float x=p.x+20,y=p.y+83,half=(p.width-52)/2,rx=x+half+12;
        Text("道具を整える",x,y,half,30,19,Ink,true);
        Text("使う餌",rx,y,half,30,19,Ink,true);
        for(int i=0;i<3;i++)
        {
            Rect r=new Rect(x,y+38+i*56,half,51);
            Row(r,game.Selection==i);
            Text(TidesGame.GearNames[i],r.x+8,r.y+3,half*.49f,25,16,Ink);
            Text($"段階 {game.GearLevel(i)}",r.x+9,r.y+28,half*.53f,19,13,Reed);
            Text(game.UpgradeCost(i)>0?$"{game.UpgradeCost(i)} 円":"最高段階",r.x+half*.52f,r.y+5,half*.46f-7,41,15,Water,align:TextAnchor.MiddleRight);
            if(Hit(r))game.Highlight(i);
        }
        for(int i=0;i<4;i++)
        {
            float bw=(half-6)/2;
            Rect r=new Rect(rx+i%2*(bw+6),y+38+i/2*59,bw,53);
            Row(r,game.Selection==3+i);
            Text(TidesGame.BaitNames[i],r.x+7,r.y+1,bw-14,28,15,Ink);
            Text(game.ActiveBait==i?"使用中":"持ち替える",r.x+8,r.y+29,bw-16,21,13,game.ActiveBait==i?Ember:Reed);
            if(Hit(r))game.Highlight(i+3);
        }
        Rect sell=new Rect(rx,y+157,half,49);
        Row(sell,game.Selection==7);
        Text($"魚籠を売却　{game.BagValue} 円",sell.x+8,sell.y+2,sell.width-16,sell.height-4,16,Water);
        if(Hit(sell))game.Highlight(7);
        int selected=game.Selection;
        string detail;
        if(selected<3)
        {
            string[] notes={"大物とのやり取りが安定します。","岸まで巻き寄せる力を高めます。","糸の張りの変化を抑えます。"};
            detail=notes[selected];
        }
        else detail=selected==7?"今の依頼に必要な魚は魚籠に取り置きします。":"餌は無料で何度でも持ち替えられます。";
        Line(x,y+224,p.width-40);
        Text(detail,x+1,y+238,p.width-42,42,15,Reed);
        Text($"所持金 {game.Coins} 円　／　魚籠 {game.BagCount} 尾",x,p.yMax-56,p.width-260,37,15,Ink);
        string verb=selected<3?"この道具を強化":selected==7?"取り置き以外を売る":"この餌を使う";
        bool enabled=selected<3?game.CanUpgrade(selected):selected!=7||game.BagValue>0;
        if(Button(new Rect(p.xMax-218,p.yMax-57,198,38),verb,enabled))
        {
            if(selected==7)game.SellAll();else game.ShopAction(selected);
        }
    }

    void Requests()
    {
        Rect p=Book(851,474,"WATERSIDE REQUESTS","水辺の依頼帖");
        float x=p.x+20,y=p.y+85,left=254,rx=x+left+17,rw=p.xMax-rx-20;
        int total=game.Orders.Length;
        if(game.FinishedSurvey)
        {
            Fill(new Rect(p.xMax-294,p.y+19,173,35),Select);
            Text($"調査修了　巡回 {game.RepeatCount} 件",p.xMax-289,p.y+19,163,35,15,Water,align:TextAnchor.MiddleCenter);
        }
        int count=total+(game.FinishedSurvey?1:0);
        int selected=Mathf.Clamp(game.Selection,0,count-1),first=selected/6*6;
        for(int i=first;i<Mathf.Min(first+6,count);i++)
        {
            Rect r=new Rect(x,y+(i-first)*45,left,41);
            Row(r,i==selected);
            bool repeat=i==total;
            string title=repeat?"巡回の依頼":game.Orders[i].name;
            Text(title,r.x+8,r.y+1,left-79,r.height-2,15,Ink);
            Text(game.OrderFinished(i)?"納品済":i==game.RequestIndex?"受付中":"未開放",r.xMax-67,r.y,59,r.height,13,i==game.RequestIndex?Ember:Reed,align:TextAnchor.MiddleRight);
            if(Hit(r))game.Highlight(i);
        }
        selected=Mathf.Clamp(game.Selection,0,count-1);
        var order=selected==total?game.ActiveOrder:game.Orders[selected];
        int fish=Array.FindIndex(game.Fish,f=>f.id==order.fishId);
        if(fish>=0)FishPicture(new Rect(rx,y,170,97),fish,game.SpeciesCaught(fish)>0);
        Text(order.name,rx+182,y+4,rw-187,43,21,Ink,true);
        Text($"{order.client}　／　謝礼 {order.reward} 円",rx+183,y+50,rw-188,43,15,Water);
        Text(order.note,rx+1,y+111,rw-2,54,15,Reed);
        Line(rx,y+175,rw);
        string target=fish>=0?game.Fish[fish].name:order.fishId;
        Text($"指定　{target}　{order.quantity} 尾",rx+1,y+181,rw-2,29,16,Ink);
        if(selected==game.RequestIndex)Text($"魚籠の該当魚　{game.OrderCount} / {order.quantity} 尾",rx+1,y+215,rw-2,29,15,Water);
        if(fish>=0)Text(game.FishHint(fish),rx+1,y+253,rw-2,55,14,Reed);
        bool active=selected==game.RequestIndex;
        if(Button(new Rect(p.xMax-200,p.yMax-57,180,38),active?"依頼に納品":game.OrderFinished(selected)?"納品済":"未開放",active&&game.CanDeliver))game.Deliver();
        if(fish>=0&&Button(new Rect(rx,p.yMax-57,rw-188,38),"この魚を追跡",true,false))game.Track(fish);
        if(count>6)
        {
            if(Button(new Rect(x,p.yMax-56,79,33),"前の頁",first>0,false))game.Highlight(first-6);
            if(Button(new Rect(x+84,p.yMax-56,79,33),"次の頁",first+6<count,false))game.Highlight(first+6);
        }
        else Text($"巡回達成　{game.RepeatCount} 件",x,p.yMax-57,left,36,14,Reed);
    }

    void WaterPicture(Rect frame,int site)
    {
        Fill(frame,Page);
        var atlas=game.skin.illustrations;
        if(!atlas)return;
        float aspect=atlas.width/(float)atlas.height;
        float w=Mathf.Max(frame.width,frame.height*aspect),h=w/aspect;
        GUI.BeginGroup(frame);
        game.Art(new Rect((frame.width-w)/2,(frame.height-h)/2,w,h),site);
        GUI.EndGroup();
    }
}
