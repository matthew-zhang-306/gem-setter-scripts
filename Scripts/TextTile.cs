using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextTile : TileItem
{
    public static string[] TEXTS = new string[] {
        "use wasd or the arrow keys to move. yeah, like that!",
        "click on neighboring ice blocks to connect to them.",
        "you can also right click to connect to many ice blocks at once.",
        "pass over switches to flip the corresponding donut blocks.",
        "filled switches will trigger a second time when you move off of it.",
        "one-way gates will block your movement in one direction.",
        "fire tiles are dangerous.",
        "pass an ice block over them to burn them out.",
        "thank you so much for playing gem setter.",
        "i hope you enjoyed your time with my small passion project. i know i enjoyed making it",
        "developing during the pandemic was wild. i often wasn't sure if i would make it.",
        "it wouldn't have been possible without the support of countless people along the journey.",
        "i am thankful to everyone who has encouraged me to push through.",
        "dealing with general doubt and lethargy would have been really tough otherwise.",
        "despite life's curveballs though, i feel like an improved person. i hope you do, too",
        "the world is a strange place but i believe that games can help with that",
        "whatever reason you may have for playing games, i'm sure that it's a very real one.",
        "it means a lot to me that you would choose my little toy for that purpose.",
        "thanks again, and i wish you the best possible rest of your life.",
        "<3",
        "if you are reading this, then the creator of this level hacked the game!",
        "i am a duck",
        "quack quack",
        "i hope you like this level!",
        "this one is tricky. good luck.",
        "here's something simple to make your day.",
        "yahaha, you found me!",
        "gaming",
        ":)",
        "that was what you call a gamer move",
        "only people with 10000 iq or more allowed entry.",
        "i'm gonna say it. i'm gonna say the f word.",
        "FUCK",
        "remember to like comment and subscribe, also check out my soundcloud",
        "wowee is this geometry dash????1??",
        "ooh s p o o k y",
        "have you ever tried putting a leaf on a lake and then putting a butterfly on the water on the leaf on the water and that attaching the leaf to a dragonfly with a spider silk or something and pulling it really fast yknow it'd be a wakeboarding butterfly",
        "crash bang blop explosion",
        "happy birthday, miyolophone!",
        "i better be getting paid for this.",
        "i am good arteest",
        "yoink!",
        "well it looks like you reached the end of the list",
        "from here on out i'm just gonna repeat the same thing over and over"
    };

    public delegate void TextTileAction(string text);
    public static TextTileAction OnDisplay;

    public string text { get { return TEXTS[Mathf.Min(textId, TEXTS.Length - 1)]; }}
    public int textId;

    public override byte GetAltCode() {
        return (byte)textId;
    }

    public override bool SetAltCode(byte alt) {
        textId = alt;
        return true;
    }

    public override void OnStrongOverlap(TileItem overlap, PlayerController player) {
        OnDisplay?.Invoke(text);
    }
}
