# BomBom Lemon Unity Project

## コミュニケーションルール

ユーザーへの指示は**コードボックス1つにコマンドだけ**を入れる。説明はその外に書く。コピペだけで済むようにすること。

## 【絶対禁止】アセットの無断代替

**ユーザーが指定したアセット（画像・フォント・スプライト等）が読み込めない場合、別のアセットで代用することを絶対に禁止する。**

- 指定ファイルが存在しない・SVGなど非対応フォーマット・パスが間違っている場合は、**代替案を実装せずユーザーに報告して指示を待つこと**
- 「〜の代わりに〜を使いました」という行動は一切不可
- null/fallback実装をコードに仕込むことも不可（ユーザーが気づかないため）
- 唯一の例外：ユーザーが明示的に「〜が使えなければ〜を使って」と指示した場合のみ

**違反した場合はユーザーの信頼を損なう重大なミスとみなす。**

## Unity アセット追加後の Git フロー

Unity上でアセット（画像・フォント・スプライトなど）を追加・変更した場合は**必ずこの順番**で実行：

```bash
# 1. まずpullしてリモートの変更を取り込む（これを忘れるとpushが弾かれる）
git pull origin claude/setup-unity-board-game-vwJPn --rebase

# 2. 変更・追加したファイルをステージング
git add Assets/Sprites/UI/kenney_ui-pack   # 例：Kenneyアセット
git add Assets/                             # または Assets 全体

# 3. コミット＆プッシュ
git commit -m "説明"
git push
```

**注意点：**
- `git push` が弾かれたら → まず `git pull --rebase` してから再度 `git push`
- `.png` や `.unity` などバイナリファイルも普通に `git add` できる（.gitignoreに除外設定なし）
- Unityが自動生成する `.meta` ファイルも必ず一緒にコミットすること

## フォントサイズの最低基準【厳守】

キャンバス設定：1080×1920、matchWidthOrHeight=0.5

iPhone SE（750×1334、2×スケール）でのスケール係数 ≈ 0.695。
`実画面サイズ(pt) = canvas_fontSize × 0.695 ÷ 2`

| 用途 | 最低canvas fontSize |
|---|---|
| 補足・キャプション・ラベル | **32pt以上** |
| ボディ・入力・説明文 | **44pt以上** |
| ヘッダー・タイトル | **56pt以上** |

- 32pt → 実画面 11.1pt（iOS最低基準ぴったり）
- 24pt → 実画面 8.4pt（**基準違反**。実機で読めない）

**UIテキストはいかなる場合も32pt未満で実装しないこと。autoSizingのfontSizeMinも32pt以上にすること。**

## ブランチ

作業ブランチ：`claude/setup-unity-board-game-vwJPn`

## シーンのリビルド

TitleSceneBuilder.csを変更したら Unity メニュー **BomBomLemon → Build Title Scene** を実行。
