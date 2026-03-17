# 同期エンジン

このドキュメントでは、VehicleVision.Pleasanter.ReplicaSync の同期エンジンの仕組みについて説明します。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [同期処理フロー](#同期処理フロー)
- [競合解決戦略](#競合解決戦略)
    - [SourceWins](#sourcewins)
    - [LastWriteWins](#lastwritewins)
    - [ManualResolution](#manualresolution)
    - [FieldLevelMerge](#fieldlevelmerge)
- [ルールエンジン](#ルールエンジン)
    - [カラム決定の優先順位](#カラム決定の優先順位)
    - [対象カラム](#対象カラム)
- [変更検出](#変更検出)
- [同期ログ](#同期ログ)
- [エラーハンドリング](#エラーハンドリング)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 同期処理フロー

`SyncEngine` は以下の手順でデータ同期を実行します。

1. ソースインスタンスから前回同期以降に変更されたレコードを取得
2. 各ターゲットマッピングに対して、ソースからターゲットへの同期が有効か確認
3. `SyncRuleEngine` を使用して同期対象カラムを決定
4. 各変更レコードについて：
    - カラムフィルタリングを適用
    - 同期キーでターゲット側のレコードを検索
    - 競合解決戦略に基づいて処理を判断
    - レコードを Upsert（挿入または更新）
5. ソースインスタンスの削除済みレコードをターゲットに反映
6. 同期結果を `SyncLogEntry` に記録

## 競合解決戦略

同期時にソースとターゲットの両方でレコードが変更されていた場合の解決方法を指定します。

### SourceWins

ソース側のデータを常に優先します。ターゲット側の変更は上書きされます。

```text
Source: ClassA = "東京" (UpdatedTime: 10:00)
Target: ClassA = "大阪" (UpdatedTime: 10:05)
→ 結果: ClassA = "東京"（ソースが常に優先）
```

### LastWriteWins

`UpdatedTime` を比較し、最後に更新された方のデータを採用します。

```text
Source: ClassA = "東京" (UpdatedTime: 10:00)
Target: ClassA = "大阪" (UpdatedTime: 10:05)
→ 結果: ClassA = "大阪"（ターゲットの方が新しいためスキップ）
```

### ManualResolution

競合を検出した場合、同期をスキップしてログに記録します。管理者が手動で解決する運用を想定しています。

```text
Source: ClassA = "東京" (UpdatedTime: 10:00)
Target: ClassA = "大阪" (UpdatedTime: 10:05)
→ 結果: 同期スキップ、ログに Conflict として記録
```

### FieldLevelMerge

各カラムの最終更新日時を比較し、フィールド単位でマージします。ソースの方が新しいカラムのみターゲットに反映されます。

```text
Source: ClassA = "東京" (10:00), ClassB = "営業" (10:05)
Target: ClassA = "大阪" (10:03), ClassB = "開発" (09:50)
→ 結果: ClassA = "大阪"（ターゲットが新しい）, ClassB = "営業"（ソースが新しい）
```

## ルールエンジン

`SyncRuleEngine` は同期対象カラムの決定ロジックを提供します。

### カラム決定の優先順位

1. `IncludeColumns` が指定されている場合、そのカラムのみを対象とする
2. `IncludeColumns` が未指定の場合、全 Pleasanter データカラムを対象とする
3. `ExcludeColumns` に指定されたカラムを除外する
4. ターゲットマッピングの `TargetExcludeColumns` に指定されたカラムをさらに除外する

### 対象カラム

Pleasanter の標準データカラムを同期対象として扱います。Results / Issues と Wikis でカラム構成が異なります。

#### Results / Issues

| 種別 | カラム名                         |
| ---- | -------------------------------- |
| 基本 | `Title`、`Body`                  |
| 分類 | `ClassA` ～ `ClassZ`             |
| 数値 | `NumA` ～ `NumZ`                 |
| 日付 | `DateA` ～ `DateZ`               |
| 説明 | `DescriptionA` ～ `DescriptionZ` |

#### Wikis

Wikis テーブルは Class / Num / Date / Description カラムを持ちません。

| 種別   | カラム名            |
| ------ | ------------------- |
| 基本   | `Title`、`Body`     |
| ロック | `Locked`（自動同期）|

## 変更検出

現在はポーリング方式のみをサポートしています。

- `UpdatedTime` を基準に前回同期以降に変更されたレコードを検出
- 削除されたレコードは Pleasanter の `_deleted` テーブルから検出
- ポーリング間隔は `SyncDefinition.PollingIntervalSeconds` で設定（1～3600秒）

## 同期ログ

すべての同期操作は `SyncLogEntry` として記録されます。

| フィールド          | 説明                                                  |
| ------------------- | ----------------------------------------------------- |
| `SyncId`            | 同期定義の識別子                                      |
| `SourceInstanceId`  | ソースインスタンスの識別子                            |
| `TargetInstanceId`  | ターゲットインスタンスの識別子                        |
| `Status`            | 結果（`Success` / `Failed` / `Conflict` / `Skipped`） |
| `RecordsProcessed`  | 処理レコード数                                        |
| `RecordsInserted`   | 挿入レコード数                                        |
| `RecordsUpdated`    | 更新レコード数                                        |
| `RecordsDeleted`    | 削除レコード数                                        |
| `ConflictsDetected` | 検出された競合数                                      |
| `ErrorMessage`      | エラーメッセージ（エラー時のみ）                      |
| `StartedAt`         | 開始日時                                              |
| `CompletedAt`       | 完了日時                                              |

## エラーハンドリング

- 同期処理中にエラーが発生した場合、`SyncLogEntry` に `Failed` ステータスとエラーメッセージを記録
- Worker サービスは個別の同期定義のエラーで停止せず、30秒のリトライ遅延後に次の同期サイクルを継続
- データベース操作はトランザクションを使用し、一貫性を確保
