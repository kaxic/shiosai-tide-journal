using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public sealed class TidesFieldWindow : EditorWindow
{
    const string DataPath = "Assets/Games/Tides/Resources/Tides/Fishery.json";
    static readonly string[] Sites = { "せせらぎの源流", "銀鏡の湖", "潮見の漁港", "白帆の外礁" };
    static readonly string[] Times = { "朝", "昼", "夕", "夜" };
    static readonly string[] Weathers = { "晴", "曇", "雨" };
    static readonly string[] Currents = { "下げ潮", "上げ潮" };
    static readonly string[] Baits = { "川虫", "練り餌", "小魚", "エビ" };
    TidesCatalog catalog;
    string baseline, error;
    int tab, selected;
    Vector2 scroll;

    [MenuItem("潮彩作品集/潮路の魚と依頼を編集")]
    static void Open()
    {
        var window = GetWindow<TidesFieldWindow>("潮路の魚と依頼");
        window.minSize = new Vector2(440, 410);
    }
    void OnEnable() { Reload(); }
    void Reload()
    {
        try
        {
            baseline = File.ReadAllText(DataPath);
            catalog = JsonUtility.FromJson<TidesCatalog>(baseline);
            catalog.Validate(); selected = 0; error = null;
        }
        catch (Exception ex) { error = ex.Message; catalog = null; }
    }

    void OnGUI()
    {
        if (error != null) EditorGUILayout.HelpBox(error, MessageType.Warning);
        if (catalog == null) { if (GUILayout.Button("魚類データを再読込")) Reload(); return; }
        int next = GUILayout.Toolbar(tab, new[] { "魚と生息条件", "漁協の依頼" });
        if (next != tab) { tab = next; selected = 0; scroll = Vector2.zero; }
        var names = tab == 0 ? Array.ConvertAll(catalog.species, s => s.name) : Array.ConvertAll(catalog.orders, o => o.name);
        using (new EditorGUILayout.HorizontalScope())
        {
            selected = EditorGUILayout.Popup(selected, names);
            if (GUILayout.Button("複製して追加", GUILayout.Width(111))) Duplicate();
        }
        scroll = EditorGUILayout.BeginScrollView(scroll);
        if (tab == 0) DrawFish(catalog.species[selected]);
        else DrawOrder(catalog.orders[selected]);
        EditorGUILayout.EndScrollView();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("検証して保存", GUILayout.Height(32))) Save();
            if (GUILayout.Button("再読込", GUILayout.Width(75), GUILayout.Height(32)) &&
                EditorUtility.DisplayDialog("保存済みの内容を開く", "現在の記入内容をファイルの値に戻します。", "再読込", "戻る")) Reload();
        }
    }

    void DrawFish(TidesSpecies fish)
    {
        fish.id = EditorGUILayout.TextField("魚のID", fish.id);
        fish.name = EditorGUILayout.TextField("魚名", fish.name);
        fish.note = EditorGUILayout.TextField("図鑑の観察メモ", fish.note);
        EditorGUILayout.Space();
        fish.site = EditorGUILayout.Popup("釣り場", fish.site, Sites);
        fish.bait = EditorGUILayout.Popup("好む餌", fish.bait, Baits);
        fish.timeMask = EditorGUILayout.MaskField("時間帯", fish.timeMask, Times) & 15;
        fish.weatherMask = EditorGUILayout.MaskField("天気", fish.weatherMask, Weathers) & 7;
        fish.tideMask = EditorGUILayout.MaskField("潮", fish.tideMask, Currents) & 3;
        float min = fish.minCast, max = fish.maxCast;
        EditorGUILayout.MinMaxSlider($"投げる距離　{min:P0} ～ {max:P0}", ref min, ref max, 0, 1);
        fish.minCast = min; fish.maxCast = max;
        fish.adaptable = EditorGUILayout.Toggle("広い条件で釣れる普通種", fish.adaptable);
        fish.legendary = EditorGUILayout.Toggle("水域の大物", fish.legendary);
        EditorGUILayout.Space();
        fish.minCm = EditorGUILayout.FloatField("最小の体長（cm）", fish.minCm);
        fish.maxCm = EditorGUILayout.FloatField("最大の体長（cm）", fish.maxCm);
        fish.bodyFactor = EditorGUILayout.FloatField("体重の換算係数", fish.bodyFactor);
        fish.fight = EditorGUILayout.Slider("引きの強さ", fish.fight, .35f, 2.5f);
        fish.price = EditorGUILayout.IntField("基本の売値", fish.price);
        EditorGUILayout.HelpBox("固有の標本画は Resources/Tides/Fish/ に魚のIDと同名のPNGを置けます。保存記録や依頼からの参照にはIDを使用します。", MessageType.Info);
    }

    void DrawOrder(TidesOrder order)
    {
        order.id = EditorGUILayout.TextField("依頼ID", order.id);
        order.name = EditorGUILayout.TextField("依頼名", order.name);
        order.client = EditorGUILayout.TextField("依頼主", order.client);
        order.note = EditorGUILayout.TextField("依頼文", order.note);
        var names = Array.ConvertAll(catalog.species, s => s.name);
        int current = Array.FindIndex(catalog.species, s => s.id == order.fishId);
        int choice = EditorGUILayout.Popup("納品する魚", current, names);
        if (choice >= 0) order.fishId = catalog.species[choice].id;
        order.quantity = EditorGUILayout.IntField("納品数", order.quantity);
        order.reward = EditorGUILayout.IntField("報酬", order.reward);
    }

    void Duplicate()
    {
        if (tab == 0)
        {
            var copy = JsonUtility.FromJson<TidesSpecies>(JsonUtility.ToJson(catalog.species[selected]));
            selected = catalog.species.Length;
            copy.id = NextId(copy.id, i => Array.Exists(catalog.species, s => s.id == i));
            copy.name += "（新種）"; Array.Resize(ref catalog.species, selected + 1); catalog.species[selected] = copy;
        }
        else
        {
            var copy = JsonUtility.FromJson<TidesOrder>(JsonUtility.ToJson(catalog.orders[selected]));
            selected = catalog.orders.Length;
            copy.id = NextId(copy.id, i => Array.Exists(catalog.orders, s => s.id == i));
            copy.name += "（新規）"; Array.Resize(ref catalog.orders, selected + 1); catalog.orders[selected] = copy;
        }
    }
    static string NextId(string prefix, Predicate<string> used)
    {
        int suffix = 2; string id;
        do { id = prefix + "_" + suffix++; } while (used(id));
        return id;
    }
    void Save()
    {
        try
        {
            catalog.Validate();
            if (File.ReadAllText(DataPath) != baseline)
                throw new IOException("開いている間にJSONが更新されました。再読込してから編集してください。");
            string next = JsonUtility.ToJson(catalog, true) + Environment.NewLine;
            string tmp = Path.GetFullPath(DataPath) + ".writing";
            File.WriteAllText(tmp, next, new UTF8Encoding(false)); File.Replace(tmp, DataPath, null);
            baseline = next; error = null; AssetDatabase.ImportAsset(DataPath);
            ShowNotification(new GUIContent("保存しました"));
        }
        catch (Exception ex) { error = ex.Message; }
    }
}
