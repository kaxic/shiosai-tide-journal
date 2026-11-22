using UnityEngine;

public sealed class TidesCatchPrint : StudioPrint
{
    public static bool Framing { get; private set; }
    protected override string Folder => "CatchCards";
    protected override string Prefix => "TideCatch";
    protected override string Caption => "釣果カード";
    protected override string CheckMarker => "TIDES_CATCH_CARD_OK";
    protected override bool Eligible => ((TidesGame)Game).Mode == TidesMode.Catch;
    protected override void SetFrame(bool value) { Framing = value; }

    public static void DrawCard(TidesGame g)
    {
        Color paper = new Color32(246, 237, 219, 255), ink = new Color32(37, 65, 64, 255);
        Color rim = new Color32(43, 81, 80, 255), gold = new Color32(182, 137, 77, 255);
        float w = g.W, h = g.H, x = w * .065f, y = h * .10f, split = w * .35f;
        g.Box(new Rect(0,0,w,h), rim);
        g.Box(new Rect(13,13,w-26,h-26), paper);
        g.Box(new Rect(x,y,w-2*x,2), gold);
        g.Label(new Rect(x,y+10,w-2*x,30),"潮路の釣り手帖　／　釣果記録",17,ink);
        g.Label(new Rect(x,y+10,w-2*x,30),$"第 {g.Day} 日　{TidesGame.TimeNames[g.TimeSlot]}",16,ink,false,TextAnchor.MiddleRight);
        float nameY = h * .27f;
        g.Label(new Rect(x,nameY,split-x,63),g.LandingName,43,ink,true);
        g.Label(new Rect(x,nameY+67,split-x,28),TidesGame.SiteNames[g.Site],19,ink);
        g.Label(new Rect(x,nameY+101,split-x,27),TidesGame.WeatherNames[g.Weather]+"　"+TidesGame.TideNames[g.Tide],16,ink);
        g.Box(new Rect(x,nameY+151,57,3),gold);
        g.Label(new Rect(x,nameY+174,split-x,37),$"体長　{g.LandingLength:0.0} cm",24,ink,true);
        g.Label(new Rect(x,nameY+217,split-x,37),"体重　"+TidesHud.FormatWeight(g.LandingWeight),23,ink,true);

        int i = g.FishIndex;
        if (i >= 0 && i < g.Fish.Length)
        {
            float aw = w-x-split, ah = Mathf.Min(aw/1.5f,h*.55f);
            var art = new Rect(split,h*.49f-ah*.5f,aw,ah);
            TidesHud.DrawFish(g,art,i,true);
            var fish = g.Fish[i];
            float bx=split+18, bw=aw-36, by=h*.80f;
            g.Box(new Rect(bx,by,bw,2),gold);
            for(int n=0;n<=10;n++)g.Box(new Rect(bx+bw*n/10f,by-3,1,n%5==0?10:6),ink);
            float mark = Mathf.InverseLerp(fish.minCm,fish.maxCm,g.LandingLength);
            g.Box(new Rect(bx+bw*mark-2,by-10,4,19),rim);
            g.Label(new Rect(bx,by+12,bw,23),$"{fish.minCm:0.#} cm",14,ink);
            g.Label(new Rect(bx,by+12,bw,23),"この魚種の体長と今回の記録",14,ink,false,TextAnchor.MiddleCenter);
            g.Label(new Rect(bx,by+12,bw,23),$"{fish.maxCm:0.#} cm",14,ink,false,TextAnchor.MiddleRight);
        }
        g.Box(new Rect(x,h*.91f,w-2*x,1),gold);
        g.Label(new Rect(x,h*.91f+3,w-2*x,25),g.GradeName,15,ink);
    }
}
