using UnityEngine;

public sealed class TidesAmbience : MonoBehaviour
{
    TidesGame game;
    StudioAmbientMix mix;

    void Start()
    {
        game = GetComponent<TidesGame>();
        mix = new StudioAmbientMix(gameObject, "Tides/Sound", "River", "Lake", "Coast", "Boat", "HarborRain");
    }

    void Update()
    {
        if (!game) return;
        float level = game.Playing ? game.EffectsVolume : 0;
        bool rain = game.Weather == 2;
        mix.Set(game.Site%4, level * (rain ? .49f : .86f), 4, rain ? level * .54f : 0);
    }
}
