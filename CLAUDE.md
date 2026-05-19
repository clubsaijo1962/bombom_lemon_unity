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

## ブランチ

作業ブランチ：`claude/setup-unity-board-game-vwJPn`

## シーンのリビルド

TitleSceneBuilder.csを変更したら Unity メニュー **BomBomLemon → Build Title Scene** を実行。
