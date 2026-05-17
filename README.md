# BomBom Lemon

2〜20人対応の2Dパーティボードゲーム（iOS / Android）

## 概要

Unity 6 で制作するモバイル向けボードゲームです。同じ端末での順番プレイ（ローカル）と、ネットワーク越しの対戦（オンライン）の両方に対応予定です。

## 使用技術

| カテゴリ | 技術 |
|---|---|
| エンジン | Unity 6 (6000.0 LTS) |
| ターゲット | iOS / Android |
| オンライン通信 | Unity Netcode for GameObjects |
| マッチメイキング | Unity Gaming Services (Relay + Lobby) |
| 認証 | Unity Authentication Service |
| UI | Unity UI (uGUI) + TextMeshPro |
| 入力 | Unity Input System |
| レンダリング | Universal Render Pipeline (URP) 2D |

## ディレクトリ構成

```
Assets/
  Scripts/
    Core/        # ゲーム全体の管理 (GameManager, TurnManager, GameState, GameSettings)
    Board/       # ボード・タイル管理 (BoardManager, Tile, TileType)
    Player/      # プレイヤーデータ・トークン (PlayerData, PlayerToken)
    Dice/        # サイコロ処理 (DiceRoller)
    Network/     # マルチプレイ管理 (NetworkSessionManager, MultiplayerMode)
    UI/          # 画面管理 (UIManager, MainMenuUI, PlayerSetupUI, GameHUD)
  Scenes/        # Unity シーンファイル
  Prefabs/       # プレハブ
  Sprites/       # スプライト素材
  Audio/         # BGM・SE
  Resources/     # 動的ロードリソース
ProjectSettings/ # Unity プロジェクト設定
Packages/        # パッケージ依存関係 (manifest.json)
```

## セットアップ

1. [Unity Hub](https://unity.com/download) で Unity 6000.0.x をインストール
2. このリポジトリをクローン
3. Unity Hub から `bombom_lemon_unity` フォルダを開く
4. Unity Gaming Services を使うには `Project Settings > Services` でプロジェクトをリンク

## プレイ人数

- 最小: 2人
- 最大: 20人
