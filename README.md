# BomBom Lemon

## 概要

**BomBom Lemon** は Unity 6 (6000.0 LTS) で開発される、iOS および Android 向けの 2D パーティボードゲームです。  
2〜20 人のプレイヤーが 1 台のデバイスを使ったパスアンドプレイ（ローカルマルチプレイ）、またはオンラインマルチプレイを楽しめます。

---

## 使用技術

| カテゴリ | ライブラリ / サービス |
|---|---|
| ゲームエンジン | Unity 6 (6000.0.25f1 LTS) |
| 2D レンダリング | Universal Render Pipeline (URP) 17.0.3 |
| スプライト / アニメーション | com.unity.2d.sprite 1.0.0 / com.unity.2d.animation 10.1.2 |
| オンライン通信 | Unity Netcode for GameObjects 2.1.1 |
| 認証 | Unity Services Authentication 3.3.3 |
| ルームマッチング | Unity Services Relay 1.1.1 / Lobby 1.2.2 |
| UI | UGUI 2.0.0 / TextMeshPro 3.2.0 |
| 入力 | Unity Input System 1.8.2 |

---

## ターゲットプラットフォーム

- **iOS** (iPhone / iPad)
- **Android** (API 22 以上)

---

## プレイヤー人数

- 最小: 2 人
- 最大: 20 人
- ローカル（パスアンドプレイ）およびオンライン対応

---

## ディレクトリ構成

```
bombom_lemon_unity/
├── Assets/
│   ├── Audio/            # BGM・SE アセット
│   ├── Prefabs/          # プレハブ
│   ├── Resources/        # 実行時ロードリソース
│   ├── Scenes/           # Unity シーンファイル
│   ├── Scripts/
│   │   ├── Board/        # ボード・タイル関連スクリプト
│   │   │   ├── BoardManager.cs
│   │   │   ├── Tile.cs
│   │   │   └── TileType.cs
│   │   ├── Core/         # ゲームフロー管理
│   │   │   ├── GameManager.cs
│   │   │   ├── GameSettings.cs
│   │   │   ├── GameState.cs
│   │   │   └── TurnManager.cs
│   │   ├── Dice/         # サイコロ処理
│   │   │   └── DiceRoller.cs
│   │   ├── Network/      # マルチプレイ・セッション管理
│   │   │   ├── MultiplayerMode.cs
│   │   │   └── NetworkSessionManager.cs
│   │   ├── Player/       # プレイヤーデータ・トークン
│   │   │   ├── PlayerData.cs
│   │   │   └── PlayerToken.cs
│   │   └── UI/           # UI コントローラ
│   │       ├── GameHUD.cs
│   │       ├── MainMenuUI.cs
│   │       ├── PlayerSetupUI.cs
│   │       └── UIManager.cs
│   └── Sprites/          # スプライト / テクスチャ
├── Packages/
│   └── manifest.json     # Unity パッケージ依存関係
├── ProjectSettings/
│   ├── ProjectSettings.asset
│   └── ProjectVersion.txt
└── README.md
```

---

## セットアップ手順

1. **Unity Hub** で Unity 6 (6000.0.25f1) を使ってプロジェクトを開きます。
2. Unity が `Packages/manifest.json` を読み込み、依存パッケージを自動でインストールします。
3. `File > Build Settings` で **iOS** または **Android** プラットフォームに切り替えます。
4. `Assets/Scripts/Core/GameSettings` を `ScriptableObject` として作成し、`Assets/Resources/` に配置します。
5. シーンに `GameManager`、`BoardManager`、`NetworkSessionManager`、`UIManager` の各 MonoBehaviour をセットアップします。

---

## ライセンス

本プロジェクトは BomBomGames のプロプライエタリライセンスの下で管理されています。
