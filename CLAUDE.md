# BomBom Lemon Unity Project

## コミュニケーションルール

ユーザーへの指示は**コードボックス1つにコマンドだけ**を入れる。説明はその外に書く。コピペだけで済むようにすること。

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

## ブランチ

作業ブランチ：`claude/setup-unity-board-game-vwJPn`

## シーンのリビルド

TitleSceneBuilder.csを変更したら Unity メニュー **BomBomLemon → Build Title Scene** を実行。
