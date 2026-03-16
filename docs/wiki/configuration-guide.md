# 設定ガイド

このドキュメントでは、VehicleVision.Pleasanter.ReplicaSync の設定方法について説明します。

<!-- START doctoc generated TOC please keep comment here to allow auto update -->
<!-- DON'T EDIT THIS SECTION, INSTEAD RE-RUN doctoc TO UPDATE -->

- [構成データベース](#構成データベース)
- [同期インスタンス（SyncInstance）](#同期インスタンスsyncinstance)
    - [設定例](#設定例)
- [同期定義（SyncDefinition）](#同期定義syncdefinition)
- [ターゲットマッピング（SyncTargetMapping）](#ターゲットマッピングsynctargetmapping)
- [カラム指定](#カラム指定)
    - [基本カラム](#基本カラム)
    - [拡張カラム](#拡張カラム)
    - [指定例](#指定例)

<!-- END doctoc generated TOC please keep comment here to allow auto update -->

## 構成データベース

同期設定を保存する構成データベースの接続先は `appsettings.json` で設定します。

```json
{
    "ConnectionStrings": {
        "ConfigDatabase": "<接続文字列>"
    },
    "ConfigDatabaseType": "SqlServer"
}
```

| 設定項目             | 説明                         | 既定値      |
| -------------------- | ---------------------------- | ----------- |
| `ConfigDatabase`     | 構成データベースの接続文字列 | -           |
| `ConfigDatabaseType` | 構成データベースの種類       | `SqlServer` |

`ConfigDatabaseType` に指定可能な値：

| 値           | 説明       |
| ------------ | ---------- |
| `SqlServer`  | SQL Server |
| `PostgreSql` | PostgreSQL |

## 同期インスタンス（SyncInstance）

同期対象となる Pleasanter インスタンスの接続情報を登録します。

| プロパティ         | 型         | 説明                                                       |
| ------------------ | ---------- | ---------------------------------------------------------- |
| `InstanceId`       | `string`   | インスタンスの一意識別子（例: `headquarters`）             |
| `DisplayName`      | `string`   | 表示名                                                     |
| `DbmsType`         | `DbmsType` | データベースの種類（`SqlServer` / `PostgreSql` / `MySql`） |
| `ConnectionString` | `string`   | Pleasanter データベースへの接続文字列                      |

### 設定例

```text
InstanceId:       headquarters
DisplayName:      本社
DbmsType:         SqlServer
ConnectionString: Server=localhost;Database=Implem.Pleasanter;Trusted_Connection=True;
```

## 同期定義（SyncDefinition）

データ同期のルールを定義します。

| プロパティ               | 型                           | 説明                                             |
| ------------------------ | ---------------------------- | ------------------------------------------------ |
| `SyncId`                 | `string`                     | 同期定義の一意識別子                             |
| `Description`            | `string`                     | 説明                                             |
| `Topology`               | `TopologyType`               | `HubSpoke` または `PeerToPeer`                   |
| `ConflictResolution`     | `ConflictResolutionStrategy` | 競合解決戦略                                     |
| `ChangeDetectionMethod`  | `ChangeDetectionMethod`      | 変更検出方法（現在は `Polling` のみ）            |
| `PollingIntervalSeconds` | `int`                        | ポーリング間隔（1～3600秒）                      |
| `SyncUserId`             | `int`                        | 同期に使用する Pleasanter ユーザーID             |
| `SyncUserName`           | `string`                     | 同期ユーザーの表示名                             |
| `SourceInstanceId`       | `int`                        | ソースインスタンスの ID                          |
| `SourceSiteId`           | `long`                       | ソース Pleasanter サイト ID                      |
| `IsEnabled`              | `bool`                       | 同期定義の有効/無効                              |
| `SyncKeyColumns`         | `string`                     | 同期キーカラム（カンマ区切り）                   |
| `IncludeColumns`         | `string`                     | 同期対象カラム（カンマ区切り、省略時は全カラム） |
| `ExcludeColumns`         | `string`                     | 同期除外カラム（カンマ区切り）                   |

## ターゲットマッピング（SyncTargetMapping）

同期定義に対するターゲットインスタンスのマッピングを設定します。

| プロパティ                     | 型       | 説明                                         |
| ------------------------------ | -------- | -------------------------------------------- |
| `TargetInstanceId`             | `int`    | ターゲットインスタンスの ID                  |
| `TargetSiteId`                 | `long`   | ターゲット Pleasanter サイト ID              |
| `SourceToTargetEnabled`        | `bool`   | ソースからターゲットへの同期を有効化         |
| `TargetToSourceEnabled`        | `bool`   | ターゲットからソースへの同期を有効化         |
| `TargetToSourceExcludeColumns` | `string` | ターゲットからソースへの同期で除外するカラム |
| `TargetExcludeColumns`         | `string` | ターゲット側で除外するカラム                 |

## カラム指定

Pleasanter のデータカラムは以下の形式で指定します。

### 基本カラム

- `Title` - タイトル
- `Body` - 本文

### 拡張カラム

- `ClassA` ～ `ClassZ` - 分類項目
- `NumA` ～ `NumZ` - 数値項目
- `DateA` ～ `DateZ` - 日付項目
- `DescriptionA` ～ `DescriptionZ` - 説明項目

### 指定例

```text
SyncKeyColumns:  ClassA
IncludeColumns:  Title,ClassA,ClassB,NumA
ExcludeColumns:  DescriptionZ
```

上記の場合、`ClassA` をキーとしてレコードを照合し、`Title`、`ClassA`、`ClassB`、`NumA` のみを同期対象とします。
`ExcludeColumns` は `IncludeColumns` が未指定の場合に全カラムから除外するカラムを指定する際に使用します。
