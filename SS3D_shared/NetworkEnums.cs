﻿using Lidgren.Network;

public enum NetMessage
{
    GameType = 0,
    LobbyChat,
    ServerName,
    ClientName,
    WelcomeMessage,
    MaxPlayers,
    PlayerCount,
    SendMap,
    ChangeTile,
    ItemMessage, // It's something the item system needs to handle
    MobMessage,
    ChatMessage,
    AtomManagerMessage,
    PlayerSessionMessage,
    JoinGame
}

public enum ItemMessage
{
    CreateItem = 0,
    InterpolationPacket,
    ClickItem,
    PickUpItem,
    DropItem,
    UseItem,
    Click,
    AttachTo,
    Detach
}

public enum AtomManagerMessage
{
    SpawnAtom,
    DeleteAtom,
    Passthrough
}

public enum MobMessage
{
    CreateMob = 0,
    InterpolationPacket,
    DeleteMob,
    ClickMob,
    AnimationState,
    AnimateOnce,
    SelectAppendage,
    DropItem,
    Death
}

public enum MobHand
{
    RHand = 0,
    LHand
}

public enum GameType
{
    MapEditor = 0,
    Game
}

public enum ClientStatus
{
    Lobby = 0,
    Game
}

public enum PlayerSessionMessage
{
    AttachToAtom,
    Verb,
    JoinLobby
}

public enum DoorState
{
    Closed = 0,
    Open,
    Broken
}

public enum LightState
{
    Off = 0,
    On
}

public enum LightDirection
{
    East = 0,
    South,
    West,
    North,
    All
}