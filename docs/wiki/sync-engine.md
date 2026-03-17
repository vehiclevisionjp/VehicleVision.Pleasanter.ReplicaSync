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
- [バージョン履歴](#バージョン履歴)
    - [スナップショットの記録タイミング](#スナップショットの記録タイミング)
    - [保持ポリシー](#保持ポリシー)
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
    - 既存レコードの更新時、バージョン履歴にスナップショットを保存
    - レコードを Upsert（挿入または更新）
    - 版数ベースの保持ポリシーを適用
5. ソースインスタンスの削除済みレコードについて：
    - ターゲット側のレコードを検索
    - 削除前のバージョン履歴にスナップショットを保存（`IsDeleteSnapshot = true`）
    - ターゲットからレコードを削除
6. 日数ベースのバージョン履歴クリーンアップを実行
7. 同期結果を `SyncLogEntry` に記録

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

| 種別   | カラム名             |
| ------ | -------------------- |
| 基本   | `Title`、`Body`      |
| ロック | `Locked`（自動同期） |

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

## バージョン履歴

同期処理でターゲットレコードが上書きされる前の状態を `RecordVersionHistory` テーブルにスナップショットとして保存します。これにより、同期による変更の追跡と必要に応じた復元が可能になります。

### スナップショットの記録タイミング

- ターゲット側に既存レコードが存在し、Upsert で**更新**される場合に記録（`IsDeleteSnapshot = false`）
- ターゲット側のレコードが同期により**削除**される場合に記録（`IsDeleteSnapshot = true`）
- 新規挿入時はスナップショット不要（上書きされるデータが存在しないため）
- `SyncDefinition.VersionHistoryEnabled` が `false` の場合は記録しない

スナップショットには以下の情報が含まれます：

| 項目             | 内容                                                      |
| ---------------- | --------------------------------------------------------- |
| タイトル         | 上書き/削除前のレコードタイトル                           |
| 本文             | 上書き/削除前のレコード本文                               |
| カラム値         | 全カラム値の JSON スナップショット                        |
| 変更者           | 上書き/削除前の最終更新 Pleasanter ユーザー ID            |
| 変更日時         | 上書き/削除前のレコード UpdatedTime                       |
| IsDeleteSnapshot | 削除前スナップショットの場合 `true`、更新前の場合 `false` |

### 保持ポリシー

バージョン履歴が無制限に蓄積されることを防ぐため、版数と日数の 2 つの保持制限を設定できます。**いずれか早いほう**の条件に該当する履歴が削除されます。

| 設定                        | 既定値 | 説明                       |
| --------------------------- | ------ | -------------------------- |
| `VersionHistoryMaxVersions` | 20     | レコード単位の最大保持版数 |
| `VersionHistoryMaxDays`     | 180    | 履歴の最大保持日数         |

- **版数制限**: 新しいスナップショット保存後、レコード単位で版数を超える古い履歴を削除
- **日数制限**: 同期サイクル完了後、同期定義単位で期限切れの履歴を一括削除
- いずれの設定も `null` にすると、その条件による制限は無効（無制限保持）
- 両方 `null` の場合、履歴は無制限に保持される

```text
例: MaxVersions = 20, MaxDays = 180

レコード A: 30 版の履歴 → 版数制限で古い 10 版を削除 → 20 版残存
レコード B: 15 版（うち 3 版が 200 日前）→ 日数制限で 3 版を削除 → 12 版残存
```

## エラーハンドリング

- 同期処理中にエラーが発生した場合、`SyncLogEntry` に `Failed` ステータスとエラーメッセージを記録
- Worker サービスは個別の同期定義のエラーで停止せず、30秒のリトライ遅延後に次の同期サイクルを継続
- データベース操作はトランザクションを使用し、一貫性を確保
